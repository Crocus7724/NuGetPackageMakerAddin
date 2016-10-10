using System.Diagnostics;
using System.Threading.Tasks;

namespace NuGetPackageMakerAddin
{
    public class ProcessService
    {
        public static Task RunNupack(string path)
            => Task.Run(() =>
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


                    process.ErrorDataReceived += (sender, args) => Debug.WriteLine(args.Data);
                    process.OutputDataReceived += (sender, args) => Debug.WriteLine(args.Data);

                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    process.WaitForExit();
                }
            });
    }
}