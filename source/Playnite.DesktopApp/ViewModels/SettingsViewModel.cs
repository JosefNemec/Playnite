using Playnite;
using Playnite.API;
using Playnite.Common;
using Playnite.Database;
using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Plugins;
using Playnite.Settings;
using Playnite.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Playnite.Windows;
using Playnite.DesktopApp.Markup;
using System.Text.RegularExpressions;
using Playnite.DesktopApp.Controls;
using System.Diagnostics;
using Playnite.SDK.Exceptions;
using Playnite.Scripting.PowerShell;
using System.Collections.ObjectModel;

namespace Playnite.DesktopApp.ViewModels
{
    public class SelectableTrayIcon
    {
        public TrayIconType TrayIcon { get; }
        public object ImageSource { get; }

        public SelectableTrayIcon(TrayIconType trayIcon)
        {
            TrayIcon = trayIcon;
            ImageSource = ResourceProvider.GetResource(TrayIcon.GetDescription());
        }
    }

    public class SettingsViewModel : ObservableObject
    {
        internal static ILogger logger = LogManager.GetLogger();
        internal IWindowFactory window;
        internal IResourceProvider resources;
        internal IDialogsFactory dialogs;
        internal GameDatabase database;
        internal PlayniteSettings originalSettings;
        internal PlayniteApplication application;
        internal List<string> editedFields = new List<string>();
        internal bool closingHanled = false;

        public List<SelectableItem<LibraryPlugin>> AutoCloseClientsList { get; } = new List<SelectableItem<LibraryPlugin>>();

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

        private PlayniteSettings settings;
        public PlayniteSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ImportExclusionItem> ImportExclusionList { get; }

        public List<LoadedPlugin> GenericPlugins
        {
            get; private set;
        }

        public List<ThemeManifest> AvailableThemes
        {
            get => ThemeManager.GetAvailableThemes(ApplicationMode.Desktop).OrderBy(a => a.Name).ToList();
        }

        public List<PlayniteLanguage> AvailableLanguages
        {
            get => Localization.AvailableLanguages;
        }

        public List<string> AvailableFonts
        {
            get => System.Drawing.FontFamily.Families.Where(a => !a.Name.IsNullOrEmpty()).Select(a => a.Name).ToList();
        }

        public List<SelectableTrayIcon> AvailableTrayIcons
        {
            get;
            private set;
        } = new List<SelectableTrayIcon>();

        private readonly Dictionary<int, UserControl> sectionViews;

        #region Commands

        public RelayCommand<object> SelectDbFileCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectDbFile();
            });
        }

        public RelayCommand<object> ClearWebCacheCommand
        {
            get => new RelayCommand<object>((url) =>
            {
                ClearWebcache();
            });
        }

        public RelayCommand<string> SetCoverArtAspectRatioCommand
        {
            get => new RelayCommand<string>((ratio) =>
            {
                SetCoverArtAspectRatio(ratio);
            });
        }

        public RelayCommand<object> SetDefaultFontSizes
        {
            get => new RelayCommand<object>((ratio) =>
            {
                Settings.FontSize = 14;
                Settings.FontSizeSmall = 12;
                Settings.FontSizeLarge = 15;
                Settings.FontSizeLarger = 20;
                Settings.FontSizeLargest = 29;
            });
        }

        public RelayCommand<object> AddDevelExtensionCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                var res = dialogs.SelectString(LOC.SettingsNewExternalExtensionBox, "", "");
                if (res.Result && !res.SelectedString.IsNullOrEmpty())
                {
                    if (Settings.DevelExtenions.FirstOrDefault(s => s.Item.Equals(res.SelectedString, StringComparison.OrdinalIgnoreCase)) != null)
                    {
                        return;
                    }

                    Settings.DevelExtenions.Add(new SelectableItem<string>(res.SelectedString) { Selected = true });
                    Settings.DevelExtenions = Settings.DevelExtenions.GetClone();
                }
            });
        }

        public RelayCommand<IList<object>> RemoveDevelExtensionCommand
        {
            get => new RelayCommand<IList<object>>((items) =>
            {
                foreach (SelectableItem<string> item in items.ToList())
                {
                    Settings.DevelExtenions.Remove(item);
                    Settings.DevelExtenions = Settings.DevelExtenions.GetClone();
                }
            }, (items) => items != null && items.Count > 0);
        }

        public RelayCommand<RoutedPropertyChangedEventArgs<object>> SettingsTreeSelectedItemChangedCommand
        {
            get => new RelayCommand<RoutedPropertyChangedEventArgs<object>>((a) =>
            {
                SettingsTreeSelectedItemChanged(a);
            });
        }

        public RelayCommand<object> SetDefaultsCommand
        {
            get => new RelayCommand<object>((ratio) =>
            {
                SetDefaults();
            });
        }

        public RelayCommand<string> TestScriptCommand
        {
            get => new RelayCommand<string>((a) =>
            {
                TestScript(a);
            }, (a) => !a.IsNullOrEmpty());
        }

        private readonly List<ImportExclusionItem> removedExclusionItems = new List<ImportExclusionItem>();
        public RelayCommand<IList<object>> RemoveImportExclusionItemCommand
        {
            get => new RelayCommand<IList<object>>((items) =>
            {
                foreach (ImportExclusionItem item in items.ToList())
                {
                    ImportExclusionList.Remove(item);
                    removedExclusionItems.Add(item);
                }
            }, (items) => items != null && items.Count > 0);
        }

        public RelayCommand<object> CancelCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CloseView();
            });
        }

        public RelayCommand<object> ConfirmCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ConfirmDialog();
            });
        }

        public RelayCommand<object> WindowClosingCommand
        {
            get => new RelayCommand<object>((a) =>
            {
            });
        }

        #endregion Commands

        public SettingsViewModel(
            GameDatabase database,
            PlayniteSettings settings,
            IWindowFactory window,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            ExtensionFactory extensions,
            PlayniteApplication app)
        {
            this.database = database;
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;
            this.application = app;
            originalSettings = settings;

            Settings = settings.GetClone();
            Settings.PropertyChanged += (s, e) => editedFields.AddMissing(e.PropertyName);

            AvailableTrayIcons = new List<SelectableTrayIcon>
            {
                new SelectableTrayIcon(TrayIconType.Default),
                new SelectableTrayIcon(TrayIconType.Bright),
                new SelectableTrayIcon(TrayIconType.Dark)
            };

            sectionViews = new Dictionary<int, UserControl>()
            {
                { 0, new Controls.SettingsSections.General() { DataContext = this } },
                { 1, new Controls.SettingsSections.AppearanceGeneral() { DataContext = this } },
                { 2, new Controls.SettingsSections.AppearanceAdvanced() { DataContext = this } },
                { 3, new Controls.SettingsSections.AppearanceDetailsView() { DataContext = this } },
                { 4, new Controls.SettingsSections.AppearanceGridView() { DataContext = this } },
                { 5, new Controls.SettingsSections.AppearanceLayout() { DataContext = this } },
                { 6, new Controls.SettingsSections.GeneralAdvanced() { DataContext = this } },
                { 7, new Controls.SettingsSections.Input() { DataContext = this } },
                { 8, new Controls.SettingsSections.AddonsMoveInfo() { DataContext = this } },
                { 9, new Controls.SettingsSections.Metadata() { DataContext = this } },
                { 11, new Controls.SettingsSections.Scripting() { DataContext = this } },
                { 12, new Controls.SettingsSections.ClientShutdown() { DataContext = this } },
                { 13, new Controls.SettingsSections.Performance() { DataContext = this } },
                { 14, new Controls.SettingsSections.ImportExlusionList() { DataContext = this } },
                { 19, new Controls.SettingsSections.Development() { DataContext = this } },
                { 20, new Controls.SettingsSections.AppearanceTopPanel() { DataContext = this } }
            };

            SelectedSectionView = sectionViews[0];
            foreach (var plugin in extensions.LibraryPlugins.Where(a => a.Properties?.CanShutdownClient == true))
            {
                AutoCloseClientsList.Add(new SelectableItem<LibraryPlugin>(plugin)
                {
                    Selected = settings.ClientAutoShutdown.ShutdownPlugins.Contains(plugin.Id)
                });
            }

            ImportExclusionList = new ObservableCollection<ImportExclusionItem>(database.ImportExclusions.OrderBy(a => a.Name));
        }

        private void SettingsTreeSelectedItemChanged(RoutedPropertyChangedEventArgs<object> selectedItem)
        {
            if (selectedItem.NewValue is TreeViewItem treeItem)
            {
                if (treeItem.Tag != null)
                {
                    var viewIndex = int.Parse(treeItem.Tag.ToString());
                    SelectedSectionView = sectionViews[viewIndex];
                }
                else
                {
                    SelectedSectionView = null;
                }
            }
            else
            {
                SelectedSectionView = null;
            }
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        public void CloseView()
        {
            closingHanled = true;
            window.Close(false);
        }

        public void ConfirmDialog()
        {
            if (editedFields.Contains(nameof(Settings.StartOnBoot)))
            {
                try
                {
                    SystemIntegration.SetBootupStateRegistration(Settings.StartOnBoot);
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, "Failed to register Playnite to start on boot.");
                    dialogs.ShowErrorMessage(resources.GetString("LOCSettingsStartOnBootRegistrationError")
                        + Environment.NewLine + e.Message, "");
                }
            }

            var shutdownPlugins = AutoCloseClientsList.Where(a => a.Selected == true).Select(a => a.Item.Id).ToList();
            Settings.ClientAutoShutdown.ShutdownPlugins = shutdownPlugins;
            var develExtListUpdated = !Settings.DevelExtenions.IsEqualJson(originalSettings.DevelExtenions);

            EndEdit();
            originalSettings.SaveSettings();
            if (editedFields?.Any(a => typeof(PlayniteSettings).HasPropertyAttribute<RequiresRestartAttribute>(a)) == true ||
                develExtListUpdated)
            {
                if (dialogs.ShowMessage(
                    resources.GetString("LOCSettingsRestartAskMessage"),
                    resources.GetString("LOCSettingsRestartTitle"),
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    application.Restart(new CmdLineOptions() { SkipLibUpdate = true });
                }
            }

            if (editedFields.Contains(nameof(Settings.DiscordPresenceEnabled)) && application.Discord != null)
            {
                application.Discord.IsPresenceEnabled = Settings.DiscordPresenceEnabled;
            }

            if (editedFields.Contains(nameof(Settings.TraceLogEnabled)))
            {
                NLogLogger.IsTraceEnabled = Settings.TraceLogEnabled;
            }

            closingHanled = true;
            window.Close(true);
        }

        public void SelectDbFile()
        {
            dialogs.ShowMessage(resources.GetString("LOCSettingsDBPathNotification"), "", MessageBoxButton.OK, MessageBoxImage.Warning);
            var path = dialogs.SelectFolder();
            if (!string.IsNullOrEmpty(path))
            {
                Settings.DatabasePath = path;
            }
        }

        public void ClearWebcache()
        {
            if (dialogs.ShowMessage(
                    resources.GetString("LOCSettingsClearCacheWarn"),
                    resources.GetString("LOCSettingsClearCacheTitle"),
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                application.Restart(new CmdLineOptions
                {
                    SkipLibUpdate = true,
                    ClearWebCache = true
                });
            }
        }

        public void SetCoverArtAspectRatio(string ratio)
        {
            var regex = Regex.Match(ratio, @"(\d+):(\d+)");
            if (regex.Success)
            {
                Settings.GridItemWidthRatio = Convert.ToInt32(regex.Groups[1].Value);
                Settings.GridItemHeightRatio = Convert.ToInt32(regex.Groups[2].Value);
            }
        }

        private void SetDefaults()
        {
            if (dialogs.ShowMessage(
                LOC.SettingsDefaultResetDesc,
                string.Empty,
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                application.Restart(new CmdLineOptions
                {
                    SkipLibUpdate = true,
                    ResetSettings = true
                });
            }
        }

        public void EndEdit()
        {
            Settings.CopyProperties(originalSettings, true, new List<string>()
            {
                nameof(PlayniteSettings.FilterSettings),
                nameof(PlayniteSettings.ViewSettings),
                nameof(PlayniteSettings.InstallInstanceId),
                nameof(PlayniteSettings.GridItemHeight),
                nameof(PlayniteSettings.WindowPositions),
                nameof(PlayniteSettings.Fullscreen)
            }, true);

            database.ImportExclusions.Remove(removedExclusionItems);
        }

        public void TestScript(string script)
        {
            try
            {
                var game = application.Api.MainView.SelectedGames.DefaultIfEmpty(new SDK.Models.Game("Test game")).FirstOrDefault();
                var expanded = game.ExpandVariables(script);
                using (var runtime = new PowerShellRuntime($"test script runtime"))
                {
                    application.GamesEditor.ExecuteScriptAction(runtime, expanded, game, true, true, GameScriptType.None);
                }
            }
            catch (Exception exc)
            {
                var message = exc.Message;
                if (exc is ScriptRuntimeException err)
                {
                    message = err.Message + "\n\n" + err.ScriptStackTrace;
                }

                Dialogs.ShowMessage(
                    message,
                    resources.GetString("LOCScriptError"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
