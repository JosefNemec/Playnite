using AmazonGamesLibrary.Services;
using Newtonsoft.Json;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazonGamesLibrary
{
    public class AmazonGamesLibrarySettings : ObservableObject, ISettings
    {
        private static ILogger logger = LogManager.GetLogger();
        private AmazonGamesLibrarySettings editingClone;
        private AmazonGamesLibrary library;
        private IPlayniteAPI api;

        #region Settings

        public int Version { get; set; }

        public bool ImportInstalledGames { get; set; } = true;

        public bool ConnectAccount { get; set; } = false;

        public bool ImportUninstalledGames { get; set; } = false;

        public bool StartGamesWithoutLauncher { get; set; } = false;

        #endregion Settings

        [JsonIgnore]
        public bool IsUserLoggedIn
        {
            get
            {
                try
                {
                    var client = new AmazonAccountClient(library);
                    return client.GetIsUserLoggedIn().GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    logger.Error(e, "");
                    return false;
                }
            }
        }

        [JsonIgnore]
        public RelayCommand<object> LoginCommand
        {
            get => new RelayCommand<object>(async (a) =>
            {
                await Login();
            });
        }

        public AmazonGamesLibrarySettings()
        {
        }

        public AmazonGamesLibrarySettings(AmazonGamesLibrary library, IPlayniteAPI api)
        {
            this.library = library;
            this.api = api;

            var settings = library.LoadPluginSettings<AmazonGamesLibrarySettings>();
            if (settings != null)
            {
                if (settings.Version == 0)
                {
                    logger.Debug("Updating Twitch settings from version 0.");
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

        private void LoadValues(AmazonGamesLibrarySettings source)
        {
            source.CopyProperties(this, false, null, true);
        }

        public class Cookie
        {
            public string value { get; set; }
        }

        private async Task Login()
        {
            try
            {
                var client = new AmazonAccountClient(library);
                await client.Login();
                OnPropertyChanged(nameof(IsUserLoggedIn));
            }
            catch (Exception e) when (!Debugger.IsAttached)
            {
                logger.Error(e, "Failed to authenticate Amazon user.");
            }
        }
    }
}