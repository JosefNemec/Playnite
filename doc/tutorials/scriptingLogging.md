Writing to Log Files
=====================

To write message into Playnite's log files use `__logger` variable which profiles [ILogger](xref:Playnite.SDK.ILogger) interface. You can log messages with different severities `Error`, `Info`, `Debug` and `Warning`.

**[PowerShell**:
```powershell
$__logger.Info("This is message with Info severity")
$__logger.Error("This is message with Error severity")
$__logger.Debug("This is message with Debug severity")
$__logger.Warn("This is message with Warning severity")
```

**IronPython**:
```python
__logger.Info('This is message with Info severity')
__logger.Error('This is message with Error severity')
__logger.Debug('This is message with Debug severity')
__logger.Warn('This is message with Warning severity')
```