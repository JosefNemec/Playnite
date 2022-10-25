using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public interface IPlayniteApplication
    {
        void Quit(bool saveSettings);
        void Restart(bool saveSettings);
        void QuitAndStart(string path, string arguments, bool asAdmin = false, bool saveSettings = true);
    }
}
