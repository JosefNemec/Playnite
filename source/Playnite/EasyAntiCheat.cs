using Newtonsoft.Json;
using Playnite.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class EasyAntiCheatLauncherSettings
    {
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "executable")]
        public string Executable { get; set; }

        [JsonProperty(PropertyName = "logo_position")]
        public string LogoPosition { get; set; }

        [JsonProperty(PropertyName = "parameters")]
        public string Parameters { get; set; }

        [JsonProperty(PropertyName = "use_cmdline_parameters")]
        public string UseCmdlineParameters { get; set; }

        [JsonProperty(PropertyName = "working_directory")]
        public string WorkingDirectory { get; set; }

        [JsonProperty(PropertyName = "wait_for_game_process_exit")]
        public string WaitForGameProcessExit { get; set; }

        [JsonProperty(PropertyName = "hide_splash_screen")]
        public string HideSplashScreen { get; set; }

        [JsonProperty(PropertyName = "ide_ui_controls")]
        public string IdeUiControls { get; set; }
    }

    public class EasyAntiCheat
    {
        public static EasyAntiCheatLauncherSettings GetLauncherSettings(string gameDirectory)
        {
            var settingsPath = Path.Combine(gameDirectory, "EasyAntiCheat", "Launcher", "Settings.json");
            if (!File.Exists(settingsPath))
            {
                throw new FileNotFoundException($"EAC launcher settings not found: {settingsPath}");
            }

            return Serialization.FromJson<EasyAntiCheatLauncherSettings>(File.ReadAllText(settingsPath));
        }
    }
}
