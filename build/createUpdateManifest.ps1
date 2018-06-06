param(
    [Parameter(Mandatory = $true)]
    [string]$Version,

    [Parameter(Mandatory = $true)]
    [array]$DownloadServers,

    [Parameter(Mandatory = $true)]
    [array]$ReleaseNotesUrlRoots,

    [Parameter(Mandatory = $true)]
    [string]$PackagesDir,

    [Parameter(Mandatory = $true)]
    [string]$ReleaseNotesDir,

    [Parameter(Mandatory = $false)]
    [string]$OutFile
)

$ErrorActionPreference = "Stop"
& .\common.ps1

$manifest = [ordered]@{
    latestVersion = $Version
    downloadServers = $DownloadServers
    releaseNotesUrlRoots = $ReleaseNotesUrlRoots
    packages = @()
    releaseNotes = @()
}

foreach ($file in Get-ChildItem $PackagesDir -Filter "*.exe")
{
    $info = Get-Content ($file.FullName + ".info")
    $baseVersion = $info[0].ToString()
    $checksum = $info[1].ToString()
    $manifest.packages += @{
        baseVersion = $baseVersion
        fileName = $file.Name
        checksum = $checksum
    }
}

foreach ($file in Get-ChildItem $ReleaseNotesDir -Filter "*.html" | Where { $_.Name -match "\d+\.\d+\.html" } | Sort-Object "Name" -Descending)
{
    $manifest.releaseNotes += @{
        version = $file.Name.Replace(".html", "")
        fileName = $file.Name
    }
}

if (!($manifest.packages | Where { $_.baseVersion -eq $Version }))
{
    throw "Update packages don't contain notes for specified version."
}

if (!($manifest.releaseNotes | Where { $_.version -eq $Version }))
{
    throw "Release notes don't contain notes for specified version."
}

if ($OutFile)
{
    $manifest | ConvertTo-Json | Out-File $OutFile
}
else
{
    $manifest | ConvertTo-Json    
}