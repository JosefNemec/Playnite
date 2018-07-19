//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Playnite.SDK.Models;
//using Microsoft.Win32;
//using System.IO;
//using Playnite.Metadata;

//namespace Playnite.Providers.Uplay
//{
//    public class UplayLibrary : IUplayLibrary
//    {
//        public GameTask GetGamePlayTask(string id)
//        {
//            return new GameTask()
//            {
//                Type = GameTaskType.URL,
//                Path = @"uplay://launch/" + id,
//                IsPrimary = true,
//                IsBuiltIn = true
//            };
//        }

//        public List<Game> GetInstalledGames()
//        {
//            var games = new List<Game>();

//            var root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
//            var installsKey = root.OpenSubKey(@"SOFTWARE\ubisoft\Launcher\Installs\");
//            if (installsKey == null)
//            {
//                root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
//                installsKey = root.OpenSubKey(@"SOFTWARE\ubisoft\Launcher\Installs\");

//                if (installsKey == null)
//                {
//                    return games;
//                }
//            }

//            foreach (var install in installsKey.GetSubKeyNames())
//            {
//                var gameData = installsKey.OpenSubKey(install);
//                var installDir = (gameData.GetValue("InstallDir") as string).Replace('/', Path.DirectorySeparatorChar);

//                var newGame = new Game()
//                {
//                    GameId = install,
//                    Provider = Provider.Uplay,
//                    Source = Enums.GetEnumDescription(Provider.Uplay),
//                    InstallDirectory = installDir,
//                    PlayTask = GetGamePlayTask(install),
//                    Name = Path.GetFileName(installDir.TrimEnd(Path.DirectorySeparatorChar))
//                };

//                games.Add(newGame);
//            }

//            return games;
//        }

//        public GameMetadata UpdateGameWithMetadata(Game game)
//        {
//            var metadata = new GameMetadata();
//            var program = Programs.GetUnistallProgramsList().FirstOrDefault(a => a.RegistryKeyName == "Uplay Install " + game.GameId);
//            if (program == null)
//            {
//                return metadata;
//            }

//            if (!string.IsNullOrEmpty(program.DisplayIcon) && File.Exists(program.DisplayIcon))
//            {
//                var iconPath = program.DisplayIcon;
//                var iconFile = Path.GetFileName(iconPath);
//                var data = File.ReadAllBytes(iconPath);
//                metadata.Icon = new MetadataFile($"images/uplay/{game.GameId}/{iconFile}", iconFile, data);
//            }

//            game.Name = StringExtensions.NormalizeGameName(program.DisplayName);
//            return metadata;
//        }
//    }
//}
