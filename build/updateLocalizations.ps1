#Requires -Version 7

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

$locProgressData = New-Object "System.Collections.Specialized.OrderedDictionary"
$locProgress = Invoke-RestMethod -Method Get -Headers $requestHeaders -Uri "$urlRoot/projects/$playnitePrjId/files/30/languages/progress?limit=100" ` -ContentType "application/json"
foreach ($lng in $locProgress.data)
{
    $locProgressData.Add($lng.data.languageId, $lng.data.translationProgress);
    $locDownloadData = Invoke-RestMethod -Method Post -Headers $requestHeaders -Uri "$urlRoot/projects/$playnitePrjId/translations/builds/files/30" `
                                         -Body "{`"targetLanguageId`":`"$($lng.data.languageId)`"}" -ContentType "application/json"

    $tempFile = Join-Path $locDir "temp.xaml"
    Remove-Item $tempFile -EA 0
    $locDownload = Invoke-WebRequest -Uri $locDownloadData.data.url -OutFile $tempFile -PassThru
    $locDownload.Headers["Content-Disposition"][0] -match '"(.+)"' | Out-Null
    $fileName = $Matches[1]
    Move-Item $tempFile (Join-Path $locDir $fileName) -Force
}

$locProgressData | ConvertTo-Json | Out-File (Join-Path $locDir "locstatus.json")
.\VerifyLanguageFiles.ps1