using Playnite.SDK.Models;
using Playnite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Providers.BattleNet
{
    public interface IBattleNetLibrary
    {
        GameTask GetGamePlayTask(string id);

        List<IGame> GetInstalledGames();

        GameMetadata UpdateGameWithMetadata(IGame game);

        List<IGame> GetLibraryGames();
    }
}
