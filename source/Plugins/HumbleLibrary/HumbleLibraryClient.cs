using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumbleLibrary
{
    public class HumbleLibraryClient : LibraryClient
    {
        public override bool IsInstalled => true;

        public override void Open()
        {
            throw new NotImplementedException();
        }
    }
}