using Playnite.SDK.Models;
using Playnite.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.SDK.Metadata;

namespace Playnite.Providers.BattleNet
{
    public interface IBattleNetLibrary
    {
        GameAction GetGamePlayTask(string id);

        List<Game> GetInstalledGames();

        GameMetadata UpdateGameWithMetadata(Game game);

        List<Game> GetLibraryGames();
    }
}
