using System;
using MonoDevelop.Components;
using MonoDevelop.Ide.Gui.Dialogs;
using FileChooserAction = MonoDevelop.Components.FileChooserAction;

namespace NuGetPackageMakerAddin
{
    public class NuGetPackageMakerSettingPanel : OptionsPanel
    {
        private NuGetPackageMakerSettingWidget _widget;

        public override Control CreatePanelWidget()
        {
            _widget = new NuGetPackageMakerSettingWidget();

            var settings = NuGetPackageMakerSettings.Current;

            _widget.BrowsButton.Clicked += OnClicked;
            var path = settings.CustomPath ?? string.Empty;

            _widget.OutputPathEntry.Text = path;

            var isCustom = settings.UsingCustomPath;
            _widget.CheckCustomPath.Active = isCustom;
            OnCheckCustomPathChanged(null, null);
            _widget.CheckCustomPath.Toggled += OnCheckCustomPathChanged;

            _widget.CheckBeforeBuildButton.Active = settings.BeforeBuild;
            _widget.CheckUseReleaseBuildButton.Active = settings.UseReleaseBuild;

            _widget.AutoPublishButton.Active = settings.AutoPublish;
            _widget.AutoPublishButton.Toggled += OnAutoPublishChanged;
            OnAutoPublishChanged(null, null);
            _widget.PublishEntry.Text = settings.PublishUrl ?? string.Empty;

            _widget.Show();

            return _widget;
        }

        private void OnAutoPublishChanged(object sender, EventArgs eventArgs)
        {
            _widget.PublishEntry.Sensitive = _widget.AutoPublishButton.Active;
        }


        private void OnCheckCustomPathChanged(object o, EventArgs eventArgs)
        {
            if (_widget.CheckCustomPath.Active)
            {
                _widget.OutputPathEntry.Sensitive = true;
                _widget.BrowsButton.Sensitive = true;
            }
            else
            {
                _widget.OutputPathEntry.Sensitive = false;
                _widget.BrowsButton.Sensitive = false;
            }
        }

        private void OnClicked(object sender, EventArgs eventArgs)
        {
            var dialog = new SelectFileDialog("保存先", FileChooserAction.SelectFolder);
            if (dialog.Run())
            {
                _widget.OutputPathEntry.Text = dialog.SelectedFile;
            }
        }

        public override void ApplyChanges()
        {
            var settings = NuGetPackageMakerSettings.Current;
            settings.CustomPath = _widget.OutputPathEntry.Text;
            settings.UsingCustomPath = _widget.CheckCustomPath.Active;
            settings.BeforeBuild = _widget.CheckBeforeBuildButton.Active;
            settings.UseReleaseBuild = _widget.CheckUseReleaseBuildButton.Active;
            settings.AutoPublish = _widget.AutoPublishButton.Active;
            settings.PublishUrl = _widget.PublishEntry.Text;
        }

        public override void Dispose()
        {
            _widget.BrowsButton.Clicked -= OnClicked;
            _widget.CheckCustomPath.Toggled -= OnCheckCustomPathChanged;
            _widget.AutoPublishButton.Toggled -= OnAutoPublishChanged;
            base.Dispose();
        }
    }
}