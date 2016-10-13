using System;
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

        protected override async void Run()
        {
            using (var monitor = ProgressMonitorService.GetNupackMonitor)
            {
                try
                {
                    var path = ProjectService.CurrentSolution.BaseDirectory;
                    var nuspecFiles = Directory.EnumerateFiles(path, "*.nuspec", SearchOption.AllDirectories);

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