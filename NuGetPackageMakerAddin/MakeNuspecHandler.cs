using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MonoDevelop.Components.Commands;
using MonoDevelop.Core;
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
            using (var monitor = ProgressMonitorService.GetNuspecMonitor)
            {
                try
                {
                    var solution = IdeApp.ProjectOperations.CurrentSelectedSolution;
                    var toolsPath = Path.Combine(solution.BaseDirectory, "tools");

                    await AddToolsFolderIfNotFound(toolsPath, monitor);

                    var nuspecPath = Path.Combine(toolsPath, $"{solution.Name}.nuspec");

                    await AddNuspecFileIfNotFound(nuspecPath, monitor);
                }
                catch (Exception e)
                {
                    monitor.ErrorLog.WriteLine($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        private async Task AddToolsFolderIfNotFound(string path, ProgressMonitor monitor)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                monitor.Log.WriteLine($"{path}を作成しました。");
            }

            if (IdeApp.ProjectOperations.CurrentSelectedSolution.RootFolder.Items.All(x => x.Name != "tools"))
            {
                ProjectService.CurrentSolution.RootFolder.AddItem(new SolutionFolder()
                {
                    BaseDirectory = path,
                    Name = "tools"
                });
                await ProjectService.CurrentSolution.SaveAsync(monitor);

                ProgressMonitorService.GetNuspecMonitor.Log.WriteLine($"{path}をソリューションに追加しました。");
            }
        }

        private async Task AddNuspecFileIfNotFound(string path, ProgressMonitor monitor)
        {
            if (!File.Exists(path))
            {
                await NuGetOperationHelper.CreateNuspec(path);
                monitor.Log.WriteLine($"{path}を作成しました。");
            }

            var solution = ProjectService.CurrentSolution;
            var folder = solution.RootFolder.Items
                .FirstOrDefault(x => x.Name == "tools") as SolutionFolder;
            if (folder.Files.FirstOrDefault(x => x.FileName == $"{solution.Name}.nuspec") == null)
            {
                IdeApp.ProjectOperations.AddFilesToSolutionFolder(folder, new[] {path});

                await ProjectService.CurrentSolution.SaveAsync(monitor);

                monitor.Log.WriteLine($"{path}をソリューションに追加しました。");
            }
        }
    }
}