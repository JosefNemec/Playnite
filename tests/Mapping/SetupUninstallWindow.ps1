
using module c:\devel\PowerShell\PSNativeAutomation\source\PSNativeAutomation\bin\Debug\PSNativeAutomation.psd1

class SetupUninstallWindow : Window
{
    [UIObject]$ButtonYes
    [UIObject]$ButtonNo
    [UIObject]$ButtonClose
    [UIObject]$TextStatus

    SetupUninstallWindow() : base({Get-UIWindow -ProcessName "Un_A"}, "SetupUninstallWindow")
    {
        $this.ButtonYes = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "6" -First}), "ButtonYes")
        $this.ButtonNo  = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "7" -First}), "ButtonNo")
        $this.ButtonClose  = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "1" -First}), "ButtonClose")
        $this.TextStatus  = [UIObject]::New($this.GetChildReference({Get-UIControl -AutomationId "1006" -First}), "TextStatus")
    }
}

return [SetupUninstallWindow]::New()