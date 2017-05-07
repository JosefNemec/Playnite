using module PSNativeAutomation

class CategoryConfigWindow : Window
{
    [UIObject]$ButtonAddCat
    [UIObject]$ButtonCancel
    [UIObject]$ButtonOK
    [ListBox]$ListCategories
    [UIObject]$TextNewCat

    CategoryConfigWindow() : base({Get-UIWindow -ProcessName "PlayniteUI" | Get-UIWindow -AutomationId "WindowCategories"}, "CategoryConfigWindow")
    {
        $this.ButtonAddCat     = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonAddCat" -First}), "ButtonAddCat")
        $this.ButtonCancel     = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonCancel" -First}), "ButtonCancel")
        $this.ButtonOK         = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonOK" -First}), "ButtonOK")
        $this.ListCategories   = [ListBox]::New($this.GetChildReference({Get-UIListBox -AutomationId "ListCategories" -First}), "ListCategories")
        $this.TextNewCat       = [UIObject]::New($this.GetChildReference({Get-UITextBox -AutomationId "TextNewCat" -First}), "TextNewCat")
    }
}

return [CategoryConfigWindow]::New()