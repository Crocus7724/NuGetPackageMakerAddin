using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Projects;

namespace NuGetPackageMakerAddin
{
    public class MakeNuspecHandler : CommandHandler
    {
        protected override void Update(CommandInfo info)
        {
            if (IdeApp.ProjectOperations.CurrentSelectedSolution == null)
            {
                info.Visible = false;
            }
            else
            {
                info.Visible = true;
                info.Enabled = IdeApp.ProjectOperations.CurrentSelectedSolution != null;
            }
        }

        protected override async void Run()
        {
            var solution = IdeApp.ProjectOperations.CurrentSelectedSolution;
            var toolsPath = Path.Combine(solution.BaseDirectory, "tools");
            //ソリューションフォルダにtoolsがなかったら
            if (solution.RootFolder.Items.All(x => x.Name != "tools"))
            {
                //追加
                await AddToolsFolder(toolsPath);
            }

            var nuspecPath = Path.Combine(toolsPath, $"{solution.Name}.nuspec");

            if (!File.Exists(nuspecPath))
            {
                await NuGetOperationHelper.CreateNuspec(nuspecPath);
            }

            var folder = solution.RootFolder.Items
                .FirstOrDefault(x => x.Name == "tools") as SolutionFolder;

            IdeApp.ProjectOperations.AddFilesToSolutionFolder(folder, new string[]{nuspecPath});
        }

        private Task AddToolsFolder(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            ProjectService.CurrentSolution.RootFolder.AddItem(new SolutionFolder()
            {
                BaseDirectory = path,
                Name = "tools"
            });
        }
    }
}