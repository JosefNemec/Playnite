using CommandLine;
using Playnite.API;
using Playnite.Common;
using Playnite.Plugins;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Playnite.Toolbox
{
    class Program
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            logger.Debug("Toolbox started.");
            logger.Debug(Environment.CommandLine);

            var cmdlineParser = new Parser(with => with.CaseInsensitiveEnumValues = true);
            var result = cmdlineParser.ParseArguments<NewCmdLineOptions, PackCmdLineOptions, UpdateCmdLineOptions>(args)
                .WithParsed<NewCmdLineOptions>(ProcessNewOptions)
                .WithParsed<PackCmdLineOptions>(ProcessPackOptions)
                .WithParsed<UpdateCmdLineOptions>(ProcessUpdateOptions);
            if (result.Tag == ParserResultType.NotParsed)
            {
                logger.Error("No acceptable arguments given.");
            }

            if (Debugger.IsAttached)
            {
                Console.ReadLine();
            }
        }

        public static ItemType GetExtensionType(string directory)
        {
            var themeMan = Path.Combine(directory, PlaynitePaths.ThemeManifestFileName);
            var extMan = Path.Combine(directory, PlaynitePaths.ExtensionManifestFileName);
            if (File.Exists(themeMan))
            {
                var desc = ThemeManager.GetDescriptionFromFile(themeMan);
                switch (desc.Mode)
                {
                    case ApplicationMode.Desktop:
                        return ItemType.DesktopTheme;
                    case ApplicationMode.Fullscreen:
                        return ItemType.FullscreenTheme;
                }
            }
            else if (File.Exists(extMan))
            {
                var desc = ExtensionFactory.GetDescriptionFromFile(extMan);
                switch (desc.Type)
                {
                    case ExtensionType.GenericPlugin:
                        return ItemType.GenericPlugin;
                    case ExtensionType.GameLibrary:
                        return ItemType.LibraryPlugin;
                    case ExtensionType.Script:
                        return desc.Module.EndsWith("ps1", StringComparison.OrdinalIgnoreCase) ? ItemType.PowerShellScript : ItemType.IronPythonScript;
                    case ExtensionType.MetadataProvider:
                        return ItemType.MetadataPlugin;
                }
            }

            return ItemType.Uknown;
        }

        public static void ProcessNewOptions(NewCmdLineOptions options)
        {
            try
            {
                var outPath = string.Empty;
                switch (options.Type)
                {
                    case ItemType.DesktopTheme:
                        outPath = Themes.GenerateNewTheme(ApplicationMode.Desktop, options.Name);
                        break;
                    case ItemType.FullscreenTheme:
                        outPath = Themes.GenerateNewTheme(ApplicationMode.Fullscreen, options.Name);
                        break;
                    case ItemType.PowerShellScript:
                        outPath = Extensions.GenerateScriptExtension(ScriptLanguage.PowerShell, options.Name, options.OutDirectory);
                        break;
                    case ItemType.IronPythonScript:
                        outPath = Extensions.GenerateScriptExtension(ScriptLanguage.IronPython, options.Name, options.OutDirectory);
                        break;
                    case ItemType.GenericPlugin:
                        outPath = Extensions.GeneratePluginExtension(ExtensionType.GenericPlugin, options.Name, options.OutDirectory);
                        break;
                    case ItemType.MetadataPlugin:
                        outPath = Extensions.GeneratePluginExtension(ExtensionType.MetadataProvider, options.Name, options.OutDirectory);
                        break;
                    case ItemType.LibraryPlugin:
                        outPath = Extensions.GeneratePluginExtension(ExtensionType.GameLibrary, options.Name, options.OutDirectory);
                        break;
                    default:
                        throw new NotSupportedException($"Uknown extension type {options.Type}.");
                }

                logger.Info($"Created new {options.Type} in \"{outPath}\"");
                logger.Info($"Don't forget to update manifest file with relevant information.");
            }
            catch (Exception e) when (!Debugger.IsAttached)
            {
                logger.Error(e, $"Failed to create new {options.Type}." + Environment.NewLine + e.Message);
            }
        }

        public static void ProcessPackOptions(PackCmdLineOptions options)
        {
            try
            {
                var outPath = string.Empty;
                var type = GetExtensionType(options.Directory);
                switch (type)
                {
                    case ItemType.DesktopTheme:
                        outPath = Themes.PackageTheme(options.Directory, options.Destination, ApplicationMode.Desktop);
                        break;
                    case ItemType.FullscreenTheme:
                        outPath = Themes.PackageTheme(options.Directory, options.Destination, ApplicationMode.Fullscreen);
                        break;
                    case ItemType.PowerShellScript:
                    case ItemType.IronPythonScript:
                    case ItemType.GenericPlugin:
                    case ItemType.MetadataPlugin:
                    case ItemType.LibraryPlugin:
                        outPath = Extensions.PackageExtension(options.Directory, options.Destination);
                        break;
                    case ItemType.Uknown:
                        throw new NotSupportedException();
                }

                logger.Info($"{type} successfully packed as \"{outPath}\"");
            }
            catch (Exception e) when (!Debugger.IsAttached)
            {
                logger.Error(e, $"Failed to pack extension." + Environment.NewLine + e.Message);
            }
        }

        public static void ProcessUpdateOptions(UpdateCmdLineOptions options)
        {
            try
            {
                var type = GetExtensionType(options.Directory);
                switch (type)
                {
                    case ItemType.DesktopTheme:
                        Themes.UpdateTheme(options.Directory, ApplicationMode.Desktop);
                        break;
                    case ItemType.FullscreenTheme:
                        Themes.UpdateTheme(options.Directory, ApplicationMode.Desktop);
                        break;
                    case ItemType.Uknown:
                    case ItemType.PowerShellScript:
                    case ItemType.IronPythonScript:
                    case ItemType.GenericPlugin:
                    case ItemType.MetadataPlugin:
                    case ItemType.LibraryPlugin:
                        throw new NotSupportedException();
                }
            }
            catch (Exception e) when (!Debugger.IsAttached)
            {
                logger.Error(e, "Failed to update extension." + Environment.NewLine + e.Message);
            }
        }
    }
}