using Playnite.Common;
using Playnite.DesktopApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Playnite.DesktopApp.Controls
{
    [TemplatePart(Name = "PART_ButtonDirectorySelect", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ButtonFileSelect", Type = typeof(Button))]
    public class PathSelectionBox : TextBox
    {
        private Button ButtonDirectorySelect;
        private Button ButtonFileSelect;

        public string FileSelectorFilter { get; set; } = "Any file|*.*";

        private bool showFileSelector = false;
        public bool ShowFileSelector
        {
            get => showFileSelector;
            set
            {
                showFileSelector = value;
                SetButtonVisibility();
            }
        }

        private bool showDirectorySelector = false;
        public bool ShowDirectorySelector
        {
            get => showDirectorySelector;
            set
            {
                showDirectorySelector = value;
                SetButtonVisibility();
            }
        }

        static PathSelectionBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PathSelectionBox), new FrameworkPropertyMetadata(typeof(PathSelectionBox)));
        }

        public PathSelectionBox()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ButtonDirectorySelect = Template.FindName("PART_ButtonDirectorySelect", this) as Button;
            if (ButtonDirectorySelect != null)
            {
                ButtonDirectorySelect.Click += (_, __) =>
                {
                    var path = Dialogs.SelectFolder();
                    if (!path.IsNullOrWhiteSpace())
                    {
                        Clear();
                        AppendText(path);
                    }
                };
            }

            ButtonFileSelect = Template.FindName("PART_ButtonFileSelect", this) as Button;
            if (ButtonFileSelect != null)
            {
                ButtonFileSelect.Click += (_, __) =>
                {
                    var path = Dialogs.SelectFile(FileSelectorFilter);
                    if (!path.IsNullOrWhiteSpace())
                    {
                        Clear();
                        AppendText(path);
                    }
                };
            }

            SetButtonVisibility();
        }

        private void SetButtonVisibility()
        {
            if (ButtonDirectorySelect != null)
            {
                ButtonDirectorySelect.Visibility = ShowDirectorySelector == true ? Visibility.Visible : Visibility.Collapsed;
            }

            if (ButtonFileSelect != null)
            {
                ButtonFileSelect.Visibility = ShowFileSelector == true ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
