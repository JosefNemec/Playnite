using Playnite;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginLibrary
{
    public class OriginClient : ILibraryClient
    {
        public bool IsInstalled { get => Origin.IsInstalled; }

        public void Open()
        {
            Origin.StartClient();
        }
    }
}
