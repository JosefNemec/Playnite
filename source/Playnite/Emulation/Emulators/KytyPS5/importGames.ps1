param(
    $ImportArgs
)

if (-not [System.IO.Directory]::Exists($ImportArgs.ScanDirectory))
{
    return
}

[array]$games = Get-ChildItem -LiteralPath $ImportArgs.ScanDirectory -Recurse -File -Filter "eboot.bin"
foreach ($game in $games)
{
    $anyFunc = [Func[string,bool]]{ param($a) $a.Equals($game.FullName, 'OrdinalIgnoreCase') }
    if ([System.Linq.Enumerable]::Any($ImportArgs.ImportedFiles, $anyFunc))
    {
        continue
    }

    $paramJsonPath = Join-Path $game.Directory.FullName "sce_sys\param.json"
    if (-not (Test-Path -LiteralPath $paramJsonPath -PathType Leaf))
    {
        continue
    }

    try
    {
        $paramJson = Get-Content -LiteralPath $paramJsonPath -Raw | ConvertFrom-Json
        $titleId = $paramJson.titleId

        if ([string]::IsNullOrWhiteSpace($titleId))
        {
            continue
        }

        $title = $null
        $localizedParameters = $paramJson.localizedParameters

        if ($null -ne $localizedParameters)
        {
            # Prefer the English (United States) title.
            if (-not [string]::IsNullOrWhiteSpace($localizedParameters.'en-US'.titleName))
            {
                $title = $localizedParameters.'en-US'.titleName
            }

            # Fall back to the default language.
            if ([string]::IsNullOrWhiteSpace($title))
            {
                $defaultLanguage = $localizedParameters.defaultLanguage

                if (-not [string]::IsNullOrWhiteSpace($defaultLanguage))
                {
                    $defaultTitle = $localizedParameters.$defaultLanguage.titleName
                    if (-not [string]::IsNullOrWhiteSpace($defaultTitle))
                    {
                        $title = $defaultTitle
                    }
                }
            }

            # Fall back to any other available localized title.
            if ([string]::IsNullOrWhiteSpace($title))
            {
                foreach ($language in $localizedParameters.PSObject.Properties)
                {
                    if ($language.Name -eq "defaultLanguage")
                    {
                        continue
                    }

                    if (-not [string]::IsNullOrWhiteSpace($language.Value.titleName))
                    {
                        $title = $language.Value.titleName
                        break
                    }
                }
            }
        }

        if ([string]::IsNullOrWhiteSpace($title))
        {
            continue
        }

        $scannedGame = New-Object "Playnite.Emulators.ScriptScannedGame"
        $scannedGame.Path = $game.FullName
        $scannedGame.Serial = $titleId
        $scannedGame.GameId = $titleId
        $scannedGame.Name = $title

        $scannedGame
    }
    catch
    {
        $__logger.Error($_.Exception, "Failed to scan PS5 param.json file $paramJsonPath")
        $__logger.Error($_.ScriptStackTrace)
    }
}

[array]$zarFiles = Get-ChildItem -LiteralPath $ImportArgs.ScanDirectory -Recurse -File -Filter "*.zar"
foreach ($zar in $zarFiles)
{
    $anyFunc = [Func[string,bool]]{ param($a) $a.Equals($zar.FullName, 'OrdinalIgnoreCase') }
    if ([System.Linq.Enumerable]::Any($ImportArgs.ImportedFiles, $anyFunc))
    {
        continue
    }

    try
    {
        # Rudimentary file parsing: Extracting metadata directly from the filename is a temporary workaround because full ZArchive parsing is currently too complex without external dependencies.
        $fileName = $zar.Name
        $baseName = $zar.BaseName

        # Grab the gameId / titleId from the filename (e.g. PPSA02929 from PPSA02929-app0.zar)
        $titleId = $null
        if ($fileName -match '^([A-Z]{4}\d{5})')
        {
            $titleId = $Matches[1]
        }

        # Set the game name to the file name
        $title = $baseName -replace '-app0$', ''

        $scannedGame = New-Object "Playnite.Emulators.ScriptScannedGame"
        $scannedGame.Name = $title
        $scannedGame.Path = $zar.FullName
        if ([string]::IsNullOrWhiteSpace($titleId))
        {
            $scannedGame.Serial = $titleId
            $scannedGame.GameId = $titleId
        }

        $scannedGame
    }
    catch
    {
        $__logger.Error($_.Exception, "Failed to scan ZArchive file $($zar.FullName)")
        $__logger.Error($_.ScriptStackTrace)
    }
}