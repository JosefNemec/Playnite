using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Playnite
{
    public class FakePlayniteLibraryPlugin : LibraryPlugin
    {
        public override string Name => "Playnite";

        public override Guid Id => Guid.Empty;

        public FakePlayniteLibraryPlugin() : base(null)
        {
        }

        public override IEnumerable<GameInfo> GetGames()
        {
            throw new NotImplementedException();
        }
    }
}
