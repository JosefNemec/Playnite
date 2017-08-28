param(
    [Parameter(Mandatory)]
    [string]$Version,    
    [string]$CompareBranch = "origin/master"
)

$ErrorActionPreference = "Stop"

$versionSize = ($Version.ToCharArray() | Where { $_ -eq "." }).Count
$updateVersion = $Version
$asmVersion = $Version + (".0" * (4 - $versionSize - 1))

$updateInfoPath = "..\web\update\update.json"
$asmInfoPath = "..\source\PlayniteUI\Properties\AssemblyInfo.cs"
$changelogPath = "..\web\update\" + $updateVersion + ".html"
$changelogTemplatePath = "..\web\update\template.html"
$asmInfoContent = Get-Content $asmInfoPath

# AssemblyInfo.cs update
$asmInfoContent = $asmInfoContent -replace '^\[.*AssemblyVersion.*$', "[assembly: AssemblyVersion(`"$asmVersion`")]"
$asmInfoContent = $asmInfoContent -replace '^\[.*AssemblyFileVersion.*$', "[assembly: AssemblyFileVersion(`"$asmVersion`")]"
$asmInfoContent  | Out-File $asmInfoPath -Encoding utf8

# update.json update
$updateInfoContent = Get-Content $updateInfoPath | ConvertFrom-Json
$updateInfoContent.stable.version = $updateVersion
$updateInfoContent.stable.url = "https://github.com/JosefNemec/Playnite/releases/download/$updateVersion/PlayniteInstaller.exe"
$releases = {$updateInfoContent.stable.releases}.Invoke()
$releases.Insert(0, (New-Object psobject -Property @{
    version = $updateVersion
    file = $updateVersion + ".html"
}))
$updateInfoContent.stable.releases = $releases
$updateInfoContent | ConvertTo-Json -Depth 10 | Out-File $updateInfoPath -Encoding utf8

# Changelog
$gitOutPath = "gitout.txt"
Start-Process "git" "log --pretty=oneline ...$CompareBranch" -NoNewWindow -Wait -RedirectStandardOutput $gitOutPath
$messages = (Get-Content $gitOutPath) -replace "^(.*?)\s" | Sort-Object -Descending
$changelogContent = (Get-Content $changelogTemplatePath) -replace "{changelog}", ($messages | ForEach { "<li>$_</li>" } | Out-String)
$changelogContent | Out-File $changelogPath -Encoding utf8
Remove-Item $gitOutPath

Write-Host "Git Changelog" -ForegroundColor Green
$messages | Write-Host

Write-Host "Others Changelog" -ForegroundColor Green
$messages | ForEach { $_ -replace "\s*#\d+", "" } | Write-Host