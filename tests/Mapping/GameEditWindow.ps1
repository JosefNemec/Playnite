using module c:\devel\PowerShell\PSNativeAutomation\source\PSNativeAutomation\bin\Debug\PSNativeAutomation.psd1

class GameTaskControl : UIObject
{
    [UIObject]$ButtonBrowse
    [UIObject]$ComboType
    [UIObject]$TextArguments
    [UIObject]$TextWorkDir
    [UIObject]$TextName
    [UIObject]$TextPath

    GameTaskControl([scriptblock]$ScriptReference, [string]$Name) : base($ScriptReference, $Name)
    {
        $this.ButtonBrowse  = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonBrowse" -First}), "ButtonBrowse")
        $this.ComboType     = [UIObject]::New($this.GetChildReference({Get-UIComboBox -AutomationId "ComboType" -First}), "ComboType")
        $this.TextArguments = [UIObject]::New($this.GetChildReference({Get-UITextBox -AutomationId "TextArguments" -First}), "TextArguments")
        $this.TextWorkDir   = [UIObject]::New($this.GetChildReference({Get-UITextBox -AutomationId "TextWorkDir" -First}), "TextWorkDir")
        $this.TextName      = [UIObject]::New($this.GetChildReference({Get-UITextBox -AutomationId "TextName" -First}), "TextName")
        $this.TextPath      = [UIObject]::New($this.GetChildReference({Get-UITextBox -AutomationId "TextPath" -First}), "TextPath")
    }

    GameTaskControl([object]$ObjectReference) : base($ObjectReference)
    {
        $this.ButtonBrowse  = [UIObject]::New(($ObjectReference | Get-UIButton -AutomationId "ButtonBrowse" -First))
        $this.ComboType     = [UIObject]::New(($ObjectReference | Get-UIComboBox -AutomationId "ComboType" -First))
        $this.TextArguments = [UIObject]::New(($ObjectReference | Get-UITextBox -AutomationId "TextArguments" -First))
        $this.TextWorkDir   = [UIObject]::New(($ObjectReference | Get-UITextBox -AutomationId "TextWorkDir" -First))
        $this.TextName      = [UIObject]::New(($ObjectReference | Get-UITextBox -AutomationId "TextName" -First))
        $this.TextPath      = [UIObject]::New(($ObjectReference | Get-UITextBox -AutomationId "TextPath" -First))
    }
}

class GameEditWindow : Window
{
    [UIObject]$ButtonSelectImage
    [UIObject]$ButtonAddPlay
    [UIObject]$ButtonUseExeIcon
    [UIObject]$ButtonPickCat
    [UIObject]$ButtonSelectIcon
    [UIObject]$ButtonDownload
    [UIObject]$ButtonCancel
    [UIObject]$ButtonOK
    [UIObject]$ButtonRemovePlay
    [UIObject]$ButtonAddAction
    [UIObject]$CheckForums
    [UIObject]$CheckWiki
    [UIObject]$CheckImage
    [UIObject]$CheckIcon
    [UIObject]$CheckDescription
    [UIObject]$CheckStore
    [UIObject]$CheckCategories
    [UIObject]$CheckGenres
    [UIObject]$CheckReleaseDate
    [UIObject]$CheckPublisher
    [UIObject]$CheckName
    [UIObject]$CheckDeveloper
    [UIObject]$ImageImage
    [UIObject]$ImageIcon
    [UIObject]$OtherTasksItems
    [GameTaskControl]$TaskPlay
    [UIObject]$TaskCustom
    [TabControl]$TabControlMain
    [UIObject]$TabActions
    [UIObject]$TextGenres
    [UIObject]$TextName
    [UIObject]$TextWiki
    [UIObject]$TextForums
    [UIObject]$TextStore
    [UIObject]$TextCategories
    [UIObject]$TextDeveloper
    [UIObject]$TextReleaseDate
    [UIObject]$TextPublisher
    [UIObject]$TextDescription
    
    GameEditWindow() : base({Get-UIWindow -ProcessName "PlayniteUI" -AutomationId "WindowMain" | Get-UIWindow -AutomationId "WindowGameEdit"}, "GameEditWindow")
    {
        $this.ButtonSelectImage = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonSelectImage" -First}), "ButtonSelectImage")
        $this.ButtonAddPlay     = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonAddPlay" -First}), "ButtonAddPlay")
        $this.ButtonUseExeIcon  = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonUseExeIcon" -First}), "ButtonUseExeIcon")
        $this.ButtonPickCat     = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonPickCat" -First}), "ButtonPickCat")
        $this.ButtonSelectIcon  = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonSelectIcon" -First}), "ButtonSelectIcon")
        $this.ButtonDownload    = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonDownload" -First}), "ButtonDownload")
        $this.ButtonCancel      = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonCancel" -First}), "ButtonCancel")
        $this.ButtonOK          = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonOK" -First}), "ButtonOK")
        $this.ButtonRemovePlay  = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonRemovePlay" -First}), "ButtonRemovePlay")
        $this.ButtonAddAction   = [UIObject]::New($this.GetChildReference({Get-UIButton -AutomationId "ButtonAddAction" -First}), "ButtonAddAction")
        $this.CheckForums       = [UIObject]::New($this.GetChildReference({Get-UICheckBox -AutomationId "CheckForums" -First}), "CheckForums")
        $this.CheckWiki         = [UIObject]::New($this.GetChildReference({Get-UICheckBox -AutomationId "CheckWiki" -First}), "CheckWiki")
        $this.CheckImage        = [UIObject]::New($this.GetChildReference({Get-UICheckBox -AutomationId "CheckImage" -First}), "CheckImage")
        $this.CheckIcon         = [UIObject]::New($this.GetChildReference({Get-UICheckBox -AutomationId "CheckIcon" -First}), "CheckIcon")
        $this.CheckDescription  = [UIObject]::New($this.GetChildReference({Get-UICheckBox -AutomationId "CheckDescription" -First}), "CheckDescription")
        $this.CheckStore        = [UIObject]::New($this.GetChildReference({Get-UICheckBox -AutomationId "CheckStore" -First}), "CheckStore")
        $this.CheckCategories   = [UIObject]::New($this.GetChildReference({Get-UICheckBox -AutomationId "CheckCategories" -First}), "CheckCategories")
        $this.CheckGenres       = [UIObject]::New($this.GetChildReference({Get-UICheckBox -AutomationId "CheckGenres" -First}), "CheckGenres")
        $this.CheckReleaseDate  = [UIObject]::New($this.GetChildReference({Get-UICheckBox -AutomationId "CheckReleaseDate" -First}), "CheckReleaseDate")
        $this.CheckPublisher    = [UIObject]::New($this.GetChildReference({Get-UICheckBox -AutomationId "CheckPublisher" -First}), "CheckPublisher")
        $this.CheckName         = [UIObject]::New($this.GetChildReference({Get-UICheckBox -AutomationId "CheckName" -First}), "CheckName")
        $this.CheckDeveloper    = [UIObject]::New($this.GetChildReference({Get-UICheckBox -AutomationId "CheckDeveloper" -First}), "CheckDeveloper")
        $this.ImageImage        = [UIObject]::New($this.GetChildReference({Get-UIImage -AutomationId "ImageImage" -First}), "ImageImage")
        $this.ImageIcon         = [UIObject]::New($this.GetChildReference({Get-UIImage -AutomationId "ImageIcon" -First}), "ImageIcon")
        $this.OtherTasksItems   = [UIObject]::New($this.GetChildReference({Get-UIControl -AutomationId "OtherTasksItems" -First}), "OtherTasksItems")
        $this.TaskPlay          = [GameTaskControl]::New($this.GetChildReference({Get-UIControl -AutomationId "TaskPlay" -First}), "TaskPlay")
        $this.TaskCustom        = [UIObject]::New($this.GetChildReference({Get-UIControl -AutomationId "TaskCustom" -First}), "TaskCustom")
        $this.TabControlMain    = [TabControl]::New($this.GetChildReference({Get-UITabControl -AutomationId "TabControlMain" -First}), "TabControlMain")
        $this.TabActions        = [UIObject]::New($this.GetChildReference({Get-UIControl -AutomationId "TabActions" -First}), "TabActions")
        $this.TextGenres        = [UIObject]::New($this.GetChildReference({Get-UITextBox -AutomationId "TextGenres" -First}), "TextGenres")
        $this.TextName          = [UIObject]::New($this.GetChildReference({Get-UITextBox -AutomationId "TextName" -First}), "TextName")
        $this.TextWiki          = [UIObject]::New($this.GetChildReference({Get-UITextBox -AutomationId "TextWiki" -First}), "TextWiki")
        $this.TextForums        = [UIObject]::New($this.GetChildReference({Get-UITextBox -AutomationId "TextForums" -First}), "TextForums")
        $this.TextStore         = [UIObject]::New($this.GetChildReference({Get-UITextBox -AutomationId "TextStore" -First}), "TextStore")
        $this.TextCategories    = [UIObject]::New($this.GetChildReference({Get-UITextBox -AutomationId "TextCategories" -First}), "TextCategories")
        $this.TextDeveloper     = [UIObject]::New($this.GetChildReference({Get-UITextBox -AutomationId "TextDeveloper" -First}), "TextDeveloper")
        $this.TextReleaseDate   = [UIObject]::New($this.GetChildReference({Get-UITextBox -AutomationId "TextReleaseDate" -First}), "TextReleaseDate")
        $this.TextPublisher     = [UIObject]::New($this.GetChildReference({Get-UITextBox -AutomationId "TextPublisher" -First}), "TextPublisher")
        $this.TextDescription   = [UIObject]::New($this.GetChildReference({Get-UITextBox -AutomationId "TextDescription" -First}), "TextDescription")
    }
}

return [GameEditWindow]::New()