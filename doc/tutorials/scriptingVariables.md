Working With Dynamic Variables
=====================

Basics
---------------------

Some game fields support user of dynamic variables. For example game action can have working directory pointing to `{InstallDir}`, Playnite then inserts game's installation directory when executing action. Playnite API provides [ResolveGameVariables](xref:Playnite.SDK.IPlayniteAPI.ResolveGameVariables(Playnite.SDK.Models.Game,System.String)) method which resolves these varibles from any string.

Full list of variable is available on [wiki](https://github.com/JosefNemec/Playnite/wiki/Emulator-Settings#dynamic-variables).

Example
---------------------

Lets say we have a game which has Play action with `Path` set to `{InstallDir}\app.exe` and `InstallDirectory` to `c:\appdir\`. To get full path `c:\appdir\app.exe` use [ResolveGameVariables](xref:Playnite.SDK.IPlayniteAPI.ResolveGameVariables(Playnite.SDK.Models.Game,System.String)) method.

**PowerShell**:

```powershell
$PlayniteApi.ResolveGameVariables($game, $game.PlayTask.Path)
```

**IronPython**:

```python
PlayniteApi.ResolveGameVariables(game, game.PlayTask.Path)
```

> [!NOTE] 
> You don't have to check if the string contains any dynamic variables to know wheter to use `ResolveGameVariables` or not. If input string doesn't contain any dynamic variables it won't be modified in any way and `ResolveGameVariables` just returns it untouched.