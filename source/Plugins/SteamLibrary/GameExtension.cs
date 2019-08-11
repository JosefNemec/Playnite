using Playnite.SDK.Models;
using SteamKit2;

namespace SteamLibrary
{
    internal static class GameExtension
    {
        public static GameID ToSteamGameID(this Game game)
        {
            return new GameID(ulong.Parse(game.GameId));
        }

        public static GameID ToSteamGameID(this GameInfo game)
        {
            return new GameID(ulong.Parse(game.GameId));
        }
    }
}
