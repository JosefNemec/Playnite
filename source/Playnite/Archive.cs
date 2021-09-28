using SharpCompress.Archives;
using System;
using System.Collections.Generic;
using System.IO;
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
            using (var archive = ArchiveFactory.Open(archivePath))
            {
                return archive.Entries.Where(a => !a.IsDirectory).Select(a => a.Key).ToList();
            }
        }

        public static Tuple<Stream, IDisposable> GetEntryStream(string archivePath, string entryName)
        {
            var archive = ArchiveFactory.Open(archivePath);
            var entry = archive.Entries.FirstOrDefault(a => a.Key == entryName);
            if (entry == null)
            {
                archive.Dispose();
                return null;
            }

            return new Tuple<Stream, IDisposable>(entry.OpenEntryStream(), archive);
        }
    }
}
