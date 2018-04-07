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
        Game GetInstalledGameFromFile(string path);

        List<Game> GetInstalledGamesFromFolder(string path);

        List<Game> GetInstalledGames();

        List<Game> GetLibraryGames(SteamSettings settings);

        List<Game> GetLibraryGames(string userName, string apiKey);

        List<Game> GetLibraryGames(string userName);

        SteamGameMetadata DownloadGameMetadata(int id);

        SteamGameMetadata UpdateGameWithMetadata(Game game);

        List<string> GetLibraryFolders();
    }
}
