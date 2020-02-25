using EpicLibrary.Services;
using Newtonsoft.Json;
using Playnite;
using Playnite.SDK;
using Playnite.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;

namespace EpicLibrary
{
    public class EpicLibrarySettings : ObservableObject, ISettings
    {
        private static ILogger logger = LogManager.GetLogger();
        private EpicLibrarySettings editingClone;
        private EpicLibrary library;
        private IPlayniteAPI api;

        #region Settings

        public int Version { get; set; }

        public bool ImportInstalledGames { get; set; } = EpicLauncher.IsInstalled;

        public bool ConnectAccount { get; set; } = false;

        public bool ImportUninstalledGames { get; set; } = false;

        public bool StartGamesWithoutLauncher { get; set; } = false;

        #endregion Settings

        [JsonIgnore]
        public bool IsFirstRunUse { get; set; }

        [JsonIgnore]
        public bool IsUserLoggedIn
        {
            get
            {
                return new EpicAccountClient(api, library.TokensPath).GetIsUserLoggedIn();
            }
        }

        [JsonIgnore]
        public RelayCommand<object> LoginCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                Login();
            });
        }

        public EpicLibrarySettings()
        {
        }

        public EpicLibrarySettings(EpicLibrary library, IPlayniteAPI api)
        {
            this.library = library;
            this.api = api;

            var settings = library.LoadPluginSettings<EpicLibrarySettings>();
            if (settings != null)
            {
                if (settings.Version == 0)
                {
                    logger.Debug("Updating Epic settings from version 0.");
                    if (settings.ImportUninstalledGames)
                    {
                        settings.ConnectAccount = true;
                    }
                }

                settings.Version = 1;
                LoadValues(settings);
            }
        }

        public void BeginEdit()
        {
            editingClone = this.GetClone();
        }

        public void CancelEdit()
        {
            LoadValues(editingClone);
        }

        public void EndEdit()
        {
            library.SavePluginSettings(this);
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = null;
            return true;
        }

        private void LoadValues(EpicLibrarySettings source)
        {
            source.CopyProperties(this, false, null, true);
        }

        private void Login()
        {
            try
            {
                var clientApi = new EpicAccountClient(api, library.TokensPath);
                clientApi.Login();
                OnPropertyChanged(nameof(IsUserLoggedIn));
            }
            catch (Exception e) when (!Debugger.IsAttached)
            {
                api.Dialogs.ShowErrorMessage(api.Resources.GetString("LOCNotLoggedInError"), "");
                logger.Error(e, "Failed to authenticate user.");
            }
        }
    }
}
