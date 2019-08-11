param(
    [Parameter(ParameterSetName="Base",Mandatory=$true)]
    [string]$Version,
    [Parameter(Mandatory=$true)]
    [string]$CompareBranch,
    [switch]$CommitAndPush,
    [switch]$Build,
    [string]$UpdateBranch = "stable",
    [switch]$PublishRelease,
    [string]$GitHubAuth,
    [switch]$PublishManifest,
    [string]$ManifestFtpUri
)

$ErrorActionPreference = "Stop"

$versionSize = ($Version.ToCharArray() | Where { $_ -eq "." }).Count
$updateVersion = $Version
$asmVersion = $Version + (".0" * (4 - $versionSize - 2)) + ".*"

Write-Host "Publishing version $updateVersion" -ForegroundColor Cyan

$updateInfoPath = "..\web\update\update.json"
$asmInfoPath = "..\source\Playnite\Properties\AssemblyInfo.cs"
$changelogPath = "..\web\update\" + $updateVersion + ".html"
$changelogTemplatePath = "..\web\update\template.html"
$asmInfoContent = Get-Content $asmInfoPath -Encoding UTF8

# AssemblyInfo.cs update
Write-Host "Changing assembly info..." -ForegroundColor Green
$asmInfoContent = $asmInfoContent -replace '^\[.*AssemblyVersion.*$', "[assembly: AssemblyVersion(`"$asmVersion`")]"
$asmInfoContent = $asmInfoContent -replace '^\[.*AssemblyFileVersion.*$', "[assembly: AssemblyFileVersion(`"$asmVersion`")]"
$asmInfoContent  | Out-File $asmInfoPath -Encoding UTF8

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

    $newItems = $messages | Where { $_.StartsWith("New:") } | ForEach { $_.Replace("New:", "").Trim() }
    $fixedItems = $messages | Where { $_.StartsWith("Fix:") } | ForEach { $_.Replace("Fix:", "").Trim() }

    Write-Host "Git Changelog" -ForegroundColor Yellow
    "New:" | Write-Host
    $newItems | ForEach { "* $_" } | Write-Host
    "Fixed:" | Write-Host
    $fixedItems | ForEach { "* $_" } | Write-Host
   
    Write-Host "Clean Changelog" -ForegroundColor Yellow
    $cleanNewItems = $newItems -replace "\s*#\d+", "" | ForEach { "* $_" }
    $cleanFixedItems = $fixedItems -replace "\s*#\d+", "" | ForEach { "* $_" }

    "New:" | Write-Host
    $cleanNewItems | Write-Host
    "Fixed:" | Write-Host
    $cleanFixedItems | Write-Host

    $changelogContent = (Get-Content $changelogTemplatePath) -replace "{new}", ($cleanNewItems | ForEach { "<li>$_</li>"})
    $changelogContent = $changelogContent -replace "{fixed}", ($cleanFixedItems | ForEach { "<li>$_</li>"})
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
    $buildPassed = .\build.ps1 -Setup -Portable -UpdateBranch $UpdateBranch -Sign
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

Write-Host "Done" -ForegroundColor Cyan