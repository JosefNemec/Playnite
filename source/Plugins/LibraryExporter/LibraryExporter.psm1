function ExportLibrary()
{
    $games = $PlayniteApi.Database.GetGames()
    $path = $PlayniteApi.Dialogs.SaveFile("CSV|*.csv|Formated TXT|*.txt")
    if ($path)
    {
        if ($path -match ".csv$")
        {
            $games | Select Name, Source, ReleaseDate, Playtime, IsInstalled | ConvertTo-Csv | Out-File $path -Encoding utf8
        }
        else
        {           
            $games | Select Name, Source, ReleaseDate, Playtime, IsInstalled | Format-Table -AutoSize | Out-File $path -Encoding utf8
        }
        
        $PlayniteApi.Dialogs.ShowMessage("Library exported successfully.");
    }
}