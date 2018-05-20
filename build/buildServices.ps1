param(
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",
    [string]$OutputPath = (Join-Path (Get-Location) "PlayniteServices\"),
    [switch]$LocalDeploy
)

$ErrorActionPreference = "Stop"

if ($LocalDeploy)
{
    Stop-Service -Name "W3SVC"
}

if (Test-Path $OutputPath)
{
    Remove-Item $OutputPath -Recurse -Exclude "servicedb.db", "patreon_tokens.json", "customSettings.json"
}

if (!(Test-Path $OutputPath))
{
    New-Item $OutputPath -ItemType Directory | Out-Null
}

Push-Location
Set-Location "..\source\PlayniteServices\"

$compiler = Start-Process "dotnet" "restore" -PassThru -NoNewWindow
$handle = $compiler.Handle # cache proc.Handle http://stackoverflow.com/a/23797762/1479211
$compiler.WaitForExit()

$arguments = "publish -c {0} -o {1}" -f $Configuration, $OutputPath
$compiler = Start-Process "dotnet" $arguments -PassThru -NoNewWindow
$handle = $compiler.Handle # cache proc.Handle http://stackoverflow.com/a/23797762/1479211
$compiler.WaitForExit()

if ($compiler.ExitCode -ne 0)
{
    Write-Host "Build failed." -ForegroundColor "Red"
}
else
{
    if ($LocalDeploy)
    {
        Start-Service -Name "W3SVC"
    }
}

Pop-Location