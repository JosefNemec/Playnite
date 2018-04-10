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
        void AddGame(Game game);

        Game GetGame(int id);

        List<Game> GetGames();

        void RemoveGame(int id);

        void UpdateGame(Game game);

        void AddEmulator(Emulator emulator);

        Emulator GetEmulator(ObjectId id);

        List<Emulator> GetEmulators();

        void RemoveEmulator(ObjectId id);

        void AddPlatform(Platform platform);

        Platform GetPlatform(ObjectId id);

        List<Platform> GetPlatforms();

        void RemovePlatform(ObjectId id);

        void AddFile(string id, string path);

        string AddFileNoDuplicates(string id, string path);

        void SaveFile(string id, string path);

        void RemoveFile(string id);

        void RemoveImage(string id, Game game);

        List<DatabaseFile> GetFiles();
    }
}