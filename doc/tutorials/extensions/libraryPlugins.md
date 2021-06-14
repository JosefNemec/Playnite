Library Plugins
=====================

To implement library plugin:

* Read the introduction to [extensions](../intro.md) and [plugins](plugins.md).
* Create new public class inheriting from [LibraryPlugin](xref:Playnite.SDK.Plugins.LibraryPlugin) abstract class.
* Add implementation for mandatory abstract members.

Mandatory members
---------------------

| Member | Description |
| -- | -- |
| Id | Unique plugin id. |
| Name | Library name. |
| GetGames | Return games available in library. |

`GetGames` returns list of [Game](xref:Playnite.SDK.Models.Game) objects and these properties must be set correctly by the plugin in order for game to be imported properly:

| Member | Description |
| -- | -- |
| GameId | Unique identifier used to differentiate games of the same plugin. |
| PluginId | Source Id of the plugin importing game. |
| PlayAction | Game action used to start the game. Only if game is reported as installed via `State` property. |
| InstallDirectory | Installation location. Only if game is reported as installed via `State` property.  |

You can implement additional functionality by overriding virtual methods from [LibraryPlugin](xref:Playnite.SDK.Plugins.LibraryPlugin) base class.

Capabilities
---------------------

If you want to provide extra features for specific library integration, like ability to close third party client after the game is close, then implemented `Capabilities` property that represents [LibraryPluginCapabilities](xref:Playnite.SDK.Plugins.LibraryPluginCapabilities).

### Supported capabilities

| Capability | Description |
| -- | -- |
| CanShutdownClient | When supported, library's client object has to implement `Shutdown` method. |
| HasCustomizedGameImport | Specifies that library is in full control over the game import mechanism. In this case the library should implement `ImportGames` method instead of `GetGames`.  |

Example plugin
---------------------

```csharp
public class LibraryPlugin : LibraryPlugin
{
    public override Guid Id { get; } = Guid.Parse("D625A3B7-1AA4-41CB-9CD7-74448D28E99B");

    public override string Name { get; } = "Test Library";

    public TestGameLibrary(IPlayniteAPI api) : base (api)
    {
    }

    public override IEnumerable<GameInfo> GetGames()
    {
        return new List<GameInfo>()
        {
            new GameInfo()
            {
                Name = "Notepad",
                GameId = "notepad",
                PlayAction = new GameAction()
                {
                    Type = GameActionType.File,
                    Path = "notepad.exe"
                },
                IsInstalled = true,
                Icon = @"c:\Windows\notepad.exe"
            },
            new GameInfo()
            {
                Name = "Calculator",
                GameId = "calc",
                PlayAction = new GameAction()
                {
                    Type = GameActionType.File,
                    Path = "calc.exe"
                },
                IsInstalled = true,
                Icon = @"https://playnite.link/applogo.png",
                BackgroundImage =  @"https://playnite.link/applogo.png"
            }
        };
    }
}
```