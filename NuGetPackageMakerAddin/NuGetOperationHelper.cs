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
                //.nuspec読み込み
                var nuspec = XElement.Load(path);

                foreach (var replace in nuspec.XPathSelectElements("metadata").Elements())
                {
                    if (replace.HasElements) break;
                    ReplaceMacro(replace);
                }

                foreach (var replace in nuspec.XPathSelectElements("//*/file").Attributes())
                {
                    ReplaceMacro(replace);
                }

                //.nuspecファイルの親ディレクトリがtoolsだったら
                if (path.ParentDirectory.FileName == "tools")
                {
                    foreach (var replace in nuspec.XPathSelectElements("*/file"))
                    {
                        //../削除
                        replace.Attribute("src").Value = Regex.Replace(replace.Attribute("src").Value, "\\.\\./", "");
                    }

                    //pathのカレントディレクトリを一段上に
                    path = path.ParentDirectory;
                }

                //実行用のダミーnuspec作成
                var nuspecPath = path.ParentDirectory.Combine($"backup.nuspec");

                nuspec.Save(nuspecPath);
                ProcessService.RunNupack(nuspecPath, monitor);
                File.Delete(nuspecPath);

                if (NuGetPackageMakerSettings.Current.AutoPublish)
                {
                    string outputPath = NuGetPackageMakerSettings.Current.UsingCustomPath
                        ? NuGetPackageMakerSettings.Current.CustomPath
                        : path.ToString();

                    var nupkg = string.Join(".", nuspec.XPathSelectElement("metadata/id").Value,
                        nuspec.XPathSelectElement("metadata/version").Value, "nupkg");


                    await ProcessService.RunPush(Path.Combine(outputPath, nupkg));
                }
            });

        //現在のソリューションの全てのプロジェクトのpackages.configが存在したら
        //読み込んでその中のpackage要素のidとversionをdependency要素のid要素とversion要素に変換して
        //idが重複してたら削除
        private static IEnumerable<XElement> GetDependencies()
            => ProjectService.CurrentSolution.GetAllProjects()
                .Where(x => File.Exists(x.BaseDirectory.Combine("packages.config")))
                .SelectMany(x => XElement.Load(x.BaseDirectory.Combine("packages.config")).Elements("package"))
                .Select(x => new XElement("dependency",
                    new XAttribute("id", x.Attribute("id").Value),
                    new XAttribute("version", x.Attribute("version").Value)))
                .Distinct(x => x.Attribute("id")?.Value);

        //現在のソリューションの全てのプロジェクトの現在の設定での出力先パスを現在のソリューションがあるディレクトリから見た相対パスで取得して
        //その中の現在の設定の文字列を$Configuration$に変換したものを..とパスで繋いでそれをfile要素のsrc属性に指定して
        //target属性にlib/を追加するけど自分で書いてて日本語おかしいと思った
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
            //正規表現で$$マクロを割り出し
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
    }
}