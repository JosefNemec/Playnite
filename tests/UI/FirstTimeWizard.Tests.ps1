Describe "First Time Wizard - run through - installed only" {
    BeforeAll {
        $global:WizardWindow = & (Join-Path $PSScriptRoot "..\Mapping\FirstTimeWizardWindow.ps1")
        $global:InstalledGamesWindow = & (Join-Path $PSScriptRoot "..\Mapping\InstalledGamesWindow.ps1")
    }

    It "Wizard is opened" {
        $WizardWindow.WaitForObject(10000)
    }

    It "Database location is presented" {
        $WizardWindow.ButtonNext.Invoke()
        $WizardWindow.RadioDbDefault.GetSelectionState() | Should Be $true
        $WizardWindow.RadioDbCustom.GetSelectionState() | Should Be $false
        $WizardWindow.TextDbFile.GetEnabledState() | Should Be $false
        $WizardWindow.ButtonBrowseDbFile.GetEnabledState() | Should Be $false       
        $WizardWindow.RadioDbCustom.Select()
        $WizardWindow.RadioDbDefault.GetSelectionState() | Should Be $false
        $WizardWindow.RadioDbCustom.GetSelectionState() | Should Be $true
        $WizardWindow.TextDbFile.GetEnabledState() | Should Be $true
        $WizardWindow.ButtonBrowseDbFile.GetEnabledState() | Should Be $true
        $WizardWindow.RadioDbDefault.Select()
    }

    It "All integrations are presented and enabled" {
        $WizardWindow.ButtonNext.Invoke()
        $WizardWindow.CheckEnableGOG.GetToggleState() | Should Be "On"
        $WizardWindow.CheckEnableOrigin.GetToggleState() | Should Be "On"
        $WizardWindow.CheckEnableSteam.GetToggleState() | Should Be "On"
    }

    It "Steam import page" {
        $WizardWindow.ButtonNext.Invoke()
        $WizardWindow.RadioInstalledSteam.GetSelectionState() | Should Be $true
        $WizardWindow.TextSteamAccount.GetEnabledState() | Should Be $false
        $WizardWindow.RadioLibrarySteam.Select()
        $WizardWindow.TextSteamAccount.GetEnabledState() | Should Be $true
        $WizardWindow.RadioInstalledSteam.Select()
    }

    It "GOG import page" {
        $WizardWindow.ButtonNext.Invoke()
        $WizardWindow.RadioInstalledGOG.GetSelectionState() | Should Be $true
        $WizardWindow.ButtonGogAuthenticate.GetEnabledState() | Should Be $false
        $WizardWindow.RadioLibraryGOG.Select()
        $WizardWindow.ButtonGogAuthenticate.GetEnabledState() | Should Be $true
        $WizardWindow.RadioInstalledGOG.Select()
    }

    It "Origin import page" {
        $WizardWindow.ButtonNext.Invoke()
        $WizardWindow.RadioInstalledOrigin.GetSelectionState() | Should Be $true
        $WizardWindow.ButtonOriginAuthenticate.GetEnabledState() | Should Be $false
        $WizardWindow.RadioLibraryOrigin.Select()
        $WizardWindow.ButtonOriginAuthenticate.GetEnabledState() | Should Be $true        
        $WizardWindow.RadioInstalledOrigin.Select()
    }

    It "Custom import page" {
        $WizardWindow.ButtonNext.Invoke()
        $WizardWindow.ButtonImportGames.GetEnabledState() | Should Be $true
        $WizardWindow.ButtonImportGames.Invoke()        
        $InstalledGamesWindow.WaitForObject(5000)
        $InstalledGamesWindow.Close()
    }

    It "Finish page" {
        $WizardWindow.ButtonNext.Invoke()
        $WizardWindow.ButtonFinish.Invoke()
        Start-Sleep -Seconds 2
        $WizardWindow.Exists() | Should Be $false
    }
}