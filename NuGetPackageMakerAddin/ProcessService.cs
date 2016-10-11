using System.Diagnostics;
using System.Threading.Tasks;
using MonoDevelop.Core;

namespace NuGetPackageMakerAddin
{
    public class ProcessService
    {
        public static void RunNupack(string path, ProgressMonitor monitor)
        {
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo("nuget",
                    $"pack {ProjectService.CurrentSolution.Name}.nuspec -Verbosity detail")
                {
                    WorkingDirectory = path,
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
    }
}