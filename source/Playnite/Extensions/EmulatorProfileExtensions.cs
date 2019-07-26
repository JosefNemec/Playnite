using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public static class EmulatorProfileExtensions
    {
        public static EmulatorProfile ExpandVariables(this EmulatorProfile profile, Game game)
        {
            var expaded = profile.GetClone();
            if (!string.IsNullOrEmpty(expaded.Arguments))
            {
                expaded.Arguments = game.ExpandVariables(expaded.Arguments);
            }

            if (!string.IsNullOrEmpty(expaded.WorkingDirectory))
            {
                expaded.WorkingDirectory = game.ExpandVariables(expaded.WorkingDirectory, true);
            }

            if (!string.IsNullOrEmpty(expaded.Executable))
            {
                expaded.Executable = game.ExpandVariables(expaded.Executable, true);
            }

            return expaded;
        }
    }
}
