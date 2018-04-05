using Playnite.Models;
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
        GameTask GetGamePlayTask(string id);

        List<IGame> GetInstalledGames();

        GameMetadata UpdateGameWithMetadata(IGame game);
    }
}
