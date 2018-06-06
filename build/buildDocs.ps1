param(
    [switch]$SkipMetadata
)

$ErrorActionPreference = "Stop"
& .\common.ps1

if (!$SkipMetadata)
{
    $metadata = StartAndWait "docfx" "metadata" "..\doc\"
    if ($metadata -ne 0)
    {
        throw "Failed to build doc metadata."
    }
}

$build = StartAndWait "docfx" "build" "..\doc\"
if ($build -ne 0)
{
    throw "Failed to build doc."
}