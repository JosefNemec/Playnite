using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Playnite.SDK.Models;
using SteamKit2;

namespace SteamLibrary
{
    internal class ModInfo
    {
        public enum ModType
        {
            HL,
            HL2,
        }

        public string Name { get; private set; }
        public GameID GameId { get; private set; }
        public string InstallFolder { get; private set; }
        public List<string> Categories { get; private set; }
        public string Developer { get; private set; }
        public List<Link> Links { get; private set; }
        public string IconPath { get; private set; }
        private readonly ModType modType;

        static private Dictionary<ModType, string> infoFileName = new Dictionary<ModType, string>()
        {
            { ModType.HL, "liblist.gam"},
            { ModType.HL2, "gameinfo.txt"}
        };

        // HL mods all use Half-Life as the base app. HL2 mods are split across multiple
        // SDK base apps. This will be our differentiator
        static private readonly uint halfLife = 70;

        public bool IsInstalled
        {
            get
            {
                return File.Exists(Path.Combine(InstallFolder, infoFileName[modType]));
            }
        }

        private ModInfo(ModType type, string installFolder)
        {
            modType = type;
            InstallFolder = installFolder;
            Name = "Unknown Mod";
            Links = new List<Link>();
            GameId = new GameID();
            Developer = "Unknown";
            Categories = new List<string>();
        }

        static public ModInfo GetFromGameID(GameID gameID)
        {
            if (!gameID.IsMod)
            {
                return null;
            }

            if (gameID.AppID == halfLife && !string.IsNullOrEmpty(Steam.ModInstallPath))
            {
                var dirInfo = new DirectoryInfo(Steam.ModInstallPath);

                foreach (var folder in dirInfo.GetDirectories())
                {
                    if (GetModFolderCRC(folder.Name) == gameID.ModID)
                    {
                        return GetFromFolder(folder.FullName, ModType.HL);
                    }
                }
            }
            else if (gameID.AppID != halfLife && !string.IsNullOrEmpty(Steam.SourceModInstallPath))
            {
                var dirInfo = new DirectoryInfo(Steam.SourceModInstallPath);

                foreach (var folder in dirInfo.GetDirectories())
                {
                    if (GetModFolderCRC(folder.Name) == gameID.ModID)
                    {
                        return GetFromFolder(folder.FullName, ModType.HL2);
                    }
                }
            }

            return null;
        }

        static public ModInfo GetFromFolder(string path, ModType modType)
        {
            if (!Directory.Exists(path))
            {
                return null;
            }

            var dirInfo = new DirectoryInfo(path);
            var gameInfoPath = Path.Combine(path, infoFileName[modType]);
            if (!File.Exists(gameInfoPath))
            {
                return null;
            }

            ModInfo modInfo = new ModInfo(modType, path);
            if (modType == ModType.HL)
            {
                modInfo.GameId.AppID = halfLife;
                PopulateModInfoFromLibList(ref modInfo, gameInfoPath);
            }
            else
            {
                PopulateModInfoFromGameInfo(ref modInfo, gameInfoPath);
            }

            modInfo.GameId.AppType = GameID.GameType.GameMod;
            modInfo.GameId.ModID = GetModFolderCRC(dirInfo.Name);

            return modInfo;
        }

        static private uint GetModFolderCRC(string folder)
        {
            uint crc = BitConverter.ToUInt32(CryptoHelper.CRCHash(Encoding.ASCII.GetBytes(folder)), 0);

            // For mods and shortcut game IDs, the high bit is always set. SteamKit doesn't do this automatically (yet).
            crc |= 0x80000000;

            return crc;
        }

        static private void PopulateModInfoFromGameInfo(ref ModInfo modInfo, string path)
        {
            var gameInfo = new KeyValue();
            gameInfo.ReadFileAsText(path);

            modInfo.GameId.AppID = gameInfo["FileSystem"]["SteamAppId"].AsUnsignedInteger();
            modInfo.Name = gameInfo["game"].Value;

            if (gameInfo["developer"] != KeyValue.Invalid)
            {
                modInfo.Developer = gameInfo["developer"].Value;
            }

            if (gameInfo["manual"] != KeyValue.Invalid)
            {
                modInfo.Links.Add(new Link("Manual", gameInfo["manual"].Value));
            }

            if (gameInfo["developer_url"] != KeyValue.Invalid)
            {
                modInfo.Links.Add(new Link("Developer URL", gameInfo["developer_url"].Value));
            }

            if (gameInfo["type"] == KeyValue.Invalid || gameInfo["type"].Value == "singleplayer_only")
            {
                modInfo.Categories.Add("Single-Player");
            }

            if (gameInfo["type"] == KeyValue.Invalid || gameInfo["type"].Value == "multiplayer_only")
            {
                modInfo.Categories.Add("Multi-Player");
            }

            modInfo.IconPath = FindIcon(modInfo.InstallFolder, gameInfo["icon"] == KeyValue.Invalid ? null : gameInfo["icon"].Value);
        }

        static private void PopulateModInfoFromLibList(ref ModInfo modInfo, string path)
        {
            using (var reader = new StreamReader(path))
            {
                string type = null, icon = null;

                var pattern = new Regex("\\s*(\\w+)\\s+\"([^\"]*)\".*", RegexOptions.Singleline);

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var match = pattern.Match(line);
                    if (match.Success)
                    {
                        if (match.Groups[1].Value == "game")
                        {
                            modInfo.Name = match.Groups[2].Value;
                        }
                        else if (match.Groups[1].Value == "manual")
                        {
                            modInfo.Links.Add(new Link("Manual", match.Groups[2].Value));
                        }
                        else if (match.Groups[1].Value == "developer_url")
                        {
                            modInfo.Links.Add(new Link("Developer URL", match.Groups[2].Value));
                        }
                        else if (match.Groups[1].Value == "type")
                        {
                            type = match.Groups[2].Value;
                        }
                        // These ones don't display in the Steam client (anymore?), but many
                        // mods seem to have them.
                        else if (match.Groups[1].Value == "url_info")
                        {
                            modInfo.Links.Add(new Link("Info", match.Groups[2].Value));
                        }
                        else if (match.Groups[1].Value == "url_dl")
                        {
                            modInfo.Links.Add(new Link("Download", match.Groups[2].Value));
                        }
                        else if (match.Groups[1].Value == "icon")
                        {
                            icon = match.Groups[2].Value;
                        }
                    }
                }

                if (type == null || type == "singleplayer_only")
                {
                    modInfo.Categories.Add("Single-Player");
                }

                if (type == null || type == "multiplayer_only")
                {
                    modInfo.Categories.Add("Multi-Player");
                }

                modInfo.IconPath = FindIcon(modInfo.InstallFolder, icon);
            }
        }

        static public ModType GetModTypeOfGameID(GameID gameId)
        {
            return gameId.AppID == halfLife ? ModType.HL : ModType.HL2;
        }

        static private string FindIcon(string modPath, string rawIconPath)
        {
            if (rawIconPath != null)
            {
                rawIconPath = rawIconPath.Replace('/', Path.DirectorySeparatorChar);

                // Gameinfo specifies an icon path. This is what Steam would use for all mod types, so try to do the same
                var steamTgaBig = Path.Combine(modPath, rawIconPath) + "_big.tga";
                if (File.Exists(steamTgaBig))
                {
                    return steamTgaBig;
                }

                var steamTga = Path.Combine(modPath, rawIconPath) + ".tga";
                if (File.Exists(steamTga))
                {
                    return steamTga;
                }
            }
            else
            {
                // Some HL1 mods seem to use this instead of specifying. (Maybe is formerly worked in Steam?)
                var gameTga = Path.Combine(modPath, "game.tga");
                if (File.Exists(gameTga))
                {
                    return gameTga;
                }

                var gameIco = Path.Combine(modPath, "game.ico");
                if (File.Exists(gameIco))
                {
                    return gameIco;
                }
            }

            return null;
        }
    }
}
