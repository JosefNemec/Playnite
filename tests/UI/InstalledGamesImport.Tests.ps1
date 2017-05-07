function CleanWindows()
{
    Get-Process -Name "notepad" -EA 0 | Stop-Process

    if ($windowOpenFile.Exists())
    {
        $windowOpenFile.Close()
    }

    if ($windowInstalled.Exists())
    {
        $windowInstalled.Close()
    }
}

Describe "Installed Games Window - Game import test" {
    BeforeAll {
        $windowMain = & (Join-Path $PSScriptRoot "..\Mapping\MainWindow.ps1")
        $windowInstalled = & (Join-Path $PSScriptRoot "..\Mapping\InstalledGamesWindow.ps1")
        $windowOpenFile = & (Join-Path $PSScriptRoot "..\Mapping\OpenFileWindow.ps1")
        $windowMain.Focus()
    }

    It "Import window can be opened and import is possible" {
        $windowMain.ImageLogo.Click()
        $windowMain.PopupMenu.InvokeItem("Add Installed Game(s)...")
        $windowInstalled.WaitForObject(2000)
        $windowInstalled.ListPrograms.WaitForObjectVisible(5000)

        # Import first program in list
        $items = $windowInstalled.ListPrograms.GetItems()
        $items.Count | Should BeGreaterThan 0
        $importedGame = $items[0]
        $importedName = $importedGame.GetName()
        $importedName | Should Not BeNullOrEmpty
        $importedGame.GetHelpText() | Should Not BeNullOrEmpty
        $importedGame.GetNativeObject() | Get-UICheckBox | Set-UIToggleState
        
        # Import custom program
        $windowInstalled.ButtonBrowse.Invoke()
        $windowOpenFile.EditFileName.SetValue("C:\Windows\System32\notepad.exe")
        $windowOpenFile.ButtonOpen.Invoke()
        $newItem = $windowInstalled.ListPrograms.GetItems() | Select-Object -Last 1
        $newItem.GetName() | Should Be "System32"
        $newItem.GetHelpText() | Should Be "C:\Windows\System32\notepad.exe"
        $newItem.GetNativeObject() | Get-UICheckBox | Get-UIToggleState | Should Be "On"

        $windowInstalled.ButtonOK.Invoke()

        # Verify that programs are added
        $items = $windowMain.ListBoxGames.GetItemNames() 
        $items -contains $importedName | Should Be $true
        $items -contains "System32" | Should Be $true

        # Imported programs can be started
        $windowMain.ListBoxGames.SelectItem("System32")
        $windowMain.ButtonPlay.Invoke()
        Start-Sleep -Seconds 1
        Get-Process -Name "notepad" -EA 0 | Should Not Be $null
        Get-Process -Name "notepad" -EA 0 | Stop-Process
    }

    AfterAll {
        CleanWindows
    }
}