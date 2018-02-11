using LiteDB;
using Playnite.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Models
{
    public class ActiveController
    {
        [BsonId]
        public int Id
        {
            get; set;
        }

        public Game Game
        {
            get; set;
        }

        public ActiveController()
        {
        }

        public ActiveController(IGameController controller)
        {
            Id = controller.Game.Id;
            Game = controller.Game.CloneJson();
        }
    }
}
