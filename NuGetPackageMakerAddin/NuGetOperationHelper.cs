using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
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
    }
}