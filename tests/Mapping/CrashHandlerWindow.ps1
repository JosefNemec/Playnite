using module PSNativeAutomation

class CrashHandlerWindow : Window
{
    [UIObject]$ButtonSaveDiag
    [UIObject]$ButtonClose
    [UIObject]$TextDetails
    
    CrashHandlerWindow() : base({Get-UIWindow -ProcessName "PlayniteUI" | Get-UIWindow -AutomationId "WindowCrash"}, "CrashHandlerWindow")
    {
        $this.ButtonSaveDiag = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonSaveDiag" -First}), "ButtonSaveDiag")
        $this.ButtonClose    = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonClose" -First}), "ButtonClose")
        $this.TextDetails    = [UIObject]::New($this.GetChildReference({Get-UITextBox -AutomationId "TextDetails" -First}), "TextDetails")
    }
}

return [CrashHandlerWindow]::New()