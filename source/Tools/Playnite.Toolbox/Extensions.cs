using Playnite.API;
using Playnite.Common;
using Playnite.Plugins;
using Playnite.SDK;
using System;
using System.Collections.Generic;
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
        private const string genericCsproj = "GenericPlugin.csproj";
        private const string libraryCsproj = "CustomLibraryPlugin.csproj";
        private const string metadataCsproj = "CustomMetadataPlugin.csproj";

        public static string GenerateScriptExtension(ScriptLanguage language, string name, string directory)
        {
            var extDirName = Common.Paths.GetSafeFilename(name).Replace(" ", string.Empty);
            var outDir = Path.Combine(directory, extDirName);
            if (Directory.Exists(outDir))
            {
                throw new Exception($"Extension already exists: {outDir}");
            }

            var templateArchive = Paths.GetScriptTemplateArchivePath(language);
            ZipFile.ExtractToDirectory(templateArchive, outDir);
            return outDir;
        }

        public static string GeneratePluginExtension(ExtensionType type, string name, string directory)
        {
            var normalizedName = Common.Paths.GetSafeFilename(name).Replace(" ", string.Empty);
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

            switch (type)
            {
                case ExtensionType.GenericPlugin:
                    File.Move(Path.Combine(outDir, genericCsproj), Path.Combine(outDir, $"{normalizedName}.csproj"));
                    break;
                case ExtensionType.GameLibrary:
                    File.Move(Path.Combine(outDir, libraryCsproj), Path.Combine(outDir, $"{normalizedName}.csproj"));
                    break;
                case ExtensionType.MetadataProvider:
                    File.Move(Path.Combine(outDir, metadataCsproj), Path.Combine(outDir, $"{normalizedName}.csproj"));
                    break;
            }

            return outDir;
        }

        public static string PackageExtension(string extDirectory, string targetPath)
        {
            var dirInfo = new DirectoryInfo(extDirectory);
            var extInfo = ExtensionFactory.GetDescriptionFromFile(Path.Combine(extDirectory, PlaynitePaths.ExtensionManifestFileName));
            var packedPath = Path.Combine(targetPath, $"{dirInfo.Name}_{extInfo.Version.ToString().Replace(".", "_")}{PlaynitePaths.PackedExtensionFileExtention}");
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

                        zipFile.CreateEntryFromFile(file, subName);
                    }
                }
            }

            return packedPath;
        }
    }
}