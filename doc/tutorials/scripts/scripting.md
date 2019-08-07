Playnite Scripting Introduction
=====================

Basics
---------------------

Playnite can be extended with additional functionality using scripts. [PowerShell](https://docs.microsoft.com/en-us/powershell/) and [IronPython](http://ironpython.net/) languages are supported.


> [!NOTE] 
> PowerShell support requires at least PowerShell 5.0 to be installed. If you are Windows 7 user you need to [install it manually](https://www.microsoft.com/en-us/download/details.aspx?id=54616) (Windows 8 and 10 includes it by default).

Accessing Playnite API
---------------------

Playnite API is available to scripts via `PlayniteAPI` variable. Variable provides [IPlayniteAPI](xref:Playnite.SDK.IPlayniteAPI) methods and interfaces. For example to get list of all games in library use [Database](xref:Playnite.SDK.IPlayniteAPI.Database) property from `IPlayniteAPI` and [Games](xref:Playnite.SDK.IGameDatabase.Games) collection.

**PowerShell**:

```powershell
$PlayniteAPI.Database.Games
```

**IronPython**:

```python
PlayniteApi.Database.Games
```

To display number of games use `Dialogs` property from `PlayniteApi` variable. `Dialogs` provides [IDialogsFactory](xref:Playnite.SDK.IDialogsFactory) interface containing method for interaction with user. `ShowMessage` method will show simple text message to user.

**PowerShell**:

```powershell
$gameCount = $PlayniteApi.Database.Games.Count
$PlayniteApi.Dialogs.ShowMessage($gameCount)
```

**IronPython**:

```python
game_count = PlayniteApi.Database.Games.Count
PlayniteApi.Dialogs.ShowMessage(str(game_count))
```

Examples
---------------------

Displays number of games in database when executing `Show Game Count` menu from `Extensions` menu.

**PowerShell**:

```powershell
function global:DisplayGameCount()
{
    $gameCount = $PlayniteApi.Database.Games.Count
    $PlayniteApi.Dialogs.ShowMessage($gameCount)
}
```

**IronPython**:

```python
def display_game_count():
    game_count = PlayniteApi.Database.Games.Count
    PlayniteApi.Dialogs.ShowMessage(str(game_count))
```

## Example manifest file

```yaml
Name: Game Counter
Author: Playnite
Version: 1.0
Module: scriptName.ps1 # or scriptName.py
Type: Script
Functions: 
    - Description: Show Game Count
      FunctionName: DisplayGameCount # or display_game_count
```

There's also [library exporter](https://github.com/JosefNemec/Playnite/tree/master/source/Plugins/LibraryExporter) script example that shows complete extension implementation.