$ErrorActionPreference = "Stop"
Add-Type -AssemblyName "PresentationFramework"

$locDir = Join-Path $pwd "..\source\Playnite\Localization\"
$proofFile = Join-Path $locDir "en_US.xaml"
$lngSourceFile = Join-Path $locDir "LocSource.xaml"

[xml]$proofXaml = Get-Content $proofFile
[xml]$lngSourceXaml = Get-Content $lngSourceFile

foreach ($node in $proofXaml.ResourceDictionary.ChildNodes)
{
    if (!$node.Key)
    {
        continue
    }

    if (![string]::IsNullOrEmpty($node.InnerXml))
    {        
        $lngSourceNode = $lngSourceXaml.ResourceDictionary.ChildNodes | Where-Object { $_.Key -eq $node.Key }
        if ($lngSourceNode)
        {
            $importNode = $lngSourceXaml.ImportNode($node, $true)
            $lngSourceXaml.ResourceDictionary.ReplaceChild($importNode, $lngSourceNode) | Out-Null
        }
    }
}

$settings = new-object System.Xml.XmlWriterSettings
$settings.Indent = $true;
$settings.IndentChars = "    ";
$settings.Encoding = [System.Text.Encoding]::UTF8;
$settings.OmitXmlDeclaration = $true;
$writer = [System.Xml.XmlWriter]::Create($lngSourceFile, $settings);
[System.Xml.Linq.XDocument]::Parse($lngSourceXaml.OuterXml).WriteTo($writer);
$writer.Flush();
$writer.Dispose();