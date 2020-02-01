using Playnite;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumbleLibrary
{
    public class HumbleGameController : BaseGameController
    {
        public HumbleGameController(Game game) : base(game)
        {
        }

        public override void Play()
        {
        }

        public override void Install()
        {
            throw new NotSupportedException("Installation is currently not supported for Humble games.");
        }

        public override void Uninstall()
        {
            throw new NotSupportedException("Uninstallation is currently not supported for Humble games.");
        }
    }
}
