Interacting with Playnite's UI
=====================

### Getting List of Selected Games

To get list of currently selected games use `MainView` from `PlayniteApi` variable. `MainView` provides [IMainViewAPI](xref:Playnite.SDK.IMainViewAPI) interface with [SelectedGames](xref:Playnite.SDK.IMainViewAPI.SelectedGames) property returning list of all selected games.

Following example exports names of selected games to a file:

**PowerShell**:
```powershell
$PlayniteApi.MainView.SelectedGames | Select -ExpandProperty Name | Out-File "SelectedGames.txt"
```

**IronPython]**:
```python
with open("SelectedGames.txt", "w") as text_file:
    for game in PlayniteApi.MainView.SelectedGames:
        text_file.write("%s\n" % game.Name)
```