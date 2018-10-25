using LiteDB;
using Playnite.Database;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.API
{
    // TODO
    public class DatabaseAPI : IGameDatabaseAPI
    {
        private GameDatabase database;

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

        public void AddEmulator(Emulator emulator)
        {
            database.Emulators.Add(emulator);
        }

        public Emulator GetEmulator(Guid id)
        {
            return database.Emulators.Get(id);
        }

        public IEnumerable<Emulator> GetEmulators()
        {
            return database.Emulators;
        }

        public Game GetGame(Guid id)
        {
            return database.Games.Get(id);
        }

        public void AddGame(Game game)
        {
            database.Games.Add(game);
        }

        public IEnumerable<Game> GetGames()
        {
            return database.Games;
        }

        public Platform GetPlatform(Guid id)
        {
            return database.Platforms.Get(id);
        }

        public IEnumerable<Platform> GetPlatforms()
        {
            return database.Platforms;
        }

        public void AddPlatform(Platform platform)
        {
            database.Platforms.Add(platform);
        }

        public void RemoveEmulator(Guid id)
        {
            database.Emulators.Remove(id);
        }

        public void RemoveGame(Guid id)
        {
            database.Games.Remove(id);
        }

        public void RemovePlatform(Guid id)
        {
            database.Platforms.Remove(id);
        }

        public void UpdateGame(Game game)
        {
            database.Games.Update(game);
        }

        public string AddFile(string path, Guid parentId)
        {
            if (!File.Exists(path))
            {
                throw new Exception("Cannot add file to db, file not found.");
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

        public void ImportCategories(List<Game> sourceGames)
        {
            database.ImportCategories(sourceGames);
        }
    }
}
