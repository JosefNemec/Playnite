param(
    [Parameter(Mandatory=$true)]
    [string]$OldCommit,
    [Parameter(Mandatory=$true)]
    [string]$NewCommit,    
    [Parameter(Mandatory=$true)]
    [string]$OldVersion,    
    [Parameter(Mandatory=$true)]
    [string]$NewVersion,
    [Parameter()]
    [string]$WinMergePath = "c:\Programs\WinMerge\WinMergeU.exe",
    [Parameter(Mandatory=$true)]
    [string]$DirffOutDir
)

$global:ErrorActionPreference = "Stop"
& .\common.ps1

$global:gitOutPath = "gitout.txt"
$changeBaseLogDir = "..\source\Tools\Playnite.Toolbox\Templates\SDK\Changelog"
$changeLogDir = Join-Path $changeBaseLogDir "$OldVersion-$NewVersion"
New-EmptyFolder $changeLogDir

function SaveDiffFile()
{
    param(
        $commitHash,
        $filePath,
        $targetPath
    )

    New-FolderFromFilePath $targetPath
    Start-Process "git" "--no-pager show $($commitHash):$($filePath)" -NoNewWindow -Wait -RedirectStandardOutput $targetPath
}

function GetFileList
{
    param (
        $commitHash,
        $rootPath
    )

    Start-Process "git" "--no-pager ls-tree -r --name-only --full-name $commitHash $rootPath" -NoNewWindow -Wait -RedirectStandardOutput $gitOutPath
    return Get-Content $gitOutPath
}

function ExportThemeFiles
{
    param (
        $commitHash,
        $themeRootPath,
        $destination
    )

    $files = GetFileList $commitHash $themeRootPath
    foreach ($file in $files)
    {
        $subPath = $file -replace "source/PlayniteSDK/", ""        
        SaveDiffFile $OldCommit $file (Join-Path $destination $subPath)        
    }
}

try
{
    Start-Process "git" "--no-pager diff --name-status $OldCommit $NewCommit" -NoNewWindow -Wait -RedirectStandardOutput $gitOutPath
    $changes = Get-Content $gitOutPath | Where { $_ -match "source/PlayniteSDK" -and  $_ -notmatch "\.csproj$" }
    $changes | Out-File (Join-Path $changeBaseLogDir "$OldVersion-$NewVersion.txt")

    ExportThemeFiles $OldCommit "../source/PlayniteSDK" $changeLogDir    
    New-ZipFromDirectory $changeLogDir (Join-Path $changeBaseLogDir "$OldVersion.zip")    
    
    $changeList = ""
    foreach ($change in $changes)
    {
        if ($change.StartsWith("M"))
        {
            $changeFileName = $change -replace "M\s+", ""
            $newPath = Join-Path ".." $changeFileName
            $oldPath = Join-Path $changeLogDir ($change -replace "M\s+source/PlayniteSDK/", "")
            $htmlDiffFile = ($changeFileName.Replace("/","_") + ".html")
            $htmlDiffOut = Join-Path $DirffOutDir $htmlDiffFile

            StartAndWait $WinMergePath "`"$oldPath`" `"$newPath`" -minimize -noninteractive -u -cfg ReportFiles/ReportType=2 -or `"$htmlDiffOut`"" | Out-Null
            $link = "https://playnite.link/sdkchangelog/$OldVersion-$NewVersion/$htmlDiffFile"
            $changeList += ("[$change]($link)" + "  `n")
        }
        else
        {
            $changeList += ($change + "  `n")
        }
    }

    $changeList | Out-File "sdkdiffchangelist.txt" -Encoding utf8
}
finally
{
    Remove-item $gitOutPath -Force -EA 0
    if (Test-Path $changeLogDir)
    {
        Remove-Item $changeLogDir -Recurse -EA 0
    }
}