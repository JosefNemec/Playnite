$global:PlayniteVariables = @{
    DefaultAppDir = Join-Path $env:LOCALAPPDATA "Playnite"
    StartMenuDir = Join-Path $env:APPDATA "\Microsoft\Windows\Start Menu\Programs\Playnite\"
    DesktopIconPath = Join-Path $env:ProgramData "Desktop\Playnite.lnk"
    UninstallRegKey32 = "Registry::HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Playnite_is1"
    UninstallRegKey64 = "Registry::HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Playnite_is1"
    UiProcessName = "PlayniteUI"
    UiExecutableName = "PlayniteUI.exe"
    DefaultUiExecutablePath = Join-Path $env:LOCALAPPDATA "Playnite\PlayniteUI.exe"
    DefaultUinstallerExecutablePath = Join-Path $env:LOCALAPPDATA "Playnite\unins000.exe"
    AppMutex = "PlayniteInstaceMutex"
}

function global:Stop-PlayniteProcesses()
{
    if (Get-Process -Name $PlayniteVariables.UiProcessName -EA 0)
    {
        Stop-Process -Name $PlayniteVariables.UiProcessName -Force
        WaitFor { (Get-Process -Name $PlayniteVariables.UiProcessName -EA 0) -eq $null }
    }
}