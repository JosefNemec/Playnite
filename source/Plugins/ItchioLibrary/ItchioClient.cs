using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItchioLibrary
{
    public class ItchioClient : ILibraryClient
    {
        public bool IsInstalled { get => Itch.IsInstalled; }

        public void Open()
        {
            Itch.StartClient();
        }
    }
}
