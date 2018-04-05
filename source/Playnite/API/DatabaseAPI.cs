using LiteDB;
using Playnite.Database;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.API
{
    public class DatabaseAPI : IGameDataseAPI
    {
        private GameDatabase database;

        public DatabaseAPI(GameDatabase database)
        {
            this.database = database;
        }

        public Emulator GetEmulator(ObjectId id)
        {
            return database.GetEmulator(id);
        }

        public List<Emulator> GetEmulators()
        {
            return database.GetEmulators();
        }

        public Game GetGame(int id)
        {
            return database.GetGame(id) as Game;
        }

        public List<Game> GetGames()
        {
            return database.GetGames().Cast<Game>().ToList();
        }

        public Platform GetPlatform(ObjectId id)
        {
            return database.GetPlatform(id);
        }

        public List<Platform> GetPlatforms()
        {
            return database.GetPlatforms();
        }

        public void RemoveEmulator(ObjectId id)
        {
            database.RemoveEmulator(id);
        }

        public void RemoveGame(int id)
        {
            database.DeleteGame(id);
        }

        public void RemovePlatform(ObjectId id)
        {
            database.RemovePlatform(id);
        }

        public void UpdateGame(Game game)
        {
            database.UpdateGameInDatabase(game);
        }
    }
}
