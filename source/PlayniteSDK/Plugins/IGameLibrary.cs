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
        UserControl SettingsView { get; }

        IEditableObject Settings { get; }

        string Name { get; }

        IEnumerable<Game> GetGames();

        IGameController GetGameController(Game game);

        ILibraryMetadataDownloader GetMetadataDownloader();
    }
}
