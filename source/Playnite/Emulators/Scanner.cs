using Playnite.Common;
using Playnite.Database;
using Playnite.Native;
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
                        var detectionStr = defProfile.InstallationFile;
                        if (detectionStr.IsNullOrEmpty())
                        {
                            detectionStr = defProfile.StartupExecutable;
                        }

                        if (detectionStr.IsNullOrEmpty() && !PlayniteEnvironment.ThrowAllErrors)
                        {
                            continue;
                        }

                        var reqMet = true;
                        var regex = new Regex(detectionStr, RegexOptions.IgnoreCase);
                        if (regex.IsMatch(file.Name))
                        {
                            if (defProfile.ProfileFiles?.Any() == true)
                            {
                                foreach (var reqFile in defProfile.ProfileFiles)
                                {
                                    if (!FileSystem.FileExists(Path.Combine(currentDir, reqFile)))
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

                                if (currentDir.StartsWith(PlaynitePaths.ProgramPath, StringComparison.OrdinalIgnoreCase))
                                {
                                    currentEmulator.InstallDir = currentDir.Replace(PlaynitePaths.ProgramPath, ExpandableVariables.PlayniteDirectory, StringComparison.OrdinalIgnoreCase);
                                }

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
        private readonly Dictionary<string, bool> isGoogleDriveCache = new Dictionary<string, bool>();
        private readonly GameScannerConfig scanner;
        private readonly IGameDatabaseMain database;
        internal List<string> importedFiles;
        private readonly Func<List<string>, List<EmulationDatabase.IEmulationDatabaseReader>> emuDbProvider;

        public GameScanner(
            GameScannerConfig scanner,
            IGameDatabaseMain database,
            Func<List<string>, List<EmulationDatabase.IEmulationDatabaseReader>> emuDbProvider = null)
        {
            this.scanner = scanner;
            this.database = database;
            if (emuDbProvider == null)
            {
                this.emuDbProvider = GetEmulationDbs;
            }
            else
            {
                this.emuDbProvider = emuDbProvider;
            }
        }

        public List<ScannedGame> Scan(
            CancellationToken cancelToken,
            out List<Platform> newPlatforms,
            out List<Region> newRegions,
            Action<string> fileScanCallback = null)
        {
            List<ScannedGame> games;
            newPlatforms = new List<Platform>();
            newRegions = new List<Region>();

            var emulator = database.Emulators[scanner.EmulatorId];
            if (emulator == null)
            {
                throw new Exception("Emulator not found.");
            }

            importedFiles = database.GetImportedRomFiles(emulator.InstallDir);
            var globalScanConfig = database.GetGameScannersSettings();
            var crcExclusions = string.Join(";",
                ListExtensions.Merge(globalScanConfig.CrcExcludeFileTypes, scanner.CrcExcludeFileTypes).
                Select(a => a.ToLower().Trim()).ToHashSet());

            var customProfile = emulator.CustomProfiles?.FirstOrDefault(a => a.Id == scanner.EmulatorProfileId);
            var builtinProfile = emulator.BuiltinProfiles?.FirstOrDefault(a => a.Id == scanner.EmulatorProfileId);
            var builtinProfileDef = Emulation.GetProfile(emulator.BuiltInConfigId, builtinProfile?.BuiltInProfileName);
            var dirToScan = PlaynitePaths.ExpandVariables(scanner.Directory, emulator.InstallDir, true);
            if (scanner.EmulatorProfileId.StartsWith(CustomEmulatorProfile.ProfilePrefix))
            {
                games = ScanDirectory(
                    dirToScan,
                    emulator,
                    customProfile,
                    cancelToken,
                    crcExclusions,
                    scanner.ScanSubfolders,
                    scanner.ScanInsideArchives,
                    fileScanCallback);
            }
            else if (scanner.EmulatorProfileId.StartsWith(BuiltInEmulatorProfile.ProfilePrefix))
            {
                games = ScanDirectory(
                    dirToScan,
                    emulator,
                    builtinProfile,
                    cancelToken,
                    crcExclusions,
                    scanner.ScanSubfolders,
                    scanner.ScanInsideArchives,
                    fileScanCallback);
            }
            else
            {
                throw new Exception("Emulator profile format not supported.");
            }

            foreach (var game in games)
            {
                game.SourceEmulator = emulator;
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
                    else if (builtinProfileDef != null)
                    {
                        assignedPlatforms.AddMissing(Emulation.GetPlatform(builtinProfileDef.Platforms.First()));
                    }

                    rom.Path = Paths.TrimLongPathPrefix(rom.Path);
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
                if (scanner.OverridePlatformId != Guid.Empty)
                {
                    var dbPlatform = database.Platforms[scanner.OverridePlatformId];
                    if (dbPlatform != null)
                    {
                        game.Platforms.Add(dbPlatform);
                    }
                }
                else
                {
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

                var releaseYear = game.Roms.FirstOrDefault(a => a.DbData?.ReleaseYear.IsNullOrEmpty() == false)?.DbData.ReleaseYear;
                if (!releaseYear.IsNullOrEmpty())
                {
                    if (ReleaseDate.TryDeserialize(releaseYear, out var releaseDate))
                    {
                        game.ReleaseDate = releaseDate;
                    }
                }
            }

            return games;
        }

        private List<ScannedGame> ScanDirectory(
            string directory,
            Emulator emulator,
            BuiltInEmulatorProfile profile,
            CancellationToken cancelToken,
            string crcExludePatterns,
            bool scanSubfolders,
            bool scanArchives,
            Action<string> fileScanCallback = null)
        {
            var emuProf = Emulation.GetProfile(emulator.BuiltInConfigId, profile.BuiltInProfileName);
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
                            Emulation.GetGameImportScriptPath(Emulation.GetDefition(emulator.BuiltInConfigId)),
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
                        return ParseScriptScanResult(
                            scannedGames, emuProf);
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
                    emuProf.ImageExtensions?.Select(a => a.Trim()).ToList(),
                    emuProf.Platforms,
                    cancelToken,
                    crcExludePatterns,
                    scanSubfolders,
                    scanArchives,
                    fileScanCallback);
            }
        }

        private List<ScannedGame> ScanDirectory(
            string directory,
            Emulator emulator,
            CustomEmulatorProfile profile,
            CancellationToken cancelToken,
            string crcExludePatterns,
            bool scanSubfolders,
            bool scanArchives,
            Action<string> fileScanCallback = null)
        {
            if (profile == null)
            {
                throw new Exception($"No profile provided.");
            }

            if (!profile.ImageExtensions.HasItems())
            {
                return new List<ScannedGame>();
            }

            var platforms = profile.Platforms?.Select(a => database.Platforms[a]?.SpecificationId).Where(a => !a.IsNullOrEmpty()).ToList();
            return ScanDirectory(
                directory,
                profile.ImageExtensions.Select(a => a.Trim()).ToList(),
                platforms,
                cancelToken,
                crcExludePatterns,
                scanSubfolders,
                scanArchives,
                fileScanCallback);
        }

        private List<ScannedGame> ScanDirectory(
            string directory,
            List<string> supportedExtensions,
            List<string> scanPlatforms,
            CancellationToken cancelToken,
            string crcExludePatterns,
            bool scanSubfolders,
            bool scanArchives,
            Action<string> fileScanCallback = null)
        {
            logger.Info($"Scanning emulated directory {directory}.");
            if (!FileSystem.DirectoryExists(directory))
            {
                throw new Exception($"Can't scan emulation directory, {directory} doesn't exist.");
            }

            var emuDbs = emuDbProvider(scanPlatforms);
            var resultRoms = new Dictionary<string, List<ScannedRom>>();

            try
            {
                ScanDirectoryBase(
                    directory,
                    supportedExtensions,
                    emuDbs,
                    resultRoms,
                    cancelToken,
                    crcExludePatterns,
                    scanSubfolders,
                    scanArchives,
                    fileScanCallback);
            }
            finally
            {
                emuDbs.ForEach(a => a.Dispose());
            }

            return resultRoms.Select(a => new ScannedGame { Name = a.Key, Roms = a.Value?.ToObservable() }).ToList();
        }

        internal void ScanDirectoryBase(
            string directory,
            List<string> supportedExtensions,
            List<EmulationDatabase.IEmulationDatabaseReader> databases,
            Dictionary<string, List<ScannedRom>> resultRoms,
            CancellationToken cancelToken,
            string crcExludePatterns,
            bool scanSubfolders,
            bool scanArchives,
            Action<string> fileScanCallback = null)
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
            List<string> dirs;
            try
            {
                directory = Paths.FixPathLength(directory);
                files = Directory.GetFiles(directory).ToList();
                dirs = Directory.GetDirectories(directory).ToList();
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to enumarete directory entires.");
                return;
            }

            fileScanCallback?.Invoke(directory);
            if (scanner.ExcludedFiles.HasItems())
            {
                foreach (var excFile in scanner.ExcludedFiles.Where(a => !a.IsNullOrWhiteSpace()).Select(a => Path.Combine(directory, a.Trim())))
                {
                    var match = files.FirstOrDefault(a => a.Equals(excFile, StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                    {
                        files.Remove(match);
                    }
                }
            }

            void processPlayListFile(string filePath, Func<string, List<string>> playListParser)
            {
                var fileExt = Path.GetExtension(filePath).TrimStart('.');
                files.Remove(filePath);

                try
                {
                    var childFiles = playListParser(filePath);
                    foreach (var child in childFiles ?? new List<string>())
                    {
                        var existingFile = files.FirstOrDefault(a => a.Equals(child, StringComparison.OrdinalIgnoreCase));
                        if (existingFile != null)
                        {
                            files.Remove(existingFile);
                        }
                    }

                    if (importedFiles.ContainsString(Path.GetFullPath(filePath), StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }

                    if (!childFiles.HasItems())
                    {
                        logger.Trace($"Detected playlist file with no referenced files: {filePath}");
                        addRom(new ScannedRom(filePath));
                        return;
                    }

                    Tuple<DatGame, string> romData = null;
                    foreach (var childPath in childFiles)
                    {
                        if (cancelToken.IsCancellationRequested)
                        {
                            return;
                        }

                        if (!File.Exists(childPath))
                        {
                            continue;
                        }

                        fileScanCallback?.Invoke(childPath);
                        var crcScan = true;
                        if (databases.HasItems())
                        {
                            if (scanner.ExcludeOnlineFiles && !IsFileDataAvailable(childPath))
                            {
                                if (scanner.UseSimplifiedOnlineFileScan)
                                {
                                    crcScan = false;
                                }
                                else
                                {
                                    logger.Trace($"Skipping scan of {childPath} rom, scan of online files is disabled.");
                                    continue;
                                }
                            }

                            if (crcScan && Paths.MathcesFilePattern(childPath, crcExludePatterns))
                            {
                                logger.Trace($"Skipping crc check of {childPath}. Excluded by pattern settings.");
                                crcScan = false;
                            }
                        }
                        else
                        {
                            crcScan = false;
                        }

                        romData = LookupGameInDb(
                            childPath,
                            Path.GetExtension(childPath).TrimStart('.'),
                            supportedExtensions,
                            databases,
                            crcScan,
                            scanArchives);
                        if (romData != null)
                        {
                            break;
                        }
                    }

                    if (romData != null)
                    {
                        logger.Trace($"Detected rom with db info:{filePath}\n{romData.Item1}");
                        addRom(new ScannedRom(filePath, romData.Item1, romData.Item2));
                    }
                    else
                    {
                        logger.Trace($"Detected rom: {filePath}");
                        addRom(new ScannedRom(filePath));
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to process {fileExt} playlist file {filePath}");
                }

                fileScanCallback?.Invoke(directory);
            }

            // Cue files have priority since they will potentionaliy remove additional .bin files to match
            if (supportedExtensions.ContainsString("cue", StringComparison.OrdinalIgnoreCase))
            {
                // ToList is needed here because we are potentionally modifing original files collection when playlist files are excluded
                foreach (var cueFile in files.Where(a => a.EndsWith(".cue", StringComparison.OrdinalIgnoreCase)).ToList())
                {
                    processPlayListFile(cueFile, (cFile) => CueSheet.GetFileEntries(cFile).Select(a => Path.Combine(directory, a.Path)).ToList());
                }
            }

            // The same as with cue but for m3u playlist
            if (supportedExtensions.ContainsString("m3u", StringComparison.OrdinalIgnoreCase))
            {
                // ToList is needed here because we are potentionally modifing original files collection when playlist files are excluded
                foreach (var m3uFile in files.ToList().Where(a => a.EndsWith(".m3u", StringComparison.OrdinalIgnoreCase)).ToList())
                {
                    processPlayListFile(m3uFile, (mFile) => M3U.GetEntries(mFile).Select(a => Path.Combine(directory, a.Path)).ToList());
                }
            }

            // gdi files are basically cue files for Dreamcast dumps
            if (supportedExtensions.ContainsString("gdi", StringComparison.OrdinalIgnoreCase))
            {
                // ToList is needed here because we are potentionally modifing original files collection when playlist files are excluded
                foreach (var gdiFile in files.ToList().Where(a => a.EndsWith(".gdi", StringComparison.OrdinalIgnoreCase)).ToList())
                {
                    processPlayListFile(gdiFile, (mFile) => GdiFile.GetEntries(mFile).Select(a => Path.Combine(directory, a.Path)).ToList());
                }
            }

            foreach (var file in files)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return;
                }

                string ext = null;
                foreach (var supportedExt in supportedExtensions)
                {
                    // This is done this way to support nested extensions like PICO-8's .p8.png
                    if (file.EndsWith("." + supportedExt))
                    {
                        ext = supportedExt;
                        break;
                    }
                    else if (supportedExt == "<none>" && Path.GetExtension(file).IsNullOrEmpty())
                    {
                        ext = "<none>";
                        break;
                    }
                }

                if (ext == null)
                {
                    continue;
                }

                if (!supportedExtensions.ContainsString(ext, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (importedFiles.ContainsString(Path.GetFullPath(file), StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    var crcScan = true;
                    if (databases.HasItems())
                    {
                        if (scanner.ExcludeOnlineFiles && !IsFileDataAvailable(file))
                        {
                            if (scanner.UseSimplifiedOnlineFileScan)
                            {
                                crcScan = false;
                            }
                            else
                            {
                                logger.Trace($"Skipping scan of {file} rom, scan of online files is disabled.");
                                continue;
                            }
                        }

                        if (crcScan && Paths.MathcesFilePattern(file, crcExludePatterns))
                        {
                            logger.Trace($"Skipping crc check of {file}. Excluded by pattern settings.");
                            crcScan = false;
                        }
                    }
                    else
                    {
                        crcScan = false;
                    }

                    if (crcScan)
                    {
                        fileScanCallback?.Invoke(file);
                    }

                    var romData = LookupGameInDb(
                        file,
                        ext,
                        supportedExtensions,
                        databases,
                        crcScan,
                        scanArchives);
                    if (romData != null)
                    {
                        logger.Trace($"Detected rom with db info:{file}\n{romData.Item1}");
                        addRom(new ScannedRom(file, romData.Item1, romData.Item2));
                    }
                    else
                    {
                        logger.Trace($"Detected rom: {file}");
                        addRom(new ScannedRom(file, ext));
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed scan rom file {file}");
                }

                fileScanCallback?.Invoke(directory);
            }

            if (scanSubfolders)
            {
                if (scanner.ExcludedDirectories.HasItems())
                {
                    foreach (var excDir in scanner.ExcludedDirectories.Where(a => !a.IsNullOrWhiteSpace()).Select(a => Path.Combine(directory, a.Trim())))
                    {
                        var match = dirs.FirstOrDefault(a => a.TrimEnd(Paths.DirectorySeparators).Equals(excDir.TrimEnd(Paths.DirectorySeparators), StringComparison.OrdinalIgnoreCase));
                        if (match != null)
                        {
                            dirs.Remove(match);
                        }
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
                        resultRoms,
                        cancelToken,
                        crcExludePatterns,
                        scanSubfolders,
                        scanArchives,
                        fileScanCallback);
                }
            }
        }

        private Tuple<DatGame, string> LookupGameInDb(
            string file,
            string fileExt,
            List<string> supportedExtensions,
            List<EmulationDatabase.IEmulationDatabaseReader> databases,
            bool scanCrc,
            bool scanArchives)
        {
            if (databases.HasItems())
            {
                file = Paths.FixPathLength(file);
                DatGame datRec = null;
                string datRecSource = null;
                List<string> crcs = new List<string>();

                if (scanCrc)
                {
                    if (IsSupportedArchiveExtension(fileExt) && scanArchives)
                    {
                        var archFiles = Archive.GetArchiveFiles(file);
                        var supportedFiles = archFiles.Where(a =>
                            supportedExtensions.ContainsString(Path.GetExtension(a).TrimStart('.')));
                        foreach (var supportedFile in supportedFiles)
                        {
                            logger.Trace($"Getting rom crc from archive file '{supportedFile}'\r\n {file}");
                            var streams = Archive.GetEntryStream(file, supportedFile);
                            if (streams != null)
                            {
                                using (streams.Item2)
                                using (streams.Item1)
                                {
                                    crcs.AddMissing(FileSystem.GetCRC32(streams.Item1));
                                }
                            }
                        }

                        if (!crcs.HasItems())
                        {
                            logger.Trace($"Failed to get crc info from archive: {file}");
                            crcs.Add(FileSystem.GetCRC32(file));
                        }
                    }
                    else
                    {
                        logger.Trace($"Getting rom crc from file: {file}");
                        crcs.Add(FileSystem.GetCRC32(file));
                    }
                }

                foreach (var db in databases)
                {
                    foreach (var crc in crcs)
                    {
                        datRec = db.GetByCrc(crc).FirstOrDefault();
                        if (datRec != null)
                        {
                            datRecSource = db.DatabaseName;
                            break;
                        }
                    }

                    if (datRecSource != null)
                    {
                        break;
                    }

                    var fileName = Path.GetFileName(file);
                    datRec = db.GetByRomName(fileName).FirstOrDefault();
                    if (datRec != null)
                    {
                        datRecSource = db.DatabaseName;
                        break;
                    }

                    // This is mainly for XBLA games that have those weird file names
                    datRec = db.GetByRomNamePartial(fileName).FirstOrDefault();
                    if (datRec != null)
                    {
                        datRecSource = db.DatabaseName;
                        break;
                    }

                    // For rare cases where ROM file name is the same as game's serial
                    datRec = db.GetBySerial(Path.GetFileNameWithoutExtension(file)).FirstOrDefault();
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

        private List<EmulationDatabase.IEmulationDatabaseReader> GetEmulationDbs(List<string> platformIds)
        {
            var supportedPlatforms = Emulation.Platforms.Where(a => platformIds?.Contains(a.Id) == true);
            var supportedDatabases = supportedPlatforms.Where(a => a.Databases.HasItems()).SelectMany(a => a.Databases).Distinct();
            var emuDbs = new List<EmulationDatabase.IEmulationDatabaseReader>();
            foreach (var supDb in supportedDatabases)
            {
                var db = EmulationDatabase.GetDatabase(supDb, PlaynitePaths.EmulationDatabasePath);
                if (db != null)
                {
                    emuDbs.Add(db);
                }
            }

            return emuDbs;
        }

        private static ScannedGame ParseScripScannedGame(
            ScriptScannedGame scriptGame,
            List<EmulationDatabase.IEmulationDatabaseReader> emuDbs)
        {
            var game = new ScannedGame();
            game.Name = scriptGame.Name ?? scriptGame.Serial;
            game.Roms = new ObservableCollection<ScannedRom>();
            game.ScriptSource = scriptGame;

            if (scriptGame.Serial.IsNullOrEmpty())
            {
                game.Roms.Add(new ScannedRom(scriptGame.Path));
            }
            else
            {
                DatGame datRec = null;
                string datRecSource = null;
                foreach (var db in emuDbs)
                {
                    datRec = db.GetBySerial(scriptGame.Serial).FirstOrDefault();
                    if (datRec != null)
                    {
                        datRecSource = db.DatabaseName;
                        break;
                    }
                }

                if (datRec == null)
                {
                    game.Roms.Add(new ScannedRom(scriptGame.Path));
                }
                else
                {
                    var romData = new ScannedRom(scriptGame.Path, datRec, datRecSource);
                    game.Roms.Add(romData);
                    game.Name = romData.Name.SanitizedName;
                }
            }

            return game;
        }

        private List<ScannedGame> ParseScriptScanResult(
            object scanResult,
            EmulatorDefinitionProfile emuProf)
        {
            if (scanResult == null)
            {
                return new List<ScannedGame>();
            }

            if (!(scanResult is ScriptScannedGame) && !(scanResult is List<object>))
            {
                if (PlayniteEnvironment.ThrowAllErrors)
                {
                    throw new Exception($"Scanning script returned unknown data type {scanResult.GetType()}");
                }
                else
                {
                    logger.Error($"Scanning script returned unknown data type {scanResult.GetType()}");
                    return new List<ScannedGame>();
                }
            }

            var emuDbs = emuDbProvider(emuProf.Platforms);
            try
            {
                if (scanResult is ScriptScannedGame game)
                {
                    return new List<ScannedGame>() { ParseScripScannedGame(game, emuDbs) };
                }
                else if (scanResult is List<object> games)
                {
                    var result = new List<ScannedGame>();
                    foreach (ScriptScannedGame scannedGame in games)
                    {
                        result.Add(ParseScripScannedGame(scannedGame, emuDbs));
                    }

                    return result;
                }
            }
            finally
            {
                emuDbs.ForEach(a => a.Dispose());
            }

            return new List<ScannedGame>();
        }

        private static bool IsSupportedArchiveExtension(string extension)
        {
            return supportedArchiveExt.ContainsString(extension, StringComparison.OrdinalIgnoreCase);
        }

        private bool IsFileDataAvailable(string path)
        {
            if (!Paths.IsFullPath(path))
            {
                path = Path.GetFullPath(path);
            }

            if (!FileSystem.FileExists(path))
            {
                return false;
            }

            var longPath = @"\\?\" + path;
            var att = Kernel32.GetFileAttributesW(longPath);
            if ((Winnt.FILE_ATTRIBUTE_OFFLINE & att) > 0)
            {
                return false;
            }

            // Used by OneDrive
            if ((Winnt.FILE_ATTRIBUTE_RECALL_ON_DATA_ACCESS & att) > 0)
            {
                return false;
            }

            if ((Winnt.FILE_ATTRIBUTE_SPARSE_FILE & att) > 0)
            {
                return false;
            }

            // GoogleDrive file check
            var driveLetter = Path.GetPathRoot(path);
            if (!isGoogleDriveCache.TryGetValue(driveLetter, out var isGoogleDrive))
            {
                var drive = DriveInfo.GetDrives().First(a => string.Equals(a.Name, driveLetter, StringComparison.OrdinalIgnoreCase));
                isGoogleDrive = drive.VolumeLabel?.Contains("Google") == true;
                isGoogleDriveCache.Add(driveLetter, isGoogleDrive);
            }

            if (isGoogleDrive)
            {
                // Based on undocumented file metadata
                // https://stackoverflow.com/questions/51439810/get-google-drive-files-links-using-drive-file-stream/52107704#52107704
                longPath = longPath += ":user.drive.itemprotostr";
                try
                {
                    // Downloaded files (even partially) will have "content-entry" property
                    var fileMetadata = File.ReadAllText(longPath);
                    return fileMetadata.Contains("key: \"content-entry\"");
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, $"Failed to get file metadata from Google Drive file. {longPath}");
                }
            }

            return true;
        }
    }

    public class ScriptScannedGame
    {
        public string Path { get; set; }
        public string Serial { get; set; }
        public string Name { get; set; }
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
        private ReleaseDate? releaseDate;
        private ScriptScannedGame scriptSource;
        private Emulator sourceEmulator;
        #endregion backing fields

        public bool Import                              { get => import; set => SetValue(ref import, value); }
        public ObservableCollection<ScannedRom> Roms    { get => roms; set => SetValue(ref roms, value); }
        public List<Platform> Platforms                 { get => platforms; set => SetValue(ref platforms, value); }
        public List<Region> Regions                     { get => regions; set => SetValue(ref regions, value); }
        public string Name                              { get => name; set => SetValue(ref name, value); }
        public GameScannerConfig SourceConfig           { get => sourceConfig; set => SetValue(ref sourceConfig, value); }
        public ReleaseDate? ReleaseDate                 { get => releaseDate; set => SetValue(ref releaseDate, value); }
        public ScriptScannedGame ScriptSource           { get => scriptSource; set => SetValue(ref scriptSource, value); }
        public Emulator SourceEmulator                  { get => sourceEmulator; set => SetValue(ref sourceEmulator, value); }

        public Game ToGame()
        {
            var game = new Game(Name)
            {
                IsInstalled = true,
                ReleaseDate = ReleaseDate
            };

            if (Platforms.HasItems())
            {
                game.PlatformIds = Platforms.Select(a => a.Id).ToList();
            }

            if (Regions.HasItems())
            {
                game.RegionIds = Regions.Select(a => a.Id).ToList();
            }

            var playAction = new GameAction
            {
                Type = GameActionType.Emulator,
                EmulatorId = SourceConfig.EmulatorId,
                EmulatorProfileId = SourceConfig.EmulatorProfileId,
                IsPlayAction = true,
                Name = Name
            };

            if (sourceConfig.PlayActionSettings == ScannerConfigPlayActionSettings.SelectProfiteOnStart)
            {
                playAction.EmulatorProfileId = null;
            }
            else if (sourceConfig.PlayActionSettings == ScannerConfigPlayActionSettings.SelectEmulatorOnStart)
            {
                playAction.EmulatorProfileId = null;
                playAction.EmulatorId = Guid.Empty;
            }

            game.GameActions = new ObservableCollection<GameAction> { playAction };
            if (Roms.HasItems())
            {
                var commonPath = Paths.GetCommonDirectory(Roms.Select(a => a.Path).ToArray());
                game.Roms = new ObservableCollection<GameRom>();

                var toReplace = string.Empty;
                var varToReplace = string.Empty;
                if (sourceConfig.ImportWithRelativePaths)
                {
                    var emuDir = GameExtensions.ExpandVariables(new Game(), SourceEmulator.InstallDir, true) ?? string.Empty;
                    if (commonPath.StartsWith(emuDir, StringComparison.OrdinalIgnoreCase))
                    {
                        varToReplace = ExpandableVariables.EmulatorDirectory.EndWithDirSeparator();
                        toReplace = emuDir.EndWithDirSeparator();
                    }
                    else if (commonPath.StartsWith(PlaynitePaths.ProgramPath, StringComparison.OrdinalIgnoreCase))
                    {
                        varToReplace = ExpandableVariables.PlayniteDirectory.EndWithDirSeparator();
                        toReplace = PlaynitePaths.ProgramPath.EndWithDirSeparator();
                    }
                }

                if (sourceConfig.ImportWithRelativePaths && !toReplace.IsNullOrEmpty())
                {
                    game.InstallDirectory = commonPath.Replace(toReplace, varToReplace, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    game.InstallDirectory = commonPath;
                }

                foreach (var rom in Roms.Where(a => a.Import))
                {
                    var gameRom = new GameRom();
                    if (rom.Name.DiscName.IsNullOrEmpty())
                    {
                        gameRom.Name = rom.Name.SanitizedName;
                    }
                    else
                    {
                        if (rom.Name.Properties.Count > 1)
                        {
                            gameRom.Name = rom.Name.DiscName + " - " + string.Join(" - ", rom.Name.Properties.Where(a => a != rom.Name.DiscName));
                        }
                        else
                        {
                            gameRom.Name = rom.Name.DiscName;
                        }
                    }

                    if (commonPath.IsNullOrEmpty())
                    {
                        gameRom.Path = rom.Path;
                    }
                    else
                    {
                        gameRom.Path = rom.Path.Replace(commonPath, ExpandableVariables.InstallationDirectory.EndWithDirSeparator(), StringComparison.OrdinalIgnoreCase);
                    }

                    game.Roms.Add(gameRom);
                }
            }

            return game;
        }
    }

    public class ScannedRom : ObservableObject
    {
        #region backing fields
        private bool import = true;
        #endregion backing fields

        public bool Import { get => import; set => SetValue(ref import, value); }
        public DatGame DbData { get; set; }
        public RomName Name { get; set; }
        public string Path { get; set; }
        public string DbDataSource { get; set; }

        public ScannedRom(string path)
        {
            Path = path;
            Name = new RomName(System.IO.Path.GetFileNameWithoutExtension(path));
        }

        public ScannedRom(string path, string scannedExtension)
        {
            Path = path;
            if (path.EndsWith("." + scannedExtension))
            {
                var fileName = System.IO.Path.GetFileName(path);
                Name = new RomName(fileName.Substring(0, fileName.LastIndexOf("." + scannedExtension)));
            }
            else
            {
                Name = new RomName(System.IO.Path.GetFileNameWithoutExtension(path));
            }
        }

        public ScannedRom(string path, DatGame dbData, string dbDataSource)
        {
            Path = path;
            DbData = dbData;
            DbDataSource = dbDataSource;
            if (!dbData.Name.IsNullOrEmpty())
            {
                Name = new RomName(dbData.Name);
            }
            else if (!dbData.RomName.IsNullOrEmpty())
            {
                Name = new RomName(dbData.RomName);
            }
            else
            {
                Name = new RomName(System.IO.Path.GetFileNameWithoutExtension(path));
            }
        }

        public ScannedRom()
        {
        }
    }

    public class RomName
    {
        private static readonly Regex propsRegex = new Regex(@"\[(.*?)\]|\((.*?)\)", RegexOptions.Compiled);
        private static readonly char[] propertySplitter = new char[] { ',' };

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

            SanitizedName = SanitizeName(originalName);
            Name = originalName;
            var matches = propsRegex.Matches(originalName);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    for (int i = 1; i < match.Groups.Count; i++)
                    {
                        if (!match.Groups[i].Value.IsNullOrEmpty())
                        {
                            Properties.AddRange(match.Groups[i].Value.Split(propertySplitter, StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()));
                        }
                    }
                }
            }

            DiscName = Properties.FirstOrDefault(a =>
                a.StartsWith("disc", StringComparison.InvariantCultureIgnoreCase) ||
                a.StartsWith("disk", StringComparison.InvariantCultureIgnoreCase) ||
                a.StartsWith("side", StringComparison.InvariantCultureIgnoreCase));
            if (DiscName == null)
            {
                DiscName = originalName;
            }
        }

        public static string SanitizeName(string name)
        {
            var newName = propsRegex.Replace(name, string.Empty);
            return newName.
                Replace('’', '\'').
                RemoveTrademarks().
                Replace("_", " ").
                Trim();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
