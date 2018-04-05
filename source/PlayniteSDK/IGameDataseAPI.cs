using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using Playnite.SDK.Models;

namespace Playnite.SDK
{
    public interface IGameDataseAPI
    {
        Game GetGame(int id);

        List<Game> GetGames();

        void RemoveGame(int id);

        void UpdateGame(Game game);

        Emulator GetEmulator(ObjectId id);

        List<Emulator> GetEmulators();

        void RemoveEmulator(ObjectId id);

        Platform GetPlatform(ObjectId id);

        List<Platform> GetPlatforms();

        void RemovePlatform(ObjectId id);
    }
}