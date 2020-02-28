using HumbleLibrary.Services;
using Newtonsoft.Json;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumbleLibrary
{
    public class HumbleLibrarySettings : ObservableObject, ISettings
    {
        private static ILogger logger = LogManager.GetLogger();
        private HumbleLibrarySettings editingClone;
        private readonly HumbleLibrary plugin;

        public bool ConnectAccount { get; set; } = false;

        public bool IgnoreThirdPartyStoreGames { get; set; } = true;

        public bool ImportTroveGames { get; set; } = false;

        [JsonIgnore]
        public bool IsUserLoggedIn
        {
            get
            {
                using (var view = plugin.PlayniteApi.WebViews.CreateOffscreenView(
                    new WebViewSettings
                    {
                        JavaScriptEnabled = false
                    }))
                {
                    var api = new HumbleAccountClient(view);
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

        public HumbleLibrarySettings()
        {
        }

        public HumbleLibrarySettings(HumbleLibrary plugin)
        {
            this.plugin = plugin;
            var savedSettings = plugin.LoadPluginSettings<HumbleLibrarySettings>();
            if (savedSettings != null)
            {
                LoadValues(savedSettings);
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
            plugin.SavePluginSettings(this);
        }

        private void LoadValues(HumbleLibrarySettings source)
        {
            source.CopyProperties(this, false, null, true);
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }

        private void Login()
        {
            try
            {
                using (var view = plugin.PlayniteApi.WebViews.CreateView(490, 670))
                {
                    var api = new HumbleAccountClient(view);
                    api.Login();
                }

                OnPropertyChanged(nameof(IsUserLoggedIn));
            }
            catch (Exception e) when (!Debugger.IsAttached)
            {
                logger.Error(e, "Failed to authenticate user.");
            }
        }
    }
}