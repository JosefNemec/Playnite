using System.IO;
using System.Text.RegularExpressions;

namespace Playnite;

public class CueSheet
{
    public class CueFileEntry
    {
        public string Path { get; }
        public string Type { get; }

        public CueFileEntry(string path, string type)
        {
            Path = path;
            Type = type;
        }
    }

    public static List<CueFileEntry> GetFileEntries(string cuePath)
    {
        var files = new List<CueFileEntry>();
        foreach (var line in File.ReadAllLines(cuePath))
        {
            if (!line.IsNullOrWhiteSpace())
            {
                var match = Regex.Match(line, @"FILE\s+""(.+)""\s+(.+)$");
                if (match.Success)
                {
                    files.Add(new CueFileEntry(match.Groups[1].Value, match.Groups[2].Value));
                }
            }
        }

        return files;
    }
}
