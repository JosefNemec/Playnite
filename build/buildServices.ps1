param(
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",
    [string]$OutputPath = (Join-Path (Get-Location) "PlayniteServices\"),
    [switch]$IISDeploy
)

$ErrorActionPreference = "Stop"
& .\common.ps1

if ($IISDeploy)
{
    Write-WarningLog "Deploying to IIS..."
    Stop-Service -Name "W3SVC"
}

if (Test-Path $OutputPath)
{
    Remove-Item $OutputPath -Recurse -Exclude "servicedb.db", "patreon_tokens.json", "customSettings.json"
}

New-Folder $OutputPath
Push-Location
Set-Location "..\source\PlayniteServices\"

try
{
    Write-OperationLog "Restoring packages..."
    $restore = StartAndWait "dotnet" "restore"
    if ($restore -ne 0)
    {
        throw "Package restore failed."
    }

    Write-OperationLog "Building..."
    $compiler = StartAndWait "dotnet" ("publish -c {0} -o {1}" -f $Configuration, $OutputPath)
    if ($compiler -ne 0)
    {
        Write-Host "Build failed." -ForegroundColor "Red"
    }
    else
    {
        if ($IISDeploy)
        {
            Start-Service -Name "W3SVC"
        }
    }
}
finally
{
    Pop-Location 
}