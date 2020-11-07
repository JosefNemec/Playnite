function OnApplicationStarted()
{
    $__logger.Info("OnApplicationStarted")
}

function OnApplicationStopped()
{
    $__logger.Info("OnApplicationStopped")
}

function OnLibraryUpdated()
{
    $__logger.Info("OnLibraryUpdated")
}

function OnGameStarting()
{
    param(
        $game
    )
    $__logger.Info("OnGameStarting $game")
}

function OnGameStarted()
{
    param(
        $game
    )
    $__logger.Info("OnGameStarted $game")
}

function OnGameStopped()
{
    param(
        $game,
        $elapsedSeconds
    )
    $__logger.Info("OnGameStopped $game $elapsedSeconds")
}

function OnGameInstalled()
{
    param(
        $game
    )
    $__logger.Info("OnGameInstalled $game")
}

function OnGameUninstalled()
{
    param(
        $game
    )
    $__logger.Info("OnGameUninstalled $game")
}

function OnGameSelected()
{
    param(
        $gameSelectionEventArgs
    )
    $__logger.Info("OnGameSelected $($gameSelectionEventArgs.OldValue) -> $($gameSelectionEventArgs.NewValue)")
}
