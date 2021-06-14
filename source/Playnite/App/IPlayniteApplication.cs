using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public interface IPlayniteApplication
    {
        void Quit();
        void Restart();
        void QuitAndStart(string path, string arguments, bool asAdmin = false);
    }
}
