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
using Playnite.Converters;
using System.Globalization;
using Playnite.SDK.Models;

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

    public class SearchSupportSettings : SearchSupport
    {
        public string SearchId { get; set; }
        public new string Keyword { get; set; }

        public SearchSupportSettings(LoadedPlugin plugin, SearchSupport support, Dictionary<string, string> customMap) : base(support.DefaultKeyword, support.Name, support.Context)
        {
            SearchId = plugin.Description.Id + support.DefaultKeyword;
            if (customMap?.ContainsKey(SearchId) == true)
            {
                Keyword = customMap[SearchId];
            }
        }
    }

    public class SettingsViewModel : ObservableObject
    {
        internal static ILogger logger = LogManager.GetLogger();
        internal IWindowFactory window;
        internal IResourceProvider resources;
        internal IDialogsFactory dialogs;
        internal IGameDatabaseMain database;
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
        public ObservableCollection<string> SortingNameRemovedArticles { get; }

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

        public List<SearchSupportSettings> Searches { get; } = new List<SearchSupportSettings>();

        private readonly Dictionary<DesktopSettingsPage, UserControl> sectionViews;

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

        public RelayCommand AddDevelExtensionCommand
        {
            get => new RelayCommand(() =>
            {
                Settings.DevelExtenions.Add(new SelectableItem<string>("<change me>") { Selected = true });
                Settings.DevelExtenions = Settings.DevelExtenions.GetClone();
            });
        }

        public RelayCommand<SelectableItem<string>> RemoveDevelExtensionCommand
        {
            get => new RelayCommand<SelectableItem<string>>((item) =>
            {
                Settings.DevelExtenions.Remove(item);
                Settings.DevelExtenions = Settings.DevelExtenions.GetClone();
            });
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

        public RelayCommand AddSortingNameRemovedArticle
        {
            get => new RelayCommand(() =>
            {
                var res = dialogs.SelectString(
                    resources.GetString(LOC.EnterName),
                    resources.GetString(LOC.AddNewItem),
                    string.Empty);
                if (res.Result && !res.SelectedString.IsNullOrEmpty())
                {
                    if (SortingNameRemovedArticles.Any(a => a.Equals(res.SelectedString, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        dialogs.ShowErrorMessage(resources.GetString(LOC.ItemAlreadyExists), string.Empty);
                    }
                    else
                    {
                        SortingNameRemovedArticles.Add(res.SelectedString);
                    }
                }
            });
        }

        public RelayCommand<IList<object>> RemoveSortingNameRemovedArticle
        {
            get => new RelayCommand<IList<object>>((selectedItems) =>
            {
                var selectedStrings = selectedItems.Cast<string>().ToList();
                foreach (string selectedItem in selectedStrings)
                {
                    SortingNameRemovedArticles.Remove(selectedItem);
                }
            }, (a) => a?.Count > 0);
        }

        public RelayCommand FillSortingNameForAllGames
        {
            get => new RelayCommand(() =>
            {
                dialogs.ActivateGlobalProgress(args =>
                {
                    args.ProgressMaxValue = database.Games.Count;
                    var c = new SortableNameConverter(SortingNameRemovedArticles, true);
                    using (database.BufferedUpdate())
                    {
                        foreach (var game in database.Games)
                        {
                            if (args.CancelToken.IsCancellationRequested)
                            {
                                return;
                            }
                            if (game.SortingName.IsNullOrEmpty())
                            {
                                string sortingName = c.Convert(game.Name);
                                if (game.Name != sortingName)
                                {
                                    game.SortingName = sortingName;
                                    database.Games.Update(game);
                                }
                            }
                            args.CurrentProgressValue++;
                        }
                    }
                }, new GlobalProgressOptions(resources.GetString(LOC.SortingNameAutofillProgress), true));
            });
        }

        public RelayCommand ResetDateTimeFormatAddedCommand
        {
            get => new RelayCommand(() =>
            {
                settings.DateTimeFormatAdded.Format = Constants.DefaultDateTimeFormat;
            });
        }

        public RelayCommand ResetDateTimeFormatModifiedCommand
        {
            get => new RelayCommand(() =>
            {
                settings.DateTimeFormatModified.Format = Constants.DefaultDateTimeFormat;
            });
        }

        public RelayCommand ResetDateTimeFormatRecentActivityCommand
        {
            get => new RelayCommand(() =>
            {
                settings.DateTimeFormatRecentActivity.Format = Constants.DefaultDateTimeFormat;
            });
        }

        public RelayCommand ResetDateTimeFormatReleaseDateCommand
        {
            get => new RelayCommand(() =>
            {
                settings.DateTimeFormatReleaseDate.Format = Constants.DefaultDateTimeFormat;
                settings.DateTimeFormatReleaseDate.PartialFormat = Constants.DefaultPartialReleaseDateTimeFormat;
            });
        }

        public RelayCommand ResetDateTimeFormatLastPlayedCommand
        {
            get => new RelayCommand(() =>
            {
                settings.DateTimeFormatLastPlayed.Format = Constants.DefaultDateTimeFormat;
            });
        }

        public RelayCommand UploadFullscreenIntroVideoCommand
        {
            get => new RelayCommand(() =>
            {
                string filePath = Dialogs.SelectFile("MP4 files (*.mp4)|*.mp4|All files (*.*)|*.*");
                if (!filePath.IsNullOrEmpty())
                {
                    Settings.TemporaryFullscreenIntroUri = new Uri(filePath);
                }
            });
        }

        public RelayCommand ClearIntroVideoCommand
        {
            get => new RelayCommand(() => Settings.TemporaryFullscreenIntroUri = null);
        }

        #endregion Commands

        public object DateTimeFormatAddedExample =>
            NullableDateToStringConverter.Instance.Convert(DateTime.Now, typeof(string), Settings.DateTimeFormatAdded, CultureInfo.CurrentCulture);

        public object DateTimeFormatLastPlayedExample =>
            DateTimeToLastPlayedConverter.Instance.Convert(DateTime.Now, typeof(string), Settings.DateTimeFormatLastPlayed, CultureInfo.CurrentCulture);

        public object DateTimeFormatModifiedExample =>
            NullableDateToStringConverter.Instance.Convert(DateTime.Now, typeof(string), Settings.DateTimeFormatModified, CultureInfo.CurrentCulture);

        public object DateTimeFormatRecentActivityExample =>
            NullableDateToStringConverter.Instance.Convert(DateTime.Now, typeof(string), Settings.DateTimeFormatRecentActivity, CultureInfo.CurrentCulture);

        public object DateTimeFormatReleaseDateExample =>
            ReleaseDateToStringConverter.Instance.Convert(new ReleaseDate(DateTime.Now), typeof(string), Settings.DateTimeFormatReleaseDate, CultureInfo.CurrentCulture);

        public object DateTimeFormatPartialReleaseDateExample =>
            ReleaseDateToStringConverter.Instance.Convert(new ReleaseDate(1999, 6), typeof(string), Settings.DateTimeFormatReleaseDate, CultureInfo.CurrentCulture);

        public SettingsViewModel(
            IGameDatabaseMain database,
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

            Settings.DateTimeFormatAdded.PropertyChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(DateTimeFormatAddedExample));
                editedFields.AddMissing(nameof(Settings.DateTimeFormatAdded));
            };

            Settings.DateTimeFormatLastPlayed.PropertyChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(DateTimeFormatLastPlayedExample));
                editedFields.AddMissing(nameof(Settings.DateTimeFormatLastPlayed));
            };

            Settings.DateTimeFormatModified.PropertyChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(DateTimeFormatModifiedExample));
                editedFields.AddMissing(nameof(Settings.DateTimeFormatModified));
            };

            Settings.DateTimeFormatRecentActivity.PropertyChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(DateTimeFormatRecentActivityExample));
                editedFields.AddMissing(nameof(Settings.DateTimeFormatRecentActivity));
            };

            Settings.DateTimeFormatReleaseDate.PropertyChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(DateTimeFormatReleaseDateExample));
                OnPropertyChanged(nameof(DateTimeFormatPartialReleaseDateExample));
                editedFields.AddMissing(nameof(Settings.DateTimeFormatReleaseDate));
            };

            AvailableTrayIcons = new List<SelectableTrayIcon>
            {
                new SelectableTrayIcon(TrayIconType.Default),
                new SelectableTrayIcon(TrayIconType.Bright),
                new SelectableTrayIcon(TrayIconType.Dark)
            };

            sectionViews = new Dictionary<DesktopSettingsPage, UserControl>()
            {
                { DesktopSettingsPage.General, new Controls.SettingsSections.General() { DataContext = this } },
                { DesktopSettingsPage.AppearanceGeneral, new Controls.SettingsSections.AppearanceGeneral() { DataContext = this } },
                { DesktopSettingsPage.AppearanceAdvanced, new Controls.SettingsSections.AppearanceAdvanced() { DataContext = this } },
                { DesktopSettingsPage.AppearanceDetailsView, new Controls.SettingsSections.AppearanceDetailsView() { DataContext = this } },
                { DesktopSettingsPage.AppearanceGridView, new Controls.SettingsSections.AppearanceGridView() { DataContext = this } },
                { DesktopSettingsPage.AppearanceLayout, new Controls.SettingsSections.AppearanceLayout() { DataContext = this } },
                { DesktopSettingsPage.GeneralAdvanced, new Controls.SettingsSections.GeneralAdvanced() { DataContext = this } },
                { DesktopSettingsPage.Input, new Controls.SettingsSections.Input() { DataContext = this } },
                { DesktopSettingsPage.Metadata, new Controls.SettingsSections.Metadata() { DataContext = this } },
                { DesktopSettingsPage.Scripting, new Controls.SettingsSections.Scripting() { DataContext = this } },
                { DesktopSettingsPage.ClientShutdown, new Controls.SettingsSections.ClientShutdown() { DataContext = this } },
                { DesktopSettingsPage.Performance, new Controls.SettingsSections.Performance() { DataContext = this } },
                { DesktopSettingsPage.ImportExlusionList, new Controls.SettingsSections.ImportExlusionList() { DataContext = this } },
                { DesktopSettingsPage.Development, new Controls.SettingsSections.Development() { DataContext = this } },
                { DesktopSettingsPage.AppearanceTopPanel, new Controls.SettingsSections.AppearanceTopPanel() { DataContext = this } },
                { DesktopSettingsPage.Sorting, new Controls.SettingsSections.Sorting() { DataContext = this } },
                { DesktopSettingsPage.Updates, new Controls.SettingsSections.Updates() { DataContext = this } },
                { DesktopSettingsPage.AppearanceListView, new Controls.SettingsSections.AppearanceListView() { DataContext = this } },
                { DesktopSettingsPage.Search, new Controls.SettingsSections.Search() { DataContext = this } },
                { DesktopSettingsPage.Backup, new Controls.SettingsSections.Backup() { DataContext = this } },
                { DesktopSettingsPage.AppearanceIntros, new Controls.SettingsSections.AppearanceIntros() { DataContext = this } }
            };

            SelectedSectionView = sectionViews[0];
            foreach (var plugin in extensions.LibraryPlugins.Where(a => a.Properties?.CanShutdownClient == true))
            {
                AutoCloseClientsList.Add(new SelectableItem<LibraryPlugin>(plugin)
                {
                    Selected = settings.ClientAutoShutdown.ShutdownPlugins.Contains(plugin.Id)
                });
            }

            foreach (var plugin in extensions.Plugins)
            {
                foreach (var search in plugin.Value.Plugin.Searches ?? new List<SearchSupport>())
                {
                    Searches.Add(new SearchSupportSettings(plugin.Value, search, Settings.CustomSearchKeywrods));
                }
            }

            ImportExclusionList = new ObservableCollection<ImportExclusionItem>(database.ImportExclusions.OrderBy(a => a.Name));
            SortingNameRemovedArticles = new ObservableCollection<string>(settings.GameSortingNameRemovedArticles.OrderBy(a => a));
        }

        private void SettingsTreeSelectedItemChanged(RoutedPropertyChangedEventArgs<object> selectedItem)
        {
            if (selectedItem.NewValue is TreeViewItem treeItem)
            {
                if (treeItem.Tag is DesktopSettingsPage viewIndex && sectionViews.ContainsKey(viewIndex))
                {
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

        public bool? OpenView(DesktopSettingsPage viewIndex)
        {
            if (sectionViews.ContainsKey(viewIndex))
            {
                SelectedSectionView = sectionViews[viewIndex];
            }

            return window.CreateAndOpenDialog(this);
        }

        public void CloseView()
        {
            closingHanled = true;
            window.Close(false);
        }

        public void ConfirmDialog()
        {
            if (Settings.AutoBackupEnabled && Settings.AutoBackupDir.IsNullOrWhiteSpace())
            {
                dialogs.ShowErrorMessage(LOC.SettingsNoBackupDirSpecifiedError);
                return;
            }

            if (editedFields.Contains(nameof(Settings.TemporaryFullscreenIntroUri)))
            {
                try
                {
                    if (Settings.TemporaryFullscreenIntroUri == null)
                    {
                        if (File.Exists(PlaynitePaths.FullscreenIntroFilePath))
                        {
                            File.Delete(PlaynitePaths.FullscreenIntroFilePath);
                        }
                    }
                    else
                    {
                        Directory.CreateDirectory(PlaynitePaths.IntrosPath);
                        File.Copy(Settings.TemporaryFullscreenIntroUri.AbsolutePath, PlaynitePaths.FullscreenIntroFilePath, true);
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, "Failed to set fullscreen intro video");
                    dialogs.ShowErrorMessage(resources.GetString("LOCSettingsFullscreenIntroVideoError")
                        + Environment.NewLine + e.Message, "");
                }
            }

            if (editedFields.Contains(nameof(Settings.StartOnBoot)) ||
                editedFields.Contains(nameof(Settings.StartOnBootClosedToTray)))
            {
                try
                {
                    SystemIntegration.SetBootupStateRegistration(Settings.StartOnBoot, Settings.StartOnBootClosedToTray);
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
            Settings.GameSortingNameRemovedArticles = SortingNameRemovedArticles.ToList();
            var develExtListUpdated = !Settings.DevelExtenions.IsEqualJson(originalSettings.DevelExtenions);

            Settings.CustomSearchKeywrods = new Dictionary<string, string>();
            foreach (var search in Searches)
            {
                if (!search.Keyword.IsNullOrWhiteSpace())
                {
                    Settings.CustomSearchKeywrods.Add(search.SearchId, search.Keyword);
                }
            }

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
                var game = application.PlayniteApiGlobal.MainView.SelectedGames.FirstOrDefault() ?? new SDK.Models.Game("Test game");
                var expandedScript = game.ExpandVariables(script);
                var startingArgs = new SDK.Events.OnGameStartingEventArgs
                {
                    Game = game,
                    SelectedRomFile = game.Roms?.FirstOrDefault()?.Path,
                    SourceAction = game.GameActions?.FirstOrDefault()
                };

                using (var runtime = new PowerShellRuntime($"test script runtime"))
                {
                    application.GamesEditor.ExecuteScriptAction(runtime, expandedScript, game, true, true, GameScriptType.None,
                        new Dictionary<string, object>
                        {
                            {  "StartingArgs", startingArgs },
                            {  "SourceAction", startingArgs.SourceAction },
                            {  "SelectedRomFile", startingArgs.SelectedRomFile }
                        });
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
