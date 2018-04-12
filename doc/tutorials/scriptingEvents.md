Reacting to events
=====================

Basics
---------------------

Playnites's API allows script to react to various events like when game is started or installed.

Available Events
---------------------

|PowerShell Name | Python Name | Event | Passed Arguments |
| - | - | - | - |
| OnScriptLoaded | on_script_loaded | Script is loaded by Playnite. | None |
| OnGameStarted | on_game_started | Game started running. | [Game](xref:Playnite.SDK.Models.Game) |
| OnGameStopped | on_game_stopped | Game stopped running.  | [Game](xref:Playnite.SDK.Models.Game) and session length in seconds |
| OnGameInstalled | on_game_installed | Game is installed. | [Game](xref:Playnite.SDK.Models.Game) |
| OnGameUninstalled | on_game_uninstalled | Game is uninstalled. | [Game](xref:Playnite.SDK.Models.Game) |

Example - Handling start/stop events
---------------------

To have a code executed on selected event define function with specific name in your script.

### Game Starting

Following example writes name of currently playing game into a text file.

**PowerShell**:

```powershell
function global:OnGameStarted()
{
    param(
        $game
    )
    
    $game.Name | Out-File "RunningGame.txt"
}
```

**IronPython**:

```python
def on_game_started(game):
    with open("RunningGame.txt", "w") as text_file:
        text_file.write(game.Name)
```

### Game Stopped

This example writes name of game that stopped running and the time game was running for into a text file.

**PowerShell**:

```powershell
function global:OnGameStopped()
{
    param(
        $game,
        $ellapsedSeconds
    )
    
    "$($game.Name) was running for $ellapsedSeconds seconds" | Out-File "StoppedGame.txt"
}
```
**IronPython**:

```python
def on_game_stopped(game, ellapsed_seconds):
    with open("StoppedGame.txt", "w") as text_file:
        text_file.write("{0} was running for {1} seconds".format(game.Name, ellapsed_seconds))
```

### Full File Examples

**PowerShell** (save as *.ps1 file):

```powershell
$global:__attributes = @{
    "Author" = "TestAuthor";
    "Version" = "1.0"
}

function global:OnGameStarted()
{
    param(
        $game
    )
    
    $game.Name | Out-File "RunningGame.txt"
}

function global:OnGameStopped()
{
    param(
        $game,
        $ellapsedSeconds
    )
    
    "$($game.Name) was running for $ellapsedSeconds seconds" | Out-File "StoppedGame.txt"
}
```

**IronPython** (save as *.py file):

```python
__attributes = {
    'Author': 'TestAuthor',
    'Version': '1.0'
}

def on_game_started(game):
    with open("RunningGame.txt", "w") as text_file:
        text_file.write(game.Name)

def on_game_stopped(game, ellapsed_seconds):
    with open("StoppedGame.txt", "w") as text_file:
        text_file.write("{0} was running for {1} seconds".format(game.Name, ellapsed_seconds))
```