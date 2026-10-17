param(
    $ImportArgs
)

if (-not [System.IO.Directory]::Exists($ImportArgs.ScanDirectory))
{
    return
}

$databasePath = Join-Path $PSScriptRoot "titles.json"
$titleDatabase = @{}

try
{
    foreach ($entry in (Get-Content -LiteralPath $databasePath -Raw | ConvertFrom-Json))
    {
        $titleDatabase[[string]$entry.TitleId] = [PSCustomObject]@{
            Name   = [string]$entry.Name
            Serial = [string]$entry.Serial
        }
    }
}
catch
{
    $__logger.Error(
        $_.Exception,
        "Failed to load Azahar title database from $databasePath"
    )
}

# Find installed base-game applications.
# The scan directory can be the SDMC root or a directory beneath it.
[array]$appFiles = Get-ChildItem -LiteralPath $ImportArgs.ScanDirectory -Recurse -File -Filter "*.app"

# Group files by their content directory and select the first file per folder since there can be multiple .app inside a game folder but only the first one is launchable
$appFiles = @(
    $appFiles |
        Group-Object { $_.DirectoryName } |
        ForEach-Object { $_.Group | Select-Object -First 1 }
)

foreach ($appFile in $appFiles)
{
    try
    {
        $anyFunc = [Func[string,bool]]{ param($a) $a.Equals($appFile.FullName, 'OrdinalIgnoreCase') }
        if ([System.Linq.Enumerable]::Any($ImportArgs.ImportedFiles, $anyFunc))
        {
            continue
        }
        
        # Only accept:
        # title\00040000\<8-digit title ID>\content\<file>.app, e.g. "title\00040000\00125500\content\00000000.app"
        $contentDirectory = $appFile.Directory
        $titleIdDirectory = $contentDirectory.Parent

        if ($null -eq $titleIdDirectory)
        {
            continue
        }

        $categoryDirectory = $titleIdDirectory.Parent
        if ($null -eq $categoryDirectory)
        {
            continue
        }

        $titleDirectory = $categoryDirectory.Parent
        if ($null -eq $titleDirectory)
        {
            continue
        }

        if ($contentDirectory.Name -ne "content" -or
            $categoryDirectory.Name -ne "00040000" -or
            $titleDirectory.Name -ne "title" -or
            $titleIdDirectory.Name -notmatch '^[0-9A-Fa-f]{8}$')
        {
            continue
        }
        
        # Combine the application category and title ID directory to build TitleId e.g \00040000\00125500 -> 0004000000125500
        $gameId = $categoryDirectory.Name + $titleIdDirectory.Name

        # Only import if app is in database, because otherwise it's a DLC
        if ($titleDatabase.ContainsKey($gameId))
        {
            $scannedGame = New-Object "Playnite.Emulators.ScriptScannedGame"
            $scannedGame.Path = $appFile.FullName
            $scannedGame.Name = $titleDatabase[$gameId].Name
            $scannedGame.Serial = $titleDatabase[$gameId].Serial
            $scannedGame.GameId = $gameId
            $scannedGame
        }

    }
    catch
    {
        $__logger.Error($_.Exception, "Failed to import Azahar installed app file $($game.FullName)")
        $__logger.Error($_.ScriptStackTrace)
    }
}