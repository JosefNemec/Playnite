using module c:\devel\PowerShell\PSNativeAutomation\source\PSNativeAutomation\bin\Debug\PSNativeAutomation.psd1

class InstalledGamesWindow : Window
{
    [UIObject]$ButtonCancel
    [UIObject]$ButtonBrowse
    [UIObject]$ButtonOK
    [ListView]$ListPrograms
    [UIObject]$TextDownloading
    
    InstalledGamesWindow() : base({Get-UIWindow -ProcessName "PlayniteUI" | Get-UIWindow -AutomationId "WindowInstalledGames"}, "InstalledGamesWindow")
    {
        $this.ButtonCancel         = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonCancel" -First}), "ButtonCancel")
        $this.ButtonBrowse         = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonBrowse" -First}), "ButtonBrowse")
        $this.ButtonOK             = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonOK" -First}), "ButtonOK")
        $this.ListPrograms         = [ListView]::New($this.GetChildReference({Get-UIListView -AutomationId "ListPrograms" -First}), "ListPrograms")
        $this.TextDownloading      = [UIObject]::New($this.GetChildReference({Get-UITextBlock -AutomationId "TextDownloading" -First}), "TextDownloading")
    }
}

return [InstalledGamesWindow]::New()