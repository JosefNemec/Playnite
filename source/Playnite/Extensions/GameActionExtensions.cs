using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public static class GameActionExtensions
    {
        public static GameAction ExpandVariables(this GameAction action, Game game)
        {
            var expaded = action.GetClone();
            expaded.AdditionalArguments = game.ExpandVariables(expaded.AdditionalArguments);
            expaded.Arguments = game.ExpandVariables(expaded.Arguments);
            expaded.WorkingDir = game.ExpandVariables(expaded.WorkingDir, true);
            if (expaded.Type != GameActionType.URL)
            {
                expaded.Path = game.ExpandVariables(expaded.Path, true);
            }

            return expaded;
        }
    }
}
