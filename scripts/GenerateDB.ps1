param(
    [Parameter(Mandatory=$true)]
    [string]$PlaynitePath,
    [Parameter(Mandatory=$true)]
    [string]$OutputPath,
    [int]$RecordCount = 100,
    [int]$CategoryCount = 10,
    [int]$DeveloperCount = $RecordCount,
    [int]$PublisherCount = $RecordCount,
    [int]$GenreCount = $CategoryCount
)

function GenerateImage()
{
    param
    (
        $width = 231,
        $height = 326
    )

    $bmp = New-Object System.Drawing.Bitmap($width, $height)
    $rand = New-Object System.Random
            
    for ($y = 0; $y -lt $height; $y++)
    {
        for ($x = 0; $x -lt $width; $x++)
        {            
            $a = $rand.Next(256);
            $r = $rand.Next(256);
            $g = $rand.Next(256);
            $b = $rand.Next(256);

            $bmp.SetPixel($x, $y, [System.Drawing.Color]::FromArgb($a, $r, $g, $b));
        }
    }

    return $bmp
}

function AddImage () {
    param(
        $database,
        $image,
        $id
    )
    
    $stream = New-Object System.IO.MemoryStream

    try
    {
        $image.Save($stream, "Png")
        $database.AddImage($id, $id + ".png", $stream.ToArray())
    }
    finally
    {
        $image.Dispose()
        $stream.Dispose()
    }    
}

$global:ErrorActionPreference = "Stop"
$random = New-Object System.Random

$playniteModule = Join-Path $PlaynitePath "Playnite.dll"
Import-Module $playniteModule | Out-Null

Remove-Item $OutputPath -EA 0 
$database = New-Object Playnite.Database.GameDatabase 
$database.OpenDatabase($OutputPath, $false) | Out-Null
$progressActivity = "Generating Playnite DB"
try
{
    for ($i = 0; $i -lt $RecordCount; $i++)
    {
        Write-Progress -Activity $progressActivity -PercentComplete (($i / $RecordCount) * 100)

        $game = New-Object Playnite.Models.Game
        $game.Name = "Game " + $i       

        $game.Image = $game.ProviderId + "Image"
        AddImage $database (GenerateImage) $game.Image

        $game.Icon = $game.ProviderId + "Icon"
        AddImage $database (GenerateImage 32 32) $game.Icon

        $game.Categories = @()
        for ($c = 1; $c -lt $random.Next(4); $c++)
        {
            $game.Categories.Add("Category_" + $random.Next($CategoryCount))
        }

        $game.Developers = @()
        for ($d = 1; $d -lt $random.Next(4); $d++)
        {
            $game.Developers.Add("Developer_" + $random.Next($DeveloperCount))
        }

        $game.Publishers = @()
        for ($p = 1; $p -lt $random.Next(4); $p++)
        {
            $game.Publishers.Add("Publisher_" + $random.Next($PublisherCount))
        }

        $game.Genres = @()
        for ($p = 1; $p -lt $random.Next(4); $p++)
        {
            $game.Genres.Add("Genre_" + $random.Next($GenreCount))
        }

        $game.InstallDirectory = "c:\test\" + $game.ProviderId        
        $game.ReleaseDate = [datetime]::Now.AddTicks($random.next([int]::MinValue, [int]::MaxValue)*10000000)        

        $database.AddGame($game)   
    } 
}
finally
{
    Write-Progress -Activity $progressActivity -Completed
    $database.CloseDatabase()
}