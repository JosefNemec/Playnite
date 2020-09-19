using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    /// <summary>
    /// Describes application settings API.
    /// </summary>
    public interface IPlayniteSettingsAPI
    {
        /// <summary>
        ///
        /// </summary>
        int Version { get; }

        /// <summary>
        ///
        /// </summary>
        int GridItemWidthRatio { get; }

        /// <summary>
        ///
        /// </summary>
        int GridItemHeightRatio { get; }

        /// <summary>
        ///
        /// </summary>
        bool FirstTimeWizardComplete { get; }

        /// <summary>
        ///
        /// </summary>
        bool DisableHwAcceleration { get; }

        /// <summary>
        ///
        /// </summary>
        bool AsyncImageLoading { get; }

        /// <summary>
        ///
        /// </summary>
        bool DownloadMetadataOnImport { get; }

        /// <summary>
        ///
        /// </summary>
        bool StartInFullscreen { get; }

        /// <summary>
        ///
        /// </summary>
        string DatabasePath { get; }

        /// <summary>
        ///
        /// </summary>
        bool MinimizeToTray { get; }

        /// <summary>
        ///
        /// </summary>
        bool CloseToTray { get; }

        /// <summary>
        ///
        /// </summary>
        bool EnableTray { get; }

        /// <summary>
        ///
        /// </summary>
        string Language { get; }

        /// <summary>
        ///
        /// </summary>
        bool UpdateLibStartup { get; }

        /// <summary>
        ///
        /// </summary>
        string DesktopTheme { get; }

        /// <summary>
        ///
        /// </summary>
        string FullscreenTheme { get; }

        /// <summary>
        ///
        /// </summary>
        bool StartMinimized { get; }

        /// <summary>
        ///
        /// </summary>
        bool StartOnBoot { get; }

        /// <summary>
        ///
        /// </summary>
        bool ForcePlayTimeSync { get; }

        /// <summary>
        ///
        /// </summary>
        string FontFamilyName { get; }

        /// <summary>
        ///
        /// </summary>
        bool DiscordPresenceEnabled { get; }

        /// <summary>
        ///
        /// </summary>
        AgeRatingOrg AgeRatingOrgPriority { get; }

        /// <summary>
        /// Checks if game is added on import exclusion list.
        /// </summary>
        /// <param name="gameId">Game ID.</param>
        /// <param name="libraryId">Library plugin ID.</param>
        /// <returns>True if game is on exclusion list.</returns>
        bool GetGameExcludedFromImport(string gameId, Guid libraryId);
    }
}
