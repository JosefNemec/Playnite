using Playnite.Database;
using Playnite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Providers.Origin
{
    public interface IOriginLibrary
    {
        string GetPathFromPlatformPath(string path);

        System.Collections.Specialized.NameValueCollection ParseOriginManifest(string path);

        List<IGame> GetInstalledGames(bool useDataCache = false);

        List<IGame> GetLibraryGames();

        OriginGameMetadata DownloadGameMetadata(string id);

        OriginGameMetadata UpdateGameWithMetadata(IGame game);
    }
}
