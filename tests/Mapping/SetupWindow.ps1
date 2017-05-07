using module PSNativeAutomation

class SetupWindow : Window
{
    [UIObject]$ButtonInstall
    [UIObject]$ButtonCancel
    [UIObject]$ButtonBrowse
    [UIObject]$TextPath
    [UIObject]$CheckRunAfterInstall
    [Window]$WindowUninstallAsk

    SetupWindow() : base({Get-UIWindow -Name "Playnite * Setup *"}, "SetupWindow")
    {
        $this.ButtonInstall         = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "1" -First}), "ButtonInstall")
        $this.ButtonCancel          = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "2" -First}), "ButtonCancel")
        $this.ButtonBrowse          = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "1001" -First}), "ButtonBrowse")
        $this.TextPath              = [UIObject]::New($this.GetChildReference({Get-UIEdit -AutomationId "1019" -First}), "TextPath")
        $this.CheckRunAfterInstall  = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "1203" -First}), "CheckRunAfterInstall")
    }
}

return [SetupWindow]::New()