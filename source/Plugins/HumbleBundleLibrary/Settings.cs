using Newtonsoft.Json;
using Playnite.SDK;
using PlayniteUI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumbleBundleLibrary
{
    class Settings : ObservableObject, ISettings
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private readonly bool firstRunSettings;
        private readonly IPlayniteAPI api;

        [JsonIgnore]
        public bool IsUserLoggedIn
        {
            get
            {
                using (var view = api.WebViews.CreateOffscreenView())
                {
                    var api = new HumbleBundleLoginApi(view);
                    return api.GetIsUserLoggedIn();
                }
            }
        }

        public Settings(bool firstRunSettings, IPlayniteAPI api)
        {
            this.firstRunSettings = firstRunSettings;
            this.api = api;
        }

        public void BeginEdit()
        {
        }

        public void CancelEdit()
        {
        }

        public void EndEdit()
        {
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }

        [JsonIgnore]
        public RelayCommand<object> LoginCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                Login();
            });
        }

        private void Login()
        {
            try
            {
                using (var view = api.WebViews.CreateView(490, 670))
                {
                    var process = new HumbleBundleLoginApi(view);
                    process.Execute();
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
