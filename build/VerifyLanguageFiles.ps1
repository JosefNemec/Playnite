$ErrorActionPreference = "Stop"
Add-Type -AssemblyName "PresentationFramework"

$locDir = "..\source\Playnite\Localization\"
$allOk = $true
foreach ($locFile in (Get-ChildItem $locDir -Filter "*.xaml"))
{
    $stream = New-Object "System.IO.StreamReader" $locFile.FullName

    try
    {
        $xaml = [System.Windows.Markup.XamlReader]::Load($stream.BaseStream)
        Write-Host "$($locFile.Name)...OK" -ForegroundColor Green
    }
    catch
    {
        $allOk = $false
        Write-Host "$($locFile.Name)...FAIL" -ForegroundColor Red
        Write-Host $_.Exception.InnerException.Message -ForegroundColor Red
    }
    finally
    {
        $stream.Dispose()
    }
}

if (-not $allOk)
{
    throw "Some localization files failed verification."
}