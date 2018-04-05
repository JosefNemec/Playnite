using Playnite.Database;
using Playnite.SDK.Models;
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

        GameLocalDataResponse GetLocalManifest(string id, string packageName = null, bool useDataCache = false);

        GameTask GetGamePlayTask(GameLocalDataResponse manifest);

        List<IGame> GetInstalledGames(bool useDataCache = false);

        List<IGame> GetLibraryGames();

        OriginGameMetadata DownloadGameMetadata(string id);

        OriginGameMetadata UpdateGameWithMetadata(IGame game);
    }
}
