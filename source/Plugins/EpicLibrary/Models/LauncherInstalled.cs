using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicLibrary.Models
{
    public class LauncherInstalled
    {
        public class InstalledApp
        {
            public string InstallLocation;
            public string AppName;
            public long AppID;
            public string AppVersion;
        }

        public List<InstalledApp> InstallationList;
    }
}
