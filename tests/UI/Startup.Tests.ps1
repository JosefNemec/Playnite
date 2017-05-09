Describe "Initial Startup" {
    BeforeAll {
        $windowMain = & (Join-Path $PSScriptRoot "..\Mapping\MainWindow.ps1")        
        $testData = Get-TestProperties
    }

    It "Playnite can be started and window is opened" {
        Start-Process (Join-Path $testData.InstallPath "PlayniteUI.exe")
        $windowMain.WaitForObject(10000)
    }
}

Describe "Game import startup" {
    BeforeAll {
        $windowMain = & (Join-Path $PSScriptRoot "..\Mapping\MainWindow.ps1")
        $windowNotifications = & (Join-Path $PSScriptRoot "..\Mapping\NotificationsWindow.ps1")
        $windowMain.WaitForObject(10000)
    }

    It "Import progress is shown and games are loaded" {
        $windowMain.ProgressControl.IsVisible() | Should Be $true
        WaitFor { $windowMain.ProgressControl.IsVisible() -eq $false } -Timeout 30000
        $windowMain.ListBoxGames.GetItemNames().Count | Should BeGreaterThan 0
        $windowNotifications.Exists() | Should Be $false
    }
}