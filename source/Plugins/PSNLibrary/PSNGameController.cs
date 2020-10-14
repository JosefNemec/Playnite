using Playnite;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSNLibrary
{
    public class PSNGameController : BaseGameController
    {
        public PSNGameController(Game game) : base(game)
        {
        }

        public override void Play()
        {
        }

        public override void Install()
        {
            throw new NotSupportedException("Installation is currently not supported for PlayStation games.");
        }

        public override void Uninstall()
        {
            throw new NotSupportedException("Uninstallation is currently not supported for PlayStation games.");
        }
    }
}
