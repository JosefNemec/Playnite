using Playnite.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BattleNetLibrary
{
    public class BattleNet
    {
        public static bool IsRunning
        {
            get
            {
                return RunningProcessesCount > 0;
            }
        }

        public static int RunningProcessesCount
        {
            get
            {
                var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(ClientExecPath));
                return processes?.Any() == true ? processes.Count() : 0;
            }
        }

        public static string ClientExecPath
        {
            get
            {
                var path = InstallationPath;
                return string.IsNullOrEmpty(path) ? string.Empty : Path.Combine(path, "Battle.net.exe");
            }
        }

        public static string InstallationPath
        {
            get
            {
                var progs = Programs.GetUnistallProgramsList().FirstOrDefault(a => a.UninstallString?.Contains("-uid=battle.net") == true);
                if (progs == null)
                {
                    return string.Empty;
                }
                else
                {
                    return progs.InstallLocation;
                }
            }
        }

        public static bool IsInstalled
        {
            get
            {
                var clientExe = ClientExecPath;
                return !string.IsNullOrEmpty(clientExe) && File.Exists(clientExe);
            }
        }

        public static string Icon => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\battleneticon.png");

        public static void StartClient()
        {
            ProcessStarter.StartProcess(ClientExecPath, string.Empty);
        }
    }
}
