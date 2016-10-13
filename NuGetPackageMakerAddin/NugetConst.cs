using System;

namespace NuGetPackageMakerAddin
{
    internal class NuGetConst
    {
        public const string DefaultId = "$id$";
        public const string DefaultTitle = "$title$";
        public const string DefaultVersion = "$version$";
        public const string DefaultAuthors = "$author$";
        public const string DefaultLicenseUrl = "http://LICENSE_URL_HERE_OR_DELETE_THIS_LINE";
        public const string DefaultProjectUrl = "http://PROJECT_URL_HERE_OR_DELETE_THIS_LINE";
        public const string DefaultIconUrl = "http://ICON_URL_HERE_OR_DELETE_THIS_LINE";
        public const bool DefaultRequireLicenseAcceptance = false;
        public const string DefaultDescription = "$description$";
        public const string DefaultReleaseNotes = "Summary of changes made in this release of the package.";
        public const string DefaultCopyright = "$copyright$";
        public const string DefaultTags = "Tag1 Tag2";


        public const string OutputPathKey = "NuGetPackageMakerAddinOutputPathKey";
        public const string CheckCustomPathKey = "NuGetPackageMakerAddinDefaultUsingKey";
    }
}