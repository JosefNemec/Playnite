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
    public class DatabaseAPI : IGameDatabaseAPI
    {
        private GameDatabase database;

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
            database.AddEmulator(emulator);
        }

        public Emulator GetEmulator(Guid id)
        {
            return database.GetEmulator(id);
        }

        public List<Emulator> GetEmulators()
        {
            return database.GetEmulators();
        }

        public Game GetGame(int id)
        {
            return database.GetGame(id);
        }

        public void AddGame(Game game)
        {
            database.AddGame(game);
        }

        public List<Game> GetGames()
        {
            return database.GetGames().Cast<Game>().ToList();
        }

        public Platform GetPlatform(Guid id)
        {
            return database.GetPlatform(id);
        }

        public List<Platform> GetPlatforms()
        {
            return database.GetPlatforms();
        }

        public void AddPlatform(Platform platform)
        {
            database.AddPlatform(platform);
        }

        public void RemoveEmulator(Guid id)
        {
            database.RemoveEmulator(id);
        }

        public void RemoveGame(int id)
        {
            database.DeleteGame(id);
        }

        public void RemovePlatform(Guid id)
        {
            database.RemovePlatform(id);
        }

        public void UpdateGame(Game game)
        {
            database.UpdateGameInDatabase(game);
        }

        public string AddFile(string id, string path)
        {
            if (!File.Exists(path))
            {
                throw new Exception("Cannot add file to db, file not found.");
            }

            return database.AddFileNoDuplicate(id, Path.GetFileName(path), File.ReadAllBytes(path));
        }

        public void SaveFile(string id, string path)
        {
            database.SaveFile(id, path);
        }

        public void RemoveFile(string id)
        {
            database.DeleteFile(id);
        }

        public void RemoveImage(string id, Game game)
        {
            database.DeleteImageSafe(id, game);
        }

        public List<DatabaseFile> GetFiles()
        {
            return database.Database.FileStorage.FindAll()?.Select(a => LiteFileToDbFile(a)).ToList();                
        }

        public DatabaseFile GetFile(string id)
        {
            var file = database.Database.FileStorage.FindById(id);
            if (file != null)
            {
                return LiteFileToDbFile(file);
            }
            else
            {
                return null;
            }
        }

        public void ImportCategories(List<Game> sourceGames)            
        {
            database.ImportCategories(sourceGames);
        }

        private DatabaseFile LiteFileToDbFile(LiteFileInfo liteDbFile)
        {
            var dbFile = new DatabaseFile()
            {
                Id = liteDbFile.Id,
                Filename = liteDbFile.Filename,
                MimeType = liteDbFile.MimeType,
                Length = liteDbFile.Length,
                UploadDate = liteDbFile.UploadDate,
            };

            if (liteDbFile.Metadata != null)
            {
                dbFile.Metadata = new Dictionary<string, object>();
                foreach (var key in liteDbFile.Metadata.Keys)
                {
                    dbFile.Metadata.Add(key, liteDbFile.Metadata[key]);
                }
            }

            return dbFile;
        }
    }
}
