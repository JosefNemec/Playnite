using Playnite.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace XboxLibrary
{
    public class Xbox
    {
        private const string xboxPassAppFN = "Microsoft.GamingApp_8wekyb3d8bbwe";

        private static bool? isXboxPassAppInstalled;
        public static bool IsXboxPassAppInstalled
        {
            get
            {
                if (isXboxPassAppInstalled != null)
                {
                    return isXboxPassAppInstalled.Value;
                }
                else
                {
                    isXboxPassAppInstalled = false;
                    if (Computer.WindowsVersion == WindowsVersion.Win10)
                    {
                        var apps = Programs.GetUWPApps();
                        var xboxApp = apps.FirstOrDefault(a => a.AppId == xboxPassAppFN);
                        isXboxPassAppInstalled = xboxApp != null;
                    }

                    return isXboxPassAppInstalled.Value;
                }
            }
        }

        public static void OpenXboxPassApp()
        {
            if (Computer.WindowsVersion == WindowsVersion.Win10)
            {
                var apps = Programs.GetUWPApps();
                var xboxApp = apps.FirstOrDefault(a => a.AppId == xboxPassAppFN);
                if (xboxApp == null)
                {
                    throw new NotSupportedException("Xbox PC Pass app is not installed.");
                }
                else
                {
                    ProcessStarter.StartProcess(xboxApp.Path, xboxApp.Arguments);
                }
            }
            else
            {
                throw new NotSupportedException("Only supported on Windows 10.");
            }
        }

        public static string Icon { get; } = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"icon.png");
    }
}
