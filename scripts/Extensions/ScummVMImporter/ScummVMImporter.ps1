function global:Get-IniContent()
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

function global:ImportScummVMGames()
{
    $scummvmConfig = Join-Path $env:APPDATA "ScummVM\scummvm.ini"
    if (!(Test-Path $scummvmConfig))
    {
        $PlayniteApi.Dialogs.ShowMessage("Couldn't find ScummVM config file at $scummvmConfig")
        return
    }

    $scummvmEmulator = $PlayniteApi.Database.Emulators | Where { $_.Name -eq "ScummVM" } | Select-Object -First 1
    if (!$scummvmEmulator)
    {
        $PlayniteApi.Dialogs.ShowMessage("Couldn't find ScummVM emulator configuration in Playnite. Make sure you have ScummVM emulator configured.")
        return
    }

    $config = Get-IniContent $scummvmConfig
    foreach ($key in $config.Keys)
    {
        if ($config[$key].gameid)
        {
            $gameName = $config[$key].description -replace "\(.*\)"
            if ($PlayniteApi.Database.Games | Where { $_.Name -eq $gameName })
            {
                continue
            }

            $game = New-Object "Playnite.SDK.Models.Game" $gameName
            $game.InstallDirectory = $config[$key].path
            $game.IsInstalled = $true

            $playTask = New-Object "Playnite.SDK.Models.GameAction"
            $playTask.Type = "Emulator"
            $playTask.AdditionalArguments = $key
            $playTask.EmulatorId = $scummvmEmulator.Id
            $playTask.EmulatorProfileId = $scummvmEmulator.Profiles[0].Id

            $game.PlayAction =  $playTask
            $PlayniteApi.Database.Games.Add($game)
        }
    }
}