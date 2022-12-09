using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public DiagnosticPackageInfo()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }

        public DiagnosticPackageInfo(string playniteVersion, bool isCrash)
        {
            PlayniteVersion = playniteVersion;
            IsCrashPackage = isCrash;
        }
    }
}
