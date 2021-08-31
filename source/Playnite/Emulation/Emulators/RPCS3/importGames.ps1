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
    $anyFunc = [Func[string,bool]]{ param($a) $a.Equals($game.FullName, 'OrdinalIgnoreCase') }
    if ([System.Linq.Enumerable]::Any($ImportArgs.ImportedFiles, $anyFunc))
    {
        continue
    }
    
    $scannedGame = New-Object "Playnite.Emulators.ScriptScannedGame"
    $scannedGame.Path = $game.FullName

    # We should be probably parsing PARAM.SFO for all games, but this is way easier
    if ($game.FullName -match 'disc\\([A-Z]+\d+)\\')
    {
        $sfb = Join-Path ([System.IO.Path]::GetDirectoryName($game.FullName)) "..\..\PS3_DISC.SFB"        
        $fs = [System.IO.File]::OpenRead($sfb)
        $buffer = New-Object byte[] 0x10
        
        try
        {
            $fs.Seek(0x220, 'Begin') | Out-Null
            $fs.Read($buffer, 0x0, 0x10) | Out-Null
            $id = [System.Text.Encoding]::ASCII.GetString($buffer)
            $scannedGame.Serial = $id.Replace("-", "").Trim("`0")
            $scannedGame
        }
        finally
        {
            $fs.Dispose()
        }
    }
    elseif  ($game.FullName -match 'game\\([A-Z]+\d+)\\')
    {
        $scannedGame.Serial = $Matches[1]
        $scannedGame
    }
}