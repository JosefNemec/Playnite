param(
    [string]$ConfigurationName,
    [string]$TargetDir,
    [string]$SolutionDir
)

$ErrorActionPreference = "Stop"
& (Join-Path $SolutionDir "..\build\common.ps1")

$excludeFiles = Get-Content (Join-Path $SolutionDir "..\build\ExtensionsRefIgnoreList.txt")
foreach ($pluginDir in (Get-ChildItem (Join-Path $SolutionDir "Plugins\") -Directory))
{
    if ($pluginDir -match "\.Tests")
    {
        continue
    }

    $extDirName = Split-Path $pluginDir -Leaf
    if (Test-Path (Join-Path $pluginDir "bin\$ConfigurationName\extension.yaml"))
    {
        $pluginBuildDir = Join-Path $pluginDir "bin\$ConfigurationName\*" 
    }
    else
    {  
        $pluginBuildDir = Join-Path $pluginDir "\*"
    }

    $targetBuildDir = Join-Path $TargetDir "Extensions\$extDirName"
    New-EmptyFolder $targetBuildDir
    Copy-Item $pluginBuildDir $targetBuildDir -Recurse -Exclude $excludeFiles
}