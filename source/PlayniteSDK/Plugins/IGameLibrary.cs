using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Playnite.SDK.Plugins
{
    public interface ILibraryPlugin : IPlugin
    {
        ILibraryClient Client { get; }

        string LibraryIcon { get; }

        string Name { get; }

        IEnumerable<Game> GetGames();

        IGameController GetGameController(Game game);

        ILibraryMetadataProvider GetMetadataDownloader();       
    }
}
