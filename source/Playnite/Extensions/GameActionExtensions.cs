using Playnite.Models;
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
            var expaded = action.CloneJson();
            expaded.AdditionalArguments = game.ExpandVariables(expaded.AdditionalArguments);
            expaded.Arguments = game.ExpandVariables(expaded.Arguments);
            expaded.Path = game.ExpandVariables(expaded.Path);
            expaded.WorkingDir = game.ExpandVariables(expaded.WorkingDir);
            return expaded;
        }
    }
}
