using NLog;
using Playnite;
using Playnite.Providers.Steam;
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
    public class FirstTimeStartupViewModel : ObservableObject
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

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private IWindowFactory window;
        private IDialogsFactory dialogs;
        private IResourceProvider resources;

        private string loginReuiredMessage = "Login Required";
        private string loginOKMessage = "OK";

        public List<LocalSteamUser> SteamUsers
        {
            get
            {
                return new SteamLibrary().GetSteamUsers();
            }
        }

        private Playnite.Providers.GOG.WebApiClient gogApiClient = new Playnite.Providers.GOG.WebApiClient();

        public string GogLoginStatus
        {
            get
            {
                if (gogApiClient.GetLoginRequired())
                {
                    return loginReuiredMessage;
                }
                else
                {
                    return loginOKMessage;
                }
            }
        }

        private Playnite.Providers.Origin.WebApiClient originApiClient = new Playnite.Providers.Origin.WebApiClient();

        public string OriginLoginStatus
        {
            get
            {
                if (originApiClient.GetLoginRequired())
                {
                    return loginReuiredMessage;
                }
                else
                {
                    return loginOKMessage;
                }
            }
        }

        private Playnite.Providers.BattleNet.WebApiClient battleNetApiClient = new Playnite.Providers.BattleNet.WebApiClient();

        public string BattleNetLoginStatus
        {
            get
            {
                if (battleNetApiClient.GetLoginRequired())
                {
                    return loginReuiredMessage;
                }
                else
                {
                    return loginOKMessage;
                }
            }
        }

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

        public bool SteamImportCategories
        {
            get; set;
        } = true;

        public ulong SteamIdCategoryImport
        {
            get; set;
        } = 0;
        
        public List<InstalledGameMetadata> ImportedGames
        {
            get;
            private set;
        } = new List<InstalledGameMetadata>();

        private Settings settings = new Settings();
        public Settings Settings
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
                CloseDialog(false);
            });
        }

        public RelayCommand<object> FinishCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CloseDialog(true);
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

        public RelayCommand<object> AuthGOGCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AuthenticateGOG();
            }, (a) => Settings.GOGSettings.LibraryDownloadEnabled);
        }

        public RelayCommand<object> AuthOriginCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AuthenticateOrigin();
            }, (a) => Settings.OriginSettings.LibraryDownloadEnabled);
        }

        public RelayCommand<object> AuthBattleNetCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AuthenticateBattleNet();
            }, (a) => Settings.BattleNetSettings.LibraryDownloadEnabled);
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
            Settings.SteamSettings.IntegrationEnabled = true;
            Settings.GOGSettings.IntegrationEnabled = true;
            Settings.GOGSettings.RunViaGalaxy = Playnite.Providers.GOG.GogSettings.IsInstalled;
            Settings.OriginSettings.IntegrationEnabled = true;
            Settings.UplaySettings.IntegrationEnabled = true;
            Settings.BattleNetSettings.IntegrationEnabled = true;

            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;

            pageValidators.Add(Page.Steam, (model) =>
            {
                if (model.Settings.SteamSettings.IntegrationEnabled && model.Settings.SteamSettings.LibraryDownloadEnabled)
                {
                    if (model.Settings.SteamSettings.IdSource == SteamIdSource.Name && string.IsNullOrEmpty(model.Settings.SteamSettings.AccountName))
                    {
                        dialogs.ShowMessage(resources.FindString("SettingsInvalidSteamAccountName"),
                            resources.FindString("InvalidDataTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }

                    if (model.Settings.SteamSettings.IdSource == SteamIdSource.LocalUser && model.Settings.SteamSettings.AccountId == 0)
                    {
                        dialogs.ShowMessage(resources.FindString("SettingsInvalidSteamAccountLibImport"),
                            resources.FindString("InvalidDataTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }

                if (model.SteamImportCategories && model.SteamIdCategoryImport == 0)
                {
                    dialogs.ShowMessage(resources.FindString("SettingsInvalidSteamAccountCatImport"),
                        resources.FindString("InvalidDataTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                return true;
            });

            pageValidators.Add(Page.Database, (model) =>
            {
                if (model.DatabaseLocation == DbLocation.Custom)
                {
                    if (!Paths.GetValidFilePath(model.Settings.DatabasePath))
                    {
                        dialogs.ShowMessage(resources.FindString("SettingsInvalidDBLocation"),
                            resources.FindString("InvalidDataTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }

                return true;
            });
        }

        public bool? ShowDialog()
        {
            return window.CreateAndOpenDialog(this);
        }

        public void CloseDialog(bool? result)
        {
            window.Close(result);
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

            if (SelectedIndex == (int)Page.Steam && !Settings.SteamSettings.IntegrationEnabled)
            {
                SelectedIndex++;
            }

            if (SelectedIndex == (int)Page.GOG && !Settings.GOGSettings.IntegrationEnabled)
            {
                SelectedIndex++;
            }

            if (SelectedIndex == (int)Page.Origin && !Settings.OriginSettings.IntegrationEnabled)
            {
                SelectedIndex++;
            }

            if (SelectedIndex == (int)Page.BattleNet && !Settings.BattleNetSettings.IntegrationEnabled)
            {
                SelectedIndex++;
            }

            if (SelectedIndex == (int)Page.Uplay && !Settings.UplaySettings.IntegrationEnabled)
            {
                SelectedIndex++;
            }
        }

        public void NavigateBack()
        {
            SelectedIndex--;

            if (SelectedIndex == (int)Page.Uplay && !Settings.UplaySettings.IntegrationEnabled)
            {
                SelectedIndex--;
            }

            if (SelectedIndex == (int)Page.BattleNet && !Settings.BattleNetSettings.IntegrationEnabled)
            {
                SelectedIndex--;
            }

            if (SelectedIndex == (int)Page.Origin && !Settings.OriginSettings.IntegrationEnabled)
            {
                SelectedIndex--;
            }

            if (SelectedIndex == (int)Page.GOG && !Settings.GOGSettings.IntegrationEnabled)
            {
                SelectedIndex--;
            }

            if (SelectedIndex == (int)Page.Steam && !Settings.SteamSettings.IntegrationEnabled)
            {
                SelectedIndex--;
            }
        }

        public void SelectDbFile()
        {
            var path = dialogs.SaveFile("Database file (*.db)|*.db", false);
            if (!string.IsNullOrEmpty(path))
            {
                Settings.DatabasePath = path;
            }
        }

        public void AuthenticateGOG()
        {
            gogApiClient.Login();
            OnPropertyChanged("GogLoginStatus");
        }

        public void AuthenticateOrigin()
        {
            originApiClient.Login();
            OnPropertyChanged("OriginLoginStatus");
        }

        public void AuthenticateBattleNet()
        {
            battleNetApiClient.Login();
            OnPropertyChanged("BattleNetLoginStatus");
        }

        public void ImportGames(InstalledGamesViewModel model)
        {
            if (model.ShowDialog() == true)
            {
                ImportedGames = model.Games;
            }
        }

        public void NavigateUrl(string url)
        {
            System.Diagnostics.Process.Start(url);
        }
    }
}
