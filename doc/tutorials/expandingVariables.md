Working With Dynamic Variables
=====================

Basics
---------------------

Some game fields support user of dynamic variables. For example game action can have working directory pointing to `{InstallDir}`, Playnite then inserts game's installation directory when executing action. Playnite API provides [ExpandGameVariables](xref:Playnite.SDK.IPlayniteAPI.ExpandGameVariables(Playnite.SDK.Models.Game,System.String)) method which resolves these variables from any string.

Full list of variables is available on [wiki](https://github.com/JosefNemec/Playnite/wiki/Emulator-Settings#dynamic-variables).

Example
---------------------

Lets say we have a game which has Play action with `Path` set to `{InstallDir}\app.exe` and `InstallDirectory` to `c:\appdir\`. To get full path `c:\appdir\app.exe` use [ExpandGameVariables](xref:Playnite.SDK.IPlayniteAPI.ExpandGameVariables(Playnite.SDK.Models.Game,System.String)) method.

# [PowerShell](#tab/tabpowershell)
```powershell
$PlayniteApi.ExpandGameVariables($game, $game.PlayTask.Path)
```

# [IronPython](#tab/tabpython)
```python
PlayniteApi.ExpandGameVariables(game, game.PlayTask.Path)
```
***

> [!NOTE] 
> You don't have to check if the string contains any dynamic variables to know whether to use `ExpandGameVariables` or not. If input string doesn't contain any dynamic variables it won't be modified in any way and `ExpandGameVariables` just returns it untouched.