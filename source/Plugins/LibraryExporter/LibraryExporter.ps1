function global:ExportLibrary()
{
    $path = $PlayniteApi.Dialogs.SaveFile("CSV|*.csv|Formated TXT|*.txt")
    if ($path)
    {
        if ($path -match ".csv$")
        {
            $PlayniteApi.Database.Games | Select Name, Source, ReleaseDate, Playtime, IsInstalled | ConvertTo-Csv | Out-File $path -Encoding utf8
        }
        else
        {           
            $PlayniteApi.Database.Games  | Select Name, Source, ReleaseDate, Playtime, IsInstalled | Format-Table -AutoSize | Out-File $path -Encoding utf8
        }
        
        $PlayniteApi.Dialogs.ShowMessage("Library exported successfully.");
    }
}

function global:GetMainMenuItems()
{
    param(
        $menuArgs
    )

    $menuItem = New-Object Playnite.SDK.Plugins.ScriptMainMenuItem
    $menuItem.Description = "Export Library"
    $menuItem.FunctionName = "ExportLibrary"
    $menuItem.MenuSection = "@"
    return $menuItem
}