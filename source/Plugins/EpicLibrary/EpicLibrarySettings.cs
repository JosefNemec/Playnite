using EpicLibrary.Services;
using Newtonsoft.Json;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EpicLibrary
{
    public class EpicLibrarySettings : ObservableObject, ISettings
    {
        private readonly ILogger logger = LogManager.GetLogger();
        private EpicLibrarySettings editingClone;
        private readonly EpicLibrary library;
        private readonly IPlayniteAPI api;

        #region Settings      

        public bool ImportInstalledGames { get; set; } = EpicLauncher.IsInstalled;

        public bool ConnectAccount { get; set; } = false;

        public bool ImportUninstalledGames { get; set; } = false;

        #endregion Settings

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