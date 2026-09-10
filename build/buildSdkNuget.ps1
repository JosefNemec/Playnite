#Requires -Version 7

param(
    [string]$Version
)

$global:ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true

dotnet pack "..\source\PlayniteSDK\Playnite.SDK.csproj" -c Release -o ".\" -p:PackageVersion=$Version