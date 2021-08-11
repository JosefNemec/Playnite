using Playnite.Common;
using Playnite.Database;
using Playnite.Scripting.PowerShell;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Playnite.Emulators
{
    public class ScannedEmulator : ObservableObject
    {
        public class ScannedEmulatorProfile : ObservableObject
        {
            private bool import = true;
            public bool Import
            {
                get => import;
                set
                {
                    import = value;
                    OnPropertyChanged();
                }
            }

            public string Name { get; set; }
            public string ProfileName { get; set; }
        }

        private bool import = true;
        public bool Import
        {
            get => import;
            set
            {
                import = value;
                OnPropertyChanged();
            }
        }

        public string Name { get; set; }
        public string Id { get; set; }
        public string InstallDir { get; set; }
        public List<ScannedEmulatorProfile> Profiles { get; set; }
    }

    public class EmulatorScanner
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        public static List<ScannedEmulator> SearchForEmulators(string path, IList<EmulatorDefinition> definitions, CancellationToken cancelToken)
        {
            logger.Info($"Looking for emulators in {path}, using {definitions.Count} definitions.");
            var imported = new Dictionary<string, ScannedEmulator>();
            foreach (var file in new SafeFileEnumerator(path, "*", SearchOption.AllDirectories))
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return new List<ScannedEmulator>();
                }

                if (file.Attributes.HasFlag(FileAttributes.Directory))
                {
                    continue;
                }

                foreach (var definition in definitions)
                {
                    var currentDir = Path.GetDirectoryName(file.FullName);
                    var importId = definition.Id + currentDir;
                    foreach (var defProfile in definition.Profiles)
                    {
                        if (defProfile.InstallationFile.IsNullOrEmpty())
                        {
                            continue;
                        }

                        var reqMet = true;
                        var regex = new Regex(defProfile.InstallationFile, RegexOptions.IgnoreCase);
                        if (regex.IsMatch(file.Name))
                        {
                            if (defProfile.ProfileFiles?.Any() == true)
                            {
                                foreach (var reqFile in defProfile.ProfileFiles)
                                {
                                    if (!File.Exists(Path.Combine(currentDir, reqFile)))
                                    {
                                        reqMet = false;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            reqMet = false;
                        }

                        if (reqMet)
                        {
                            imported.TryGetValue(importId, out var currentEmulator);
                            if (currentEmulator == null)
                            {
                                currentEmulator = new ScannedEmulator()
                                {
                                    Name = definition.Name,
                                    InstallDir = currentDir,
                                    Id = definition.Id,
                                    Profiles = new List<ScannedEmulator.ScannedEmulatorProfile>()
                                };

                                imported.Add(importId, currentEmulator);
                            }

                            currentEmulator.Profiles.Add(new ScannedEmulator.ScannedEmulatorProfile
                            {
                                ProfileName = defProfile.Name,
                                Name = defProfile.Name
                            });
                        }
                    }
                }
            }

            return imported.Values.ToList();
        }
    }

    public class GameScanner
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static readonly string[] supportedArchiveExt = new string[] { "rar", "7z", "zip", "tar", "bzip2", "gzip", "lzip" };

        private static bool IsSupportedArchiveExtension(string extension)
        {
            return supportedArchiveExt.ContainsString(extension, StringComparison.OrdinalIgnoreCase);
        }

        public static List<ScannedGame> Scan(
            GameScannerConfig scanner,
            GameDatabase database,
            List<string> importedFiles,
            CancellationToken cancelToken,
            List<Platform> newPlatforms,
            List<Region> newRegions)
        {
            List<ScannedGame> games;
            var emulator = database.Emulators[scanner.EmulatorId];
            if (emulator == null)
            {
                throw new Exception("Emulator not found.");
            }

            var customProfile = emulator.CustomProfiles?.FirstOrDefault(a => a.Id == scanner.EmulatorProfileId);
            var builtinProfile = emulator.BuiltinProfiles?.FirstOrDefault(a => a.Id == scanner.EmulatorProfileId);
            var builtinProfileDef = EmulatorDefinition.GetProfile(emulator.BuiltInConfigId, builtinProfile.BuiltInProfileName);
            if (scanner.EmulatorProfileId.StartsWith(CustomEmulatorProfile.ProfilePrefix))
            {
                games = ScanDirectory(
                    scanner.Directory,
                    emulator,
                    customProfile,
                    database,
                    importedFiles,
                    cancelToken);
            }
            else if (scanner.EmulatorProfileId.StartsWith(BuiltInEmulatorProfile.ProfilePrefix))
            {
                games = ScanDirectory(
                    scanner.Directory,
                    emulator,
                    builtinProfile,
                    importedFiles,
                    cancelToken);
            }
            else
            {
                throw new Exception("Emulator profile format not supported.");
            }

            foreach (var game in games)
            {
                game.SourceConfig = scanner;
                var assignedRegions = new List<EmulatedRegion>();
                var assignedPlatforms = new List<EmulatedPlatform>();
                foreach (var rom in game.Roms)
                {
                    // REGIONS
                    if (rom.DbData?.Region.IsNullOrEmpty() == false)
                    {
                        var region = Emulation.GetRegionByCode(rom.DbData.Region);
                        if (region != null)
                        {
                            assignedRegions.AddMissing(region);
                        }
                    }
                    else if (rom.Name.Properties.HasItems())
                    {
                        foreach (var prop in rom.Name.Properties)
                        {
                            var region = Emulation.GetRegionByCode(prop);
                            if (region != null)
                            {
                                assignedRegions.AddMissing(region);
                                break;
                            }
                        }
                    }

                    // PLATFORMS
                    if (rom.DbData != null)
                    {
                        var platform = Emulation.GetPlatformByDatabase(rom.DbDataSource);
                        if (platform != null)
                        {
                            assignedPlatforms.AddMissing(platform);
                        }
                    }
                    else if (builtinProfile != null)
                    {
                        assignedPlatforms.AddMissing(Emulation.GetPlatform(builtinProfileDef.Platforms.First()));
                    }
                }

                game.Regions = new List<Region>();
                foreach (var asRegion in assignedRegions)
                {
                    var dbRegion = database.Regions.FirstOrDefault(a => a.SpecificationId == asRegion.Id);
                    if (dbRegion != null)
                    {
                        game.Regions.Add(dbRegion);
                    }
                    else
                    {
                        var generatedReg = newRegions.FirstOrDefault(a => a.SpecificationId == asRegion.Id);
                        if (generatedReg == null)
                        {
                            var newReg = new Region(asRegion.Name) { SpecificationId = asRegion.Id };
                            newRegions.Add(newReg);
                            game.Regions.Add(newReg);
                        }
                        else
                        {
                            game.Regions.Add(generatedReg);
                        }
                    }
                }

                game.Platforms = new List<Platform>();
                if (builtinProfile != null)
                {
                    foreach (var asPlatform in assignedPlatforms)
                    {
                        var dbPlatform = database.Platforms.FirstOrDefault(a => a.SpecificationId == asPlatform.Id);
                        if (dbPlatform != null)
                        {
                            game.Platforms.Add(dbPlatform);
                        }
                        else
                        {
                            var generatedPlat = newPlatforms.FirstOrDefault(a => a.SpecificationId == asPlatform.Id);
                            if (generatedPlat == null)
                            {
                                var newPlat = new Platform(asPlatform.Name) { SpecificationId = asPlatform.Id };
                                newPlatforms.Add(newPlat);
                                game.Platforms.Add(newPlat);
                            }
                            else
                            {
                                game.Platforms.Add(generatedPlat);
                            }
                        }
                    }
                }
                else if (customProfile.Platforms.HasItems())
                {
                    foreach (var asPlatform in customProfile.Platforms)
                    {
                        var dbPlatform = database.Platforms[asPlatform];
                        if (dbPlatform != null)
                        {
                            game.Platforms.Add(dbPlatform);
                        }
                    }
                }
            }

            return games;
        }

        private static List<ScannedGame> ScanDirectory(
            string directory,
            Emulator emulator,
            BuiltInEmulatorProfile profile,
            List<string> importedFiles,
            CancellationToken cancelToken)
        {
            var emuProf = EmulatorDefinition.GetProfile(emulator.BuiltInConfigId, profile.BuiltInProfileName);
            if (emuProf == null)
            {
                throw new Exception($"Emulator {emulator.BuiltInConfigId} and profile {profile.BuiltInProfileName} not found.");
            }

            if (emuProf.ScriptGameImport)
            {
                object scannedGames = null;
                Exception failExc = null;
                var importRuntime = new PowerShellRuntime("Emu game import");
                var scriptTask = Task.Run(() =>
                {
                    try
                    {
                        scannedGames = importRuntime.ExecuteFile(
                            EmulatorDefinition.GetDefition(emulator.BuiltInConfigId).GameImportScriptPath,
                            emulator.InstallDir,
                            new Dictionary<string, object>
                            {
                                { "CancelToken", cancelToken },
                                { "Emulator", emulator },
                                { "EmulatorProfile", emuProf },
                                { "ScanDirectory", directory },
                                { "PlayniteApi", SDK.API.Instance },
                                { "ImportedFiles", importedFiles }
                            });
                    }
                    catch (Exception e)
                    {
                        failExc = e;
                        logger.Error(e, "Failed to scan directory using emulator.");
                    }
                    finally
                    {
                        importRuntime.Dispose();
                    }
                });

                while (true)
                {
                    Thread.Sleep(200);
                    if (cancelToken.IsCancellationRequested)
                    {
                        scriptTask.Wait(5000);
                        if (!importRuntime.IsDisposed)
                        {
                            importRuntime.Dispose();
                        }

                        return new List<ScannedGame>();
                    }

                    if (failExc != null)
                    {
                        throw failExc;
                    }

                    if (scriptTask.IsCompleted)
                    {
                        if (scannedGames is ScannedGame game)
                        {
                            return new List<ScannedGame> { game };
                        }
                        else if (scannedGames is List<ScannedGame> games)
                        {
                            return games;
                        }
                        else if (scannedGames == null)
                        {
                            return new List<ScannedGame>();
                        }
                        else
                        {
                            logger.Error($"Scanning script returned unknown data type {scannedGames.GetType()}");
                            return new List<ScannedGame>();
                        }
                    }

                    if (scriptTask.IsCanceled || scriptTask.IsFaulted)
                    {
                        return new List<ScannedGame>();
                    }
                }
            }
            else
            {
                return ScanDirectory(
                    directory,
                    emuProf.ImageExtensions,
                    emuProf.Platforms,
                    PlaynitePaths.EmulationDatabasePath,
                    importedFiles,
                    cancelToken);
            }
        }

        private static List<ScannedGame> ScanDirectory(
            string directory,
            Emulator emulator,
            CustomEmulatorProfile profile,
            GameDatabase database,
            List<string> importedFiles,
            CancellationToken cancelToken)
        {
            if (profile == null)
            {
                throw new Exception($"No profile provided.");
            }

            if (!profile.ImageExtensions.HasItems())
            {
                return new List<ScannedGame>();
            }

            var platforms = profile.Platforms?.Select(a => database.Platforms[a].SpecificationId).Where(a => !a.IsNullOrEmpty()).ToList();
            return ScanDirectory(
                directory,
                profile.ImageExtensions,
                platforms,
                PlaynitePaths.EmulationDatabasePath,
                importedFiles,
                cancelToken);
        }

        public static List<ScannedGame> ScanDirectory(
            string directory,
            List<string> supportedExtensions,
            List<string> scanPlatforms,
            string databaseDir,
            List<string> importedFiles,
            CancellationToken cancelToken)
        {
            logger.Info($"Scanning emulated directory {directory}.");
            if (!Directory.Exists(directory))
            {
                throw new Exception($"Can't scan emulation directory, {directory} doesn't exist.");
            }

            var supportedPlatforms = Emulation.Platforms.Where(a => scanPlatforms?.Contains(a.Id) == true).ToList();
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

            var resultRoms = new Dictionary<string, List<ScannedRom>>();

            try
            {
                ScanDirectoryBase(
                    directory,
                    supportedExtensions,
                    emuDbs,
                    importedFiles,
                    resultRoms,
                    cancelToken);
            }
            finally
            {
                emuDbs.ForEach(a => a.Dispose());
            }

            return resultRoms.Select(a => new ScannedGame { Name = a.Key, Roms = a.Value?.ToObservable() }).ToList();
        }

        private static void ScanDirectoryBase(
            string directory,
            List<string> supportedExtensions,
            List<EmulationDatabase.EmulationDatabaseReader> databases,
            List<string> importedFiles,
            Dictionary<string, List<ScannedRom>> resultRoms,
            CancellationToken cancelToken)
        {
            void addRom(ScannedRom rom)
            {
                if (resultRoms.TryGetValue(rom.Name.SanitizedName, out var addedRoms))
                {
                    addedRoms.Add(rom);
                }
                else
                {
                    resultRoms.Add(rom.Name.SanitizedName, new List<ScannedRom> { rom });
                }
            }

            List<string> files;
            string[] dirs;
            try
            {
                files = Directory.GetFiles(directory).ToList();
                dirs = Directory.GetDirectories(directory);
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to enumarete directory entires.");
                return;
            }

            // Cue files have priority since they will potentionaliy remove additional .bin files to match
            if (supportedExtensions.ContainsString("cue", StringComparison.OrdinalIgnoreCase))
            {
                var cueFiles = files.Where(a => a.EndsWith(".cue", StringComparison.OrdinalIgnoreCase)).ToArray();
                foreach (var cueFile in cueFiles)
                {
                    try
                    {
                        files.Remove(cueFile);
                        var bins = CueSheet.GetFileEntries(cueFile).Select(a => Path.Combine(directory, a.Path)).ToList();
                        if (bins.HasItems())
                        {
                            bins.ForEach(a => files.Remove(a));
                        }

                        if (importedFiles.ContainsString(cueFile, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        Tuple<DatGame, string> romData = null;
                        foreach (var bin in bins)
                        {
                            if (cancelToken.IsCancellationRequested)
                            {
                                return;
                            }

                            romData = LookupGameInDb(
                                bin,
                                Path.GetExtension(bin).TrimStart('.'),
                                supportedExtensions,
                                databases);
                            if (romData != null)
                            {
                                break;
                            }
                        }

                        if (romData != null)
                        {
                            logger.Trace($"Detected rom with db info:{cueFile}\n{romData.Item1}");
                            addRom(new ScannedRom(cueFile, romData.Item1, romData.Item2));
                        }
                        else
                        {
                            logger.Trace($"Detected rom: {cueFile}");
                            addRom(new ScannedRom(cueFile));
                        }
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, $"Failed to process cue file {cueFile}");
                    }
                }
            }

            foreach (var file in files)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return;
                }

                var ext = Path.GetExtension(file).TrimStart('.');
                if (!supportedExtensions.ContainsString(ext, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (importedFiles.ContainsString(file, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    var romData = LookupGameInDb(
                        file,
                        ext,
                        supportedExtensions,
                        databases);
                    if (romData != null)
                    {
                        logger.Trace($"Detected rom with db info:{file}\n{romData.Item1}");
                        addRom(new ScannedRom(file, romData.Item1, romData.Item2));
                    }
                    else
                    {
                        logger.Trace($"Detected rom: {file}");
                        addRom(new ScannedRom(file));
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed scan rom file {file}");
                }
            }

            foreach (var dir in dirs)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    break;
                }

                ScanDirectoryBase(
                    dir,
                    supportedExtensions,
                    databases,
                    importedFiles,
                    resultRoms,
                    cancelToken);
            }
        }

        private static Tuple<DatGame, string> LookupGameInDb(
            string file,
            string fileExt,
            List<string> supportedExtensions,
            List<EmulationDatabase.EmulationDatabaseReader> databases)
        {
            if (databases.HasItems())
            {
                DatGame datRec = null;
                string datRecSource = null;
                string crc = null;

                if (IsSupportedArchiveExtension(fileExt))
                {
                    var archFiles = Archive.GetArchiveFiles(file);
                    var supportedFile = archFiles.FirstOrDefault(a =>
                        supportedExtensions.ContainsString(Path.GetExtension(a).TrimStart('.')));
                    if (supportedFile != null)
                    {
                        logger.Trace($"Getting rom crc from archive file '{supportedFile}'\r\n {file}");
                        var streams = Archive.GetEntryStream(file, supportedFile);
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
                        logger.Trace($"Failed to get crc info from archive: {file}");
                        crc = FileSystem.GetCRC32(file);
                    }
                }
                else
                {
                    logger.Trace($"Getting rom crc from file: {file}");
                    crc = FileSystem.GetCRC32(file);
                }

                if (crc.IsNullOrEmpty())
                {
                    return null;
                }

                foreach (var db in databases)
                {
                    datRec = db.GetByCrc(crc).FirstOrDefault();
                    if (datRec != null)
                    {
                        datRecSource = db.DatabaseName;
                        break;
                    }
                }

                if (datRec == null)
                {
                    return null;
                }
                else
                {
                    return new Tuple<DatGame, string>(datRec, datRecSource);
                }
            }
            else
            {
                return null;
            }
        }
    }

    public class ScannedGame : ObservableObject
    {
        #region backing fields
        private bool import = true;
        private ObservableCollection<ScannedRom> roms;
        private List<Platform> platforms;
        private List<Region> regions;
        private string name;
        private GameScannerConfig sourceConfig;
        #endregion backing fields

        public bool Import                              { get => import; set => SetValue(ref import, value); }
        public ObservableCollection<ScannedRom> Roms    { get => roms; set => SetValue(ref roms, value); }
        public List<Platform> Platforms                 { get => platforms; set => SetValue(ref platforms, value); }
        public List<Region> Regions                     { get => regions; set => SetValue(ref regions, value); }
        public string Name                              { get => name; set => SetValue(ref name, value); }
        public GameScannerConfig SourceConfig           { get => sourceConfig; set => SetValue(ref sourceConfig, value); }

        public Game ToGame()
        {
            var game = new Game(Name) { IsInstalled = true };
            if (Platforms.HasItems())
            {
                game.PlatformIds = Platforms.Select(a => a.Id).ToList();
            }

            if (Regions.HasItems())
            {
                game.RegionIds = Regions.Select(a => a.Id).ToList();
            }

            game.GameActions = new ObservableCollection<GameAction>
            {
                new GameAction
                {
                    Type = GameActionType.Emulator,
                    EmulatorId = SourceConfig.EmulatorId,
                    EmulatorProfileId = SourceConfig.EmulatorProfileId,
                    IsPlayAction = true,
                    Name = Name
                }
            };

            var roms = Roms.Where(a => a.Import).ToList();
            game.InstallDirectory = Paths.GetCommonDirectory(roms.Select(a => a.Path).ToArray());
            game.Roms = new ObservableCollection<GameRom>();
            foreach (var rom in roms)
            {
                var romPath = game.InstallDirectory.IsNullOrEmpty()
                    ? rom.Path
                    : rom.Path.Replace(game.InstallDirectory, ExpandableVariables.InstallationDirectory + Path.DirectorySeparatorChar);
                game.Roms.Add(new GameRom(rom.Name.DiscName ?? rom.Name.SanitizedName, romPath));
            }

            return game;
        }
    }

    public class ScannedRom
    {
        public bool Import { get; set; } = true;
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

        public ScannedRom()
        {
        }
    }

    public class RomName
    {
        private static readonly string propsMatchStr = @"\[(.*?)\]|\((.*?)\)";

        public string Name { get; set; }
        public string SanitizedName { get; set; }
        public string DiscName { get; set; }
        public List<string> Properties { get; set; } = new List<string>();

        public RomName()
        {
        }

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
