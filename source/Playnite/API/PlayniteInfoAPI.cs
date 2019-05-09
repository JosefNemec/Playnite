using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.API
{
    public class PlayniteInfoAPI : IPlayniteInfoAPI
    {
        public System.Version ApplicationVersion { get => Updater.GetCurrentVersion(); }

        public ApplicationMode Mode => PlayniteApplication.Current.Mode;

        public bool IsPortable => PlayniteSettings.IsPortable;

        public bool InOfflineMode => PlayniteEnvironment.InOfflineMode;

        public bool IsDebugBuild => PlayniteEnvironment.IsDebugBuild;

        public bool ThrowAllErrors => PlayniteEnvironment.ThrowAllErrors;
    }
}
