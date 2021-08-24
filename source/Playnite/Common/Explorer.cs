using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Common
{
    public class Explorer
    {
        public static void NavigateToFileSystemEntry(string path)
        {
            ProcessStarter.StartProcess("explorer.exe", $"/select,\"{path}\"");
        }

        public static void OpenDirectory(string path)
        {
            ProcessStarter.StartProcess("explorer.exe", $"\"{path}\"");
        }
    }
}
