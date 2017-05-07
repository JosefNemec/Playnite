using module PSNativeAutomation

class FirstTimeWizardWindow : Window
{
    [UIObject]$ButtonNext
    [UIObject]$ButtonBack
    [UIObject]$ButtonFinish
    [UIObject]$ButtonBrowseDbFile
    [UIObject]$ButtonGogAuthenticate
    [UIObject]$ButtonOriginAuthenticate
    [UIObject]$ButtonImportGames
    [UIObject]$CheckEnableGOG
    [UIObject]$CheckEnableOrigin
    [UIObject]$CheckEnableSteam
    [UIObject]$RadioDbCustom
    [UIObject]$RadioDbDefault
    [UIObject]$RadioInstalledGOG
    [UIObject]$RadioInstalledOrigin
    [UIObject]$RadioInstalledSteam
    [UIObject]$RadioLibraryGOG
    [UIObject]$RadioLibraryOrigin
    [UIObject]$RadioLibrarySteam
    [UIObject]$TextDbFile
    [UIObject]$TextSteamAccount

    FirstTimeWizardWindow() : base({Get-UIWindow -ProcessName "PlayniteUI" | Get-UIWindow -AutomationId "WindowWizard"}, "FirstTimeWizardWindow")
    {
        $this.ButtonNext                = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonNext"}), "ButtonNext");
        $this.ButtonBack                = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonBack"}), "ButtonBack");
        $this.ButtonFinish              = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonFinish"}), "ButtonFinish");
        $this.ButtonBrowseDbFile        = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonBrowserDbFile"}), "ButtonBrowserDbFile");
        $this.ButtonGogAuthenticate     = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonGogAuthenticate"}), "ButtonGogAuthenticate");
        $this.ButtonOriginAuthenticate  = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonOriginAuthenticate"}), "ButtonOriginAuthenticate");
        $this.ButtonImportGames         = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonImportGames"}), "ButtonImportGames");
        $this.CheckEnableGOG            = [UIObject]::New($this.GetChildReference({Get-UICheckBox -AutomationId "CheckEnableGOG"}), "CheckEnableGOG");
        $this.CheckEnableOrigin         = [UIObject]::New($this.GetChildReference({Get-UICheckBox -AutomationId "CheckEnableOrigin"}), "CheckEnableOrigin");
        $this.CheckEnableSteam          = [UIObject]::New($this.GetChildReference({Get-UICheckBox -AutomationId "CheckEnableSteam"}), "CheckEnableSteam");
        $this.RadioDbCustom             = [UIObject]::New($this.GetChildReference({Get-UIRadioButton -AutomationId "RadioDbCustom"}), "RadioDbCustom");
        $this.RadioDbDefault            = [UIObject]::New($this.GetChildReference({Get-UIRadioButton -AutomationId "RadioDbDefault"}), "RadioDbDefault");
        $this.RadioInstalledGOG         = [UIObject]::New($this.GetChildReference({Get-UIRadioButton -AutomationId "RadioInstalledGOG"}), "RadioInstalledGOG");
        $this.RadioInstalledOrigin      = [UIObject]::New($this.GetChildReference({Get-UIRadioButton -AutomationId "RadioInstalledOrigin"}), "RadioInstalledOrigin");
        $this.RadioInstalledSteam       = [UIObject]::New($this.GetChildReference({Get-UIRadioButton -AutomationId "RadioInstalledSteam"}), "RadioInstalledSteam");
        $this.RadioLibraryGOG           = [UIObject]::New($this.GetChildReference({Get-UIRadioButton -AutomationId "RadioLibraryGOG"}), "RadioLibraryGOG");
        $this.RadioLibraryOrigin        = [UIObject]::New($this.GetChildReference({Get-UIRadioButton -AutomationId "RadioLibraryOrigin"}), "RadioLibraryOrigin");
        $this.RadioLibrarySteam         = [UIObject]::New($this.GetChildReference({Get-UIRadioButton -AutomationId "RadioLibrarySteam"}), "RadioLibrarySteam");
        $this.TextDbFile                = [UIObject]::New($this.GetChildReference({Get-UITextBox -AutomationId "TextDbFile"}), "TextDbFile");
        $this.TextSteamAccount          = [UIObject]::New($this.GetChildReference({Get-UITextBox -AutomationId "TextSteamAccount"}), "TextSteamAccount");
    }
}

return [FirstTimeWizardWindow]::New()