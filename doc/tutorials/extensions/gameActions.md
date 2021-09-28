Game actions
=====================

Introduction
---------------------

Extensions can "inject" game actions dynamically when a game is started rather then assigning them permanently to a game. How this works is that when starting a game, Playnite asks all installed plugins if they know how to handle specific operation for a selected game and if they return controller for that specific action, Playnite will give user an option to use implementation from those plugins.

> [!NOTE] 
> Make sure you are returning actions only for games your plugin is able to handle properly. For example by checking if game's source `PluginId` matches yours if you are implementing a controller for a library plugin. You should not be returning any controllers for a game that you can't provide support for.

Play actions
---------------------

Play actions control how a game is started and are also responsible for play time tracking. To implement play action handling, override `GetPlayActions` method from your plugin and return [PlayController](xref:Playnite.SDK.Plugins.PlayController) object implementing game startup functionality.

In you play controller: call `InvokeOnStarted` after the game is started and `InvokeOnStopped` when the game stops running. `InvokeOnStopped` should provide information about last game session, mainly how long the session lasted, for Playnite to record play time properly.

"Automatic" play actions
---------------------

If game startup procedure for your plugin doesn't require any special handling, you can use [AutomaticPlayController](xref:Playnite.SDK.Plugins.AutomaticPlayController) to simplify play action controller implementation. `AutomaticPlayController` mirrors functionality of manual play action configuration you can assign via Playnite UI, it provides the same fields and functionality.

```csharp
public override IEnumerable<PlayController> GetPlayActions(GetPlayActionsArgs args)
{
    yield return new AutomaticPlayController(args.Game)
    {
        Type = GenericPlayActionType.File,
        TrackingMode = TrackingMode.Process,
        Name = "Notepad",
        Path = "notepad.exe"
    };
}
```

Install an uninstall actions
---------------------

Similar to how Play controllers are implemented, override `GetInstallActions` and/or `GetUninstallActions` methods. Install controllers should provide [GameInstallationData](xref:Playnite.SDK.Plugins.GameInstallationData) when calling `InvokeOnInstalled` for installation status to be set properly.

Examples
---------------------

You can check [built-in Playnite extensions](https://github.com/JosefNemec/PlayniteExtensions) for various examples of controller implementations.