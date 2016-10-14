using System;
using Gtk;

namespace NuGetPackageMakerAddin
{
    [System.ComponentModel.ToolboxItem(true)]
    public partial class NuGetPackageMakerSettingWidget : Gtk.Bin
    {
        public RadioButton CheckCustomPath => checkCustomPath;

        public Entry OutputPathEntry => outputPathEntry;

        public Button BrowsButton => browsButton;

        public CheckButton CheckBeforeBuildButton => checkBeforeBuildButton;

        public CheckButton AutoPublishButton => autoPublishButton;

        public CheckButton CheckUseReleaseBuildButton => checkUseReleaseBuildButton;

        public Entry PublishEntry => publishEntry;

        public NuGetPackageMakerSettingWidget()
        {
            this.Build();
        }
    }
}