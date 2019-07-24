using Newtonsoft.Json;
using OriginLibrary.Services;
using Playnite.SDK;
using System;
using System.Collections.Generic;

namespace OriginLibrary
{
    public class OriginLibrarySettings : ObservableObject, ISettings
    {
        private readonly ILogger logger = LogManager.GetLogger();
        private OriginLibrarySettings editingClone;
        private readonly OriginLibrary library;
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
                    var api = new OriginAccountClient(view);
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

        public OriginLibrarySettings()
        {
        }

        public OriginLibrarySettings(OriginLibrary library, IPlayniteAPI playniteApi)
        {
            this.library = library;
            this.playniteApi = playniteApi;

            var settings = library.LoadPluginSettings<OriginLibrarySettings>();
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

        private void LoadValues(OriginLibrarySettings source)
        {
            source.CopyProperties(this, false, null, true);
        }

        private void Login()
        {
            try
            {
                using (var view = playniteApi.WebViews.CreateView(490, 670))
                {
                    var api = new OriginAccountClient(view);
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