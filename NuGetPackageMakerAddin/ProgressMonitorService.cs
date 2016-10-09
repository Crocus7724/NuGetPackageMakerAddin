using MonoDevelop.Core;
using MonoDevelop.Ide;

namespace NuGetPackageMakerAddin
{
    public class ProgressMonitorService
    {
        public static ProgressMonitor GetNuspecMonitor =>
            IdeApp.Workbench.ProgressMonitors.GetBackgroundProgressMonitor("Make Nuspec", IconId.Null);
    }
}