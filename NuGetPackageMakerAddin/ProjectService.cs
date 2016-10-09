using MonoDevelop.Ide;
using MonoDevelop.Projects;

namespace NuGetPackageMakerAddin
{
    public class ProjectService
    {
        public static Project CurrentProject
            => IdeApp.ProjectOperations.CurrentSelectedProject;

        public static Solution CurrentSolution
            => IdeApp.ProjectOperations.CurrentSelectedSolution;
    }
}