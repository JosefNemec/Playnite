using BattleNetLibrary.Services;
using Newtonsoft.Json;
using Playnite.SDK;
using System;
using System.Collections.Generic;

namespace BattleNetLibrary
{
    public class BattleNetLibrarySettings : ObservableObject, ISettings
    {
        private readonly ILogger logger = LogManager.GetLogger();
        private BattleNetLibrarySettings editingClone;
        private readonly BattleNetLibrary library;
        private readonly IPlayniteAPI playniteApi;

        #region Settings      

        public bool ImportInstalledGames { get; set; } = true;

        public bool ConnectAccount { get; set; } = false;

        public bool ImportUninstalledGames { get; set; } = false;

        #endregion Settings

        [JsonIgnore]
        public bool IsUserLoggedIn
        {
            get
            {
                using (var view = playniteApi.WebViews.CreateOffscreenView())
                {
                    var api = new BattleNetAccountClient(view);
                    return api.GetIsUserLoggedIn();
                }
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

        public BattleNetLibrarySettings()
        {
        }

        public BattleNetLibrarySettings(BattleNetLibrary library, IPlayniteAPI playniteApi)
        {
            this.library = library;
            this.playniteApi = playniteApi;

            var settings = library.LoadPluginSettings<BattleNetLibrarySettings>();
            if (settings != null)
            {
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

        private void LoadValues(BattleNetLibrarySettings source)
        {
            source.CopyProperties(this, false, null, true);
        }

        private void Login()
        {
            try
            {
                using (var view = playniteApi.WebViews.CreateView(400, 500))
                {
                    var api = new BattleNetAccountClient(view);
                    api.Login();
                }

                OnPropertyChanged(nameof(IsUserLoggedIn));
            }
            catch (Exception e) when (!Environment.IsDebugBuild)
            {
                logger.Error(e, "Failed to authenticate user.");
            }
        }
    }
}