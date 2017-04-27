$ErrorActionPreference = "Stop"

Import-Module "Pester"
Import-Module "c:\devel\PowerShell\PSNativeAutomation\source\PSNativeAutomation\bin\Debug\PSNativeAutomation.psd1"
Invoke-Expression ".\TestExtensions.ps1"

Invoke-Pester -TestName $args[0]

