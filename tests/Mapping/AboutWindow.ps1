using module c:\devel\PowerShell\PSNativeAutomation\source\PSNativeAutomation\bin\Debug\PSNativeAutomation.psd1

class AboutWindow : Window
{
    [UIObject]$ButtonCreatePackage
    [UIObject]$ButtonClose
    [UIObject]$HyperlinkSource
    [UIObject]$HyperlinkHomepage
    [UIObject]$TextVersionInfo

    AboutWindow() : base({Get-UIWindow -ProcessName "PlayniteUI" | Get-UIWindow -AutomationId "WindowAbout"}, "WindowAbout")
    {
        $this.ButtonCreatePackage = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonCreatePackage" -First}), "ButtonCreatePackage")
        $this.ButtonClose         = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonClose" -First}), "ButtonClose")
        $this.HyperlinkSource     = [UIObject]::New($this.GetChildReference({Get-UIControl -AutomationId "HyperlinkSource" -First}), "HyperlinkSource")
        $this.HyperlinkHomepage   = [UIObject]::New($this.GetChildReference({Get-UIControl -AutomationId "HyperlinkHomepage" -First}), "HyperlinkHomepage")
        $this.TextVersionInfo     = [UIObject]::New($this.GetChildReference({Get-UITextBlock -AutomationId "TextVersionInfo" -First}), "TextVersionInfo")
    }
}

return [AboutWindow]::New()