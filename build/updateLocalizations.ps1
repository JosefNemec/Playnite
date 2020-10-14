param(
    [string]$AccessToken
)

$ErrorActionPreference = "Stop"

$locDir = Join-Path $pwd "..\source\Playnite\Localization"
$urlRoot = "https://crowdin.com/api/v2"
$playnitePrjId = 345875
$requestHeaders = @{
    "Authorization" = "Bearer $AccessToken"
}

$locProgressData = @{}
$locProgress = Invoke-RestMethod -Headers $requestHeaders -Uri "$urlRoot/projects/$playnitePrjId/languages/progress?limit=100"
foreach ($lng in $locProgress.data)
{
    $locProgressData.Add($lng.data.languageId, $lng.data.translationProgress);
    $locDownloadData = Invoke-RestMethod -Method Post -Headers $requestHeaders -Uri "$urlRoot/projects/$playnitePrjId/translations/builds/files/30" `
                                         -Body "{`"targetLanguageId`":`"$($lng.data.languageId)`"}" -ContentType "application/json"

    $locDownload = Invoke-WebRequest -Uri $locDownloadData.data.url
    $locDownload.Headers["Content-Disposition"][0] -match '"(.+)"' | Out-Null
    $fileName = $Matches[1]
    [System.IO.File]::WriteAllBytes((Join-Path $locDir $fileName), $locDownload.Content)
}

$locProgressData | ConvertTo-Json | Out-File (Join-Path $locDir "locstatus.json")
.\VerifyLanguageFiles.ps1