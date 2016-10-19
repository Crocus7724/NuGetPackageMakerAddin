using MonoDevelop.Core;

namespace NuGetPackageMakerAddin
{
    public class NuGetPackageMakerSettings
    {
        private bool _usingCustomPath;
        private string _customPath;
        private bool _beforeBuild;
        private bool _autoPublish;
        private bool _useReleaseBuild;
        private string _publishUrl;

        public static NuGetPackageMakerSettings Current { get; } = new NuGetPackageMakerSettings();


        public bool UsingCustomPath
        {
            get { return _usingCustomPath; }
            set
            {
                _usingCustomPath = value;
                PropertyService.Set(NuGetConst.CheckCustomPathKey, value);
            }
        }

        public string CustomPath
        {
            get { return _customPath; }
            set
            {
                _customPath = value;
                PropertyService.Set(NuGetConst.OutputPathKey, value);
            }
        }

        public bool BeforeBuild
        {
            get { return _beforeBuild; }
            set
            {
                _beforeBuild = value;
                PropertyService.Set(NuGetConst.BeforeBuildKey, value);
            }
        }

        public bool AutoPublish
        {
            get { return _autoPublish; }
            set
            {
                _autoPublish = value;
                PropertyService.Set(NuGetConst.CheckUseAutoPublishKey, value);
            }
        }

        public bool UseReleaseBuild
        {
            get { return _useReleaseBuild; }
            set
            {
                _useReleaseBuild = value;
                PropertyService.Set(NuGetConst.UseReleaseBuildKey, value);
            }
        }

        public string PublishUrl
        {
            get { return _publishUrl; }
            set
            {
                _publishUrl = value;
                PropertyService.Set(NuGetConst.PublishUrlKey, value);
            }
        }

        public NuGetPackageMakerSettings()
        {
            _usingCustomPath = PropertyService.Get<bool>(NuGetConst.CheckCustomPathKey);
            _customPath = PropertyService.Get<string>(NuGetConst.OutputPathKey);
            _beforeBuild = PropertyService.Get<bool>(NuGetConst.BeforeBuildKey);
            _useReleaseBuild = PropertyService.Get<bool>(NuGetConst.UseReleaseBuildKey);
            _autoPublish = PropertyService.Get<bool>(NuGetConst.CheckUseAutoPublishKey);
            _publishUrl = PropertyService.Get<string>(NuGetConst.PublishUrlKey);
        }
    }
}