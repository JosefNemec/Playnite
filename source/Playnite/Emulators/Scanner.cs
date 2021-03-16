using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Playnite.Emulation
{
    public class PlatformDefinition2
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static string platformsFile => Path.Combine(PlaynitePaths.ProgramPath, "Emulation", "Platforms.yaml");

        public string Name { get; set; }
        public string Id { get; set; }
        public List<string> Databases { get; set; }
        public List<string> Emulators { get; set; }

        public static List<PlatformDefinition2> GetDefinitions()
        {
            if (File.Exists(platformsFile))
            {
                return Serialization.FromYamlFile<List<PlatformDefinition2>>(platformsFile);
            }
            else
            {
                return new List<PlatformDefinition2>();
            }
        }
    }

    public class EmulatorDefinition2Profile
    {
        public string Name { get; set; }
        public string DefaultArguments { get; set; }
        public List<string> Platforms { get; set; }
        public List<string> ImageExtensions { get; set; }
        public string ExecutableLookup { get; set; }
    }

    public class EmulatorDefinition2
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static string emulatorDefDir => Path.Combine(PlaynitePaths.ProgramPath, "Emulation", "Emulators");

        public string Name { get; set; }
        public string Website { get; set; }
        public List<EmulatorDefinition2Profile> Profiles { get; set; }

        [YamlIgnore]
        public string FileName { get; set; }

        public static List<EmulatorDefinition2> GetDefinitions()
        {
            var platforms = new List<EmulatorDefinition2>();
            if (!Directory.Exists(emulatorDefDir))
            {
                return platforms;
            }

            foreach (var file in Directory.GetFiles(emulatorDefDir, "*.yaml"))
            {
                try
                {
                    var data = Serialization.FromYamlFile<EmulatorDefinition2>(file);
                    data.FileName = Path.GetFileNameWithoutExtension(file);
                    platforms.Add(data);
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to load emulator definition file {file}");
                }
            }

            return platforms;
        }
    }

    public class Scanner
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static readonly string[] supportedArchiveExt = new string[] { "rar", "7z", "zip", "tar", "bzip2", "gzip", "lzip" };

        private static List<PlatformDefinition2> platforms;
        public static List<PlatformDefinition2> Platforms
        {
            get
            {
                if (platforms == null)
                {
                    platforms = PlatformDefinition2.GetDefinitions();
                }

                return platforms;
            }
        }

        private static List<EmulatorDefinition2> emulators;
        public static List<EmulatorDefinition2> Emulators
        {
            get
            {
                if (emulators == null)
                {
                    emulators = EmulatorDefinition2.GetDefinitions();
                }

                return emulators;
            }
        }

        private static bool IsSupportedArchiveExtension(string extension)
        {
            return supportedArchiveExt.ContainsString(extension, StringComparison.OrdinalIgnoreCase);
        }

        public static List<ScannedGame> ScanDirectory(string directory, EmulatorDefinition2Profile emuProfile, string databaseDir)
        {
            logger.Info($"Scanning emulated directory {directory}.");

            if (!Directory.Exists(directory))
            {
                logger.Error($"Can't scan emulation directory, {directory} doesn't exist.");
                return null;
            }

            if (!emuProfile.ImageExtensions.HasItems())
            {
                logger.Error($"Can't scan emulation directory, {emuProfile} doesn't have image extensions specified.");
                return null;
            }

            var roms = new Dictionary<string, List<ScannedRom>>();
            var supportedPlatforms = Platforms.Where(a => emuProfile.Platforms.Contains(a.Id)).ToList();
            var supportedDatabases = supportedPlatforms.SelectMany(a => a.Databases).Distinct().ToList();
            var emuDbs = new List<EmulationDatabase.EmulationDatabaseReader>();
            foreach (var supDb in supportedDatabases)
            {
                var db = EmulationDatabase.GetDatabase(supDb, databaseDir);
                if (db != null)
                {
                    emuDbs.Add(db);
                }
            }

            void addRom(ScannedRom rom)
            {
                if (roms.TryGetValue(rom.Name.SanitizedName, out var addedRoms))
                {
                    addedRoms.Add(rom);
                }
                else
                {
                    roms.Add(rom.Name.SanitizedName, new List<ScannedRom> { rom });
                }
            }

            var files = new SafeFileEnumerator(directory, "*.*", SearchOption.AllDirectories).ToList();
            foreach (var file in files)
            {
                if (file.Attributes.HasFlag(FileAttributes.Directory))
                {
                    continue;
                }

                var ext = file.Extension.TrimStart('.');
                if (!emuProfile.ImageExtensions.ContainsString(ext, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    if (emuDbs.HasItems())
                    {
                        DatGame datRec = null;
                        string datRecSource = null;
                        string crc = null;

                        if (IsSupportedArchiveExtension(ext))
                        {
                            var archFiles = Archive.GetArchiveFiles(file.FullName);
                            var supportedFile = archFiles.FirstOrDefault(a =>
                                emuProfile.ImageExtensions.ContainsString(Path.GetExtension(a).TrimStart('.')));
                            if (supportedFile != null)
                            {
                                logger.Trace($"Getting rom crc from archive file '{supportedFile}'\r\n {file.FullName}");
                                var streams = Archive.GetEntryStream(file.FullName, supportedFile);
                                if (streams != null)
                                {
                                    using (streams.Item2)
                                    using (streams.Item1)
                                    {
                                        crc = FileSystem.GetCRC32(streams.Item1);
                                    }
                                }
                            }

                            if (crc == null)
                            {
                                logger.Trace($"Failed to get crc info from archive: {file.FullName}");
                                crc = FileSystem.GetCRC32(file.FullName);
                            }
                        }
                        else
                        {
                            logger.Trace($"Getting rom crc from file: {file.FullName}");
                            crc = FileSystem.GetCRC32(file.FullName);
                        }

                        foreach (var db in emuDbs)
                        {
                            datRec = db.GetByCrc(crc).FirstOrDefault();
                            if (datRec != null)
                            {
                                datRecSource = db.DatabaseName;
                                break;
                            }
                        }

                        logger.Trace($"Detected rom with db info: {file.FullName}\r\n {datRec}");
                        addRom(new ScannedRom(file.FullName, datRec, datRecSource));
                    }
                    else
                    {
                        logger.Trace($"Detected rom: {file.FullName}");
                        addRom(new ScannedRom(file.FullName));
                    }
                }
                catch (Exception e) when(!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed scan rom file {file.FullName}");
                }
            }

            emuDbs.ForEach(a => a.Dispose());
            return roms.Select(a => new ScannedGame { Name = a.Key, Roms = a.Value }).ToList();
        }

        public static void CleanupScanResult(List<ScannedGame> scanList, EmulatorDefinition2Profile emuProfile)
        {
        }
    }

    public class ScannedGame
    {
        public List<ScannedRom> Roms { get; set; }
        public string Name { get; set; }
        public string PlatformId { get; set; }
    }

    public class ScannedRom
    {
        public DatGame DbData { get; set; }
        public RomName Name { get; set; }
        public string Path { get; set; }
        public string DbDataSource { get; set; }

        public ScannedRom(string path)
        {
            Path = path;
            Name = new RomName(System.IO.Path.GetFileNameWithoutExtension(path));
        }

        public ScannedRom(string path, DatGame dbData, string dbDataSource) : this(path)
        {
            DbData = dbData;
            DbDataSource = dbDataSource;
        }
    }

    public class RomName
    {
        private static readonly string propsMatchStr = @"\[(.*?)\]|\((.*?)\)";

        public string Name { get; set; }
        public string SanitizedName { get; set; }
        public string DiscName { get; set; }
        public List<string> Properties { get; set; } = new List<string>();

        public RomName(string originalName)
        {
            if (originalName.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(originalName));
            }

            Name = originalName;
            var matches = Regex.Matches(originalName, propsMatchStr);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    for (int i = 1; i < match.Groups.Count; i++)
                    {
                        if (!match.Groups[i].Value.IsNullOrEmpty())
                        {
                            Properties.Add(match.Groups[i].Value);
                        }
                    }
                }
            }

            DiscName = Properties.FirstOrDefault(a => a.StartsWith("disc", StringComparison.InvariantCultureIgnoreCase));
            SanitizedName = SanitizeName(originalName);
        }

        public static string SanitizeName(string name)
        {
            var newName = Regex.Replace(name, propsMatchStr, string.Empty);
            return newName.Replace('’', '\'').Trim();
        }
    }
}
