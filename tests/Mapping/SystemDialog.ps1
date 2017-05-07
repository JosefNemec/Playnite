using module PSNativeAutomation

class SystemDialog : Window
{
    [UIObject]$ButtonYes
    [UIObject]$ButtonNo
    
    SystemDialog() : base({Get-UIWindow -ProcessName "PlayniteUI" | Get-UIControl -ControlType "Dialog" -First}, "SystemDialog")
    {
        $this.ButtonYes = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "6" -First}), "ButtonYes")
        $this.ButtonNo  = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "7" -First}), "ButtonNo")
    }
}

return [SystemDialog]::New()