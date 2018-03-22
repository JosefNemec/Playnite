using Playnite.Database;
using Playnite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Providers.GOG
{
    public interface IGogLibrary
    {
        List<IGame> GetInstalledGames();

        List<IGame> GetInstalledGames(InstalledGamesSource source);

        List<IGame> GetLibraryGames();

        GogGameMetadata DownloadGameMetadata(string id, string storeUrl = null);

        GogGameMetadata UpdateGameWithMetadata(IGame game);

        void CacheGogDatabases(string targetPath, string dbfile);
    }
}
