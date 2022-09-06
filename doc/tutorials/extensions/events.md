Reacting to events
=====================

Introduction
---------------------

Playnite's API allows extensions to react to various events, like when a game is started or installed.

Available Events
---------------------

| Name | Event | Passed Arguments |
| --- | --- | --- |
| OnGameStarting | Before game is started. | [OnGameStartingEventArgs](xref:Playnite.SDK.Events.OnGameStartingEventArgs) |
| OnGameStarted | Game started running. | [OnGameStartedEventArgs](xref:Playnite.SDK.Events.OnGameStartedEventArgs) |
| OnGameStopped | Game stopped running.  | [OnGameStoppedEventArgs](xref:Playnite.SDK.Events.OnGameStoppedEventArgs) |
| OnGameInstalled | Game is installed. | [OnGameInstalledEventArgs](xref:Playnite.SDK.Events.OnGameInstalledEventArgs) |
| OnGameUninstalled | Game is uninstalled. | [OnGameUninstalledEventArgs](xref:Playnite.SDK.Events.OnGameUninstalledEventArgs) |
| OnGameSelected | Game selection changed. | [OnGameSelectedEventArgs](xref:Playnite.SDK.Events.OnGameSelectedEventArgs) |
| OnApplicationStarted | Playnite was started. | [OnApplicationStartedEventArgs](xref:Playnite.SDK.Events.OnApplicationStartedEventArgs) |
| OnApplicationStopped | Playnite is shutting down. | [OnApplicationStoppedEventArgs](xref:Playnite.SDK.Events.OnApplicationStoppedEventArgs) |
| OnLibraryUpdated | Library was updated. | [OnLibraryUpdatedEventArgs](xref:Playnite.SDK.Events.OnLibraryUpdatedEventArgs) |

Cancelling game startup
---------------------

If you want to cancel game startup from `OnGameStarting` event, set `CancelStartup` property of [OnGameStartingEventArgs](xref:Playnite.SDK.Events.OnGameStartingEventArgs) to `true`.

Example - Handling start/stop events
---------------------

### Game Starting

Starting event is executed before a game is actually started. Game startup procedure can be cancelled by setting `CancelStartup` property of `OnGameStartingEventArgs` object (passed to an event method) to `true`.

### Game Started

Following example writes name of currently playing game into a text file.

# [C#](#tab/csharp)
```csharp
# To have a code executed on a specific event, override selected event method in your plugin.
public override void OnGameStarted(OnGameStartedEventArgs args)
{
    logger.Info($"Game started: {args.Game.Name}");
}
```

# [PowerShell](#tab/tabpowershell)
```powershell
# To have a code executed on a specific event, define script function with selected name and export it from your PowerShell extension module.
function OnGameStarted()
{
    param($args)
    $ags.Game.Name | Out-File "RunningGame.txt"
}
```
***

### Game Stopped

This example writes name of game that stopped running and the time game was running for into a text file.

# [C#](#tab/csharp)
```csharp
public override void OnGameStopped(OnGameStoppedEventArgs args)
{
    logger.Info($"{args.Game.Name} was running for {args.ElapsedSeconds} seconds");
}
```

# [PowerShell](#tab/tabpowershell)
```powershell
function OnGameStopped()
{
    param($args)
    "$($args.Game.Name) was running for $($args.ElapsedSeconds) seconds" | Out-File "StoppedGame.txt"
}
```
***