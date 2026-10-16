param(
    $ImportArgs
)

if (-not [System.IO.Directory]::Exists($ImportArgs.ScanDirectory))
{
    return
}

function Get-NullTerminatedString
{
    param(
        [Array]$bytes,
        [int]$offset
    )

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
    param(
        [string]$path,
        [string]$key
    )

    # Thanks to https://www.psdevwiki.com/vita/PARAM.SFO
    [byte[]]$bytes = Get-Content -LiteralPath $path -Encoding Byte -Raw

    # "PSF" Magic check
    if ($bytes.Length -lt 0x14 -or
        $bytes[0] -ne 0 -or
        $bytes[1] -ne 0x50 -or
        $bytes[2] -ne 0x53 -or
        $bytes[3] -ne 0x46)
    {
        throw "Invalid PARAM.SFO file: $Path"
    }

    $keyTableOffset = [System.BitConverter]::ToUInt32($bytes, 0x08)
    $dataTableOffset = [System.BitConverter]::ToUInt32($bytes, 0x0c)
    $entryCount = [System.BitConverter]::ToUInt32($bytes, 0x10)

    $indexTableOffset = 0x14
    $indexRowLength = 0x10

    if ($indexTableOffset + ($entryCount * $indexRowLength) -gt $keyTableOffset)
    {
        throw "Invalid PARAM.SFO index table: $Path"
    }

    for ($i = 0; $i -lt $entryCount; $i++)
    {
        $entryOffset = $indexTableOffset + ($i * $indexRowLength)

        $relativeKeyOffset = [System.BitConverter]::ToUInt16($bytes, $entryOffset)
        $keyOffset = $keyTableOffset + $relativeKeyOffset

        if ($keyOffset -ge $bytes.Length)
        {
            throw "Invalid PARAM.SFO key offset: $Path"
        }

        $foundKey = Get-NullTerminatedString $bytes $keyOffset
        if ($foundKey -ne $Key)
        {
            continue
        }

        $dataFormat = [System.BitConverter]::ToUInt16($bytes, $entryOffset + 0x02)
        $dataLength = [System.BitConverter]::ToUInt32($bytes, $entryOffset + 0x04)
        $relativeDataOffset = [System.BitConverter]::ToUInt32($bytes, $entryOffset + 0x0C)
        $dataOffset = $dataTableOffset + $relativeDataOffset

        if ($dataOffset + $dataLength -gt $bytes.Length)
        {
            throw "Invalid PARAM.SFO data offset or length: $Path"
        }

        if ($dataFormat -eq 0x0404) # uint32 1028
        {
            if ($dataLength -lt 4)
            {
                throw "Invalid PARAM.SFO integer length: $Path"
            }

            $data = [System.BitConverter]::ToUInt32($bytes, $dataOffset)
        }
        else
        {
            $data = [System.Text.Encoding]::UTF8.GetString($bytes, $dataOffset, $dataLength).Trim([char]0)
        }

        return $data
    }

    return $null
}

# Find EBOOT.BIN files inside directories matching the standard PS Vita title ID format (four uppercase letters followed by five digits), e.g. PCSA00150, PCSE00553, PCSH10219, PCSG00812, NPEB01082.
[array]$games = Get-ChildItem -LiteralPath $ImportArgs.ScanDirectory -Recurse -File -Filter "eboot.bin" | Where-Object { $_.Directory.Name -cmatch '^[A-Z]{4}\d{5}$' }
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
            $titleId = Get-ParamSfoValue $paramSfoPath "TITLE_ID"
            $scannedGame.Serial = $titleId
            $scannedGame.GameId = $titleId
            if ($null -ne $scannedGame.Serial)
            {
                $scannedGame.Name = Get-ParamSfoValue $paramSfoPath "TITLE"
                $scannedGame
            }
        }
        catch
        {
            $__logger.Error($_.Exception, "Failed to scan PS Vita PARAM.SFO file $paramSfoPath")
            $__logger.Error($_.ScriptStackTrace)
        }
    }
}