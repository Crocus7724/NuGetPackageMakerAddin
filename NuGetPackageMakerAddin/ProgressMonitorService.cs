using System;
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using MonoDevelop.Ide;
using MonoDevelop.Ide.TypeSystem;

namespace NuGetPackageMakerAddin
{
    public class ProgressMonitorService:OutputProgressMonitor
    {
        private static ProgressMonitorService _nuspecMonitor;
        private static ProgressMonitorService _nupackMonitor;

        private bool disposed = false;
        private readonly ProgressMonitor _monitor;

        public static ProgressMonitor GetNuspecMonitor =>
            (_nuspecMonitor != null && !_nuspecMonitor.disposed)
                ? _nuspecMonitor
                : (_nuspecMonitor = new ProgressMonitorService("Make Nuspec"));
            //IdeApp.Workbench.ProgressMonitors.GetOutputProgressMonitor("Make Nuspec", IconId.Null, true, true, true);

        public static ProgressMonitor GetNupackMonitor =>
            (_nupackMonitor != null && !_nuspecMonitor.disposed)
                ? _nupackMonitor
                : (_nupackMonitor = new ProgressMonitorService("Make Package"));
            //IdeApp.Workbench.ProgressMonitors.GetOutputProgressMonitor("Make Nupack", IconId.Null, true, true, true);

        private ProgressMonitorService(string title)
        {
            _monitor = IdeApp.Workbench.ProgressMonitors.GetOutputProgressMonitor(title, IconId.Null, true, true, true);
            Log = _monitor.Log;
            ErrorLog = _monitor.ErrorLog;
        }


        public override OperationConsole Console => ((OutputProgressMonitor) _monitor).Console;

        public override void Dispose()
        {
            if(disposed)return;

            _monitor.Dispose();
            disposed = true;
        }
    }
}