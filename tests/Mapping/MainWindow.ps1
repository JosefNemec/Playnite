using module PSNativeAutomation

class MainWindow : Window
{
    [ListBox]$ListBoxGames
    [ListView]$ListViewGames
    [Menu]$PopupMenu
    [UIObject]$TextFilter

    [UIObject]$ButtonInstall
    [UIObject]$ButtonMore
    [UIObject]$ButtonSetupProgress
    [UIObject]$ButtonPlay
    [UIObject]$HyperlinkStore
    [UIObject]$HyperlinkForum
    [UIObject]$HyperlinkWiki

    [Menu]$MenuMainMenu
    [Menu]$MenuViewSettings
    [Menu]$MenuViewMode
    [UIObject]$FilterSelector
    [UIObject]$GridGamesView
    [UIObject]$ImagesGamesView
    [UIObject]$ListGamesView
    [UIObject]$SearchBoxFilter
    [UIObject]$CheckFilterView
    [UIObject]$ImageLogo
    [UIObject]$ProgressControl
    [UIObject]$TabControlView
    [UIObject]$TextView
    [UIObject]$TextGroup

    MainWindow() : base({Get-UIWindow -ProcessName "PlayniteUI" -AutomationId "WindowMain"}, "MainWindow")
    {
        $this.ListBoxGames      = [ListBox]::New($this.GetChildReference({Get-UIListBox -AutomationId "ListGames" -First}), "ListBoxGames");
        $this.ListViewGames     = [ListView]::New($this.GetChildReference({Get-UIListView -AutomationId "GridGames" -First}), "ListViewGames");
        $this.PopupMenu         = [Menu]::New($this.GetChildReference({Get-UIControl -Class "Popup" -First}), "PopupMenu");
        $this.TextFilter        = [UIObject]::New($this.GetChildReference({Get-UITextBox -AutomationId "TextFilter" -First}), "TextFilter");

        $this.ButtonInstall       = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonInstall" -First}), "ButtonInstall")
        $this.ButtonMore          = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonMore" -First}), "ButtonMore")
        $this.ButtonSetupProgress = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonSetupProgress" -First}), "ButtonSetupProgress")
        $this.ButtonPlay          = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonPlay" -First}), "ButtonPlay")
        $this.HyperlinkStore      = [UIObject]::New($this.GetChildReference({Get-UIControl -AutomationId "HyperlinkStore" -First}), "HyperlinkStore")
        $this.HyperlinkForum      = [UIObject]::New($this.GetChildReference({Get-UIControl -AutomationId "HyperlinkForum" -First}), "HyperlinkForum")
        $this.HyperlinkWiki       = [UIObject]::New($this.GetChildReference({Get-UIControl -AutomationId "HyperlinkWiki" -First}), "HyperlinkWiki")

        $this.MenuMainMenu     = [Menu]::New($this.GetChildReference({Get-UIContextMenu -AutomationId "MenuMainMenu" -First}), "MenuMainMenu")
        $this.MenuViewSettings = [Menu]::New($this.GetChildReference({Get-UIContextMenu -AutomationId "MenuViewSettings" -First}), "MenuViewSettings")
        $this.MenuViewMode     = [Menu]::New($this.GetChildReference({Get-UIContextMenu -AutomationId "MenuViewMode" -First}), "MenuViewMode")
        $this.FilterSelector   = [UIObject]::New($this.GetChildReference({Get-UIControl -AutomationId "FilterSelector" -First}), "FilterSelector")
        $this.GridGamesView    = [UIObject]::New($this.GetChildReference({Get-UIControl -AutomationId "GridGamesView" -First}), "GridGamesView")
        $this.ImagesGamesView  = [UIObject]::New($this.GetChildReference({Get-UIControl -AutomationId "ImagesGamesView" -First}), "ImagesGamesView")
        $this.ListGamesView    = [UIObject]::New($this.GetChildReference({Get-UIControl -AutomationId "ListGamesView" -First}), "ListGamesView")
        $this.SearchBoxFilter  = [UIObject]::New($this.GetChildReference({Get-UIControl -AutomationId "SearchBoxFilter" -First}), "SearchBoxFilter")
        $this.CheckFilterView  = [UIObject]::New($this.GetChildReference({Get-UICheckBox -AutomationId "CheckFilterView" -First}), "CheckFilterView")
        $this.ImageLogo        = [UIObject]::New($this.GetChildReference({Get-UIImage -AutomationId "ImageLogo" -First}), "ImageLogo")
        $this.ProgressControl  = [UIObject]::New($this.GetChildReference({Get-UIControl -AutomationId "ProgressControl" -First}), "ProgressControl")
        $this.TabControlView   = [UIObject]::New($this.GetChildReference({Get-UIControl -AutomationId "TabControlView" -First}), "TabControlView")
        $this.TextView         = [UIObject]::New($this.GetChildReference({Get-UITextBlock -AutomationId "TextView" -First}), "TextView")
        $this.TextGroup        = [UIObject]::New($this.GetChildReference({Get-UITextBlock -AutomationId "TextGroup" -First}), "TextGroup")
    }
}

return [MainWindow]::New()