$global:__attributes = @{
    "Author" = "";
    "Version" = ""
}

$global:__exports = @(
    @{
        "Name" = "Display Game Count";
        "Function" = "DisplayGameCount"
    }
)

function global:OnScriptLoaded()
{

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

function global:DisplayGameCount()
{
    $gameCount = $PlayniteApi.Database.GetGames().Count
    $PlayniteApi.Dialogs.ShowMessage($gameCount)
    $__logger.Info("This is message with Info severity")
    $__logger.Error("This is message with Error severity")
    $__logger.Debug("This is message with Debug severity")
    $__logger.Warn("This is message with Warning severity")

    $game = $PlayniteApi.Database.GetGame(20)
    $__logger.Error($game.Name)
}