$global:__attributes = @{
    "Author" = "";
    "Version" = ""
}

$global:__exports = @{
    "Function menu string" = @{
        "Function" = "ExportedFunction"
    }
}

function global:OnScriptLoaded()
{

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

function global:ExportedFunction()
{

}