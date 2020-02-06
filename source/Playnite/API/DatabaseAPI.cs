using LiteDB;
using Playnite.Database;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.API
{
    public class DatabaseAPI : IGameDatabaseAPI
    {
        private GameDatabase database;

#pragma warning disable CS0067
        public event EventHandler DatabaseOpened;
#pragma warning restore CS0067

        public IItemCollection<Game> Games => database.Games;
        public IItemCollection<Platform> Platforms => database.Platforms;
        public IItemCollection<Emulator> Emulators => database.Emulators;
        public IItemCollection<Genre> Genres => database.Genres;
        public IItemCollection<Company> Companies => database.Companies;
        public IItemCollection<Tag> Tags => database.Tags;
        public IItemCollection<Category> Categories => database.Categories;
        public IItemCollection<Series> Series => database.Series;
        public IItemCollection<AgeRating> AgeRatings => database.AgeRatings;
        public IItemCollection<Region> Regions => database.Regions;
        public IItemCollection<GameSource> Sources => database.Sources;
        public IItemCollection<GameFeature> Features => database.Features;

        public string DatabasePath
        {
            get => database?.DatabasePath;
        }

        public bool IsOpen
        {
            get => database?.IsOpen == true;
        }

        public DatabaseAPI(GameDatabase database)
        {
            this.database = database;
        }

        public string AddFile(string path, Guid parentId)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Cannot add file to database, file not found.");
            }

            return database.AddFile(path, parentId);
        }

        public void SaveFile(string id, string path)
        {
            database.CopyFile(id, path);
        }

        public void RemoveFile(string id)
        {
            database.RemoveFile(id);
        }

        public IDisposable BufferedUpdate()
        {
            return database.BufferedUpdate();
        }

        public string GetFileStoragePath(Guid parentId)
        {
            return database.GetFileStoragePath(parentId);
        }

        public string GetFullFilePath(string databasePath)
        {
            return database.GetFullFilePath(databasePath);
        }

        public Game ImportGame(GameInfo game)
        {
            return database.ImportGame(game);
        }

        public Game ImportGame(GameInfo game, LibraryPlugin sourcePlugin)
        {
            return database.ImportGame(game, sourcePlugin.Id);
        }
    }
}
