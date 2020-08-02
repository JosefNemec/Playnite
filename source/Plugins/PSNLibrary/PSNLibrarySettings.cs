using Newtonsoft.Json;
using Playnite.SDK;
using PSNLibrary.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSNLibrary
{
    public class PSNLibrarySettings : ObservableObject, ISettings
    {
        private static ILogger logger = LogManager.GetLogger();
        private PSNLibrarySettings editingClone;
        private readonly PSNLibrary plugin;
        private PSNAccountClient clientApi;

        public bool ConnectAccount { get; set; } = false;

        [JsonIgnore]
        public bool IsUserLoggedIn
        {
            get
            {
                return clientApi.GetIsUserLoggedIn().GetAwaiter().GetResult();
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

        public PSNLibrarySettings()
        {
        }

        public PSNLibrarySettings(PSNLibrary plugin)
        {
            this.plugin = plugin;
            clientApi = new PSNAccountClient(plugin);
            var savedSettings = plugin.LoadPluginSettings<PSNLibrarySettings>();
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

        private void LoadValues(PSNLibrarySettings source)
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
                clientApi.Login();
                OnPropertyChanged(nameof(IsUserLoggedIn));
            }
            catch (Exception e) when (!Debugger.IsAttached)
            {
                logger.Error(e, "Failed to authenticate user.");
            }
        }
    }
}