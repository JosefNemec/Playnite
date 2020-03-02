using Playnite.SDK;
using Playnite.SDK.Metadata;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Steam;

namespace SteamLibrary
{
    public class SteamMetadataProvider : LibraryMetadataProvider
    {
        private ILogger logger = LogManager.GetLogger();
        private SteamLibrary library;
        private SteamApiClient apiClient;

        public SteamMetadataProvider(SteamLibrary library, SteamApiClient apiClient)
        {
            this.library = library;
            this.apiClient = apiClient;
        }

        public override GameMetadata GetMetadata(Game game)
        {
            var gameData = new Game("SteamGame")
            {
                GameId = game.GameId
            };

            var gameId = game.ToSteamGameID();
            if (gameId.IsMod)
            {
                var data = library.GetInstalledModFromFolder(game.InstallDirectory, ModInfo.GetModTypeOfGameID(gameId));
                return new GameMetadata(data, null, null, null);
            }
            else
            {
                return new MetadataProvider(apiClient).GetGameMetadata(
                    gameId.AppID,
                    library.LibrarySettings.BackgroundSource,
                    library.LibrarySettings.DownloadVerticalCovers);
            }
        }
    }
}