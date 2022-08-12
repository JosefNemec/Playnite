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
    public class FullscreenSettingsAPI : IFullscreenSettingsAPI
    {
        private readonly FullscreenSettings settings;

        public bool IsMusicMuted
        {
            get => settings.IsMusicMuted;
            set => settings.IsMusicMuted = value;
        }

        public FullscreenSettingsAPI(FullscreenSettings settings)
        {
            this.settings = settings;
        }
    }

    public class CompletionStatusSettignsApi : ICompletionStatusSettignsApi
    {
        private readonly GameDatabase db;
        public CompletionStatusSettignsApi(GameDatabase database)
        {
            db = database;
        }

        public Guid DefaultStatus => db.GetCompletionStatusSettings().DefaultStatus;
        public Guid PlayedStatus => db.GetCompletionStatusSettings().DefaultStatus;
    }

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
        public bool InstallSizeScanUseSizeOnDisk => settings.InstallSizeScanUseSizeOnDisk;
        public bool ScanLibInstallSizeOnLibUpdate => settings.ScanLibInstallSizeOnLibUpdate;
        public bool StartInFullscreen => settings.StartInFullscreen;
        public string DatabasePath => settings.DatabasePath;
        public bool MinimizeToTray => settings.MinimizeToTray;
        public bool CloseToTray => settings.CloseToTray;
        public bool EnableTray => settings.EnableTray;
        public string Language => settings.Language == "english" ? "en_US" : settings.Language;
        public bool UpdateLibStartup => settings.CheckForLibraryUpdates == LibraryUpdateCheckFrequency.OnEveryStartup;
        public string DesktopTheme => settings.Theme;
        public string FullscreenTheme => settings.Fullscreen.Theme;
        public bool StartMinimized => settings.StartMinimized;
        public bool StartOnBoot => settings.StartOnBoot;
        public string FontFamilyName => settings.FontFamilyName;
        public bool DiscordPresenceEnabled => settings.DiscordPresenceEnabled;
        public AgeRatingOrg AgeRatingOrgPriority => settings.AgeRatingOrgPriority;
        public bool SidebarVisible => settings.ShowSidebar;
        public Dock SidebarPosition => settings.SidebarPosition;
        public IFullscreenSettingsAPI Fullscreen { get; }
        public ICompletionStatusSettignsApi CompletionStatus { get; }
        public bool ForcePlayTimeSync => false;
        public PlaytimeImportMode PlaytimeImportMode => settings.PlaytimeImportMode;

        public PlayniteSettingsAPI(PlayniteSettings settings, GameDatabase db)
        {
            this.settings = settings;
            this.db = db;
            Fullscreen = new FullscreenSettingsAPI(settings.Fullscreen);
            CompletionStatus = new CompletionStatusSettignsApi(db);
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
