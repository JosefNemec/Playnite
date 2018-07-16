using Playnite.SDK;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGameLibrary
{
    public class TestGameLibrary : IGameLibrary
    {
        public IEditableObject Settings => null;

        public Guid LibraryId { get; } = Guid.Parse("D625A3B7-1AA4-41CB-9CD7-74448D28E99B");

        public TestGameLibrary(IPlayniteAPI api)
        {
        }

        public void Dispose()
        {

        }
    }
}
