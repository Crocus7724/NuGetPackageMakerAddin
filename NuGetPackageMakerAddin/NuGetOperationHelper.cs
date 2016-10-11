using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using NuGetPackageMakerAddin.Extensions;

namespace NuGetPackageMakerAddin
{
    public class NuGetOperationHelper
    {
        public static Task CreateNuspec(string path)
            => Task.Run((() =>
            {
                var nuspec = new XElement("package",
                    new XElement("metadata",
                        new XElement("id", NugetConst.DefaultId),
                        new XElement("version", NugetConst.DefaultVersion),
                        new XElement("authors", NugetConst.DefaultAuthors),
                        new XElement("licenseUrl", NugetConst.DefaultLicenseUrl),
                        new XElement("projectUrl", NugetConst.DefaultProjectUrl),
                        new XElement("iconUrl", NugetConst.DefaultIconUrl),
                        new XElement("requireLicenseAcceptance", NugetConst.DefaultRequireLicenseAcceptance),
                        new XElement("description", NugetConst.DefaultDescription),
                        new XElement("releaseNotes", NugetConst.DefaultReleaseNotes),
                        new XElement("copyright", NugetConst.DefaultCopyright),
                        new XElement("tags", NugetConst.DefaultTags),
                        new XElement("dependencies", GetDependencies())),
                    new XElement("files", GetFiles()));

                nuspec.Save(path);
            }));


        public static Task CreateNupack(FilePath path, ProgressMonitor monitor)
            => Task.Run(() =>
            {
                var nuspec = XElement.Load(path);
                foreach (var replace in nuspec.Elements()
                    .Where(x => x.Name == "metadata" || x.Name == "files").Elements())
                {
                    ReplaceMacro(replace);
                }

                foreach (var replace in nuspec.Element("files").Elements().Attributes())
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
                        $"{ProjectService.CurrentSolution.Name}.nuspec");
                    nuspec.Save(nuspecPath);
                    ProcessService.RunNupack(path.ParentDirectory.ParentDirectory, monitor);
                    File.Delete(nuspecPath);
                }
                else
                {
                    nuspec.Save(path.ParentDirectory);
                    ProcessService.RunNupack(path, monitor);
                }
            });

        private static IEnumerable<XElement> GetDependencies()
            => ProjectService.CurrentSolution.GetAllProjects()
                .Where(x => File.Exists(Path.Combine(x.BaseDirectory, "packages.config")))
                .SelectMany(x => XElement.Load(Path.Combine(x.BaseDirectory, "packages.config")).Elements("package"))
                .Distinct(x => x.Attribute("id"))
                .Select(x => new XElement("dependency",
                    new XAttribute("id", x.Attribute("id").Value),
                    new XAttribute("version", x.Attribute("version").Value)));

        private static IEnumerable<XElement> GetFiles()
            => ProjectService.CurrentSolution.GetAllProjects()
                .Select(x => new XElement("file",
                    new XAttribute("src", Path.Combine("..", x.Name, "bin", "$configuration$", $"{x.Name}.dll"))));

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
                    case NugetConst.DefaultId:
                    case NugetConst.DefaultTitle:
                        ProgressMonitorService.GetNupackMonitor.Log.WriteLine($"{match.Value} -> {solution.Name}");
                        input = input.Replace(match.Value, solution.Name);
                        break;
                    case NugetConst.DefaultVersion:
                        ProgressMonitorService.GetNupackMonitor.Log.WriteLine($"{match.Value} -> {solution.Version}");
                        input = input.Replace(match.Value, solution.Version);
                        break;
                    case NugetConst.DefaultAuthors:
                        ProgressMonitorService.GetNupackMonitor.Log.WriteLine(
                            $"{match.Value} -> {solution.AuthorInformation.Name}");
                        input = input.Replace(match.Value, solution.AuthorInformation.Name);
                        break;
                    case NugetConst.DefaultDescription:
                        ProgressMonitorService.GetNupackMonitor.Log.WriteLine($"{match.Value} -> {solution.Description}");
                        input = input.Replace(match.Value, solution.Description);
                        break;
                    case NugetConst.DefaultCopyright:
                        ProgressMonitorService.GetNupackMonitor.Log.WriteLine(
                            $"{match.Value} -> {solution.AuthorInformation.Copyright}");
                        input = input.Replace(match.Value, solution.AuthorInformation.Copyright);
                        break;
                    case "$configuration$":
                        ProgressMonitorService.GetNupackMonitor.Log.WriteLine(
                            $"{match.Value} -> {IdeApp.Workspace.ActiveConfigurationId}");
                        input = input.Replace(match.Value, IdeApp.Workspace.ActiveConfigurationId);
                        break;
                }

                match = match.NextMatch();
            }

            return input;
        }
    }
}