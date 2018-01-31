$Global:ErrorActionPreference = "Stop"
[System.Reflection.Assembly]::LoadWithPartialName("System.Xml.Linq") | Out-Null

function UpdateLocFile()
{
    param(
        [string]$file
    )

    $origContent = Get-Content $file
    for ($i = 0; $i -lt $origContent.Count; $i++)
    {
        $line = $origContent[$i]
        if ($line -match "x:Key=`"(.*?)`"")
        {
            if ($locKeys -contains $Matches[1] -and (@("LocalizationLanguage", "LocalizationString") -notcontains $Matches[1]))
            {            
                $line = $line -replace "x:Key=`"(.*?)`"", "x:Key=`"LOC$($Matches[1])`""
                $origContent[$i] = $line
            }
        }
    }

    Write-Host "Updating $file" -ForegroundColor Green
    $origContent | Out-File $file -Encoding utf8
}

function UpdateSourceFile()
{
    param(
        [string]$file,
        $locKeys
    )

    $replaced = $false
    $origContent = Get-Content $file
    for ($i = 0; $i -lt $origContent.Count; $i++)
    {
        $line = $origContent[$i]
        if ($line -match "\.FindString\(`"(.*?)`"\)")
        {
            if ($locKeys -contains $Matches[1])
            {            
                $line = $line -replace "\.FindString\(`"$($Matches[1])`"\)", ".FindString(`"LOC$($Matches[1])`")"
                $origContent[$i] = $line
                $replaced = $true
            }
        }
    }

    if ($replaced)
    {
        Write-Host "Updating $file" -ForegroundColor Green
        $origContent | Out-File $file -Encoding utf8
    }
}

function UpdateXamlFile()
{
    param(
        [string]$file,
        $locKeys
    )

    $replaced = $false
    $origContent = Get-Content $file
    for ($i = 0; $i -lt $origContent.Count; $i++)
    {
        $line = $origContent[$i]
        if ($line -match "{DynamicResource\s+(.*?)}")
        {
            if ($locKeys -contains $Matches[1])
            {            
                $line = $line -replace "{DynamicResource\s+$($Matches[1])}", "{DynamicResource LOC$($Matches[1])}"
                $origContent[$i] = $line
                $replaced = $true
            }
        }

        if ($line -match "{StaticResource\s+(.*?)}")
        {
            if ($locKeys -contains $Matches[1])
            {            
                $line = $line -replace "{StaticResource\s+$($Matches[1])}", "{StaticResource LOC$($Matches[1])}"
                $origContent[$i] = $line
                $replaced = $true
            }
        }
    }

    if ($replaced)
    {
        Write-Host "Updating $file" -ForegroundColor Green
        $origContent | Out-File $file -Encoding utf8
    }
}

function GetLocalizationKeys()
{
    param(
        [string]$file
    )

    [xml]$source = Get-Content $file
    foreach ($node in $source.ResourceDictionary.ChildNodes)
    {
        if (!$node.Key)
        {
            continue
        }

        $node.Key
    }
}

$sourceFolders = @("..\source\Playnite", "..\source\PlayniteUI")
$locFolder = "..\source\PlayniteUI\Localization\"
$locFiles = Get-ChildItem $locFolder -Filter "*.xaml"
$locKeys = GetLocalizationKeys (Join-Path $locFolder "english.xaml")

Write-Host "Updating localization files..." -ForegroundColor Yellow
foreach ($locFile in $locFiles)
{
    UpdateLocFile $locFile.FullName
}

Write-Host "Updating cs files..." -ForegroundColor Yellow
foreach ($sourceFolder in $sourceFolders)
{
    $files = Get-ChildItem $sourceFolder -Filter "*.cs" -Recurse
    foreach ($file in $files)
    {
        UpdateSourceFile $file.FullName $locKeys
    }
}

Write-Host "Updating xaml files..." -ForegroundColor Yellow
$sourceFolders = @("..\source\Playnite", "..\source\PlayniteUI")
foreach ($sourceFolder in $sourceFolders)
{
    $files = Get-ChildItem $sourceFolder -Filter "*.xaml" -Recurse
    foreach ($file in $files)
    {
        UpdateXamlFile $file.FullName $locKeys
    }
}