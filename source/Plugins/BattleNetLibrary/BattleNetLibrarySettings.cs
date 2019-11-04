using BattleNetLibrary.Services;
using Newtonsoft.Json;
using Playnite;
using Playnite.SDK;
using Playnite.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleNetLibrary
{
    public class BattleNetLibrarySettings : ObservableObject, ISettings
    {
        private static ILogger logger = LogManager.GetLogger();
        private BattleNetLibrarySettings editingClone;
        private BattleNetLibrary library;
        private IPlayniteAPI api;

        #region Settings   

        public int Version { get; set; }

        public bool ImportInstalledGames { get; set; } = true;

        public bool ConnectAccount { get; set; } = false;

        public bool ImportUninstalledGames { get; set; } = false;

        #endregion Settings

        [JsonIgnore]
        public bool IsUserLoggedIn
        {
            get
            {
                using (var view = api.WebViews.CreateOffscreenView())
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

        public BattleNetLibrarySettings(BattleNetLibrary library, IPlayniteAPI api)
        {
            this.library = library;
            this.api = api;

            var settings = library.LoadPluginSettings<BattleNetLibrarySettings>();
            if (settings != null)
            {
                if (settings.Version == 0)
                {
                    logger.Debug("Updating battle.net settings from version 0.");
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

        private void LoadValues(BattleNetLibrarySettings source)
        {
            source.CopyProperties(this, false, null, true);
        }

        private void Login()
        {
            try
            {
                using (var view = api.WebViews.CreateView(400, 500))
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
