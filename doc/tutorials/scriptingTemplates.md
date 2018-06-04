Script file templates
=====================

These are full script file templates that specify all event functions and one custom function added into main menu.

**PowerShell** Save as *.ps1 file:
```powershell
$global:__attributes = @{
    "Author" = "";
    "Version" = ""
}

$global:__exports = @(
    @{
        "Name" = "PowerShell Function";
        "Function" = "MenuFunction"
    }
)

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
        $ellapsedSeconds
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
__attributes = {
    'Author': '',
    'Version': ''
}

__exports = [
    {
        'Name': 'Python Function',
        'Function' : 'menu_function'
    }
]

def on_script_loaded():
    pass

def on_game_starting(game):
    pass

def on_game_started(game):
    pass

def on_game_stopped(game, ellapsed_seconds):
    pass

def on_game_installed(game):
    pass

def on_game_uninstalled(game):
    pass

def menu_function():
    pass
```