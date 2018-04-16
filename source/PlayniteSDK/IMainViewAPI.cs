using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    /// <summary>
    /// Describes object providing API for main UI view.
    /// </summary>
    public interface IMainViewAPI
    {
        /// <summary>
        /// Gets list of currently selected games.
        /// </summary>
        IEnumerable<Game> SelectedGames
        {
            get;
        }
    }
}
