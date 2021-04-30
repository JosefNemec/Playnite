Custom application menus
=====================

Basics
---------------------

You can add new entries into main menu and game menu. Custom menu entries are currently only supported in Desktop mode.

Basics
---------------------

To add new custom menu entries, implement appropriate menu script/plugin functions. Functions should return list of items that will be addded to a menu. Playnite passes [GetGameMenuItemsArgs](xref:Playnite.SDK.Plugins.GetGameMenuItemsArgs) and [GetMainMenuItemsArgs](xref:Playnite.SDK.Plugins.GetMainMenuItemsArgs) objects when the menu function is called.

You can use those argument objects to decide what elements to return. For example, in case of game menu, `GetGameMenuItemsArgs` contains `Games` field listing all currently selected games that are being used as a source for the game menu.

When the function associated with the menu item is invoked, it will be passed an arguments object. For game menu actions, C# extensions will be passed [GameMenuItemActionArgs](xref:Playnite.SDK.Plugins.GameMenuItemActionArgs) and script extensions will be passed [ScriptGameMenuItemActionArgs](xref:Playnite.SDK.Plugins.ScriptGameMenuItemActionArgs). `GameMenuItemActionArgs` has a `Games` property to access the selected list of games. For main menu actions, C# extensions will be passed [MainMenuItemActionArgs](xref:Playnite.SDK.Plugins.MainMenuItemActionArgs) and script extensions will be passed [ScriptMainMenuItemActionArgs](xref:Playnite.SDK.Plugins.ScriptMainMenuItemActionArgs).

> [!NOTE] 
> `Get*MenuItems` menu methods are executed each time a menu is opened. For that reason, make sure you are not executing long running code in those methods. It would otherwise result in a noticeable delay when opening the menu.

# [C#](#tab/csharp)
```csharp
// To add new game menu items override GetGameMenuItems
public override List<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
{
    return new List<GameMenuItem>
    {
        new GameMenuItem
        {
            Description = "Name of game menu item",
            Action = (args) =>
            {
                 // use args.Games to get list of games attached to the menu source
                 Console.WriteLine("Invoked from game menu item!");
            }
        }
    };
}

// To add new main menu items override GetMainMenuItems
public override List<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
{
    return new List<MainMenuItem>
    {
        new MainMenuItem
        {
            Description = "Name of main menu item",
            Action = (args) => Console.WriteLine("Invoked from main menu item!")
        }
    };
}
```

# [PowerShell](#tab/tabpowershell)
```powershell
# To add new game menu items implement GetGameMenuItems
function GetGameMenuItems()
{
    param($getGameMenuItemsArgs)

    $menuItem = New-Object Playnite.SDK.Plugins.ScriptGameMenuItem
    $menuItem.Description = "PowerShell game menu item"
    $menuItem.FunctionName = "InvokeGameMenuFunction"
	return $menuItem
}

# To add new main menu items implement GetMainMenuItems
function GetMainMenuItems()
{
    param($getMainMenuItemsArgs)

    $menuItem = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem.Description = "PowerShell main menu item"
    $menuItem.FunctionName = "InvokeMainMenuFunction"
	return $menuItem
}

function InvokeGameMenuFunction()
{
    param($scriptGameMenuItemActionArgs)
    # use $scriptGameMenuItemActionArgs.Games to get list of games attached to the menu source
}

function InvokeMainMenuFunction()
{
    param($scriptMainMenuItemActionArgs)
}
```

# [IronPython](#tab/tabpython)
```python
from Playnite.SDK.Plugins import ScriptGameMenuItem
from Playnite.SDK.Plugins import ScriptMainMenuItem

def get_gamemenu_items(menu_args):
    menu_item2 = ScriptGameMenuItem()
    menu_item2.Description = "IronPython game menu item"
    menu_item2.FunctionName = "game_menu_function"
    yield menu_item2

def get_mainmenu_items(menu_args):
    menu_item = ScriptMainMenuItem()
    menu_item.Description = "IronPython"
    menu_item.FunctionName = "main_menu_function"
    yield menu_item

def game_menu_function(menu_args):
    # use menu_args.Games to get list of games attached to the menu source
    pass

def main_menu_function(menu_args):
    pass
```
***

Sub sections
---------------------

You can add sub sections to menus by assigning `MenuSection` property, sections can be nested. Sub sections are shared between extensions allowing multiple extensions to add items to a single sub section.

To add a menu item under single sub section, assign a string value: `Section name`.

To add nested sections, separate definition with pipes: `Section name|Sub section name`.

To add items under "Extensions" main menu section, add `@` to the beginning of the section definition.

Icons
---------------------

To add an icon to a menu item, assign a value to `Icon` property of menu item. Currently supported values are:

- Full path to an image file.
- Theme relative file path to an image file.
- Key of an application resource.

Separators
---------------------

To add a separator, return new menu item with `Description` field se to `-`.

```csharp
new GameMenuItem { Description = "-" }
```