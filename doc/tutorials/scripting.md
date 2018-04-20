Playnite Scripting Introduction
=====================

Basics
---------------------

Playnite can be extended with additional functionality using scripts. Currently scripts written in [PowerShell](https://docs.microsoft.com/en-us/powershell/) and [IronPython](http://ironpython.net/) languages are supported.

If you want to use compiled .NET languages (C#, VB.NET etc.) to extend Playnite please see documentation related to [Plugins](plugins.md).

> [!NOTE] 
> PowerShell support requires at least PowerShell 5.0 to be installed. If you are Windows 7 user you need to [install it manually](https://www.microsoft.com/en-us/download/details.aspx?id=54616) (Windows 8 and 10 includes it by default).

### Script Location

When using portable version of Playnite script files are stored in Playnite's program folder inside `\Scripts\PowerShell` folder for PowerShell scripts and `\Scripts\IronPython` for IronPython scripts. Only scripts placed directly inside the script folder are loaded, any scripts inside additional subfolders are ignored.

When using installed version of Playnite place scripts inside `%AppData%\Playnite\` folder instead of program folder while keeping the same structure based on used script language.

### Reloading Scripts

Scripts are automatically loaded when application is started and any changes to scripts files are NOT reflected while Playnite is already running. If you want to force Playnite to reload scripts while already running then use **Reload Scripts** option under **Tools** menu or use `F12` shortcut.

Simple script file
---------------------

Following example shows how to create simple script which adds new menu option to Playnite. When invoked menu options show how many dialog message with how many games are in library.

### Creating Scripts Files

For **PowerShell**, create empty `.ps1` file inside `PowerShell` folder.

For **IronPython**, create empty `.py` file inside `IronPython` folder.

Additionally you can use prepared [templates](scriptingTemplates.md), which include complete skeleton for simple script file.

### Add Script Attributes

Scripts attributes are used by Playnite to get basic information about script author and its version.

For **PowerShell** initialize new global hastable variable `__attributes`:

```powershell
$global:__attributes = @{
    "Author" = "TestAuthor";
    "Version" = "1.0"
}
```
For **IronPython** initialize new dictionary variable `__attributes`:

```python
__attributes = {
    'Author': 'TestAuthor',
    'Version': '1.0'
}
```

### Initialize Menu Functions

Every script which adds new executable menu entry must define list of exportable functions and their properties.

For **PowerShell** initialize new global arrary variable `__exports` with list of exported functions:

```powershell
$global:__exports = @(
    @{
        "Name" = "Function menu string";
        "Function" = "DisplayGameCount"
    }
)
```
For **IronPython** initialize new array variable `__exports` with list of exported functions:

```python
__exports = [
    {
        'Name': 'Function menu string',
        'Function' : 'display_game_count'
    }
]
```
Available attributes:
|Attribute        | Description          | Mandatory  |
| ------------- |-------------| -----|
| Name | Function name to be displayed as new menu entry |  Yes |
| Function | Name of the script function to be executed |   Yes |

### Define Menu Function

For **PowerShell** define global function:

```powershell
function global:DisplayGameCount()
{
}
```

For **IronPython** define function:

```python
def display_game_count():
```

Functions must be defined without any parameters as Playnite doesn't pass any when invoking them.

### Accessing Playnite API

Playnite API is available to scripts via `PlayniteAPI` variable. Variable provides [IPlayniteAPI](xref:Playnite.SDK.IPlayniteAPI) methods and interfaces. To get list of all games in library use [Database](xref:Playnite.SDK.IPlayniteAPI.Database) property from `IPlayniteAPI`, method [GetGames](xref:Playnite.SDK.IGameDatabaseAPI.GetGames) returns list of all games.

**PowerShell**:

```powershell
$PlayniteAPI.Database.GetGames()
```

**IronPython**:

```python
PlayniteApi.Database.GetGames()
```

To display number of games use `Dialogs` property from `PlayniteApi` variable. `Dialogs` provides [IDialogsFactory](xref:Playnite.SDK.IDialogsFactory) interface containing method for interaction with user. `ShowMessage` method will show simple text message to user.

**PowerShell**:

```powershell
$gameCount = $PlayniteApi.Database.GetGames().Count
$PlayniteApi.Dialogs.ShowMessage($gameCount)
```

**IronPython**:

```python
game_count = PlayniteApi.Database.GetGames().Count
PlayniteApi.Dialogs.ShowMessage(str(game_count))
```

### Full Example

**PowerShell**:

```powershell
$global:__attributes = @{
    "Author" = "TestAuthor";
    "Version" = "1.0"
}

$global:__exports = @(
    @{
        "Name" = "Function menu string";
        "Function" = "DisplayGameCount"
    }
)

function global:DisplayGameCount()
{
    $gameCount = $PlayniteApi.Database.GetGames().Count
    $PlayniteApi.Dialogs.ShowMessage($gameCount)
}
```

**IronPython**:

```python
__attributes = {
    'Author': 'TestAuthor',
    'Version': '1.0'
}

__exports = [
    {
        'Name': 'Function menu string',
        'Function' : 'display_game_count'
    }
]

def display_game_count():
    game_count = PlayniteApi.Database.GetGames().Count
    PlayniteApi.Dialogs.ShowMessage(str(game_count))
```

What Next
---------------------

Check additional resources for more advanced scripting:

[Reacting to game events](scriptingEvents.md)

[Working with game database](scriptingDatabase.md)

[Interaction with UI](scriptingUI.md)

[Writing to log files](scriptingLogging.md)

[Script templates](scriptingTemplates.md)