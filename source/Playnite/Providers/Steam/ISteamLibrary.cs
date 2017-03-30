using Playnite.Database;
using Playnite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Providers.Steam
{
    public interface ISteamLibrary
    {
        IGame GetInstalledGamesFromFile(string path);

        List<IGame> GetInstalledGamesFromFolder(string path);

        List<IGame> GetInstalledGames();

        List<IGame> GetLibraryGames(string userName);

        SteamGameMetadata DownloadGameMetadata(int id);

        SteamGameMetadata UpdateGameWithMetadata(IGame game);
    }
}
