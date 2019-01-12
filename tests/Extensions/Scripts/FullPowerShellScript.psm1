$__attributes = @{
    Author = "Josef Nemec"
    Version = "1.0"
}

$__exports = @(
    @{
        Name = "PowerShell Function"
        Function = "MenuFunction"
    }
)

function OnApplicationStarted()
{
    $__logger.Info("FullPowerShellScript OnApplicationStarted")
}

function OnScriptLoaded()
{
    $__logger.Info("FullPowerShellScript OnScriptLoaded")
}

function OnGameStarting()
{
    param(
        $game
    )

    $__logger.Info("FullPowerShellScript OnGameStarting $($game.Name)")
}

function OnGameStarted()
{
    param(
        $game
    )
    
    $__logger.Info("FullPowerShellScript OnGameStarted $($game.Name)")
}

function OnGameStopped()
{
    param(
        $game,
        $ellapsedSeconds
    )
    
    $__logger.Info("FullPowerShellScript OnGameStopped $($game.Name)")
}

function OnGameInstalled()
{
    param(
        $game
    )   
    
    $__logger.Info("FullPowerShellScript OnGameInstalled $($game.Name)")  
}

function OnGameUninstalled()
{
    param(
        $game
    ) 
    
    $__logger.Info("FullPowerShellScript OnGameUninstalled $($game.Name)")   
}

function MenuFunction()
{
    
    $__logger.Info("FullPowerShellScript MenuFunction")
}