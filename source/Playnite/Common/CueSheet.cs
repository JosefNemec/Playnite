using Playnite.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Playnite
{
    public class CueSheet
    {
        public struct CueFileEntry
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
            foreach (var line in File.ReadAllLines(Paths.FixPathLength(cuePath)))
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
}
