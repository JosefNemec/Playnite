Script file templates
=====================

These are full script file templates that specify all event functions and one custom function added into main menu.

**PowerShell** Save as *.ps1 file:
```powershell
function global:OnScriptLoaded()
{
}

function global:OnGameStarting()
{
    param(
        $game
    )
}

function global:OnGameStarted()
{
    param(
        $game
    )
}

function global:OnGameStopped()
{
    param(
        $game,
        $elapsedSeconds
    )
}

function global:OnGameInstalled()
{
    param(
        $game
    )     
}

function global:OnGameUninstalled()
{
    param(
        $game
    )    
}

function global:MenuFunction()
{
}
```

**IronPython** Save as *.py file:
```python
def on_script_loaded():
    pass

def on_game_starting(game):
    pass

def on_game_started(game):
    pass

def on_game_stopped(game, elapsed_seconds):
    pass

def on_game_installed(game):
    pass

def on_game_uninstalled(game):
    pass

def menu_function():
    pass
```

**Manifest file**

```yaml
Name: Script Extension
Author: Author's Name
Version: 1.0
Module: scriptName.ps1 # or scriptName.py
Type: Script
Functions: 
    - Description: Menu Function Name
      FunctionName: MenuFunction # or menu_function
```