using Playnite.Metadata;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Providers.Uplay
{
    public interface IUplayLibrary
    {
        GameAction GetGamePlayTask(string id);

        List<Game> GetInstalledGames();

        GameMetadata UpdateGameWithMetadata(Game game);
    }
}
