using Playnite;
using Playnite.Common;
using Playnite.SDK.Models;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UplayLibrary.Models;

namespace UplayLibrary
{
    public class Uplay
    {
        public const string AssertUrlBase = @"https://ubistatic3-a.akamaihd.net/orbit/uplay_launcher_3_0/assets/";

        public static string ClientExecPath
        {
            get
            {
                var path = InstallationPath;
                return string.IsNullOrEmpty(path) ? string.Empty : Path.Combine(path, "Uplay.exe");
            }
        }

        public static string InstallationPath
        {
            get
            {
                var progs = Programs.GetUnistallProgramsList().FirstOrDefault(a => a.DisplayName == "Uplay");
                if (progs == null)
                {
                    return string.Empty;
                }
                else
                {
                    return progs.InstallLocation;
                }
            }
        }

        public static string ConfigurationsCachePath
        {
            get
            {
                var installPath = InstallationPath;
                if (installPath.IsNullOrEmpty())
                {
                    return string.Empty;
                }
                else
                {
                    return Path.Combine(installPath, "cache", "configuration", "configurations");
                }
            }
        }

        public static bool IsInstalled
        {
            get
            {
                var progs = Programs.GetUnistallProgramsList().FirstOrDefault(a => a.DisplayName == "Uplay");
                if (progs == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public static string Icon => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\uplayicon.png");

        public static bool GetGameRequiresUplay(Game game)
        {
            if (string.IsNullOrEmpty(game.InstallDirectory) || !Directory.Exists(game.InstallDirectory))
            {
                return false;
            }

            var fileEnumerator = new SafeFileEnumerator(game.InstallDirectory, "uplay_*_loader*", SearchOption.AllDirectories);
            return fileEnumerator.Any() == true;
        }

        public static List<ProductInformation> GetLocalProductCache()
        {
            var products = new List<ProductInformation>();
            var cachePath = ConfigurationsCachePath;
            if (!File.Exists(cachePath))
            {
                throw new FileNotFoundException("Uplay client was not initialized, please start the client at least once.");
            }

            using (var file = File.OpenRead(cachePath))
            {
                foreach (var item in Serializer.Deserialize<UplayCacheGameCollection>(file).Games)
                {
                    if (!item.GameInfo.IsNullOrEmpty())
                    {
                        var productInfo = Serialization.FromYaml<ProductInformation>(item.GameInfo);
                        var root = productInfo.root;
                        var loc = productInfo.localizations?.@default ?? new Dictionary<string, string>();
                        if (!root.name.IsNullOrEmpty())
                        {
                            if (loc.TryGetValue(root.name, out var locValue))
                            {
                                root.name = locValue;
                            }
                        }

                        if (!root.background_image.IsNullOrEmpty())
                        {
                            if (loc.TryGetValue(root.background_image, out var locValue))
                            {
                                root.background_image = locValue;
                            }

                            root.background_image = AssertUrlBase + root.background_image;
                        }

                        if (!root.thumb_image.IsNullOrEmpty())
                        {
                            if (loc.TryGetValue(root.thumb_image, out var locValue))
                            {
                                root.thumb_image = locValue;
                            }

                            root.thumb_image = AssertUrlBase + root.thumb_image;
                        }

                        if (!root.logo_image.IsNullOrEmpty())
                        {
                            if (loc.TryGetValue(root.logo_image, out var locValue))
                            {
                                root.logo_image = locValue;
                            }

                            root.logo_image = AssertUrlBase + root.logo_image;
                        }

                        if (!root.dialog_image.IsNullOrEmpty())
                        {
                            if (loc.TryGetValue(root.dialog_image, out var locValue))
                            {
                                root.dialog_image = locValue;
                            }

                            root.dialog_image = AssertUrlBase + root.dialog_image;
                        }

                        if (!root.icon_image.IsNullOrEmpty())
                        {
                            if (loc.TryGetValue(root.icon_image, out var locValue))
                            {
                                root.icon_image = locValue;
                            }

                            root.icon_image = AssertUrlBase + root.icon_image;
                        }

                        productInfo.uplay_id = item.UplayId;
                        productInfo.install_id = item.InstallId;
                        products.Add(productInfo);
                    }
                }
            }

            return products;
        }
    }
}
