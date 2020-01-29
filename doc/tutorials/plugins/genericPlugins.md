Generic Plugins
=====================

To implement generic plugin:

* Read the introduction to [extensions](../intro.md) and [plugins](plugins.md).
* Create new public class inheriting from [Plugin](xref:Playnite.SDK.Plugins.Plugin) abstract class.
* Add implementation for mandatory abstract members.

Mandatory members
---------------------

| Member | Description |
| -- | -- |
| Id | Unique plugin id. |

Optional members
---------------------

You can implement additional functionality by overriding virtual methods from [Plugin](xref:Playnite.SDK.Plugins.Plugin) base class.

Example plugin
---------------------

```csharp
public class TestPlugin : Plugin
{
    private ILogger logger;

    public override Guid Id { get; } = Guid.Parse("D51194CD-AA44-47A0-8B89-D1FD544DD9C9");

    public TestPlugin(IPlayniteAPI api) : base(api)
    {
        logger = api.CreateLogger();
    }

    public override IEnumerable<ExtensionFunction> GetFunctions()
    {
        return new List<ExtensionFunction>()
        {
            new ExtensionFunction(
                "Execute function from TestPlugin",
                () =>
                {
                    // Add code to be execute when user invokes this menu entry.
                })
        };
    }

    public override void OnGameInstalled(Game game)
    {
        // Add code to be executed when game is finished installing.
    }

    public override void OnGameStarted(Game game)
    {
        // Add code to be executed when game is started running.
    }

    public override void OnGameStarting(Game game)
    {
        // Add code to be executed when game is preparing to be started.
    }

    public override void OnGameStopped(Game game, long elapsedSeconds)
    {
        // Add code to be executed when game is preparing to be started.
    }

    public override void OnGameUninstalled(Game game)
    {
        // Add code to be executed when game is uninstalled.
    }

    public override void OnApplicationStarted()
    {
        // Add code to be execute when Playnite is initialized.
    }

    public override void OnApplicationStopped()
    {
        // Add code to be executed when Playnite is shutting down.
    }

    public override void OnLibraryUpdated()
    {
        // Add code to be execute when library is updated.
    }
}
```