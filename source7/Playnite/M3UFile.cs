using System.IO;

namespace Playnite;

public class M3UFile
{
    public class M3UEntry
    {
        public Dictionary<string, string> Extensions { get; set; } = new Dictionary<string, string>();
        public string Path { get; set; } = string.Empty;

        public override string ToString()
        {
            return Path;
        }
    }

    public static List<M3UEntry> GetEntries(string filePath)
    {
        var entries = new List<M3UEntry>();
        var currentEntry = new M3UEntry();
        foreach (var line in File.ReadAllLines(filePath))
        {
            if (line.IsNullOrWhiteSpace() || line == "#EXTM3U")
            {
                continue;
            }

            if (line.StartsWith("#", StringComparison.Ordinal))
            {
                var sep = line.IndexOf(':', StringComparison.Ordinal);
                currentEntry.Extensions.AddOrUpdate(line.Substring(0, sep), line.Substring(sep + 1));
            }
            else
            {
                currentEntry.Path = line;
                entries.Add(currentEntry);
                currentEntry = new M3UEntry();
            }
        }

        return entries;
    }
}
