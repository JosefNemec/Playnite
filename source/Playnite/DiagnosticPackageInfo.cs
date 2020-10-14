using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class DiagnosticPackageInfo
    {
        public static readonly string PackageInfoFileName = "packageInfo.txt";

        public string PlayniteVersion { get; set; }
        public bool IsCrashPackage { get; set; } = false;
    }
}
