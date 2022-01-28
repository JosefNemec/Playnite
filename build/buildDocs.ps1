param(
    [switch]$SkipMetadata
)

$ErrorActionPreference = "Stop"
& .\common.ps1

$buildOut = "..\doc\"
$siteDir = Join-Path $buildOut "_site"
if (Test-Path -LiteralPath $siteDir)
{
    Remove-Item -LiteralPath $siteDir -Recurse
}

if (!$SkipMetadata)
{
    $metadata = StartAndWait "docfx" "metadata" $buildOut
    if ($metadata -ne 0)
    {
        throw "Failed to build doc metadata."
    }
}

$build = StartAndWait "docfx" "build" $buildOut
if ($build -ne 0)
{
    throw "Failed to build doc."
}