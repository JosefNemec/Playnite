using module c:\devel\PowerShell\PSNativeAutomation\source\PSNativeAutomation\bin\Debug\PSNativeAutomation.psd1

class SettingsWindow : Window
{
    [UIObject]$ButtonBrowserDbFile
    [UIObject]$ButtonGogAuth
    [UIObject]$ButtonOK
    [UIObject]$ButtonOriginAuth
    [UIObject]$ButtonCancel
    [UIObject]$CheckGogEnabled
    [UIObject]$CheckGogIcons
    [UIObject]$CheckOriginEnabled
    [UIObject]$CheckGogRunGalaxy
    [UIObject]$CheckSteamEnabled
    [UIObject]$RadioLibraryOrigin
    [UIObject]$RadioInstalledOrigin
    [UIObject]$RadioLibrarySteam
    [UIObject]$RadioInstalledSteam
    [UIObject]$RadioInstalledGOG
    [UIObject]$RadioLibraryGOG
    [UIObject]$TextSteamAccountName
    [UIObject]$TextDatabase

    SettingsWindow() : base({Get-UIWindow -ProcessName "PlayniteUI" | Get-UIWindow -AutomationId "WindowSettings"}, "SettingsWindow")
    {
        $this.ButtonBrowserDbFile  = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonBrowserDbFile" -First}), "ButtonBrowserDbFile")
        $this.ButtonGogAuth        = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonGogAuth" -First}), "ButtonGogAuth")
        $this.ButtonOK             = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonOK" -First}), "ButtonOK")
        $this.ButtonOriginAuth     = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonOriginAuth" -First}), "ButtonOriginAuth")
        $this.ButtonCancel         = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonCancel" -First}), "ButtonCancel")
        $this.CheckGogEnabled      = [UIObject]::New($this.GetChildReference({Get-UICheckBox -AutomationId "CheckGogEnabled" -First}), "CheckGogEnabled")
        $this.CheckGogIcons        = [UIObject]::New($this.GetChildReference({Get-UICheckBox -AutomationId "CheckGogIcons" -First}), "CheckGogIcons")
        $this.CheckOriginEnabled   = [UIObject]::New($this.GetChildReference({Get-UICheckBox -AutomationId "CheckOriginEnabled" -First}), "CheckOriginEnabled")
        $this.CheckGogRunGalaxy    = [UIObject]::New($this.GetChildReference({Get-UICheckBox -AutomationId "CheckGogRunGalaxy" -First}), "CheckGogRunGalaxy")
        $this.CheckSteamEnabled    = [UIObject]::New($this.GetChildReference({Get-UICheckBox -AutomationId "CheckSteamEnabled" -First}), "CheckSteamEnabled")
        $this.RadioLibraryOrigin   = [UIObject]::New($this.GetChildReference({Get-UIRadioButton -AutomationId "RadioLibraryOrigin" -First}), "RadioLibraryOrigin")
        $this.RadioInstalledOrigin = [UIObject]::New($this.GetChildReference({Get-UIRadioButton -AutomationId "RadioInstalledOrigin" -First}), "RadioInstalledOrigin")
        $this.RadioLibrarySteam    = [UIObject]::New($this.GetChildReference({Get-UIRadioButton -AutomationId "RadioLibrarySteam" -First}), "RadioLibrarySteam")
        $this.RadioInstalledSteam  = [UIObject]::New($this.GetChildReference({Get-UIRadioButton -AutomationId "RadioInstalledSteam" -First}), "RadioInstalledSteam")
        $this.RadioInstalledGOG    = [UIObject]::New($this.GetChildReference({Get-UIRadioButton -AutomationId "RadioInstalledGOG" -First}), "RadioInstalledGOG")
        $this.RadioLibraryGOG      = [UIObject]::New($this.GetChildReference({Get-UIRadioButton -AutomationId "RadioLibraryGOG" -First}), "RadioLibraryGOG")
        $this.TextSteamAccountName = [UIObject]::New($this.GetChildReference({Get-UITextBox -AutomationId "TextSteamAccountName" -First}), "TextSteamAccountName")
        $this.TextDatabase         = [UIObject]::New($this.GetChildReference({Get-UITextBox -AutomationId "TextDatabase" -First}), "TextDatabase")
    }
}

return [SettingsWindow]::New()