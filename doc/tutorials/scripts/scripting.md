Playnite Scripting Introduction
=====================

Basics
---------------------

Playnite can be extended with additional functionality using scripts. [PowerShell](https://docs.microsoft.com/en-us/powershell/) and [IronPython](http://ironpython.net/) languages are supported.

> [!NOTE] 
> PowerShell support requires at least PowerShell 5.0 to be installed. If you are Windows 7 user you need to [install it manually](https://www.microsoft.com/en-us/download/details.aspx?id=54616) (Windows 8 and 10 includes it by default).

PowerShell extensions are imported as a [PowerShell module](https://docs.microsoft.com/en-us/powershell/scripting/developer/module/how-to-write-a-powershell-script-module?view=powershell-5.1). The extension of the file must be `.psm1` (or `.psd1` if you use a [PowerShell module manifest](https://docs.microsoft.com/en-us/powershell/scripting/developer/module/how-to-write-a-powershell-module-manifest?view=powershell-5.1)). Any exported functions from your extension must be exported from the module. In a `.psm1` file all functions in the module scope are exported by default, but functions that are in the global scope (defined like `function global:OnGameStarted()`) will _not_ be correctly exported. Exported functions must accept the _exact_ number of arguments that Playnite passes to them.

> [!NOTE]
> Importing PowerShell extensions as modules is a breaking change as of Playnite 9. Existing extensions will be need to be updated. For many extensions this will be as simple as renaming the file from `.ps1` to `.psm1`, updating the manifest and removing `global:` from any exported functions. A `params()` section may need to be added to exported functions that were not explicitly accepting the correct number of arguments.

Creating script extensions
---------------------

Run [Toolbox](../toolbox.md) with arguments specific to a type of script you want to create.

For example, to create new PowerShell script extension:

```
Toolbox.exe new PowerShellScript "Some script" "d:\somefolder"
```

Accessing Playnite API
---------------------

Playnite API is available to scripts via `PlayniteAPI` variable. Variable provides [IPlayniteAPI](xref:Playnite.SDK.IPlayniteAPI) methods and interfaces. For example to get list of all games in library use [Database](xref:Playnite.SDK.IPlayniteAPI.Database) property from `IPlayniteAPI` and [Games](xref:Playnite.SDK.IGameDatabase.Games) collection.

# [PowerShell](#tab/tabpowershell)
```powershell
$PlayniteAPI.Database.Games
```

# [IronPython](#tab/tabpython)
```python
PlayniteApi.Database.Games
```
***

To display number of games use `Dialogs` property from `PlayniteApi` variable. `Dialogs` provides [IDialogsFactory](xref:Playnite.SDK.IDialogsFactory) interface containing method for interaction with user. `ShowMessage` method will show simple text message to user.

# [PowerShell](#tab/tabpowershell)
```powershell
$gameCount = $PlayniteApi.Database.Games.Count
$PlayniteApi.Dialogs.ShowMessage($gameCount)
```

# [IronPython](#tab/tabpython)
```python
game_count = PlayniteApi.Database.Games.Count
PlayniteApi.Dialogs.ShowMessage(str(game_count))
```
***

Examples
---------------------

Displays number of games in the game library when executing `Show Game Count` menu from `Extensions` menu.

# [PowerShell](#tab/tabpowershell)
```powershell
function DisplayGameCount()
{
    param(
        $scriptMainMenuItemActionArgs
    )

    $gameCount = $PlayniteApi.Database.Games.Count
    $PlayniteApi.Dialogs.ShowMessage($gameCount)
}

function GetMainMenuItems()
{
    param(
        $getMainMenuItemsArgs
    )

    $menuItem = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem.Description = "Show Game Count"
    $menuItem.FunctionName = "DisplayGameCount"
    $menuItem.MenuSection = "@"
	return $menuItem
}
```

# [IronPython](#tab/tabpython)
```python
from Playnite.SDK.Plugins import ScriptMainMenuItem

def display_game_count():
    game_count = PlayniteApi.Database.Games.Count
    PlayniteApi.Dialogs.ShowMessage(str(game_count))

def get_mainmenu_items(menu_args):
    menu_item = ScriptMainMenuItem()
    menu_item.Description = "Show Game Count"
    menu_item.FunctionName = "display_game_count"
    menu_item.MenuSection = "@"
    yield menu_item
```
***