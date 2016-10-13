using System;
using System.IO;
using Gtk;
using MonoDevelop.Components;
using MonoDevelop.Core;
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

            _widget.BrowsButton.Clicked += OnClicked;
            var path = PropertyService.Get<string>(NuGetConst.OutputPathKey) ?? string.Empty;

            _widget.OutputPathEntry.Text = path;

            var isCustom = PropertyService.Get<bool>(NuGetConst.CheckCustomPathKey);
            _widget.CheckCustomPath.Active = isCustom;
            OnStateChanged(null,null);
            _widget.CheckCustomPath.Toggled += OnStateChanged;

            return _widget;
        }

        private void OnStateChanged(object o, EventArgs eventArgs)
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
            PropertyService.Set(NuGetConst.CheckCustomPathKey, _widget.CheckCustomPath.Active);
            PropertyService.Set(NuGetConst.OutputPathKey, _widget.OutputPathEntry.Text);
        }

        public override void Dispose()
        {
            _widget.BrowsButton.Clicked -= OnClicked;
            _widget.CheckCustomPath.Toggled -= OnStateChanged;

            base.Dispose();
        }
    }
}