Game scripts
=====================

Introduction
---------------------

Custom PowerShell scripts can be assigned via Playnite's UI to game events. This is similar in a way that extensions support hooking into game events, but doesn't require creation of custom extension and can be assigned on multiple levels:

- Global: executed for every game in a library.
- Emulator: executed for a specific emulator profile on all games using that profile.
- Game: executed for a specific game.

This is not the same as [script extensions](../tutorials/extensions/intro.md) which offer extended functionality and should be used for more complex scenarios.

> [!NOTE] 
> PowerShell support requires PowerShell 5.1 to be installed on your machine. If you are Windows 7 user, you need to [install it manually](https://www.microsoft.com/en-us/download/details.aspx?id=54616) (Windows 8 and 10 includes it by default). This also means that PowerShell functionality is restricted to 5.1 version. Playnite currently doesn't support newer PowerShell Core runtime (PowerShell versions 6 and newer).

Execution
---------------------

Scripts are executed in the following order:

global pre -> emulator pre -> game pre -> game post -> emulator post -> global post -> emulator exit -> game exit -> global exit

where:

- pre - before game is started.
- post - after game is started running.
- exit - after game stopped running.

If a game has installation directory set and that directory actually exists, Playnite will set current working directory of a script runtime to that installation directory.

All scripts share the same runtime environment for a single game session. This means that you can share data between them if you declare your variables on global scope, for example `$global:testVar = 2`.

> [!NOTE] 
> All scripts are executed synchronously, meaning that Playnite will wait for a script to finish and will block any other execution, including UI since the script runtime runs on main thread.

Startup script is an exception to mentioned execution behavior, see startup script section for more details.

Script variables
---------------------

Playnite provides some built-in global variables that scripts can use to get more information about current game session.

| Variable | Description |
| :-- | :-- |
| PlayniteApi | Instance of Playnite API. |
| Game | Game library object for current game session. |

Startup script
---------------------

If you need more complex startup and game tracking procedure other then just starting an executable, or default session tracking doesn't work for a specific game, then you can start and manage game session using a script. This is currently available to emulators (custom profiles) and games.

The script should keep running while you want Playnite to detect the game as running. Once the script finishes with execution, Playnite considers the game to stop running and will record game session length based on how long the script was running (adding that time to overall game play time).

> [!NOTE]
> Startup scripts run in a separate script runtime and thread compared to other game scripts. Therefore you can't share data with other game/global scripts and you can't directly call any code that must be run on main thread (most UI things)!
>
> Breaking exceptions from startup script are not propagated to UI via error message (like for other game/global scripts) and are only logged into Playnite's log file.

### Startup script example

```powershell
$process = [System.Diagnostics.Process]::Start("game.exe")
$process.WaitForExit()
$process.Dispose()
```

### Reacting to tracking cancellation

You can use `$CancelToken.IsCancellationRequested` to detect if user cancelled game session manually from Playnite. When cancellation is initiated, Playnite sets mentioned property to `true` and gives the script 10 seconds to exit gracefully. If the script fails to exit in that time, script's runtime is killed.

```powershell
# WaitForExit() is synchronous check so it can't be used if you want to support session cancellation
$process = [System.Diagnostics.Process]::Start("game.exe")
while ($true)
{
    # Check if user cancelled game session
    if ($CancelToken.IsCancellationRequested)
    {
        break
    }

    # Check if process is still running
    if (!(Get-Process -Name "game" -EA 0))
    {
        break
    }

    # Sleep for a while to not waste CPU
    Start-Sleep -s 1
}

$process.Dispose()
```

### Startup script variables

| Variable | Description |
| :-- | :-- |
| PlayniteApi | Instance of Playnite API. |
| Game | Game library object for current game session. |
| IsPlayAction | Indicates whether an action was started as play action. |

Examples
---------------------

### Starting additional application(s) before game starts and killing it after game exits.

* Edit game and go to `Scripts` tab
* Change runtime to `PowerShell`
* Set first script to start your application using [Start-Process](https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.management/start-process?view=powershell-5.1) cmdlet

```powershell
Start-Process "c:\somepath\someapp.exe" "-some arguments"
```

* Set second script to kill the application using [Stop-Process]https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.management/stop-process?view=powershell-5.1) cmdlet

```powershell
Stop-Process -Name "someapp"
```

* If the application requires elevated rights to start then you need to start Playnite as admin too, otherwise the `Stop-Process` will fail due to insufficient privileges.
* If you want to start application minimized and application doesn't have native support for it then add `-WindowStyle` argument.

```powershell
Start-Process "c:\somepath\someapp.exe" "-some arguments" -WindowStyle Minimized
```

Troubleshooting
---------------------

### Application doesn't start

Some applications won't work properly (or even start) when started using working directory outside of their application director. In that case you need to use `-WorkingDirectory` parameter and specify working directory manually.

### Can't shutdown process

You won't be able to use `Stop-Process` on processes that are started with elevated rights. In that case, you need to use WMI to shutdown a process:

```powershell
(Get-WmiObject -Class Win32_Process -Filter "name = 'someapp.exe'").Terminate()
```
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