using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class Archive
    {
        public static List<string> GetArchiveFiles(string archivePath)
        {
            using (var zip = ZipFile.OpenRead(archivePath))
            {
                return zip.Entries.Select(a => a.FullName).ToList();
            }
        }
    }
}
