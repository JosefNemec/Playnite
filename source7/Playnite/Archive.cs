using SharpCompress.Archives;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Playnite;

public static class Archive
{
    public static List<string> GetArchiveFiles(string archivePath)
    {
        using (var archive = ArchiveFactory.Open(archivePath))
        {
            return archive.Entries.Where(a => !a.IsDirectory).Select(a => a.Key).ToList();
        }
    }

    public static Tuple<Stream, IDisposable>? GetEntryStream(string archivePath, string entryName)
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

    public static void CreateEntryFromDirectory(this ZipArchive archive, string directory, string entryName, CancellationToken cancelToken)
    {
        if (cancelToken.IsCancellationRequested)
        {
            return;
        }

        foreach (var file in Directory.GetFiles(directory))
        {
            if (cancelToken.IsCancellationRequested)
            {
                return;
            }

            archive.CreateEntryFromFile(file, Path.Combine(entryName, Path.GetFileName(file)));
        }

        if (cancelToken.IsCancellationRequested)
        {
            return;
        }

        foreach (var dir in Directory.GetDirectories(directory))
        {
            if (cancelToken.IsCancellationRequested)
            {
                return;
            }

            CreateEntryFromDirectory(archive, dir, Path.Combine(entryName, Path.GetFileName(dir)), cancelToken);
        }
    }
}
