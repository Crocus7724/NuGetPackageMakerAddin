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

        public NuGetPackageMakerSettingWidget()
        {
            this.Build();
        }
    }
}
