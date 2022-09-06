using Playnite.API;
using Playnite.Common;
using Playnite.Plugins;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Toolbox
{
    public class Extensions
    {
        private const string nameReplaceMask = "_name_";
        private const string namespaceReplaceMask = "_namespace_";
        private const string guidReplaceMask = "00000000-0000-0000-0000-000000000001";
        private const string genericPluginProjectName = "GenericPlugin";
        private const string libraryPluginProjectName = "CustomLibraryPlugin";
        private const string metadataPluginProjectName = "CustomMetadataPlugin";

        public static string ConvertToValidIdentifierName(string input)
        {
            // https://stackoverflow.com/questions/950616/what-characters-are-allowed-in-c-sharp-class-name
            bool isValidInIdentifier(char c, bool firstChar = true)
            {
                switch (char.GetUnicodeCategory(c))
                {
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.TitlecaseLetter:
                    case UnicodeCategory.ModifierLetter:
                    case UnicodeCategory.OtherLetter:
                        // Always allowed in C# identifiers
                        return true;

                    case UnicodeCategory.LetterNumber:
                    case UnicodeCategory.NonSpacingMark:
                    case UnicodeCategory.SpacingCombiningMark:
                    case UnicodeCategory.DecimalDigitNumber:
                    case UnicodeCategory.ConnectorPunctuation:
                    case UnicodeCategory.Format:
                        // Only allowed after first char
                        return !firstChar;
                    default:
                        return false;
                }
            }

            if (input.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(input));
            }

            var sb = new StringBuilder(input.Length);
            for (int i = 0; i < input.Length; i++)
            {
                var chr = input[i];
                if (isValidInIdentifier(chr, i == 0))
                {
                    sb.Append(chr);
                }
            }

            return sb.ToString();
        }

        public static string GenerateScriptExtension(string name, string directory)
        {
            var normalizedName = ConvertToValidIdentifierName(name);
            var outDir = Path.Combine(directory, normalizedName);
            if (Directory.Exists(outDir))
            {
                throw new Exception($"Extension already exists: {outDir}");
            }

            var templateArchive = Path.Combine(PlaynitePaths.ProgramPath, "Templates", "Extensions", "PowerShellScript.zip"); ;
            ZipFile.ExtractToDirectory(templateArchive, outDir);
            var pluginId = Guid.NewGuid();
            foreach (var filePath in Directory.GetFiles(outDir, "*.*", SearchOption.AllDirectories))
            {
                var changed = false;
                var fileContent = File.ReadAllText(filePath, Encoding.UTF8);
                if (fileContent.Contains(nameReplaceMask))
                {
                    fileContent = fileContent.Replace(nameReplaceMask, normalizedName);
                    changed = true;
                }

                if (fileContent.Contains(guidReplaceMask))
                {
                    fileContent = fileContent.Replace(guidReplaceMask, pluginId.ToString());
                    changed = true;
                }

                if (changed)
                {
                    File.WriteAllText(filePath, fileContent, Encoding.UTF8);
                }

                if (filePath.Contains(nameReplaceMask))
                {
                    File.Move(filePath, filePath.Replace(nameReplaceMask, normalizedName));
                }
            }

            return outDir;
        }

        public static string GeneratePluginExtension(ExtensionType type, string name, string directory)
        {
            var normalizedName = Common.Paths.GetSafePathName(name).Replace(" ", string.Empty);
            var outDir = Path.Combine(directory, normalizedName);
            if (Directory.Exists(outDir))
            {
                throw new Exception($"Extension already exists: {outDir}");
            }

            var templateArchive = Paths.GetPluginTemplateArchivePath(type);
            ZipFile.ExtractToDirectory(templateArchive, outDir);
            var pluginId = Guid.NewGuid();

            foreach (var filePath in Directory.GetFiles(outDir, "*.*", SearchOption.AllDirectories))
            {
                var changed = false;
                var fileContent = File.ReadAllText(filePath, Encoding.UTF8);
                if (fileContent.Contains(nameReplaceMask))
                {
                    fileContent = fileContent.Replace(nameReplaceMask, normalizedName);
                    changed = true;
                }

                if (fileContent.Contains(namespaceReplaceMask))
                {
                    fileContent = fileContent.Replace(namespaceReplaceMask, normalizedName);
                    changed = true;
                }

                if (fileContent.Contains(guidReplaceMask))
                {
                    fileContent = fileContent.Replace(guidReplaceMask, pluginId.ToString());
                    changed = true;
                }

                if (changed)
                {
                    File.WriteAllText(filePath, fileContent, Encoding.UTF8);
                }

                if (filePath.Contains(nameReplaceMask))
                {
                    File.Move(filePath, filePath.Replace(nameReplaceMask, normalizedName));
                }
            }

            var outProjectFile = Path.Combine(outDir, normalizedName + ".csproj");
            var outSolutionFile = Path.Combine(outDir, normalizedName + ".sln");
            var baseProjectName = genericPluginProjectName;

            switch (type)
            {
                case ExtensionType.GenericPlugin:
                    baseProjectName = genericPluginProjectName;
                    break;
                case ExtensionType.GameLibrary:
                    baseProjectName = libraryPluginProjectName;
                    break;
                case ExtensionType.MetadataProvider:
                    baseProjectName = metadataPluginProjectName;
                    break;
            }

            File.Move(Path.Combine(outDir, baseProjectName + ".csproj"), outProjectFile);
            File.Move(Path.Combine(outDir, baseProjectName + ".sln"), outSolutionFile);
            FileSystem.ReplaceStringInFile(outProjectFile, baseProjectName, normalizedName);
            FileSystem.ReplaceStringInFile(outSolutionFile, baseProjectName, normalizedName);
            return outDir;
        }

        public static string PackageExtension(string extDirectory, string targetPath)
        {
            var dirInfo = new DirectoryInfo(extDirectory);
            var manifestPath = Path.Combine(extDirectory, PlaynitePaths.ExtensionManifestFileName);
            if (!File.Exists(manifestPath))
            {
                throw new Exception($"Manifest file ({PlaynitePaths.ExtensionManifestFileName}) not found!");
            }

            var extInfo = ExtensionInstaller.GetExtensionManifest(manifestPath);
            if (extInfo.Id.IsNullOrEmpty())
            {
                throw new Exception("Cannot package extension, ID is missing!");
            }

            extInfo.VerifyManifest();

            var packedPath = Path.Combine(targetPath, $"{Common.Paths.GetSafePathName(extInfo.Id).Replace(' ', '_')}_{extInfo.Version.ToString().Replace(".", "_")}{PlaynitePaths.PackedExtensionFileExtention}");
            FileSystem.PrepareSaveFile(packedPath);
            var ignoreFiles = File.ReadAllLines(Paths.ExtFileIgnoreListPath);

            using (var zipStream = new FileStream(packedPath, FileMode.Create))
            {
                using (var zipFile = new ZipArchive(zipStream, ZipArchiveMode.Create))
                {
                    foreach (var file in Directory.GetFiles(extDirectory, "*.*", SearchOption.AllDirectories))
                    {
                        var subName = file.Replace(extDirectory, "").TrimStart(Path.DirectorySeparatorChar);
                        if (ignoreFiles.ContainsString(subName, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        zipFile.CreateEntryFromFile(file, subName, CompressionLevel.Optimal);
                    }
                }
            }

            return packedPath;
        }
    }
}