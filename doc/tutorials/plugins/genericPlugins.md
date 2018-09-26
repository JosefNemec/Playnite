Generic Plugins
=====================

To implement generic plugin:

* Create new public class describing [IGenericPlugin](xref:Playnite.SDK.Plugins.IGenericPlugin) interface.
* Add constructor accepting single [IPlayniteAPI](xref:Playnite.SDK.IPlayniteAPI) argument.
* Implement mandatory members from `IGenericPlugin`.

Mandatory members
---------------------

| Member | Description |
| -- | -- |
| Id | Unique plugin id. |

Optional members
---------------------

Return null or specify empty method if you don't want to provide implementation for these.

| Member | Description |
| -- | -- |
| Settings | [Plugin settings object.](pluginSettings.md) |
| SettingsView | [Plugin settings view.](pluginSettings.md) |
| GetFunctions | Adds executable menu entries into `Extensions` submenu in Playnite's main menu. |


Example plugin
---------------------

```csharp
public class TestPlugin : IGenericPlugin
{
    private ILogger logger = LogManager.GetLogger();

    private IPlayniteAPI api;

    public Guid Id { get; } = Guid.Parse("D51194CD-AA44-47A0-8B89-D1FD544DD9C8");

    public TestPlugin(IPlayniteAPI api)
    {
        this.api = api;
    }

    public void Dispose()
    {
        // Add code to be executed when plugin is being unloaded.
    }

    public IEnumerable<ExtensionFunction> GetFunctions()
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

    public void OnGameInstalled(Game game)
    {
        // Add code to be executed when game is finished installing.
    }

    public void OnGameStarted(Game game)
    {
        // Add code to be executed when game is started running.
    }

    public void OnGameStarting(Game game)
    {
        // Add code to be executed when game is preparing to be started.
    }

    public void OnGameStopped(Game game, long elapsedSeconds)
    {
        // Add code to be execute when game stops running.
    }

    public void OnGameUninstalled(Game game)
    {
        // Add code to be executed when game is uninstalled.
    }

    public void OnApplicationStarted()
    {
        // Add code to be execute when Playnite is initialized.
    }
}
```