Writing to Log Files
=====================

Scripts
---------------------

To write message into Playnite's log files use `__logger` variable which profiles [ILogger](xref:Playnite.SDK.ILogger) interface.

Plugins
---------------------

Use static `GetLogger` method from [LogManager](xref:Playnite.SDK.LogManager) class, to create logger instance.

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

# [IronPython](#tab/tabpython)
```python
__logger.Info('This is message with Info severity')
```
***

