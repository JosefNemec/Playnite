﻿param(
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",
    [string]$OutputPath = (Join-Path $PWD "$($Configuration)SDK"),
    [switch]$SkipBuild = $false,
    [switch]$Sign = $false
)

$ErrorActionPreference = "Stop"
& .\common.ps1

# -------------------------------------------
#            Compile SDK
# -------------------------------------------
if (!$SkipBuild)
{
    New-EmptyFolder $OutputPath

    # Restore NuGet packages
    & (Get-NuGet) restore "..\source\PlayniteSDK\packages.config" -PackagesDirectory "..\source\packages"

    $project = Join-Path $pwd "..\source\PlayniteSDK\Playnite.SDK.csproj"
    & (Get-MSBuild) "$project" "/p:OutputPath=`"$outputPath`";Configuration=$configuration" "/t:Build"
    if ($LASTEXITCODE -ne 0)
    {
        throw "Build failed."
    }
    else
    {
        if ($Sign)
        {
            Join-Path $OutputPath "Playnite.SDK.dll" | SignFile
        }
    }
}

# -------------------------------------------
#            Create NUGET
# -------------------------------------------
$version = (Get-ChildItem (Join-Path $OutputPath "Playnite.SDK.dll")).VersionInfo.ProductVersion
$version = $version -replace "\.0$", ""
$spec = Get-Content "PlayniteSDK.nuspec"
$spec = $spec -replace "{Version}", $version
$spec = $spec -replace "{OutDir}", $OutputPath
$specFile = "nuget.nuspec"

try
{
    $spec | Out-File $specFile
    & (Get-NuGet) pack $specFile
    if ($LASTEXITCODE -ne 0)
    {
        throw "Nuget packing failed."
    }
}
finally
{
    Remove-Item $specFile -EA 0
}

return $true