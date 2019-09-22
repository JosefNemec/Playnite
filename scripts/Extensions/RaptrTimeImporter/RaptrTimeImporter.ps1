# Imports time from Raptr database export
function global:ImportRaptr()
{
    if (!($raptrPath = $PlayniteApi.Dialogs.SelectFile("*.json|*.json")))
    {
        return
    }

    $raptrGames = Get-Content $raptrPath -Raw | ConvertFrom-Json
    $raptrGames | ForEach {
        $_.title = $_.title -replace "\s\(.+?\)$", ""
    }

    $updateCount = 0
    :mainLoop foreach ($game in $PlayniteApi.Database.Games)
    {
        foreach ($raptrGame in $raptrGames)
        {
            if ($game.Name -eq $raptrGame.title)
            {
                $game.PlayTime = $raptrGame.total_playtime_seconds
                $PlayniteApi.Database.Games.Update($game)
                $updateCount++
                continue :mainLoop
            }
        }
    }

    $PlayniteApi.Dialogs.ShowMessage("Imported play time for $updateCount games.")
}
