using System.IO;
using System.Text.Json.Serialization;

namespace Playnite;

public class EasyAntiCheatLauncherSettings
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("executable")]
    public string? Executable { get; set; }

    [JsonPropertyName("logo_position")]
    public string? LogoPosition { get; set; }

    [JsonPropertyName("parameters")]
    public string? Parameters { get; set; }

    [JsonPropertyName("use_cmdline_parameters")]
    public string? UseCmdlineParameters { get; set; }

    [JsonPropertyName("working_directory")]
    public string? WorkingDirectory { get; set; }

    [JsonPropertyName("wait_for_game_process_exit")]
    public string? WaitForGameProcessExit { get; set; }

    [JsonPropertyName("hide_splash_screen")]
    public string? HideSplashScreen { get; set; }

    [JsonPropertyName("ide_ui_controls")]
    public string? IdeUiControls { get; set; }
}

public class EasyAntiCheat
{
    public static EasyAntiCheatLauncherSettings? GetLauncherSettings(string gameDirectory)
    {
        var settingsPath = Path.Combine(gameDirectory, "EasyAntiCheat", "Launcher", "Settings.json");
        if (!File.Exists(settingsPath))
        {
            throw new FileNotFoundException($"EAC launcher settings not found: {settingsPath}");
        }

        return Serialization.FromJson<EasyAntiCheatLauncherSettings>(File.ReadAllText(settingsPath));
    }
}