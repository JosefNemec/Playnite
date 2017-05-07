using module PSNativeAutomation

class MetadataLookupWindow : Window
{
    [UIObject]$ButtonOK
    [UIObject]$ButtonCancel
    [UIObject]$ButtonSearch
    [ListBox]$ListSearch
    [UIObject]$TextDownloading
    [UIObject]$TextSearch

    MetadataLookupWindow() : base({Get-UIWindow -ProcessName "PlayniteUI" | Get-UIWindow -AutomationId "WindowMetaSearch"}, "MetadataLookupWindow")
    {
        $this.ButtonOK         = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonOK" -First}), "ButtonOK")
        $this.ButtonCancel     = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonCancel" -First}), "ButtonCancel")
        $this.ButtonSearch     = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonSearch" -First}), "ButtonSearch")
        $this.ListSearch       = [ListBox]::New($this.GetChildReference({Get-UIListBox -AutomationId "ListSearch" -First}), "ListSearch")
        $this.TextDownloading  = [UIObject]::New($this.GetChildReference({Get-UITextBlock -AutomationId "TextDownloading" -First}), "TextDownloading")
        $this.TextSearch       = [UIObject]::New($this.GetChildReference({Get-UITextBox -AutomationId "TextSearch" -First}), "TextSearch")
    }
}

return [MetadataLookupWindow]::New()