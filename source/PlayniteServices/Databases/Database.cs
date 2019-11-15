using LiteDB;
using PlayniteServices.Models.IGDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PlayniteServices.Databases
{
    public class Database : IDisposable
    {
        public const string SteamIgdbMatchCollectionName = "IGBDSteamIdCache";
        public const string IGBDGameIdMatchesCollectionName = "IGBDGameIdMatches";
        public const string IGDBSearchIdMatchesCollectionName = "IGDBSearchIdMatches";

        public static LiteCollection<GameIdMatch> IGBDGameIdMatches { get; private set; }
        public static LiteCollection<SearchIdMatch> IGDBSearchIdMatches { get; private set; }
        public static LiteCollection<SteamIdGame> SteamIgdbMatches { get; private set; }

        public static string Path
        {
            get
            {
                var path = Startup.Configuration.GetSection("DbPath").Value;
                if (System.IO.Path.IsPathRooted(path))
                {
                    return path;
                }
                else
                {
                    return System.IO.Path.Combine(Paths.ExecutingDirectory, path);
                }               
            }
        }

        private LiteDatabase liteDB;

        public Database(string path)
        {
            liteDB =  new LiteDatabase(string.Format("Filename={0}", path));

            IGBDGameIdMatches = liteDB.GetCollection<GameIdMatch>(IGBDGameIdMatchesCollectionName);
            IGBDGameIdMatches.EnsureIndex(nameof(GameIdMatch.Id));

            IGDBSearchIdMatches = liteDB.GetCollection<SearchIdMatch>(IGDBSearchIdMatchesCollectionName);
            IGDBSearchIdMatches.EnsureIndex(nameof(SearchIdMatch.Id));

            SteamIgdbMatches = liteDB.GetCollection<SteamIdGame>(SteamIgdbMatchCollectionName);
            SteamIgdbMatches.EnsureIndex(nameof(SteamIdGame.steamId));
        }

        public void Dispose()
        {
            liteDB.Dispose();
        }

        public LiteCollection<T> GetCollection<T>(string name)
        {
            return liteDB.GetCollection<T>(name);
        }

        public LiteCollection<BsonDocument> GetCollection(string name)
        {
            return liteDB.GetCollection(name);
        }

        public bool DropCollection(string name)
        {
            return liteDB.DropCollection(name);
        }
    }
}
