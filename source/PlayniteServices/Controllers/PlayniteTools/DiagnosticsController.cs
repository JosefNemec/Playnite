using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Playnite;
using Playnite.Common;
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
        private AppSettings appSettings;

        public DiagnosticsController(IOptions<AppSettings> settings)
        {
            appSettings = settings.Value;
        }

        [HttpGet("{packageId}/{serviceKey}")]
        public IActionResult GetPackage(Guid packageId, string serviceKey)
        {
            if (appSettings.ServiceKey != serviceKey)
            {
                return BadRequest();
            }

            var diagFiles = Directory.GetFiles(Playnite.DiagsLocation, $"{packageId}.zip", SearchOption.AllDirectories);
            if (diagFiles.Length == 0)
            {
                return NotFound();
            }

            var diagFile = new FileInfo(diagFiles[0]);
            return PhysicalFile(diagFile.FullName, System.Net.Mime.MediaTypeNames.Application.Zip, diagFile.Name);
        }

        [HttpGet("{serviceKey}")]
        public ServicesResponse<List<string>> GetPackages(string serviceKey)
        {
            if (appSettings.ServiceKey != serviceKey)
            {
                return new ServicesResponse<List<string>>(null) { Error = "bad request" };
            }

            var diagFiles = Directory.
                GetFiles(Playnite.DiagsLocation, "*.zip", SearchOption.AllDirectories).
                Select(a => a.Replace(Playnite.DiagsLocation, "").Trim(Path.DirectorySeparatorChar) + $",{new FileInfo(a).CreationTime}").
                ToList();
            return new ServicesResponse<List<string>>(diagFiles);
        }

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
                var diagInfo = zip.GetEntry(DiagnosticPackageInfo.PackageInfoFileName);
                if (diagInfo != null)
                {
                    using (var infoStream = diagInfo.Open())
                    {
                        var info = Serialization.FromStream<DiagnosticPackageInfo>(infoStream);
                        version = info.PlayniteVersion;
                        isCrash = info.IsCrashPackage;
                    }
                }
                else
                {
                    var log = zip.GetEntry("playnite.log");
                    if (log != null)
                    {
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
                    }

                    var playniteInfo = zip.GetEntry("playniteInfo.txt");
                    if (playniteInfo != null)
                    {
                        using (var infoStream = playniteInfo.Open())
                        {
                            var info = Serialization.FromStream<Dictionary<string, object>>(infoStream);
                            version = info["Version"].ToString();
                        }
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
