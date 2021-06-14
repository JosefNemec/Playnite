Interacting with Playnite's UI
=====================

Getting list of selected games
---------------------

To get list of currently selected games use `MainView` from `PlayniteApi` variable. `MainView` provides [IMainViewAPI](xref:Playnite.SDK.IMainViewAPI) interface with [SelectedGames](xref:Playnite.SDK.IMainViewAPI.SelectedGames) property returning list of all selected games.

# [C#](#tab/csharp)
```csharp
var gameCount = PlayniteApi.MainView.SelectedGames.Count;
PlayniteApi.Dialogs.ShowMessage($"Selected {gameCount} games");
```

# [PowerShell](#tab/tabpowershell)
```powershell
$PlayniteApi.MainView.SelectedGames | Select -ExpandProperty Name | Out-File "SelectedGames.txt"
```

# [IronPython](#tab/tabpython)
```python
with open("SelectedGames.txt", "w") as text_file:
    for game in PlayniteApi.MainView.SelectedGames:
        text_file.write("%s\n" % game.Name)
```
***

Dialogs
---------------------

[IDialogsFactory](xref:Playnite.SDK.IDialogsFactory) API can be used to show various dialogs.

# [C#](#tab/csharp)
```csharp
PlayniteApi.Dialogs.ShowMessage("Hello world!");
```

# [PowerShell](#tab/tabpowershell)
```powershell
$PlayniteApi.Dialogs.ShowMessage("Hello world!")
```

# [IronPython](#tab/tabpython)
```python
PlayniteApi.Dialogs.ShowMessage('Hello world!')
```
***