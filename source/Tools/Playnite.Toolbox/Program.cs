using CommandLine;
using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Playnite.Toolbox
{
    class Program
    {
        public static List<string> PackageFileBlackList = new List<string>
        {
            ThemeManager.ThemeManifestFileName,
            Themes.ThemeProjName,
            Themes.ThemeSlnName,
            Themes.AppXamlName,
            Themes.GlobalResourcesName,
            Themes.LocSourceName
        };

        public static bool AreFilesEqual(string file1, string file2)
        {
            var file1Info = new FileInfo(file1);
            var file2Info = new FileInfo(file2);
            if (file1Info.Length != file2Info.Length)
            {
                return false;
            }

            return FileSystem.GetMD5(file1) == FileSystem.GetMD5(file2);
            // Check xaml content
        }

        public static string PackageTheme(string themeDirectory, string targetPath, ApplicationMode mode)
        {
            var defaultThemeDir = Path.Combine(Paths.GetThemesPath(mode), "Default");
            targetPath = Path.Combine(targetPath, Path.GetFileName(themeDirectory) + ThemeManager.PackedThemeFileExtention);
            FileSystem.PrepareSaveFile(targetPath);
            using (var zipStream = new FileStream(targetPath, FileMode.Create))
            {
                using (var zipFile = new ZipArchive(zipStream, ZipArchiveMode.Create))
                {
                    zipFile.CreateEntryFromFile(Path.Combine(themeDirectory, ThemeManager.ThemeManifestFileName), ThemeManager.ThemeManifestFileName);

                    foreach (var file in Directory.GetFiles(themeDirectory, "*.*", SearchOption.AllDirectories))
                    {
                        var subName = file.Replace(themeDirectory, "").TrimStart(Path.DirectorySeparatorChar);
                        if (file == targetPath ||
                            PackageFileBlackList.ContainsString(subName) ||
                            subName.StartsWith("Fonts\\") ||
                            subName.StartsWith(".vs\\") ||
                            subName.StartsWith("bin\\") ||
                            subName.StartsWith("obj\\"))
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

        public static void UpdateTheme(string themeDirectory, ApplicationMode mode)
        {
            var defaultThemeDir = Path.Combine(Paths.GetThemesPath(mode), "Default");
            foreach (var file in Directory.GetFiles(defaultThemeDir, "*.*", SearchOption.AllDirectories))
            {
                var subName = file.Replace(defaultThemeDir, "");
            }
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
            var defaultThemeXamlFiles = new List<string>();

            // Modify paths in App.xaml
            var appXaml = XDocument.Load(Paths.GetThemeTemplateFilePath(mode, Themes.AppXamlName));
            foreach (var resDir in appXaml.Descendants().Where(a =>
                a.Name.LocalName == "ResourceDictionary" && a.Attribute("Source")?.Value.StartsWith("Themes") == true))
            {
                var val = resDir.Attribute("Source").Value.Replace($"Themes/{mode.GetDescription()}/Default/", "");
                resDir.Attribute("Source").Value = val;
                defaultThemeXamlFiles.Add(val);
            }

            // Change localization file reference
            var langElem = appXaml.Descendants().First(a => a.Attribute("Source")?.Value.EndsWith(Themes.LocSourceName) == true);
            langElem.Attribute("Source").Value = Themes.LocSourceName;

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

            // Copy to output
            FileSystem.CopyDirectory(defaultThemeDir, outDir);
            appXaml.Save(Path.Combine(outDir, Themes.AppXamlName));
            csproj.Save(Path.Combine(outDir, Themes.ThemeProjName));

            FileSystem.CopyFile(Paths.GetThemeTemplatePath(Themes.LocSourceName), Path.Combine(outDir, Themes.LocSourceName));
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

            var themeDesc = new ThemeDescription()
            {
                Author = "Your Name Here",
                Name = themeName,
                Version = "1.0",
                Mode = mode
            };

            File.WriteAllText(Path.Combine(outDir, ThemeManager.ThemeManifestFileName), Serialization.ToYaml(themeDesc));
            Explorer.NavigateToFileSystemEntry(Path.Combine(outDir, Themes.ThemeSlnName));
            return outDir;
        }

        static void Main(string[] args)
        {
            var cmdlineParser = new Parser(with => with.CaseInsensitiveEnumValues = true);
            var result = cmdlineParser.ParseArguments<NewCmdLineOptions, PackCmdLineOptions>(args)
                .WithParsed<NewCmdLineOptions>(ProcessNewOptions)
                .WithParsed<PackCmdLineOptions>(ProcessPackOptions);
            if (result.Tag == ParserResultType.NotParsed)
            {
                Console.Write("No acceptable arguments given.");
            }
        }

        public static void ProcessNewOptions(NewCmdLineOptions options)
        {
            if (options.Type == ItemType.Theme)
            {
                var mode = options.TargetType.Equals("desktop", StringComparison.OrdinalIgnoreCase) ? ApplicationMode.Desktop : ApplicationMode.Fullscreen;
                try
                {
                    var path = GenerateNewTheme(mode, options.Name);
                    Console.WriteLine($"Created new theme in \"{path}\"");
                    Console.WriteLine($"Don't forget to update \"{ThemeManager.ThemeManifestFileName}\" with relevant information.");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to create new theme." + Environment.NewLine + e.Message);
                }
            }
        }

        public static void ProcessPackOptions(PackCmdLineOptions options)
        {
            if (options.Type == ItemType.Theme)
            {
                try
                {
                    var mode = options.TargetType.Equals("desktop", StringComparison.OrdinalIgnoreCase) ? ApplicationMode.Desktop : ApplicationMode.Fullscreen;
                    var sourceDir = Path.Combine(Paths.GetThemesPath(mode), options.Name);
                    var path = PackageTheme(sourceDir, options.DestinationPath, mode);
                    Console.WriteLine($"Theme successfully packed in \"{path}\"");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to pack theme file." + Environment.NewLine + e.Message);
                }
            }
        }
    }
}
