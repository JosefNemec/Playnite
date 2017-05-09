function CleanWindows()
{
    if ($windowGameEdit.Exists())
    {
        $windowGameEdit.Close()
    }
}

Describe "Custom Games - Game Creation" {
    BeforeAll {
        $windowMain = & (Join-Path $PSScriptRoot "..\Mapping\MainWindow.ps1")
        $windowGameEdit = & (Join-Path $PSScriptRoot "..\Mapping\GameEditWindow.ps1")
        $dialogSystem = & (Join-Path $PSScriptRoot "..\Mapping\SystemDialog.ps1")
        
        $gameName = "AA Test Game"
        $gamePath = "c:\Program Files\Windows NT\Accessories\wordpad.exe"
        $windowMain.Focus()
    }

    It "Custom game can be added" {
        $windowMain.ImageLogo.Click()
        $windowMain.PopupMenu.InvokeItem("Add Game...")
        $windowGameEdit.WaitForObject(2000)

        # Fill fields
        $windowGameEdit.TextName.SetValue($gameName)

        # Add Play Action
        $windowGameEdit.TabControlMain.SelectItem("Actions")
        $windowGameEdit.ButtonAddPlay.Invoke()
        $windowGameEdit.TaskPlay.TextPath.SetValue($gamePath)

        $windowGameEdit.ButtonOK.Invoke()
    }

    It "Custom game can be started" {
        $windowMain.ListBoxGames.GetItemNames() -contains $gameName | Should Be $true
        $windowMain.ListBoxGames.GetItem($gameName).ClickDouble()
        Start-Sleep -Seconds 1
        Get-Process -Name "wordpad" -EA 0 | Should Not Be $null        
        Get-Process -Name "wordpad" -EA 0 | Stop-Process
    }

    It "Custom game can be removed" {
        $windowMain.ListBoxGames.SelectItem($gameName)
        $windowMain.ButtonMore.Invoke()
        $windowMain.PopupMenu.InvokeItem("Remove")
        $dialogSystem.WaitForObject(2000)
        $dialogSystem.ButtonYes.Invoke()
        $windowMain.ListBoxGames.GetItemNames() -contains $gameName | Should Be $false
    }

    AfterAll {
        CleanWindows
    }
}