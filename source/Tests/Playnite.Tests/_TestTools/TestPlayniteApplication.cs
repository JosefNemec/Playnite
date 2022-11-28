using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Playnite.API;
using Playnite.SDK;

namespace Playnite.Tests
{
    public class TestPlayniteApplication : PlayniteApplication
    {
        public TestPlayniteApplication()
        {
        }

        public override PlayniteAPI GetApiInstance(ExtensionManifest pluginOwner)
        {
            throw new NotImplementedException();
        }

        public override PlayniteAPI GetApiInstance()
        {
            throw new NotImplementedException();
        }

        public override void InitializeNative()
        {
            throw new NotImplementedException();
        }

        public override void InstantiateApp()
        {
            throw new NotImplementedException();
        }

        public override void Minimize()
        {
            throw new NotImplementedException();
        }

        public override void Restart(bool saveSettings)
        {
            throw new NotImplementedException();
        }

        public override void Restart(CmdLineOptions options, bool saveSettings)
        {
            throw new NotImplementedException();
        }

        public override void Restore()
        {
            throw new NotImplementedException();
        }

        public override void ShowWindowsNotification(string title, string body, Action action)
        {
            throw new NotImplementedException();
        }

        public override bool Startup()
        {
            throw new NotImplementedException();
        }

        public override void SwitchAppMode(ApplicationMode mode)
        {
            throw new NotImplementedException();
        }

        public override void ConfigureViews()
        {
            throw new NotImplementedException();
        }
    }
}
