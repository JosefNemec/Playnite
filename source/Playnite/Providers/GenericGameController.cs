using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Playnite.Models;

namespace Playnite.Providers
{
    public class GenericGameController : GameController
    {
        public GenericGameController(Game game) : base(game)
        {            
        }

        public override void Install()
        {
            throw new NotImplementedException();
        }

        public override void Uninstall()
        {
            throw new NotImplementedException();
        }
    }
}
