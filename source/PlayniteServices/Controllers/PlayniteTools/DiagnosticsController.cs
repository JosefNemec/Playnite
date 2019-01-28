using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.PlayniteTools
{
    [Route("playnite/diag")]
    public class DiagnosticsController : Controller
    {
        [HttpPost]
        public ServicesResponse<Guid> UploadPackage()
        {
            var packageId = Guid.NewGuid();
            var targetPath = Path.Combine(Playnite.DiagsLocation, $"{packageId}.zip");
            if (!Directory.Exists(Playnite.DiagsLocation))
            {
                Directory.CreateDirectory(Playnite.DiagsLocation);
            }

            if (!Directory.Exists(Playnite.DiagsCrashLocation))
            {
                Directory.CreateDirectory(Playnite.DiagsCrashLocation);
            }

            using (var fs = new FileStream(targetPath, FileMode.OpenOrCreate))
            {
                Request.Body.CopyTo(fs);
            }

            var isCrash = false;
            var version = string.Empty;

            using (var zip = ZipFile.OpenRead(targetPath))
            {
                var log = zip.GetEntry("playnite.log");
                using (var logStream = log.Open())
                {
                    using (var tr = new StreamReader(logStream))
                    {
                        while (!tr.EndOfStream)
                        {
                            var line = tr.ReadLine();
                            if (line.Contains("Unhandled exception"))
                            {
                                isCrash = true;
                                break;
                            }
                        }
                    }
                }

                var playniteInfo = zip.GetEntry("playniteInfo.txt");
                using (var infoStream = playniteInfo.Open())
                {
                    using (var tr = new StreamReader(infoStream))
                    {
                        var verLine = tr.ReadLine();
                        version = verLine.Split(':')[1].Trim();
                    }
                }

            }

            if (isCrash)
            {
                var dir = Playnite.DiagsCrashLocation;
                if (!string.IsNullOrEmpty(version))
                {
                    dir = Path.Combine(dir, version);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                }

                var newPath = Path.Combine(dir, Path.GetFileName(targetPath));
                System.IO.File.Move(targetPath, newPath);
            }

            return new ServicesResponse<Guid>(packageId);
        }
    }
}
