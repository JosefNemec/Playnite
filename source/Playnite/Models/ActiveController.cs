using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.SDK.Models;
using Playnite.SDK;

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
