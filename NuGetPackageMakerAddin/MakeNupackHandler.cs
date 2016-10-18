using System;
using System.IO;
using System.Linq;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;

namespace NuGetPackageMakerAddin
{
    public class MakeNupackHandler : CommandHandler
    {
        protected override void Update(CommandInfo info)
            => info.Visible = ProjectService.CurrentSolution != null;

        protected override async void Run()
        {
            using (var monitor = ProgressMonitorService.GetNupackMonitor)
            {
                try
                {
                    var solution = ProjectService.CurrentSolution;
                    var path = solution.BaseDirectory;

                    //ソリューションの子ディレクトリ全てで.nuspecファイルを取得
                    var nuspecFiles = Directory.EnumerateFiles(path, "*.nuspec", SearchOption.AllDirectories);
                    if (NuGetPackageMakerSettings.Current.BeforeBuild)
                    {
                        using (var buildMonitor = IdeApp.Workbench.ProgressMonitors.GetBuildProgressMonitor())
                        {
                            //ビルド(Releaseビルドを使う設定だったらReleaseビルドそれ以外なら現在の設定)
                            var reslut = await solution.Build(buildMonitor,
                                NuGetPackageMakerSettings.Current.UseReleaseBuild
                                    ? solution.Configurations["Release"].Selector
                                    : IdeApp.Workspace.ActiveConfiguration);

                            //結果でエラーが合ったら
                            if (reslut.HasErrors)
                            {
                                //エラーを出力して終了
                                monitor.ErrorLog.WriteLine($"\n\n===ビルドに失敗しました。===\n\n");
                                monitor.ErrorLog.WriteLine("===Error===");
                                monitor.ErrorLog.WriteLine(
                                    $"{reslut.Errors.Select(x => x.ErrorText).Aggregate((x, y) => x + "\n" + y)}");
                                monitor.ErrorLog.WriteLine("===End Error===");
                                return;
                            }
                            else
                            {
                                monitor.Log.WriteLine("\n\n==ビルドに成功しました。==\n\n");
                            }
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