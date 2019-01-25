using EpicLibrary.Services;
using Newtonsoft.Json;
using Playnite;
using Playnite.SDK;
using PlayniteUI.Commands;
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

        public bool ImportInstalledGames { get; set; } = EpicLauncher.IsInstalled;

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

            var settings = api.LoadPluginSettings<EpicLibrarySettings>(library);
            if (settings != null)
            {
                LoadValues(settings);
            }
        }

        public void BeginEdit()
        {
            editingClone = this.CloneJson();
        }

        public void CancelEdit()
        {
            LoadValues(editingClone);
        }

        public void EndEdit()
        {
            api.SavePluginSettings(library, this);
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
                api.Dialogs.ShowErrorMessage(api.Resources.FindString("LOCNotLoggedInError"), "");
                logger.Error(e, "Failed to authenticate user.");
            }
        }
    }
}
