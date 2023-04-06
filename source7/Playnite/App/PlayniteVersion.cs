using System.Reflection;

namespace Playnite;

public class PlayniteVersion
{
    public static Version CurrentVersion { get; }

    static PlayniteVersion()
    {
        CurrentVersion = Assembly.GetExecutingAssembly()!.GetName()!.Version!;
    }
}