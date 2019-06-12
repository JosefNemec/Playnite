using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Playnite.UIWrapper
{
    class Program
    {
        static void Main(string[] args)
        {
            var exeName = "Playnite.DesktopApp.exe";
            var targetPath = Path.Combine(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory), exeName);
            var executable = Assembly.GetCallingAssembly().Location;
            if (File.Exists(targetPath))
            {
                var cmdLine = Environment.CommandLine.Replace(executable, "").Trim();
                Process.Start(targetPath, cmdLine);
            }
            else
            {
                MessageBox.Show("Playnite executable was not found!", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
