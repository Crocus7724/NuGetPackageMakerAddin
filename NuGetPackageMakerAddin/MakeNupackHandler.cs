using System.IO;
using MonoDevelop.Components.Commands;

namespace NuGetPackageMakerAddin
{
    public class MakeNupackHandler : CommandHandler
    {
        protected override void Update(CommandInfo info)
        {
            info.Visible = ProjectService.CurrentSolution != null;
        }

        protected override void Run()
        {
            var path = ProjectService.CurrentSolution.BaseDirectory;
            var nuspecFiles = Directory.EnumerateFiles(path, "*.nuspec", SearchOption.AllDirectories);

            foreach (var nuspecPath in nuspecFiles)
            {
                NuGetOperationHelper.CreateNupack(nuspecPath);
            }
        }
    }
}