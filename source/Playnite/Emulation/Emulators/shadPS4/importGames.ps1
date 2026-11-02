param(
    $ImportArgs
)

if (-not [System.IO.Directory]::Exists($ImportArgs.ScanDirectory))
{
    return
}

function Get-NullTerminatedString
{
    param([Array]$bytes, [int]$offset)

    $strBytes = @()
    for ($j = $offset; $j -lt $bytes.Count; $j++)
    {
        $b = $bytes[$j]
        if($b -eq 0) { break } #strings end on null terminator
        $strBytes += $b
    }
    return [System.Text.Encoding]::UTF8.GetString($strBytes)
}

function Get-ParamSfoValue
{
    param([string]$path, [string]$key)

    #thanks to https://psdevwiki.com/ps4/PARAM.SFO
    [byte[]]$bytes = Get-Content -LiteralPath $path -Encoding Byte -Raw
    $keyTableOffset = [System.BitConverter]::ToUInt32($bytes, 0x08)
    $dataTableOffset = [System.BitConverter]::ToUInt32($bytes, 0x0c)

    $indexTableOffset = 0x14
    $indexRowLength = 0x10

    #go through each index table row
    for ($i = $indexTableOffset; $i -lt $keyTableOffset; $i += $indexRowLength)
    {
        $relativeKeyOffset = [System.BitConverter]::ToUInt16($bytes, $i)
        $foundKey = Get-NullTerminatedString $bytes ($keyTableOffset + $relativeKeyOffset)
        if($foundKey -ne $key)
        {
            continue
        }

        $dataFormat = [System.BitConverter]::ToUInt16($bytes, $i + 0x02)
        $dataLength = [System.BitConverter]::ToUInt32($bytes, $i + 0x04)
        $relativeDataOffset = [System.BitConverter]::ToUInt32($bytes, $i + 0x0c)
        if ($dataFormat -eq 1028) #uint32
        {
            $data = [System.BitConverter]::ToUInt32($bytes, $dataTableOffset + $relativeDataOffset)
        }
        else #string (usually null-terminated)
        {
            $data = [System.Text.Encoding]::UTF8.GetString($bytes, $dataTableOffset + $relativeDataOffset, $dataLength).Trim("`0")
        }
        return $data
    }
    return $null
}

[array]$games = Get-ChildItem -LiteralPath $ImportArgs.ScanDirectory -Recurse | Where-Object { $_.DirectoryName -match 'CUSA\d+$' -and ($_.Name -eq "ISO.BIN.EDAT" -or $_.Name -eq "EBOOT.BIN") }
foreach ($game in $games)
{
    $anyFunc = [Func[string,bool]]{ param($a) $a.Equals($game.FullName, 'OrdinalIgnoreCase') }
    if ([System.Linq.Enumerable]::Any($ImportArgs.ImportedFiles, $anyFunc))
    {
        continue
    }

    $scannedGame = New-Object "Playnite.Emulators.ScriptScannedGame"
    $scannedGame.Path = $game.FullName
    $paramSfoPath = Join-Path $game.Directory "sce_sys\param.sfo"

    if (Test-Path -LiteralPath $paramSfoPath -PathType Leaf)
    {
        try
        {
            $scannedGame.Serial = Get-ParamSfoValue $paramSfoPath "TITLE_ID"
            if ($null -ne $scannedGame.Serial)
            {
                $scannedGame.Name = Get-ParamSfoValue $paramSfoPath "TITLE"
                $scannedGame
            }
        }
        catch
        {
            $__logger.Error($_.Exception, "Failed to scan PS4 PARAM.SFO file $paramSfoPath")
            $__logger.Error($_.ScriptStackTrace)
        }
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

        # Grab the gameId / titleId from the filename (e.g. CUSA12112 from CUSA12112.zar)
        $titleId = $null
        if ($fileName -match '^([A-Z]{4}\d{5})')
        {
            $titleId = $Matches[1]
        }

        # Set the game name to the file name
        $title = $baseName

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
