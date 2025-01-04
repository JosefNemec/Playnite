using Playnite.FullscreenApp.Controls.SettingsSections;
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
        private readonly Dictionary<int, SettingsSectionControl> sectionViews;
        private IInputElement oldFocus;
        private List<string> editedFields = new List<string>();

        private SettingsSectionControl selectedSectionView;
        public SettingsSectionControl SelectedSectionView
        {
            get => selectedSectionView;
            set
            {
                selectedSectionView = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSubMenuOpened));
            }
        }

        public bool IsSubMenuOpened { get => SelectedSectionView != null; }

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

        public RelayCommand CloseCommand
        {
            get => new RelayCommand(() =>
            {
                Close();
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
            mainModel.AppSettings.Fullscreen.PropertyChanged += (_, e) => editedFields.AddMissing(e.PropertyName);
            sectionViews = new Dictionary<int, SettingsSectionControl>()
            {
                { 0, new Controls.SettingsSections.General(mainModel) { DataContext = this } },
                { 1, new Controls.SettingsSections.Visuals(mainModel) { DataContext = this } },
                { 2, new Controls.SettingsSections.Layout(mainModel) { DataContext = this } },
                { 3, new Controls.SettingsSections.Menus(mainModel) { DataContext = this } },
                { 4, new Controls.SettingsSections.Input(mainModel) { DataContext = this } },
                { 5, new Controls.SettingsSections.Audio(mainModel) { DataContext = this } },
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
                if (editedFields?.Any(a => typeof(FullscreenSettings).HasPropertyAttribute<RequiresRestartAttribute>(a)) == true)
                {
                    if (Dialogs.ShowMessage(
                        LOC.SettingsRestartAskMessage, LOC.SettingsRestartTitle,
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        PlayniteApplication.Current.Restart(new CmdLineOptions() { SkipLibUpdate = true });
                    }
                }

                window.Close(true);
            }
            else
            {
                SelectedSectionView.Dispose();
                SelectedSectionView = null;
                IsMenuEnabled = true;
                oldFocus?.Focus();
            }
        }

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
            if (e.NewFocus is ComboBoxItem comboItem)
            {
                var cb = ItemsControl.ItemsControlFromItemContainer(comboItem) as ComboBox;
                OptionDescription = cb?.Tag?.ToString();
            }
            else if (e.NewFocus is FrameworkElement frm)
            {
                OptionDescription = frm.Tag?.ToString();
            }
        }
    }
}
