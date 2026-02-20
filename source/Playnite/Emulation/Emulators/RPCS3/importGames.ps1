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

    #thanks to https://psdevwiki.com/ps3/PARAM.SFO
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

[array]$games = Get-ChildItem -LiteralPath $ImportArgs.ScanDirectory -Recurse | Where { $_.Name -eq "ISO.BIN.EDAT" -or $_.Name -eq "EBOOT.BIN" -or $_.Extension -ieq ".iso" }
foreach ($game in $games)
{
    $anyFunc = [Func[string,bool]]{ param($a) $a.Equals($game.FullName, 'OrdinalIgnoreCase') }
    if ([System.Linq.Enumerable]::Any($ImportArgs.ImportedFiles, $anyFunc))
    {
        continue
    }

    $scannedGame = New-Object "Playnite.Emulators.ScriptScannedGame"
    $scannedGame.Path = $game.FullName
    
    if ($game.Extension -ieq '.iso')
    {
        try
        {
            $DiskImage = Mount-DiskImage -ImagePath $game.FullName -StorageType ISO -NoDriveLetter -PassThru
            New-PSDrive -Name ISOFile -PSProvider FileSystem -Root (Get-Volume -DiskImage $DiskImage).UniqueId | Out-Null
            Push-Location ISOFile: | Out-Null

            try
            {
                $paramSfoPath = (Get-ChildItem ISOFile: -Filter "param.sfo" -Recurse -File | Where { $_.DirectoryName -like "*PS3_GAME*" })[0].FullName

                $scannedGame.Serial = Get-ParamSfoValue $paramSfoPath "TITLE_ID"
                if ($null -ne $scannedGame.Serial)
                {
                    $scannedGame.Name = Get-ParamSfoValue $paramSfoPath "TITLE"
                }
            }
            finally
            {
                Pop-Location | Out-Null
                Remove-PSDrive ISOFile | Out-Null
                Dismount-DiskImage -StorageType ISO -ImagePath $DiskImage.ImagePath | Out-Null
            }
        }
        catch
        {
            $__logger.Error($_.Exception, "Failed to scan PS3 .iso PARAM.SFO file $paramSfoPath")
            $__logger.Error($_.ScriptStackTrace)
            
            $scannedGame.Name = [System.IO.Path]::GetFileNameWithoutExtension($game.Name)
            if ($game.Name -match '(BLUS|BLES|NPUB|NPEB)\d{5}')
            {
                $scannedGame.Serial = $matches[0]
            }
        }

        $scannedGame
    }
    else
    {
        $parentDir = $game.Directory.Parent.FullName
        $paramSfoPath = Join-Path $parentDir "PARAM.SFO"
    
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
                $__logger.Error($_.Exception, "Failed to scan PS3 PARAM.SFO file $paramSfoPath")
                $__logger.Error($_.ScriptStackTrace)
            }
        }
    }
}
