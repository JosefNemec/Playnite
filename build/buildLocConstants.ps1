$ErrorActionPreference = "Stop"
& .\common.ps1

$locFile = Join-Path $pwd "..\source\Playnite\Localization\LocSource.xaml"
$locKeysSrc = Join-Path $pwd "..\source\Playnite\Localization\LocalizationKeys.cs"


[xml]$locFileXaml = Get-Content $locFile

@"
///
/// DO NOT MODIFY! Automatically generated via buildLocConstants.ps1 script.
/// 
namespace Playnite
{
    public static class LOC
    {
"@ | Out-File $locKeysSrc -Encoding utf8

foreach ($node in $locFileXaml.ResourceDictionary.ChildNodes)
{
    if (!$node.Key)
    {
        continue
    }

    if (![string]::IsNullOrEmpty($node.InnerXml))
    {
@"
        /// <summary>
        {0}
        /// </summary>
"@ -f (($node.InnerXml -split "\n") -replace "^", "/// ") | Out-File $locKeysSrc -Encoding utf8 -Append
        "        public const string $($node.Key -replace `"^LOC`", `"`") = `"$($node.Key)`";" `
         | Out-File $locKeysSrc -Encoding utf8 -Append
    }
}

@"
    }
}
"@ | Out-File $locKeysSrc -Encoding utf8 -Append