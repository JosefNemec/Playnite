#Requires -Version 7

$global:ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true

dotnet pack "..\source\PlayniteSDK\Playnite.SDK.csproj" -c Release -o ".\"
dotnet pack "..\source\Playnite.Deps\Playnite.Deps.csproj" -c Release -o ".\"