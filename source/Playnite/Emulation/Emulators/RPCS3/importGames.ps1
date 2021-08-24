param(
    $ImportArgs
)

if (-not [System.IO.Directory]::Exists($ImportArgs.ScanDirectory))
{
    return
}

[array]$games = Get-ChildItem -LiteralPath $ImportArgs.ScanDirectory -Recurse | Where { $_.Name -eq "ISO.BIN.EDAT" -or $_.Name -eq "EBOOT.BIN" }
foreach ($game in $games)
{
    $scannedGame = New-Object "Playnite.Emulators.ScriptScannedGame"
    $scannedGame.Path = $game.FullName

    if ($game.FullName -match 'disc\\([A-Z]+\d+)\\')
    {
        $scannedGame.Serial = $Matches[1]
        $scannedGame
    }
    elseif  ($game.FullName -match 'game\\([A-Z]+\d+)\\')
    {
        $scannedGame.Serial = $Matches[1]
        $scannedGame
    }
}