param(
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",
    [string]$OutputPath = (Join-Path (Get-Location) "PlayniteServices\")
)

$ErrorActionPreference = "Stop"

if (Test-Path $OutputPath)
{
    Remove-Item $OutputPath -Recurse
}

New-Item $OutputPath -ItemType Directory | Out-Null

Push-Location
Set-Location "..\source\PlayniteServices\"

$arguments = "publish -c {0} -o {1}" -f $Configuration, $OutputPath
$compiler = Start-Process "dotnet" $arguments -PassThru -NoNewWindow
$handle = $compiler.Handle # cache proc.Handle http://stackoverflow.com/a/23797762/1479211
$compiler.WaitForExit()

if ($compiler.ExitCode -ne 0)
{
    Write-Host "Build failed." -ForegroundColor "Red"
}

Pop-Location