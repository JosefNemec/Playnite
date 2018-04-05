using Playnite.Database;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Providers.Steam
{
    public interface ISteamLibrary
    {
        IGame GetInstalledGameFromFile(string path);

        List<IGame> GetInstalledGamesFromFolder(string path);

        List<IGame> GetInstalledGames();

        List<IGame> GetLibraryGames(SteamSettings settings);

        List<IGame> GetLibraryGames(string userName, string apiKey);

        List<IGame> GetLibraryGames(string userName);

        SteamGameMetadata DownloadGameMetadata(int id);

        SteamGameMetadata UpdateGameWithMetadata(IGame game);

        List<string> GetLibraryFolders();
    }
}
