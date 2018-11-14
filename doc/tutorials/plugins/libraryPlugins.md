Library Plugins
=====================

To implement library plugin:

* Create new public class describing [ILibraryPlugin](xref:Playnite.SDK.Plugins.ILibraryPlugin) interface.
* Add constructor accepting single [IPlayniteAPI](xref:Playnite.SDK.IPlayniteAPI) argument.
* Implement mandatory members from `ILibraryPlugin`.

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

Optional members
---------------------

Return null or specify empty method if you don't want to provide implementation for these.

| Member | Description |
| -- | -- |
| Client | Returns client application for this library. If present adds entry into `Open 3rd party client` menu. |
| LibraryIcon | Default library icon shown if no game icon is available. |
| GetSettings | [Plugin settings object.](pluginSettings.md) |
| GetSettingsView | [Plugin settings view.](pluginSettings.md) |
| GetGameController | Custom controller handling actions like game execution and installation. |
| GetMetadataDownloader | Custom metadata downloader used when `Official Store` source is used when downloading metadata. |

Example plugin
---------------------

```csharp
public class LibraryPlugin : ILibraryPlugin
{
    public LibraryPlugin(IPlayniteAPI api)
    {
    }

    public void Dispose()
    {
        // Add code to be executed when plugin is being unloaded.
    }

    public Guid Id { get; } = Guid.Parse("CB91DFC9-B977-43BF-8E70-55F46E410FCC");

    public string Name { get; } = "LibraryPlugin";

    public ILibraryClient Client { get; }

    public string LibraryIcon { get; }

    public ISettings GetSettings(bool firstRunSettings)
    {
        return null;
    }

    public UserControl GetSettingsView(bool firstRunView)
    {
        return null
    }

    public IEnumerable<Game> GetGames()
    {
        // Return list of library games.
    }

    public IGameController GetGameController(Game game)
    {
        return null;
    }

    public ILibraryMetadataProvider GetMetadataDownloader()
    {
        return null;        
    }
}
```