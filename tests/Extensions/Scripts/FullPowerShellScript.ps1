$global:__attributes = @{
    Author = "Josef Nemec"
    Version = "1.0"
}

$global:__exports = @(
    @{
        Name = "PowerShell Function"
        Function = "MenuFunction"
    }
)

function global:OnScriptLoaded()
{
    $__logger.Info("FullPowerShellScript OnScriptLoaded")
}

function global:OnGameStarting()
{
    param(
        $game
    )

    $__logger.Info("FullPowerShellScript OnGameStarting $($game.Name)")
}

function global:OnGameStarted()
{
    param(
        $game
    )
    
    $__logger.Info("FullPowerShellScript OnGameStarted $($game.Name)")
}

function global:OnGameStopped()
{
    param(
        $game,
        $ellapsedSeconds
    )
    
    $__logger.Info("FullPowerShellScript OnGameStopped $($game.Name)")
}

function global:OnGameInstalled()
{
    param(
        $game
    )   
    
    $__logger.Info("FullPowerShellScript OnGameInstalled $($game.Name)")  
}

function global:OnGameUninstalled()
{
    param(
        $game
    ) 
    
    $__logger.Info("FullPowerShellScript OnGameUninstalled $($game.Name)")   
}

function global:MenuFunction()
{
    
    $__logger.Info("FullPowerShellScript MenuFunction")
}