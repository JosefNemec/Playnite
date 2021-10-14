using Playnite.Common;
using Playnite.Emulators;
using SqlNado;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Utilities
{
    public class DatParser
    {
        enum Token
        {
            None,
            Property,
            PropertyValue,
            PropertyStringValue
        }

        public class DatProperty
        {
            public string Name { get; set; }
            public object Value { get; set; }

            public override string ToString()
            {
                return $"{Name} : {Value}";
            }
        }

        public class DatObject
        {
            public string Name { get; set; }
            public List<DatProperty> Properties { get; set; } = new List<DatProperty>();

            public DatObject()
            {
            }

            public DatObject(string name)
            {
                Name = name;
            }
        }

        readonly Dictionary<string, PropertyInfo> datGameProperties = new Dictionary<string, PropertyInfo>();

        public int ReadIndex { get; set; } = 0;
        public char[] Content { get; set; }
        public int CurrentLine { get; set; } = 1;
        public int CurrentLinePos { get; set; } = 1;

        public DatParser()
        {
            foreach (var prop in typeof(DatGame).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.GetCustomAttribute(typeof(DatPropertyAttribute)) is DatPropertyAttribute datAtt)
                {
                    datGameProperties.Add(datAtt.Name, prop);
                }
            }
        }

        private DatGame ParseGame(DatObject datObj)
        {
            if (datObj == null)
            {
                throw new ArgumentNullException("Can't parse game object, no object given.");
            }

            var datGame = new DatGame();
            foreach (var prop in datObj.Properties)
            {
                var romAdded = false;
                if (prop.Name == "rom" && !romAdded)
                {
                    romAdded = true;
                    foreach (var romDatProp in (prop.Value as DatObject).Properties)
                    {
                        if (datGameProperties.TryGetValue("rom." + romDatProp.Name, out var romPropInfo))
                        {
                            SetParsedProperty(romPropInfo, datGame, (string)romDatProp.Value);
                        }
                    }
                }
                else if (datGameProperties.TryGetValue(prop.Name, out var propInfo))
                {
                    if (prop.Value is DatObject)
                    {
                        throw new Exception("Can't assign object to uknown dat property.");
                    }

                    SetParsedProperty(propInfo, datGame, (string)prop.Value);
                }
            }

            datGame.FixData();
            return datGame;
        }

        private void SetParsedProperty(PropertyInfo prop, object datGame, string propValue)
        {
            if (prop.PropertyType == typeof(uint))
            {
                prop.SetValue(datGame, uint.Parse(propValue));
            }
            else if (prop.PropertyType == typeof(int))
            {
                prop.SetValue(datGame, int.Parse(propValue));
            }
            else if (prop.PropertyType == typeof(long))
            {
                prop.SetValue(datGame, long.Parse(propValue));
            }
            else if (prop.PropertyType == typeof(string))
            {
                prop.SetValue(datGame, propValue);
            }
            else
            {
                throw new Exception("Uknown target type.");
            }
        }

        private DatProperty ParseProperty()
        {
            var datProperty = new DatProperty();
            var propName = string.Empty;
            var propValue = string.Empty;
            Token curToken = Token.Property;
            while (curToken != Token.None)
            {
                if (ReadIndex >= Content.Length)
                {
                    break;
                }

                var chr = Content[ReadIndex];
                ReadIndex++;
                CurrentLinePos++;
                var whiteSpace = char.IsWhiteSpace(chr);

                if (whiteSpace && curToken == Token.Property)
                {
                    curToken = Token.PropertyValue;
                    datProperty.Name = propName;
                    continue;
                }

                if (!whiteSpace && curToken == Token.Property)
                {
                    propName += chr;
                    continue;
                }

                if (chr == '"' && curToken == Token.PropertyValue)
                {
                    curToken = Token.PropertyStringValue;
                    continue;
                }

                if (chr == '"' && curToken == Token.PropertyStringValue)
                {
                    datProperty.Value = propValue;
                    curToken = Token.None;
                    continue;
                }

                if (whiteSpace && curToken == Token.PropertyStringValue ||
                    !whiteSpace && curToken == Token.PropertyStringValue)
                {
                    propValue += chr;
                    continue;
                }

                if (chr == '(')
                {
                    datProperty.Value = ParseObject();
                    break;
                }

                if (!whiteSpace && curToken == Token.PropertyValue)
                {
                    propValue += chr;
                    continue;
                }

                if (curToken == Token.PropertyValue && (whiteSpace || chr == ')'))
                {
                    datProperty.Value = propValue;
                    curToken = Token.None;
                    continue;
                }

                break;
            }

            return datProperty;
        }

        private DatObject ParseObject()
        {
            var datObject = new DatObject();
            Token curToken = Token.Property;
            while (curToken != Token.None)
            {
                if (ReadIndex >= Content.Length)
                {
                    break;
                }

                var chr = Content[ReadIndex];
                ReadIndex++;
                CurrentLinePos++;
                if (chr == '\n')
                {
                    CurrentLinePos = 0;
                    CurrentLine++;
                    continue;
                }

                if (chr == '\r')
                {
                    continue;
                }

                var whiteSpace = char.IsWhiteSpace(chr);
                if (whiteSpace)
                {
                    continue;
                }

                if (chr == ')')
                {
                    break;
                }

                ReadIndex--;
                datObject.Properties.Add(ParseProperty());
            }

            return datObject;
        }

        public static List<DatGame> ConsolidateDatFiles(IEnumerable<string> datFiles, Func<DatGame, string> groupSelector)
        {
            var parsedDatFiles = datFiles.AsParallel().SelectMany(a => new DatParser().ParseFile(a)).ToList();
            var consolidatedGames = new List<DatGame>();
            foreach (var group in parsedDatFiles.GroupBy(groupSelector, StringComparer.OrdinalIgnoreCase))
            {
                if (group.Count() == 1)
                {
                    consolidatedGames.Add(group.First());
                }
                else
                {
                    var baseObj = group.First();
                    foreach (var obj in group)
                    {
                        if (baseObj != obj)
                        {
                            obj.CopyTo(baseObj);
                        }
                    }

                    consolidatedGames.Add(baseObj);
                }
            }

            return consolidatedGames;
        }

        public List<DatGame> ParseFile(string path)
        {
            ReadIndex = 0;
            CurrentLine = 1;
            CurrentLinePos = 1;
            Content = File.ReadAllText(path, Encoding.UTF8).ToCharArray();
            var root = ParseObject();
            var games = new List<DatGame>(root.Properties.Count);
            foreach (var child in root.Properties)
            {
                if (child.Name == "game")
                {
                    try
                    {
                        games.Add(ParseGame(child.Value as DatObject));
                    }
                    catch
                    {
                        Console.WriteLine($"Failed top parse game child in {path}");
                    }
                }
            }

            return games;
        }

        public static void ProcessLibretroDb(string libretroDir, string outputDir, bool generateSql, bool generateYaml)
        {
            FileSystem.CreateDirectory(outputDir);
            Func<DatGame, string> libretroDbDefaultGroupSelectors = (g) => g.RomCrc;
            var libretroDbGroupSelectors = new Dictionary<string, Func<DatGame, string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "Sony - PlayStation", (g) => g.Serial },
                { "Sony - PlayStation 3", (g) => g.Serial },
                { "Nintendo - GameCube", (g) => g.Serial },
                { "Nintendo - Wii", (g) => g.Serial },
                { "Sega - Dreamcast", (g) => g.Serial },
                { "Sega - Mega-CD - Sega CD", (g) => g.Serial },
                { "Sega - Saturn", (g) => g.Serial },
                { "Sony - PlayStation 2", (g) => g.Serial },
                { "Sony - PlayStation Portable", (g) => g.Serial },
                { "Sony - PlayStation Vita", (g) => g.Serial },
                { "Nintendo - Wii U", (g) => g.Serial }
            };

            var datDirs = new List<string>
            {
                Path.Combine(libretroDir, @"dat"),
                Path.Combine(libretroDir, @"metadat\no-intro"),
                Path.Combine(libretroDir, @"metadat\redump"),
                Path.Combine(libretroDir, @"metadat\libretro-dats"),
                Path.Combine(libretroDir, @"metadat\fbneo-split"),
                Path.Combine(libretroDir, @"metadat\mame"),
                Path.Combine(libretroDir, @"metadat\mame-member"),
                Path.Combine(libretroDir, @"metadat\mame-split"),
                Path.Combine(libretroDir, @"metadat\mame-nonmerged"),
                Path.Combine(libretroDir, @"metadat\homebrew"),
                Path.Combine(libretroDir, @"metadat\hacks"),
                Path.Combine(libretroDir, @"metadat\headered"),
                Path.Combine(libretroDir, @"metadat\serial"),
                Path.Combine(libretroDir, @"metadat\releaseyear"),
                Path.Combine(libretroDir, @"metadat\origin")
            };

            var ignoreList = new List<string>
            {
                "MAME 2000 XML.dat",
                "MAME BIOS.dat",
                "FBNeo_romcenter.dat",
                "System.dat",
                "MAME 2000 BIOS.dat",
                "Cannonball.dat",
                "Cave Story.dat",
                "Dinothawr.dat",
                "DOOM.dat",
                "Flashback.dat",
                "ChaiLove.dat",
                "LeapFrog - LeapPad.dat",
                "Lutro.dat",
                "Microsoft - Xbox One.dat",
                "Mobile - J2ME.dat",
                "Mobile - Palm OS.dat",
                "Mobile - Symbian.dat",
                "Mobile - Zeebo.dat",
                "MrBoom.dat",
                "Quake.dat",
                "Quake II.dat",
                "Quake III.dat",
                "Rick Dangerous.dat",
                "RPG Maker.dat",
                "Sony - PlayStation 4.dat",
                "Sony - PlayStation Portable (UMD Music).dat",
                "Sony - PlayStation Portable (UMD Video).dat",
                "Tiger - Game.com.dat",
                "Tiger - Gizmondo.dat",
                "Tomb Raider.dat",
                "Wolfenstein 3D.dat",
                "Microsoft - XBOX 360 (Title Updates).dat",
                "HBMAME.dat",
                "MAME 2000.dat",
                "MAME 2003.dat",
                "MAME 2003-Plus.dat",
                "MAME 2010.dat",
                "MAME 2015.dat",
                "MAME 2016.dat",
                "MAME.dat",
                "ScummVM.dat",
                "Atomiswave.dat",
                "Sony - PlayStation Minis.dat",
                "Thomson - MOTO.dat"
            };

            var datFiles = new List<FileInfo>();
            datDirs.ForEach(d => datFiles.AddRange(
                Directory.GetFiles(d, "*.dat", SearchOption.AllDirectories).Where(f => !ignoreList.Contains(Path.GetFileName(f), StringComparer.OrdinalIgnoreCase)).
                Select(f => new FileInfo(f))));
            var databases = datFiles.GroupBy(a => a.Name, StringComparer.OrdinalIgnoreCase);
            Parallel.ForEach(
                databases,
                (db, _, __) =>
                {
                    var dbName = Path.GetFileNameWithoutExtension(db.Key);
                    var selector = libretroDbDefaultGroupSelectors;
                    if (libretroDbGroupSelectors.TryGetValue(dbName, out var sel))
                    {
                        selector = sel;
                    }

                    var cons = DatParser.ConsolidateDatFiles(db.Select(a => a.FullName), selector);
                    if (dbName == "Sony - PlayStation 3")
                    {
                        string ps3ResgionCharToRegion(char regionKey)
                        {
                            switch (regionKey)
                            {
                                case 'A': return "Asia";
                                case 'C': return "China";
                                case 'E': return "Europe";
                                case 'H': return "Hong Kong";
                                case 'J': return "Japan";
                                case 'K': return "Korea";
                                case 'P': return "Japan";
                                case 'U': return "USA";
                                default: return null;
                            }
                        }

                        foreach (var game in cons)
                        {
                            if (game.Region.IsNullOrEmpty() && !game.Serial.IsNullOrEmpty())
                            {
                                game.Region = ps3ResgionCharToRegion(game.Serial[2]);
                            }
                        }
                    }

                    if (generateYaml)
                    {
                        var yamlPath = Path.Combine(outputDir, $"{dbName}.yaml");
                        FileSystem.DeleteFile(yamlPath);
                        File.WriteAllText(
                            yamlPath,
                            Serialization.ToYaml(cons),
                            Encoding.UTF8);
                    }

                    if (generateSql)
                    {
                        var dbPath = Path.Combine(outputDir, $"{dbName}.db");
                        FileSystem.DeleteFile(dbPath);
                        using (var sqlDb = new SQLiteDatabase(dbPath))
                        {
                            sqlDb.ExecuteNonQuery("PRAGMA journal_mode = OFF;");
                            sqlDb.Save(cons);
                        }
                    }
                });
        }
    }
}
