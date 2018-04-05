using NLog;
using Playnite;
using Playnite.Providers.Steam;
using Playnite.SDK;
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

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private IWindowFactory window;
        private IDialogsFactory dialogs;
        private IResourceProvider resources;

        public List<LocalSteamUser> SteamUsers
        {
            get
            {
                return new SteamLibrary().GetSteamUsers();
            }
        }
        
        public ulong SteamIdCategoryImport
        {
            get; set;
        }

        private Playnite.Providers.GOG.WebApiClient gogApiClient = new Playnite.Providers.GOG.WebApiClient();

        public string GogLoginStatus
        {
            get
            {
                try
                {
                    if (gogApiClient.GetLoginRequired())
                    {
                        return resources.FindString("LoginRequired");
                    }
                    else
                    {
                        return resources.FindString("OKLabel");
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, "Failed to test GOG login status.");
                    return resources.FindString("LoginFailed");
                }
            }
        }

        private Playnite.Providers.Origin.WebApiClient originApiClient = new Playnite.Providers.Origin.WebApiClient();

        public string OriginLoginStatus
        {
            get
            {
                try
                {
                    if (originApiClient.GetLoginRequired())
                    {
                        return resources.FindString("LoginRequired");
                    }
                    else
                    {
                        return resources.FindString("OKLabel");
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, "Failed to test Origin login status.");
                    return resources.FindString("LoginFailed");
                }
            }
        }

        private Playnite.Providers.BattleNet.WebApiClient battleNetApiClient = new Playnite.Providers.BattleNet.WebApiClient();

        public string BattleNetLoginStatus
        {
            get
            {
                try
                {
                    if (battleNetApiClient.GetLoginRequired())
                    {
                        return resources.FindString("LoginRequired");
                    }
                    else
                    {
                        return resources.FindString("OKLabel");
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, "Failed to test BattleNet login status.");
                    return resources.FindString("LoginFailed");
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
            SteamIdCategoryImport = SteamUsers?.FirstOrDefault()?.Id ?? 0;
            Settings.SteamSettings.IntegrationEnabled = true;
            Settings.SteamSettings.AccountId = SteamIdCategoryImport;
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
            battleNetApiClient?.Dispose();
            originApiClient?.Dispose();
            gogApiClient?.Dispose();
            battleNetApiClient = null;
            originApiClient = null;
            gogApiClient = null;
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
            try
            {
                gogApiClient.Login();
                OnPropertyChanged("GogLoginStatus");
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "GOG auth failed.");
                dialogs.ShowMessage(resources.FindString("LoginFailed"), "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void AuthenticateOrigin()
        {
            try
            {
                originApiClient.Login();
                OnPropertyChanged("OriginLoginStatus");
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Origin auth failed.");
                dialogs.ShowMessage(resources.FindString("LoginFailed"), "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void AuthenticateBattleNet()
        {
            try
            {
                battleNetApiClient.Login();
                OnPropertyChanged("BattleNetLoginStatus");
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "BattleNet auth failed.");
                dialogs.ShowMessage(resources.FindString("LoginFailed"), "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ImportGames(InstalledGamesViewModel model)
        {
            if (model.OpenView() == true)
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
