Reacting to events
=====================

Basics
---------------------

Playnite's API allows script to react to various events like when game is started or installed.

Available Events
---------------------

|PowerShell Name | Python Name | Event | Passed Arguments |
| - | - | - | - |
| OnGameStarting | on_game_starting | Before game is started. | [Game](xref:Playnite.SDK.Models.Game) |
| OnGameStarted | on_game_started | Game started running. | [Game](xref:Playnite.SDK.Models.Game) |
| OnGameStopped | on_game_stopped | Game stopped running.  | [Game](xref:Playnite.SDK.Models.Game) and session length in seconds |
| OnGameInstalled | on_game_installed | Game is installed. | [Game](xref:Playnite.SDK.Models.Game) |
| OnGameUninstalled | on_game_uninstalled | Game is uninstalled. | [Game](xref:Playnite.SDK.Models.Game) |
| OnGameSelected | on_game_selected | Game selection changed. | [GameSelectionEventArgs](xref:Playnite.SDK.Events.GameSelectionEventArgs) |
| OnApplicationStarted | on_application_started | Playnite was started. | None |
| OnApplicationStopped | on_application_stopped | Playnite is shutting down. | None |
| OnLibraryUpdated | on_library_updated | Library was updated. | None |

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
        $elapsedSeconds
    )
    
    "$($game.Name) was running for $elapsedSeconds seconds" | Out-File "StoppedGame.txt"
}
```
**IronPython**:

```python
def on_game_stopped(game, elapsed_seconds):
    with open("StoppedGame.txt", "w") as text_file:
        text_file.write("{0} was running for {1} seconds".format(game.Name, elapsed_seconds))
```

### Full File Examples

**PowerShell** (save as *.ps1 file):

```powershell
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
        $elapsedSeconds
    )
    
    "$($game.Name) was running for $elapsedSeconds seconds" | Out-File "StoppedGame.txt"
}
```

**IronPython** (save as *.py file):

```python
def on_game_started(game):
    with open("RunningGame.txt", "w") as text_file:
        text_file.write(game.Name)

def on_game_stopped(game, elapsed_seconds):
    with open("StoppedGame.txt", "w") as text_file:
        text_file.write("{0} was running for {1} seconds".format(game.Name, elapsed_seconds))
```