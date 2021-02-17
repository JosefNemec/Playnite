using Playnite.SDK;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Playnite.FullscreenApp.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IWindowFactory window;
        private readonly Dictionary<int, UserControl> sectionViews;

        private UserControl selectedSectionView;
        public UserControl SelectedSectionView
        {
            get => selectedSectionView;
            set
            {
                selectedSectionView = value;
                OnPropertyChanged();
            }
        }

        private bool isMenuEnabled = true;
        public bool IsMenuEnabled
        {
            get => isMenuEnabled;
            set
            {
                isMenuEnabled = value;
                OnPropertyChanged();
            }
        }

        private string optionDescription;
        public string OptionDescription
        {
            get => optionDescription;
            set
            {
                optionDescription = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand<KeyboardFocusChangedEventArgs> PreviewGotKeyboardFocusCommand
        {
            get => new RelayCommand<KeyboardFocusChangedEventArgs>((args) =>
            {
                PreviewGotKeyboardFocus(args);
            });
        }

        public RelayCommand<object> CloseCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                Close();
            });
        }

        public RelayCommand<object> CloseSectionCommand
        {
            get => new RelayCommand<object>((a) =>
            {
            });
        }

        public RelayCommand<string> OpenSectionCommand
        {
            get => new RelayCommand<string>((a) => OpenSection(a));
        }

        public SettingsViewModel(
            IWindowFactory window,
            FullscreenAppViewModel mainModel)
        {
            this.window = window;

            sectionViews = new Dictionary<int, UserControl>()
            {
                { 0, new Controls.SettingsSections.General(mainModel) { DataContext = this } },
                { 1, new Controls.SettingsSections.Visuals(mainModel) { DataContext = this } },
                { 2, new Controls.SettingsSections.Layout(mainModel) { DataContext = this } },
                { 3, new Controls.SettingsSections.Menus(mainModel) { DataContext = this } },
                { 4, new Controls.SettingsSections.Input(mainModel) { DataContext = this } },
            };
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        public void Close()
        {
            if (SelectedSectionView == null)
            {
                window.Close(true);
            }
            else
            {
                SelectedSectionView = null;
                IsMenuEnabled = true;
                oldFocus?.Focus();
            }
        }

        private IInputElement oldFocus;

        private void OpenSection(string section)
        {
            oldFocus = Keyboard.FocusedElement;
            var sec = int.Parse(section);
            IsMenuEnabled = false;
            SelectedSectionView = sectionViews[sec];
            SelectedSectionView.Focus();
        }

        private void PreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            if (e.NewFocus is FrameworkElement frm)
            {
                OptionDescription = frm.Tag?.ToString();
            }
        }
    }
}
