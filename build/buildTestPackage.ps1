param(
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",
    [string]$OutputPath = (Join-Path (Get-Location) "TestPackage\"),
    [switch]$SkipBuild = $false
)

$ErrorActionPreference = "Stop"

if (Test-Path $OutputPath)
{
    Remove-Item $OutputPath -Recurse
}

New-Item $OutputPath -ItemType Directory | Out-Null

if (-not $SkipBuild)
{
    $result = & .\build.ps1 -Configuration $Configuration -Setup -Portable
    if (-not $result)
    {
        throw "Build failed, test package cannot be created."
    }
}

Copy-Item "PlayniteInstaller.exe" (Join-Path $OutputPath "PlayniteInstaller.exe")
Copy-Item "PlaynitePortable.zip" (Join-Path $OutputPath "PlaynitePortable.zip")
Copy-Item "..\tests\" $OutputPath -Recurse

$config = @{
    "Package" = "..\PlayniteInstaller.exe";
    "PortablePackage" = "..\PlaynitePortable.zip";
    "InstallPath" = "C:\Program Files\Playnite\"
}

$config | ConvertTo-Json | Out-File (Join-Path $OutputPath "tests\testConfig.json")