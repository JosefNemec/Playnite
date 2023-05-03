Writing to Log Files
=====================

Introduction
---------------------

Playnite provides built-in API for message logging. Messages from extensions are logged into `extensions.log` file that can be found in `%AppData%\Playnite` (for installed version) or in Playnite's installation directory (for portable versions).

> [!NOTE]
> Messages with `Trace` severity are not written into log files unless enabled in `For developers` settings menu.

Scripts
---------------------

To write a message use `$__logger` variable which provides instance of [ILogger](xref:Playnite.SDK.ILogger).

Plugins
---------------------

Use static `GetLogger` method from [LogManager](xref:Playnite.SDK.LogManager) class, to create [ILogger](xref:Playnite.SDK.ILogger) instance.

Examples
---------------------

# [C#](#tab/csharp)
```csharp
var logger = LogManager.GetLogger();
logger.Info("This is message with Info severity");
```

# [PowerShell](#tab/tabpowershell)
```powershell
$__logger.Info("This is message with Info severity")
```
***

