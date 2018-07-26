using Playnite;
using Playnite.Common.System;
using Playnite.SDK;
using Playnite.Settings;
using PlayniteUI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static PlayniteUI.ViewModels.InstalledGamesViewModel;

namespace PlayniteUI.ViewModels
{    
    public class FirstTimeStartupViewModel : ObservableObject, IDisposable
    {
        public enum DbLocation
        {
            ProgramData,
            Custom
        }

        public enum Page : int
        {
            Intro = 0,
            Database = 1,
            ProviderSelect = 2,
            Steam = 3,
            GOG = 4,
            Origin = 5,
            BattleNet = 6,
            Uplay = 7,
            Custom = 8,
            Finish = 9
        }

        private static ILogger logger = LogManager.GetLogger();
        private IWindowFactory window;
        private IDialogsFactory dialogs;
        private IResourceProvider resources;        

        private DbLocation databaseLocation = DbLocation.ProgramData;
        public DbLocation DatabaseLocation
        {
            get
            {
                return databaseLocation;
            }

            set
            {
                databaseLocation = value;
                OnPropertyChanged("DatabaseLocation");
            }
        }

        public bool ShowFinishButton
        {
            get => SelectedIndex == (int)Page.Finish;
        }
        
        public List<InstalledGameMetadata> ImportedGames
        {
            get;
            private set;
        } = new List<InstalledGameMetadata>();

        private PlayniteSettings settings = new PlayniteSettings();
        public PlayniteSettings Settings
        {
            get
            {
                return settings;
            }

            set
            {
                settings = value;
                OnPropertyChanged("Settings");
            }
        }

        private int selectedIndex = 0;
        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }

            set
            {
                selectedIndex = value;
                OnPropertyChanged("ShowFinishButton");
                OnPropertyChanged("SelectedIndex");
            }
        }

        private Dictionary<Page, Func<FirstTimeStartupViewModel, bool>> pageValidators =
            new Dictionary<Page, Func<FirstTimeStartupViewModel, bool>>();

        public RelayCommand<object> CloseCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CloseView(false);
            });
        }

        public RelayCommand<object> FinishCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CloseView(true);
            });
        }

        public RelayCommand<object> SelectDbFileCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectDbFile();
            });
        }

        public RelayCommand<object> NextCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                NavigateNext();
            }, (a) => SelectedIndex < (int)Page.Finish);
        }

        public RelayCommand<object> BackCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                NavigateBack();
            }, (a) => SelectedIndex > 0);
        }

        public RelayCommand<object> ImportGamesCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ImportGames(new InstalledGamesViewModel(
                    InstalledGamesWindowFactory.Instance, new DialogsFactory()));
            });
        }

        public RelayCommand<Uri> NavigateUrlCommand
        {
            get => new RelayCommand<Uri>((url) =>
            {
                NavigateUrl(url.AbsoluteUri);
            });
        }

        public FirstTimeStartupViewModel(IWindowFactory window, IDialogsFactory dialogs, IResourceProvider resources)
        {
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;

            pageValidators.Add(Page.Steam, (model) =>
            {
                //if (model.Settings.SteamSettings.IntegrationEnabled && model.Settings.SteamSettings.LibraryDownloadEnabled)
                //{
                //    if (model.Settings.SteamSettings.IdSource == SteamIdSource.Name && string.IsNullOrEmpty(model.Settings.SteamSettings.AccountName))
                //    {
                //        dialogs.ShowMessage(resources.FindString("LOCSettingsInvalidSteamAccountName"),
                //            resources.FindString("LOCInvalidDataTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                //        return false;
                //    }

                //    if (model.Settings.SteamSettings.IdSource == SteamIdSource.LocalUser && model.Settings.SteamSettings.AccountId == 0)
                //    {
                //        dialogs.ShowMessage(resources.FindString("LOCSettingsInvalidSteamAccountLibImport"),
                //            resources.FindString("LOCInvalidDataTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                //        return false;
                //    }
                //}

                //if (model.SteamImportCategories && model.SteamIdCategoryImport == 0)
                //{
                //    dialogs.ShowMessage(resources.FindString("LOCSettingsInvalidSteamAccountCatImport"),
                //        resources.FindString("LOCInvalidDataTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                //    return false;
                //}

                return true;
            });

            pageValidators.Add(Page.Database, (model) =>
            {
                if (model.DatabaseLocation == DbLocation.Custom)
                {
                    if (!Paths.GetValidFilePath(model.Settings.DatabasePath))
                    {
                        dialogs.ShowMessage(resources.FindString("LOCSettingsInvalidDBLocation"),
                            resources.FindString("LOCInvalidDataTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }

                return true;
            });
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        public void CloseView(bool? result)
        {
            window.Close(result);
            Dispose();
        }

        public void Dispose()
        {
        }

        public void NavigateNext()
        {
            if (pageValidators.ContainsKey((Page)SelectedIndex))
            {
                if (pageValidators[(Page)SelectedIndex](this) == false)
                {
                    return;
                }
            }

            SelectedIndex++;
        }

        public void NavigateBack()
        {
            SelectedIndex--;
        }

        public void SelectDbFile()
        {
            var path = dialogs.SaveFile("Database file (*.db)|*.db", false);
            if (!string.IsNullOrEmpty(path))
            {
                Settings.DatabasePath = path;
            }
        }        

        public void ImportGames(InstalledGamesViewModel model)
        {
            if (model.OpenView() == true)
            {
                logger.Debug($"Selected {model.Games} custom games from first time wizard.");
                ImportedGames = model.Games;
            }
        }

        public void NavigateUrl(string url)
        {
            System.Diagnostics.Process.Start(url);
        }
    }
}
