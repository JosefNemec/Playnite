param(
    [Parameter(ParameterSetName="Base",Mandatory=$true)]
    [string]$Version,
    [Parameter(Mandatory=$true)]
    [string]$CompareBranch,
    [switch]$CommitAndPush,
    [switch]$Build,
    [switch]$PublishRelease,
    [string]$GitHubAuth,
    [switch]$PublishManifest,
    [string]$ManifestFtpUri
)

$ErrorActionPreference = "Stop"

$versionSize = ($Version.ToCharArray() | Where { $_ -eq "." }).Count
$updateVersion = $Version
$asmVersion = $Version + (".0" * (4 - $versionSize - 1))

Write-Host "Publishing version $updateVersion" -ForegroundColor Cyan

$updateInfoPath = "..\web\update\update.json"
$asmInfoPath = "..\source\PlayniteUI\Properties\AssemblyInfo.cs"
$changelogPath = "..\web\update\" + $updateVersion + ".html"
$changelogTemplatePath = "..\web\update\template.html"
$asmInfoContent = Get-Content $asmInfoPath

# AssemblyInfo.cs update
Write-Host "Changing assembly info..." -ForegroundColor Green
$asmInfoContent = $asmInfoContent -replace '^\[.*AssemblyVersion.*$', "[assembly: AssemblyVersion(`"$asmVersion`")]"
$asmInfoContent = $asmInfoContent -replace '^\[.*AssemblyFileVersion.*$', "[assembly: AssemblyFileVersion(`"$asmVersion`")]"
$asmInfoContent  | Out-File $asmInfoPath -Encoding utf8

# update.json update
Write-Host "Updating update manifest..." -ForegroundColor Green
$updateInfoContent = Get-Content $updateInfoPath | ConvertFrom-Json
$updateInfoContent.stable.version = $updateVersion
$updateInfoContent.stable.url = "https://github.com/JosefNemec/Playnite/releases/download/$updateVersion/PlayniteInstaller.exe"
$releases = {$updateInfoContent.stable.releases}.Invoke()
if ($releases[0].version -ne $updateVersion)
{
    $releases.Insert(0, (New-Object psobject -Property @{
        version = $updateVersion
        file = $updateVersion + ".html"
    }))
    $updateInfoContent.stable.releases = $releases
}

$updateInfoContent | ConvertTo-Json -Depth 10 | Out-File $updateInfoPath -Encoding utf8

# Changelog
Write-Host "Generating changelog..." -ForegroundColor Green
$gitOutPath = "gitout.txt"
$tempMessagesPath = "messages.txt"

try
{
    Start-Process "git" "log --pretty=oneline ...$CompareBranch" -NoNewWindow -Wait -RedirectStandardOutput $gitOutPath
    $messages = (Get-Content $gitOutPath) -replace "^(.*?)\s" | Sort-Object -Descending
    $messages | Out-File $tempMessagesPath -Encoding utf8
    Start-Process "notepad" $tempMessagesPath -PassThru -Wait | Out-Null
    $messages = Get-Content $tempMessagesPath

    Write-Host "Git Changelog" -ForegroundColor Yellow
    $messages | Write-Host
    
    Write-Host "Clean Changelog" -ForegroundColor Yellow
    $htmlChangelog = $messages | ForEach { $_ -replace "\s*#\d+", "" }
    $htmlChangelog | Out-Host

    $changelogContent = (Get-Content $changelogTemplatePath) -replace "{changelog}", ($htmlChangelog | ForEach { "<li>$_</li>" } | Out-String)
    $changelogContent | Out-File $changelogPath -Encoding utf8  
}
finally
{
    Remove-Item $gitOutPath -EA 0
    Remove-Item $tempMessagesPath -EA 0
}

# Commit to repository
if ($CommitAndPush)
{
    Write-Host "Pushing release to repository..." -ForegroundColor Green
    Start-Process "git" "add -A" -Wait -NoNewWindow
    Start-Process "git" "commit -m `"Release $updateVersion`"" -Wait -NoNewWindow
    Start-Process "git" "tag $updateVersion" -Wait -NoNewWindow
    Start-Process "git" "push origin master --follow-tags" -Wait -NoNewWindow
}

# Build binaries
if ($Build)
{
    Write-Host "Building release..." -ForegroundColor Green
    $buildPassed = .\build.ps1 -Setup -Portable
    if (!$buildPassed)
    {
        throw "Building release failed."
    }
}

# Publish release to GitHub
if ($PublishRelease)
{
    $releasesUrl = "https://api.github.com/repos/JosefNemec/Playnite/releases"
    $release = @{
        tag_name = $updateVersion;
        name = $updateVersion;
        body = $messages | Out-String;
        draft = $true
    } | ConvertTo-Json

    $basetoken = [System.Convert]::ToBase64String([char[]]$GitHubAuth)
    $headers = @{
        Authorization = "Basic " + $basetoken
    }

    Write-Host "Pushing release to GitHub..." -ForegroundColor Green
    $releaseResponse = Invoke-RestMethod -Uri $releasesUrl -Body $release -Method Post -Headers $headers

    Write-Host "Uploading installer..." -ForegroundColor Green
    $headers.Add("Content-Type", "application/vnd.microsoft.portable-executable")
    $uploadUrl = $releaseResponse.upload_url -replace "assets.*$", "assets?name=PlayniteInstaller.exe"    
    $uploadResponse = Invoke-RestMethod -Uri $uploadUrl -Body ([System.IO.File]::ReadAllBytes("PlayniteInstaller.exe")) -Headers $headers -Method Post

    Write-Host "Uploading portable package..." -ForegroundColor Green
    $headers["Content-Type"] = "application/zip"
    $uploadUrl = $releaseResponse.upload_url -replace "assets.*$", "assets?name=PlaynitePortable.zip"    
    $uploadResponse = Invoke-RestMethod -Uri $uploadUrl -Body ([System.IO.File]::ReadAllBytes("PlaynitePortable.zip")) -Headers $headers -Method Post
}

# Publish update manifest
if ($PublishManifest)
{
    Write-Host "Publishing update manifest..." -ForegroundColor Green
    $webclient = New-Object System.Net.WebClient

    $manifestUrl = $ManifestFtpUri + (Split-Path $updateInfoPath -Leaf)
    $webclient.UploadFile((New-Object System.Uri($manifestUrl)), $updateInfoPath)

    $changelogUrl = $ManifestFtpUri + (Split-Path $changelogPath -Leaf)
    $webclient.UploadFile((New-Object System.Uri($changelogUrl)), $changelogPath)
}

Write-Host "Done" -ForegroundColor Cyan