using Playnite.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class M3U
    {
        public class M3UEntry
        {
            public Dictionary<string, string> Extensions { get; set; } = new Dictionary<string, string>();
            public string Path { get; set; }

            public override string ToString()
            {
                return Path ?? base.ToString();
            }
        }

        // Technically # is also supported for in zip file references, but it's also valid file path character
        // so we'll just roll without it for now. It would need an actual zip entry validation that we can't do here.
        private static readonly char[] retroArchExtensionSeparators = new char[] { ':', '|' };

        public static List<M3UEntry> GetEntries(string filePath)
        {
            var entries = new List<M3UEntry>();
            var currentEntry = new M3UEntry();
            foreach (var line in File.ReadAllLines(Paths.FixPathLength(filePath)))
            {
                if (line.IsNullOrWhiteSpace() || line == "#EXTM3U")
                {
                    continue;
                }

                if (line.StartsWith("#"))
                {
                    var sep = line.IndexOf(':');
                    currentEntry.Extensions.AddOrUpdate(line.Substring(0, sep), line.Substring(sep + 1));
                }
                else
                {
                    // https://github.com/JosefNemec/Playnite/issues/4267
                    // Parse out Retroarch stuff they attach to the actual path instead of making it m3u extension...
                    if (line.ContainsAny(retroArchExtensionSeparators))
                    {
                        var split = line.Split(retroArchExtensionSeparators);
                        currentEntry.Path = split[0];
                        currentEntry.Extensions.Add("RA", split[1]);
                    }
                    else
                    {
                        currentEntry.Path = line; 
                    }

                    entries.Add(currentEntry);
                    currentEntry = new M3UEntry();
                }
            }

            return entries;
        }
    }
}
