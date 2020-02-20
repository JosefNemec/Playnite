using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Playnite.Toolbox
{
    public class FileChange
    {
        public string ChangeType { get; set; }
        public string Path { get; set; }

        public override string ToString()
        {
            return $"{ChangeType}: {Path}";
        }
    }

    public class Themes
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public const string ThemeSlnName = "Theme.sln";
        public const string ThemeProjName = "Theme.csproj";
        public const string AppXamlName = "App.xaml";
        public const string GlobalResourcesName = "GlobalResources.xaml";

        public static List<string> PackageFileBlackList { get; } = new List<string>
        {
            PlaynitePaths.ThemeManifestFileName,
            ThemeProjName,
            ThemeSlnName,
            AppXamlName,
            GlobalResourcesName,
            PlaynitePaths.EngLocSourceFileName
        };

        public static List<FileChange> GetThemeChangelog(Version baseVersion, ApplicationMode mode, string changelogDir)
        {
            if (!Directory.Exists(changelogDir))
            {
                throw new FileNotFoundException("Theme changelog not found.");
            }

            var changes = new List<FileChange>();
            foreach (var changeFile in Directory.GetFiles(changelogDir, "*.txt").OrderBy(a => a))
            {
                var match = Regex.Match(Path.GetFileName(changeFile), @"(.+)-(.+)\.txt");
                if (!match.Success)
                {
                    continue;
                }

                var oldVersion = new Version(match.Groups[1].Value);
                var newVersion = new Version(match.Groups[2].Value);
                if (oldVersion < baseVersion)
                {
                    continue;
                }

                foreach (var line in File.ReadAllLines(changeFile))
                {
                    var lineMatch = Regex.Match(line, $"([A-Z])\\s(.+{ThemeManager.GetThemeRootDir(mode)}\\/Default.+)");
                    if (!lineMatch.Success)
                    {
                        continue;
                    }

                    var changeType = lineMatch.Groups[1].Value;
                    var changePath = lineMatch.Groups[2].Value;

                    var exist = changes.FirstOrDefault(a => a.Path == changePath);
                    if (exist != null)
                    {
                        changes.Remove(exist);
                    }

                    changes.Add(new FileChange
                    {
                        ChangeType = changeType,
                        Path = changePath
                    });
                }
            }

            return changes;
        }

        public static List<FileChange> GetThemeChangelog(Version baseVersion, ApplicationMode mode)
        {
            return GetThemeChangelog(baseVersion, mode, Paths.ChangeLogsDir);
        }

        public static void CopyThemeDirectory(string sourceDirName, string destDirName, List<string> approvedXamls)
        {
            var dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            var dirs = dir.GetDirectories();
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            var files = dir.GetFiles();
            foreach (var file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                if (file.Extension.Equals(".xaml", StringComparison.OrdinalIgnoreCase))
                {
                    if (approvedXamls.ContainsString(file.FullName))
                    {
                        file.CopyTo(temppath, true);
                    }
                }
                else
                {
                    file.CopyTo(temppath, true);
                }
            }

            foreach (var subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                CopyThemeDirectory(subdir.FullName, temppath, approvedXamls);
            }
        }

        public static bool AreFilesEqual(string file1, string file2)
        {
            var file1Info = new FileInfo(file1);
            var file2Info = new FileInfo(file2);
            if (file1Info.Length != file2Info.Length)
            {
                return false;
            }

            if (file1Info.Extension.Equals(".xaml", StringComparison.OrdinalIgnoreCase))
            {
                return Xml.AreEqual(File.ReadAllText(file1), File.ReadAllText(file2));
            }
            else
            {
                return FileSystem.GetMD5(file1) == FileSystem.GetMD5(file2);
            }
        }

        public static string PackageTheme(string themeDirectory, string targetPath, ApplicationMode mode)
        {
            var dirInfo = new DirectoryInfo(themeDirectory);
            var extInfo = ThemeManager.GetDescriptionFromFile(Path.Combine(themeDirectory, PlaynitePaths.ThemeManifestFileName));
            var defaultThemeDir = Path.Combine(Paths.GetThemesPath(mode), "Default");
            targetPath = Path.Combine(targetPath, $"{dirInfo.Name}_{extInfo.Version.ToString().Replace(".", "_")}{PlaynitePaths.PackedThemeFileExtention}");
            FileSystem.PrepareSaveFile(targetPath);
            using (var zipStream = new FileStream(targetPath, FileMode.Create))
            {
                using (var zipFile = new ZipArchive(zipStream, ZipArchiveMode.Create))
                {
                    zipFile.CreateEntryFromFile(Path.Combine(themeDirectory, PlaynitePaths.ThemeManifestFileName), PlaynitePaths.ThemeManifestFileName);

                    foreach (var file in Directory.GetFiles(themeDirectory, "*.*", SearchOption.AllDirectories))
                    {
                        var subName = file.Replace(themeDirectory, "").TrimStart(Path.DirectorySeparatorChar);
                        if (file == targetPath ||
                            PackageFileBlackList.ContainsString(subName) ||
                            subName.StartsWith("Fonts\\") ||
                            subName.StartsWith(".vs\\") ||
                            subName.StartsWith("bin\\") ||
                            subName.StartsWith("obj\\") ||
                            subName.StartsWith("backup_"))
                        {
                            continue;
                        }

                        var defaultFile = Path.Combine(defaultThemeDir, subName);
                        if (File.Exists(defaultFile))
                        {
                            if (!AreFilesEqual(file, defaultFile))
                            {
                                zipFile.CreateEntryFromFile(file, subName);
                            }
                        }
                        else
                        {
                            zipFile.CreateEntryFromFile(file, subName);
                        }
                    }
                }
            }

            return targetPath;
        }

        public static void BackupTheme(string themeDirectory, string destination)
        {
            var dir = new DirectoryInfo(themeDirectory);
            var dirs = dir.GetDirectories();
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var targetPath = Path.Combine(destination, file.Name);
                file.CopyTo(targetPath, true);
            }

            foreach (var subdir in dirs)
            {
                if (!subdir.Name.StartsWith("backup_"))
                {
                    string targetPath = Path.Combine(destination, subdir.Name);
                    BackupTheme(subdir.FullName, targetPath);
                }
            }
        }

        public static void UpdateTheme(string themeDirectory, ApplicationMode mode)
        {
            var themeManifestPath = Path.Combine(themeDirectory, "theme.yaml");
            var currentThemeMan = ThemeDescription.FromFile(themeManifestPath);
            var origThemeApiVersion = new Version(currentThemeMan.ThemeApiVersion);

            if (!File.Exists(Path.Combine(themeDirectory, Themes.ThemeProjName)))
            {
                throw new Exception("Cannot update theme that was not generated via Toolbox utility.");
            }

            if (ThemeManager.GetApiVersion(mode) == origThemeApiVersion)
            {
                logger.Warn("Theme is already updated to current API version.");
                return;
            }

            var folder = Paths.GetNextBackupFolder(themeDirectory);
            BackupTheme(themeDirectory, Paths.GetNextBackupFolder(themeDirectory));
            logger.Info($"Current theme backed up into \"{Path.GetFileName(folder)}\" folder.");

            var defaultThemeDir = Path.Combine(Paths.GetThemesPath(mode), "Default");
            var origFilesZip = Path.Combine(Paths.ChangeLogsDir, currentThemeMan.ThemeApiVersion + ".zip");
            var themeChanges = Themes.GetThemeChangelog(origThemeApiVersion, mode);
            if (!themeChanges.HasItems())
            {
                logger.Info("No files to update.");
                return;
            }

            // Update files
            var notUpdated = new List<string>();
            using (var origFiles = ZipFile.OpenRead(origFilesZip))
            {
                foreach (var changedFile in themeChanges)
                {
                    var subpath = Common.Paths.FixSeparators(Regex.Replace(changedFile.Path, ".+Themes/(Desktop|Fullscreen)/Default/", ""));
                    var curThemePath = Path.Combine(themeDirectory, subpath);
                    var defaultPath = Path.Combine(defaultThemeDir, subpath);
                    if (changedFile.ChangeType == "D")
                    {
                        FileSystem.DeleteFile(curThemePath);
                    }
                    else
                    {
                        var canUpdate = false;
                        if (File.Exists(curThemePath))
                        {
                            var origEntry = origFiles.GetEntry(ThemeManager.GetThemeRootDir(mode) + "\\" + subpath);
                            if (origEntry == null)
                            {
                                canUpdate = false;
                            }
                            else
                            {
                                var origContent = string.Empty;
                                using (var reader = new StreamReader(origEntry.Open()))
                                {
                                    origContent = reader.ReadToEnd();
                                }

                                if (subpath.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (Xml.AreEqual(origContent, File.ReadAllText(curThemePath)))
                                    {
                                        canUpdate = true;
                                    }
                                }
                                else
                                {
                                    if (origContent == FileSystem.GetMD5(curThemePath))
                                    {
                                        canUpdate = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            canUpdate = true;
                        }

                        if (canUpdate)
                        {
                            FileSystem.CopyFile(defaultPath, curThemePath);
                        }
                        else
                        {
                            logger.Debug($"Can't update {subpath}.");
                            notUpdated.Add(subpath);
                        }
                    }
                }
            }

            if (notUpdated.HasItems())
            {
                logger.Warn("Couldn't update some theme files, please update them manually:");
                notUpdated.ForEach(a => logger.Warn(a));
            }

            // Update common files
            GenerateCommonThemeFiles(mode, themeDirectory);

            // Update manifest
            currentThemeMan.ThemeApiVersion = ThemeManager.GetApiVersion(mode).ToString(3);
            File.WriteAllText(themeManifestPath, Serialization.ToYaml(currentThemeMan));
        }

        public static List<string> GenerateCommonThemeFiles(ApplicationMode mode, string outDir)
        {
            var defaultThemeXamlFiles = new List<string>();

            // Modify paths in App.xaml
            var appXaml = XDocument.Load(Paths.GetThemeTemplateFilePath(mode, Themes.AppXamlName));
            foreach (var resDir in appXaml.Descendants().Where(a =>
                a.Name.LocalName == "ResourceDictionary" && a.Attribute("Source")?.Value.StartsWith("Themes") == true))
            {
                var val = resDir.Attribute("Source").Value.Replace($"Themes/{mode.GetDescription()}/Default/", "");
                resDir.Attribute("Source").Value = val;
                defaultThemeXamlFiles.Add(val.Replace('/', '\\'));
            }

            // Change localization file reference
            var langElem = appXaml.Descendants().First(a => a.Attribute("Source")?.Value.EndsWith(PlaynitePaths.EngLocSourceFileName) == true);
            langElem.Attribute("Source").Value = PlaynitePaths.EngLocSourceFileName;

            // Update theme project file
            XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";
            var csproj = XDocument.Load(Paths.GetThemeTemplateFilePath(mode, Themes.ThemeProjName));
            var groupRoot = new XElement(ns + "ItemGroup");
            csproj.Root.Add(groupRoot);

            foreach (var resDir in appXaml.Descendants().Where(a =>
               a.Name.LocalName == "ResourceDictionary" && a.Attribute("Source") != null))
            {
                groupRoot.Add(new XElement(ns + "Content",
                                new XAttribute("Include", resDir.Attribute("Source").Value.Replace('/', '\\')),
                                new XElement(ns + "Generator", "MSBuild:Compile"),
                                new XElement(ns + "SubType", "Designer")));
            }

            appXaml.Save(Path.Combine(outDir, Themes.AppXamlName));
            csproj.Save(Path.Combine(outDir, Themes.ThemeProjName));

            FileSystem.CopyFile(Paths.GetThemeTemplatePath(PlaynitePaths.EngLocSourceFileName), Path.Combine(outDir, PlaynitePaths.EngLocSourceFileName));
            FileSystem.CopyFile(Paths.GetThemeTemplateFilePath(mode, Themes.GlobalResourcesName), Path.Combine(outDir, Themes.GlobalResourcesName));
            FileSystem.CopyFile(Paths.GetThemeTemplateFilePath(mode, Themes.ThemeSlnName), Path.Combine(outDir, Themes.ThemeSlnName));

            var commonFontsDirs = Paths.GetThemeTemplatePath("Fonts");
            if (Directory.Exists(commonFontsDirs))
            {
                foreach (var fontFile in Directory.GetFiles(commonFontsDirs))
                {
                    var targetPath = Path.Combine(outDir, "Fonts", Path.GetFileName(fontFile));
                    FileSystem.CopyFile(fontFile, targetPath);
                }
            }

            var modeFontDir = Paths.GetThemeTemplateFilePath(mode, "Fonts");
            if (Directory.Exists(modeFontDir))
            {
                foreach (var fontFile in Directory.GetFiles(modeFontDir))
                {
                    var targetPath = Path.Combine(outDir, "Fonts", Path.GetFileName(fontFile));
                    FileSystem.CopyFile(fontFile, targetPath);
                }
            }

            return defaultThemeXamlFiles;
        }

        public static string GenerateNewTheme(ApplicationMode mode, string themeName)
        {
            var themeDirName = Common.Paths.GetSafeFilename(themeName).Replace(" ", string.Empty);
            var defaultThemeDir = Path.Combine(Paths.GetThemesPath(mode), "Default");
            var outDir = Path.Combine(PlaynitePaths.ThemesProgramPath, mode.GetDescription(), themeDirName);
            if (Directory.Exists(outDir))
            {
                throw new Exception($"Theme directory \"{outDir}\" already exists.");
            }

            FileSystem.CreateDirectory(outDir);
            var defaultThemeXamlFiles = GenerateCommonThemeFiles(mode, outDir);
            CopyThemeDirectory(defaultThemeDir, outDir, defaultThemeXamlFiles.Select(a => Path.Combine(defaultThemeDir, a)).ToList());

            var themeDesc = new ThemeDescription()
            {
                Author = "Your Name Here",
                Name = themeName,
                Version = "1.0",
                Mode = mode,
                ThemeApiVersion = ThemeManager.GetApiVersion(mode).ToString()
            };

            File.WriteAllText(Path.Combine(outDir, PlaynitePaths.ThemeManifestFileName), Serialization.ToYaml(themeDesc));
            Explorer.NavigateToFileSystemEntry(Path.Combine(outDir, Themes.ThemeSlnName));
            return outDir;
        }
    }
}