using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using GLib;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using NuGetPackageMakerAddin.Extensions;

namespace NuGetPackageMakerAddin
{
    internal class NuGetOperationHelper
    {
        public static Task CreateNuspec(string path)
            => Task.Run((() =>
            {
                var nuspec = new XElement("package",
                    new XElement("metadata",
                        new XElement("id", NuGetConst.DefaultId),
                        new XElement("version", NuGetConst.DefaultVersion),
                        new XElement("authors", NuGetConst.DefaultAuthors),
                        new XElement("licenseUrl", NuGetConst.DefaultLicenseUrl),
                        new XElement("projectUrl", NuGetConst.DefaultProjectUrl),
                        new XElement("iconUrl", NuGetConst.DefaultIconUrl),
                        new XElement("requireLicenseAcceptance", NuGetConst.DefaultRequireLicenseAcceptance),
                        new XElement("description", NuGetConst.DefaultDescription),
                        new XElement("releaseNotes", NuGetConst.DefaultReleaseNotes),
                        new XElement("copyright", NuGetConst.DefaultCopyright),
                        new XElement("tags", NuGetConst.DefaultTags),
                        new XElement("dependencies", GetDependencies())),
                    new XElement("files", GetFiles()));

                nuspec.Save(path);
            }));


        public static Task CreateNupack(FilePath path, ProgressMonitor monitor)
            => Task.Run(async () =>
            {
                var nuspec = XElement.Load(path);
                foreach (var replace in nuspec.XPathSelectElements("metadata").Elements())
                {
                    ReplaceMacro(replace);
                }

                foreach (var replace in nuspec.XPathSelectElements("//*/file").Attributes())
                {
                    ReplaceMacro(replace);
                }

                if (path.ParentDirectory.FileName == "tools")
                {
                    foreach (var replace in nuspec.XPathSelectElements("*/file"))
                    {
                        replace.Attribute("src").Value = Regex.Replace(replace.Attribute("src").Value, "\\.\\./", "");
                    }

                    var nuspecPath = Path.Combine(path.ParentDirectory.ParentDirectory,
                        $"{path.FileNameWithoutExtension}-backup.nuspec");
                    nuspec.Save(nuspecPath);
                    ProcessService.RunNupack(nuspecPath, monitor);
                    File.Delete(nuspecPath);

                    await PublishIfEnable(nuspecPath);
                }
                else
                {
                    var backupPath = Path.Combine(path.ParentDirectory, $"{path.FileNameWithoutExtension}-backup.nuspec");
                    nuspec.Save(backupPath);
                    ProcessService.RunNupack(backupPath, monitor);
                    File.Delete(backupPath);

                    await PublishIfEnable(backupPath);
                }
            });

        private static IEnumerable<XElement> GetDependencies()
            => ProjectService.CurrentSolution.GetAllProjects()
                .Where(x => File.Exists(Path.Combine(x.BaseDirectory, "packages.config")))
                .SelectMany(x => XElement.Load(Path.Combine(x.BaseDirectory, "packages.config")).Elements("package"))
                .Distinct(x => x.Attribute("id")?.Value)
                .Select(x => new XElement("dependency",
                    new XAttribute("id", x.Attribute("id").Value),
                    new XAttribute("version", x.Attribute("version").Value)));

        private static IEnumerable<XElement> GetFiles()
            => ProjectService.CurrentSolution.GetAllProjects()
                .Select(x => new XElement("file",
                    new XAttribute("src",
                        Path.Combine("..",
                            x.GetOutputFileName(IdeApp.Workspace.ActiveConfiguration)
                                .ToRelative(ProjectService.CurrentSolution.BaseDirectory)).Replace(
                            IdeApp.Workspace.ActiveConfigurationId, "$Configuration$")),
                    new XAttribute("target", "lib/")));

        private static void ReplaceMacro(XElement input) => input.Value = ConvertMacro(input.Value);

        private static void ReplaceMacro(XAttribute input) => input.Value = ConvertMacro(input.Value);

        private static string ConvertMacro(string input)
        {
            var match = Regex.Match(input, @"\$.*?\$");
            if (!match.Success) return input;

            var solution = ProjectService.CurrentSolution;
            while (match.Success)
            {
                switch (match.Value)
                {
                    case NuGetConst.DefaultId:
                    case NuGetConst.DefaultTitle:
                        ProgressMonitorService.GetNupackMonitor.Log.WriteLine($"{match.Value} -> {solution.Name}");
                        input = input.Replace(match.Value, solution.Name);
                        break;
                    case NuGetConst.DefaultVersion:
                        ProgressMonitorService.GetNupackMonitor.Log.WriteLine($"{match.Value} -> {solution.Version}");
                        input = input.Replace(match.Value, solution.Version);
                        break;
                    case NuGetConst.DefaultAuthors:
                        ProgressMonitorService.GetNupackMonitor.Log.WriteLine(
                            $"{match.Value} -> {solution.AuthorInformation.Name}");
                        input = input.Replace(match.Value, solution.AuthorInformation.Name);
                        break;
                    case NuGetConst.DefaultDescription:
                        ProgressMonitorService.GetNupackMonitor.Log.WriteLine($"{match.Value} -> {solution.Description}");
                        input = input.Replace(match.Value, solution.Description);
                        break;
                    case NuGetConst.DefaultCopyright:
                        ProgressMonitorService.GetNupackMonitor.Log.WriteLine(
                            $"{match.Value} -> {solution.AuthorInformation.Copyright}");
                        input = input.Replace(match.Value, solution.AuthorInformation.Copyright);
                        break;
                    case "$Configuration$":
                        var config = NuGetPackageMakerSettings.Current.UseReleaseBuild
                            ? "Release"
                            : IdeApp.Workspace.ActiveConfigurationId;
                        ProgressMonitorService.GetNupackMonitor.Log.WriteLine(
                            $"{match.Value} -> {config}");
                        input = input.Replace(match.Value, config);
                        break;
                }

                match = match.NextMatch();
            }

            return input;
        }

        private static async Task PublishIfEnable(FilePath nuspecPath)
        {
            var settings = NuGetPackageMakerSettings.Current;
            if (!settings.AutoPublish) return;

            string nupkgPath = settings.UsingCustomPath ? settings.CustomPath : nuspecPath.ParentDirectory.ToString();

            await ProcessService.RunPush(nupkgPath);
        }
    }
}