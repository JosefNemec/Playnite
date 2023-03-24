using System.IO;
using System.Text.RegularExpressions;

namespace Playnite;

// gdi media descriptor file used mostly in Dreamcast dumps
public class GdiFile
{
    public class GdiEntry
    {
        public string Path { get; set; }

        public GdiEntry(string path)
        {
            Path = path;
        }

        public override string ToString()
        {
            return Path;
        }
    }

    public static List<GdiEntry> GetEntries(string filePath)
    {
        var entries = new List<GdiEntry>();
        // Apparently there are some scuffed "big" gdi files, no idea what they are,
        // but to be sure lets ignore files larged than 10kB
        if (new FileInfo(filePath).Length > 10240)
        {
            return entries;
        }

        foreach (var line in File.ReadAllLines(filePath).Skip(1))
        {
            var match = Regex.Match(line, @"^\d+\s+\d+\s+\d+\s+\d+\s+(.+)\s+\d+$");
            if (!match.Success)
            {
                continue;
            }

            entries.Add(new GdiEntry(match.Groups[1].Value.Trim('"')));
        }

        return entries;
    }
}
