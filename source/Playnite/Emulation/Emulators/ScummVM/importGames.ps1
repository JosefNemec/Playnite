param(
    $ImportArgs
)

function Get-IniContent()
{
    param(
        $filePath
    )

    $ini = @{}
    switch -regex -file $FilePath
    {
        "^\[(.+)\]" # Section
        {
            $section = $matches[1]
            $ini[$section] = @{}
            $CommentCount = 0
        }
        "^(;.*)$" # Comment
        {
            $value = $matches[1]
            $CommentCount = $CommentCount + 1
            $name = "Comment" + $CommentCount
            $ini[$section][$name] = $value
        } 
        "(.+?)\s*=(.*)" # Key
        {
            $name,$value = $matches[1..2]
            $ini[$section][$name] = $value
        }
    }

    return $ini
}

$scummvmConfig = Join-Path $env:APPDATA "ScummVM\scummvm.ini"
if (!(Test-Path $scummvmConfig))
{
    $scummvmConfig = Join-Path $ImportArgs.ScanDirectory "scummvm.ini"
    if (!(Test-Path $scummvmConfig))
    {
        $ImportArgs.PlayniteApi.Dialogs.ShowErrorMessage("Couldn't find ScummVM config file at $scummvmConfig", "") | Out-Null
        return
    }
}

$config = Get-IniContent $scummvmConfig
foreach ($key in $config.Keys)
{
    if ($config[$key].gameid)
    {
        $romPath = Join-Path $config[$key].path $key        
        $anyFunc = [Func[string,bool]]{ param($a) $a.Equals($romPath, 'OrdinalIgnoreCase') }
        if ([System.Linq.Enumerable]::Any($ImportArgs.ImportedFiles, $anyFunc))
        {
            continue
        }

        $scannedGame = New-Object "Playnite.Emulators.ScriptScannedGame"
        $scannedGame.Name = ($config[$key].description -replace "\(.*\)").Trim()
        $scannedGame.Path = $romPath
        $scannedGame
    }
}