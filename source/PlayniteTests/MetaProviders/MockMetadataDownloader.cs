using Playnite.MetaProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.Models;

namespace PlayniteTests
{
    public class MockMetadataDownloader : MetadataDownloader
    {
        public int CallCount
        {
            get; set;
        } = 0;

        public MockMetadataDownloader(
            IMetadataProvider steamProvider,
            IMetadataProvider originProvider,
            IMetadataProvider gogProvider,
            IMetadataProvider battleNetProvider,
            IMetadataProvider igdbProvider)
            : base(steamProvider, originProvider, gogProvider, battleNetProvider, igdbProvider)
        {
        }

        public Func<string, string, IMetadataProvider, GameMetadata> DownloadDataHandler
        {
            get; set;
        }

        public override GameMetadata DownloadGameData(string gameName, string id, IMetadataProvider provider)
        {
            CallCount++;
            if (DownloadDataHandler != null)
            {
                return DownloadDataHandler(gameName, id, provider);
            }
            else
            {
                return base.DownloadGameData(gameName, id, provider);
            }            
        }
    }

    public class MockMetadataProvider : IMetadataProvider
    {
        public Func<string, GameMetadata> GetGameDataHandler
        {
            get; set;
        }

        public Func<bool> GetSupportsIdSearchHandler
        {
            get; set;
        }

        public Func<string, List<MetadataSearchResult>> SearchGamesHandler
        {
            get; set;
        }

        public GameMetadata GetGameData(string gameId)
        {
            return GetGameDataHandler(gameId);
        }

        public bool GetSupportsIdSearch()
        {
            return GetSupportsIdSearchHandler();
        }

        public List<MetadataSearchResult> SearchGames(string gameName)
        {
            return SearchGamesHandler(gameName);
        }
    }
}
