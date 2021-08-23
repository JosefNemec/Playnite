Playnite Scripting Introduction
=====================

Basics
---------------------

Playnite can be extended with additional functionality using scripts. [PowerShell](https://docs.microsoft.com/en-us/powershell/) is currently the only supported scripting language.

> [!NOTE] 
> PowerShell support requires PowerShell 5.1 to be installed on your machine. If you are Windows 7 user, you need to [install it manually](https://www.microsoft.com/en-us/download/details.aspx?id=54616) (Windows 8 and 10 includes it by default). This also means that PowerShell functionality is restricted to 5.x version. Playnite currently doesn't support newer PowerShell Core runtime (PowerShell versions 6 and newer).

PowerShell extensions are imported as a [PowerShell module](https://docs.microsoft.com/en-us/powershell/scripting/developer/module/how-to-write-a-powershell-script-module?view=powershell-5.1). The extension of the file must be `.psm1` (or `.psd1` if you use a [PowerShell module manifest](https://docs.microsoft.com/en-us/powershell/scripting/developer/module/how-to-write-a-powershell-module-manifest?view=powershell-5.1)).

Any exported functions from your extension must be exported from the module. In a `.psm1` file all functions in the module scope are exported by default, but functions in the global scope (defined like `function global:OnGameStarted()`) will _not_ be correctly exported.

Creating script extensions
---------------------

Run [Toolbox](../toolbox.md) with arguments specific to a type of script you want to create.

To create new PowerShell script extension:

```cmd
Toolbox.exe new PowerShellScript "Some script" "d:\somefolder"
```

Accessing Playnite API
---------------------

Playnite API is available to scripts via `PlayniteAPI` variable. Variable provides [IPlayniteAPI](xref:Playnite.SDK.IPlayniteAPI) methods and interfaces. For example to get list of all games in library use [Database](xref:Playnite.SDK.IPlayniteAPI.Database) property from `IPlayniteAPI` and [Games](xref:Playnite.SDK.IGameDatabase.Games) collection.

```powershell
$PlayniteAPI.Database.Games
```

To display number of games use `Dialogs` property from `PlayniteApi` variable. `Dialogs` provides [IDialogsFactory](xref:Playnite.SDK.IDialogsFactory) interface containing method for interaction with user. `ShowMessage` method will show simple text message to user.

```powershell
$gameCount = $PlayniteApi.Database.Games.Count
$PlayniteApi.Dialogs.ShowMessage($gameCount)
```

Examples
---------------------

Displays number of games in the game library when executing `Show Game Count` menu from `Extensions` menu.

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

Troubleshooting
---------------------

### Debugging

See [scripting debugging](scriptingDebugging.md) page for more details about how to debug PowerShell code running in Playnite.

### Exceptions related to directory not being found

Paths (and strings in general) in various places in PowerShell are handled not as literal strings, but as strings with wildcard patterns. This has unfortunate issue if specific command doesn't allow you to pass literal string via a specific argument (for example `-LiteralPath`) or has an option to disable wildcard parsing.

This can cause issues if game's installation path contains wildcard pattern characters. For example game installed in `d:\[test] game name\` will cause issues to `Start-Process` cmdlet, because of `[` and `]` pattern characters. This is because Playnite sets script runtime's working directory to game's installation path and Start-Process tries to parse it before launching a program.

Solution is to either use literal path arguments or use different cmdlet or .NET classes directly. For example to start a process:

```powershell
# Instead of:
Start-Process "game.exe"

# call .NET class directly:
[System.Diagnostics.Process]::Start("game.exe")
```

Note: This issue has been fixed in newer versions of PowerShell, but since Playnite has to use older version (5.1) until we switch to newer .NET runtime, you may encounter this issue.