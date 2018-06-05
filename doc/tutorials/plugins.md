Plugins Introduction
=====================

Basics
---------------------

Plugins use the same Playnite API as [scripts](scripting.md). All available interfaces and classes are exactly the same as is their use. For those reasons this plugin documentation mainly focuses on how to compile plugins and other differences compared to scripting. For specific API use cases see [scripts](scripting.md) documentation sections.

Plugins can be written in any .NET Framework compatible languages, this includes C#, VB.NET, F# and others, targeting .NET Framework 4.6.

Creating Plugin
---------------------

Following examples will focus on use of C# and Visual Studio, but use with other languages is fairly similiar.

### Plugin Location

When using portable version of Playnite plugin files (dll files) are stored in Playnite's program folder inside `Plugins`. Plugins must be placed inside subfolder for them to be loaded. When using installed version of Playnite place plugins inside `%AppData%\Playnite\Plugins\` folder.

Example plugin location:

```%AppData%\Playnite\Plugins\TestPlugin\TestPlugin.dll```

When distributing plugins only distribute plugin dll and additional references except `Json.Net` and `LiteDB`, which are automatically referenced by Playnite.

### Reloading Plugins

Compared to scipts Playnite doesn't support hot reloading of plugin assemblies and therefore Playnite must be restarted when changing plugin files.

### Creating Plugin Project

Start by creating new `Class Library` project targeting `.NET Framework 4.6.2`. Add [PlayniteSDK](https://www.nuget.org/packages/PlayniteSDK/) nuget package reference and set reference to not require specific version (right-click on `PlayniteSDK` reference, choose `Properties` and set `Specific Version` to false).


> [!NOTE] 
> PlayniteSDK is designed in a way that all versions from one major version branch (for example 1.0, 1.1, 1.2 etc.) are backwards compatible. Therefore plugin written for SDK version 1.0 will also work with Playnite containing all 1.x versions of SDK. When loading plugins Playnites checks all SDK references and won't load plugins referencing incompatible SDK versions.

### Writing Simple Plugin

Create public class inheriting from [Playnite.SDK.Plugin](xref:Playnite.SDK.Plugin) and implement all abstract members.

Plugin adding one menu entry and reacting to event when game starts should look like this:

```csharp
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Linq;

namespace PlaynitePluginExample
{
    public class PlaynitePlugin : Plugin
    {
        private ILogger logger;

        public PlaynitePlugin(IPlayniteAPI api) : base(api)
        {
            logger = PlayniteApi.CreateLogger("TestPlugin");
        }

        public override PluginProperties GetPluginProperties()
        {
            return new PluginProperties("Test Plugin", "Test Author", "0.1");
        }

        public override List<ExtensionFunction> GetFunctions()
        {
            return new List<ExtensionFunction>()
            {
                new ExtensionFunction(
                    "Test Func from C#",
                    () =>
                    {
                        logger.Error("Log with Error severity from C#");
                        PlayniteApi.Dialogs.ShowMessage($"You have {PlayniteApi.MainView.SelectedGames.Count().ToString()} games selected.");
                    })
            };
        }

        public override void OnGameStarted(Game game)
        {
            logger.Error($"Game {game.Name} was started.");
        }
    }
}
```