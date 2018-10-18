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
    if (-not (Test-Path "nuget.exe"))
    {
        Invoke-WebRequest -Uri $NugetUrl -OutFile "nuget.exe"
    }

    StartAndWait "nuget.exe" "restore ..\source\PlayniteSDK\packages.config -PackagesDirectory ..\source\packages"
    $project = Join-Path $pwd "..\source\PlayniteSDK\Playnite.SDK.csproj"
    $msbuildPath = "c:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe";
    $arguments = "`"$project`" /p:OutputPath=`"$outputPath`";Configuration=$configuration /t:Build";
    $compilerResult = StartAndWait $msbuildPath $arguments
    if ($compilerResult -ne 0)
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
    $packageRes = StartAndWait "nuget.exe" "pack $specFile"
    if ($packageRes -ne 0)
    {
        throw "Nuget packing failed."
    }
}
finally
{
    Remove-Item $specFile -EA 0
}

return $true