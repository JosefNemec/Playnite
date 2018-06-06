param(
    [string]$TestName
)

$ErrorActionPreference = "Stop"

Import-Module Pester
Import-Module PSNativeAutomation
Import-Module powershell-yaml
Invoke-Expression ".\TestExtensions.ps1"
Invoke-Expression ".\PlayniteCommon.ps1"

if ($TestName)
{
    Invoke-Pester -TestName $TestName
}
else
{
    @(
        "Initial Startup",
        "First Time Wizard - run through - installed only",
        "Game import startup",
        "Edit Window - Basic Test",
        "Edit Window - Categories",
        "Installed Games Window - Game import test",
        "Custom Games - Game Creation"
    ) | ForEach-Object {
        Invoke-Pester -TestName $_
    }
}