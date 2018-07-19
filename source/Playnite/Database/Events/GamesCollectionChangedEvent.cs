using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database.Events
{
    public delegate void GamesCollectionChangedEventHandler(object sender, GamesCollectionChangedEventArgs args);

    public class GamesCollectionChangedEventArgs : EventArgs
    {
        public List<Game> AddedGames
        {
            get; set;
        }

        public List<Game> RemovedGames
        {
            get; set;
        }

        public GamesCollectionChangedEventArgs(List<Game> addedGames, List<Game> removedGames)
        {
            AddedGames = addedGames;
            RemovedGames = removedGames;
        }
    }
}
