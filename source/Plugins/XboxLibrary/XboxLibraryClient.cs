using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XboxLibrary
{
    public class XboxLibraryClient : LibraryClient
    {
        private readonly XboxLibrarySettings settings;

        public override bool IsInstalled => true;

        public override string Icon => Xbox.Icon;

        public XboxLibraryClient(XboxLibrarySettings settings)
        {
            this.settings = settings;
        }

        public override void Open()
        {
            if (Computer.WindowsVersion != WindowsVersion.Win10)
            {
                throw new Exception("Xbox game library is only supported on Windows 10.");
            }

            if (settings.XboxAppClientPriorityLaunch && Xbox.IsXboxPassAppInstalled)
            {
                Xbox.OpenXboxPassApp();
            }
            else
            {
                ProcessStarter.StartUrl("ms-windows-store://");
            }
        }
    }
}