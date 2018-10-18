using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database.Events
{
    public class GameUpdateEvent
    {
        public Game OldData
        {
            get; set;
        }

        public Game NewData
        {
            get; set;
        }

        public GameUpdateEvent(Game oldData, Game newData)
        {
            OldData = oldData;
            NewData = newData;
        }
    }

    public delegate void GameUpdatedEventHandler(object sender, GameUpdatedEventArgs args);

    public class GameUpdatedEventArgs : EventArgs
    {
        public List<GameUpdateEvent> UpdatedGames
        {
            get; set;
        }

        public GameUpdatedEventArgs(Game oldData, Game newData)
        {
            UpdatedGames = new List<GameUpdateEvent>() { new GameUpdateEvent(oldData, newData) };
        }

        public GameUpdatedEventArgs(List<GameUpdateEvent> updatedGames)
        {
            UpdatedGames = updatedGames;
        }
    }
}
