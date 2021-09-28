using Playnite.Database;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Playnite.API
{
    public class PlayniteSettingsAPI : IPlayniteSettingsAPI
    {
        private readonly PlayniteSettings settings;
        private readonly GameDatabase db;

        public int Version => settings.Version;
        public int GridItemWidthRatio => settings.GridItemWidthRatio;
        public int GridItemHeightRatio => settings.GridItemHeightRatio;
        public bool FirstTimeWizardComplete => settings.FirstTimeWizardComplete;
        public bool DisableHwAcceleration => settings.DisableHwAcceleration;
        public bool AsyncImageLoading => settings.AsyncImageLoading;
        public bool DownloadMetadataOnImport => settings.DownloadMetadataOnImport;
        public bool StartInFullscreen => settings.StartInFullscreen;
        public string DatabasePath => settings.DatabasePath;
        public bool MinimizeToTray => settings.MinimizeToTray;
        public bool CloseToTray => settings.CloseToTray;
        public bool EnableTray => settings.EnableTray;
        public string Language => settings.Language == "english" ? "en_US" : settings.Language;
        public bool UpdateLibStartup => settings.UpdateLibStartup;
        public string DesktopTheme => settings.Theme;
        public string FullscreenTheme => settings.Fullscreen.Theme;
        public bool StartMinimized => settings.StartMinimized;
        public bool StartOnBoot => settings.StartOnBoot;
        public bool ForcePlayTimeSync => settings.ForcePlayTimeSync;
        public string FontFamilyName => settings.FontFamilyName;
        public bool DiscordPresenceEnabled => settings.DiscordPresenceEnabled;
        public AgeRatingOrg AgeRatingOrgPriority => settings.AgeRatingOrgPriority;
        public bool SidebarVisible => settings.ShowSidebar;
        public Dock SidebarPosition => settings.SidebarPosition;

        public PlayniteSettingsAPI(PlayniteSettings settings, GameDatabase db)
        {
            this.settings = settings;
            this.db = db;
        }

        public bool GetGameExcludedFromImport(string gameId, Guid libraryId)
        {
            if (gameId.IsNullOrEmpty() || libraryId == Guid.Empty)
            {
                throw new ArgumentNullException("gameId and libraryId must be specified.");
            }

            return db.ImportExclusions.Get(ImportExclusionItem.GetId(gameId, libraryId)) != null;
        }
    }
}
