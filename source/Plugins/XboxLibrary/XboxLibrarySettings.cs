using Newtonsoft.Json;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XboxLibrary.Services;

namespace XboxLibrary
{
    public class XboxLibrarySettings : ObservableObject, ISettings
    {
        private static ILogger logger = LogManager.GetLogger();
        private XboxLibrarySettings editingClone;
        private readonly XboxLibrary plugin;

        public bool ConnectAccount { get; set; } = false;

        public bool ImportInstalledGames { get; set; } = true;

        public bool ImportUninstalledGames { get; set; } = false;

        public bool XboxAppClientPriorityLaunch { get; set; } = false;

        public bool Import360Games { get; set; } = false;

        public bool ImportXboneGames { get; set; } = false;

        [JsonIgnore]
        public bool IsFirstRunUse { get; set; }

        [JsonIgnore]
        public bool IsUserLoggedIn
        {
            get
            {
                return new XboxAccountClient(plugin).GetIsUserLoggedIn().GetAwaiter().GetResult();
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

        public XboxLibrarySettings()
        {
        }

        public XboxLibrarySettings(XboxLibrary plugin)
        {
            this.plugin = plugin;
            var savedSettings = plugin.LoadPluginSettings<XboxLibrarySettings>();
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

        private void LoadValues(XboxLibrarySettings source)
        {
            source.CopyProperties(this, false, null, true);
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }

        private async Task Login()
        {
            try
            {
                var client = new XboxAccountClient(plugin);
                await client.Login();
                OnPropertyChanged(nameof(IsUserLoggedIn));
            }
            catch (Exception e) when (!Debugger.IsAttached)
            {
                logger.Error(e, "Failed to authenticate user.");
            }
        }
    }
}