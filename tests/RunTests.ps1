$ErrorActionPreference = "Stop"

Import-Module "Pester"
Import-Module "PSNativeAutomation"
Invoke-Expression ".\TestExtensions.ps1"

Invoke-Pester -TestName $args[0]

