using System;
using System.IO;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;

namespace NuGetPackageMakerAddin
{
    public class MakeNupackHandler : CommandHandler
    {
        protected override void Update(CommandInfo info)
        {
            info.Visible = ProjectService.CurrentSolution != null;
        }

        protected override async void Run()
        {
            using (var monitor = ProgressMonitorService.GetNupackMonitor)
            {
                try
                {
                    var solution = ProjectService.CurrentSolution;
                    var path = solution.BaseDirectory;

                    var nuspecFiles = Directory.EnumerateFiles(path, "*.nuspec", SearchOption.AllDirectories);
                    if (NuGetPackageMakerSettings.Current.BeforeBuild)
                    {
                        var reslut=await solution.Build(IdeApp.Workbench.ProgressMonitors.GetBuildProgressMonitor(),
                            NuGetPackageMakerSettings.Current.UseReleaseBuild
                                ? solution.Configurations["Release"].Selector
                                : IdeApp.Workspace.ActiveConfiguration);

                        if (reslut.Failed)
                        {
                            monitor.ErrorLog.WriteLine($"\n\n===ビルドに失敗しました。===\n\n");
                            return;
                        }
                        else
                        {
                            monitor.Log.WriteLine("\n\n==ビルドに成功しました。==\n\n");
                        }
                    }

                    foreach (var nuspecPath in nuspecFiles)
                    {
                        monitor.Log.WriteLine($"{nuspecPath}で実行中...");
                        await NuGetOperationHelper.CreateNupack(nuspecPath, monitor);
                    }

                }
                catch (Exception e)
                {
                    monitor.ErrorLog.WriteLine($"{e.Message}\n{e.StackTrace}");
                }
            }
        }
    }
}