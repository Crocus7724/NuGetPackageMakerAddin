using System.Diagnostics;
using System.Threading.Tasks;
using MonoDevelop.Core;

namespace NuGetPackageMakerAddin
{
    internal class ProcessService
    {
        public static void RunNupack(FilePath path, ProgressMonitor monitor)
        {
            using (var process = new Process())
            {
                var outputDirectory = NuGetPackageMakerSettings.Current.UsingCustomPath
                    ? $"-OutputDirectory {NuGetPackageMakerSettings.Current.CustomPath}"
                    : string.Empty;
                process.StartInfo = new ProcessStartInfo("nuget",
                    $"pack {path} -Verbosity detail {outputDirectory}")
                {
                    WorkingDirectory = path.ParentDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                process.ErrorDataReceived += (sender, args) => monitor.ErrorLog.WriteLine(args.Data);
                process.OutputDataReceived += (sender, args) => monitor.Log.WriteLine(args.Data);

                process.Start();


                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();
            }
        }

        public static async Task RunPush(FilePath path) =>
            await Task.Run(() =>
            {
                using (var process = new Process())
                {
                    var monitor = ProgressMonitorService.GetNupackMonitor;
                    var source = NuGetPackageMakerSettings.Current.PublishUrl;
                    process.StartInfo = new ProcessStartInfo("nuget", $"push {path} -Source {source}")
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                    };

                    process.ErrorDataReceived += (sender, args) => monitor.ErrorLog.WriteLine(args.Data);
                    process.OutputDataReceived += (sender, args) => monitor.Log.WriteLine(args.Data);

                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    process.WaitForExit();
                }
            });
    }
}