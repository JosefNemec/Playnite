using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.Playnite
{
    [Route("api/playnite/diag")]
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

            using (var fs = new FileStream(targetPath, FileMode.OpenOrCreate))
            {
                Request.Body.CopyTo(fs);
            }

            return new ServicesResponse<Guid>(packageId, string.Empty);
        }
    }
}
