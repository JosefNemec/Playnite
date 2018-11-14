function global:ExportLibrary()
{
    $games = $PlayniteApi.Database.GetGames()
    $path = $PlayniteApi.Dialogs.SaveFile("CSV|*.csv|Formated TXT|*.txt")
    if ($path)
    {
        if ($path -match ".csv$")
        {
            $games | Select Name, Provider, ReleaseDate, Playtime, Source, IsInstalled | ConvertTo-Csv | Out-File $path -Encoding utf8
        }
        else
        {           
            $games | Select Name, Provider, ReleaseDate, Playtime, Source, IsInstalled | Format-Table -AutoSize | Out-File $path -Encoding utf8
        }
        
        $PlayniteApi.Dialogs.ShowMessage("Library exported successfully.");
    }
}