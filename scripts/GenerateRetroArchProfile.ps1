param(
    [string]$RetroArchDir
)

$ErrorActionPreference = "Stop"
function ParseInfoFile()
{
    param(
        [Parameter(Mandatory=$true)]
        [string]$Path
    )

    $properties = @{}
    foreach ($line in (Get-Content $Path))
    {
        if ($line.StartsWith("#"))
        {
            continue
        }

        if ($line -match "^(.*)\s*=\s*`"(.*)`"$")
        {
            $property = $Matches[1].Trim()
            if (!$properties.ContainsKey($property))
            {
                $properties.Add($property, $Matches[2].Trim())
            }
        }
    }

    return $properties
}

$ignoreList = @( 
    "00_example_libretro.info",
    "2048_libretro.info",
    "3dengine_libretro.info",
    "openlara_libretro.info",
    "opentyrian_libretro.info",
    "tyrquake_libretro.info",
    "ffmpeg_libretro.info",
    "imageviewer_libretro.info",
    "lutro_libretro.info",
    "advanced_tests_libretro.info",
    "craft_libretro.info",
    "dinothawr_libretro.info",
    "gme_libretro.info",
    "mrboom_libretro.info",
    "nxengine_libretro.info",
    "pascal_pong_libretro.info",
    "pocketcdg_libretro.info",
    "prboom_libretro.info",
    "pokemini_libretro.info",
    "remotejoy_libretro.info",
    "scummvm_libretro.info",
    "stonesoup_libretro.info",
    "test_libretro.info",
    "test_netplay_libretro.info",
    "testaudio_callback_libretro.info",
    "testaudio_no_callback_libretro.info",
    "testaudio_playback_wav_libretro.info",
    "testgl_compute_shaders_libretro.info",
    "testgl_ff_libretro.info",
    "testgl_libretro.info",
    "testinput_buttontest_libretro.info",
    "testretroluxury_libretro.info",
    "testsw_libretro.info",
    "testsw_vram_libretro.info",
    "testvulkan_async_compute_libretro.info",
    "testvulkan_libretro.info",
    "xrick_libretro.info",
    "cruzes_libretro.info",
    "chaigame_libretro.info",
    "chailove_libretro.info",
    "freej2me_libretro.info",
    "thepowdertoy_libretro.info",
    "reminiscence_libretro.info",
    "mpv_libretro.info",
    "cannonball_libretro.info",
    "dhewm3_libretro.info",
    "freechaf_libretro.info",
    "mu_libretro.info",
    "oberon_libretro.info",
    "quasi88_libretro.info",
    "redbook_libretro.info",
    "simcp_libretro.info",
    "squirreljme_libretro.info",
    "tic80_libretro.info",
    "vitaquake2_libretro.info",
    "vitaquake2-rogue_libretro.info",
    "vitaquake2-xatrix_libretro.info",
    "vitaquake2-zaero_libretro.info",
    "vitaquake3_libretro.info",
    "vitavoyager_libretro.info",
    "bk_libretro.info",
    "boom3_libretro.info",
    "boom3_xp_libretro.info",
    "ecwolf_libretro.info",
    "hbmame_libretro.info",
    "x1_libretro.info"
)

$platformsTranslate = @{
    "The 3DO Company 3DO" = "3DO Interactive Multiplayer";
    "Nintendo Super Nintendo Entertainment System" = "Super Nintendo Entertainment System";
    "Nintendo Super Nintendo Entertainment System Hacks" = "Super Nintendo Entertainment System";
    "Nintendo DS Decrypted" = "Nintendo DS";
    "Nintendo DS (Download Play)" = "Nintendo DS";
    "RPG Maker 2000" = "RPG Maker";
    "RPG Maker 2003" = "RPG Maker";
    "FB Alpha Arcade Games" = "Various";
    "FBNeo Arcade Games" = "Various";
    "MAME 2000" = "Various";
    "MAME 2003" = "Various";
    "MAME 2003 (Midway)" = "Various";
    "MAME 2009" = "Various";
    "MAME 2010" = "Various";
    "MAME 2011" = "Various";
    "MAME 2012" = "Various";
    "MAME 2013" = "Various";
    "MAME 2014" = "Various";
    "MAME 2015" = "Various";
    "MAME 2016" = "Various";
    "MAME" = "Various";
    "Nintendo Game Boy Advance (e Cards)" = "Nintendo Game Boy Advance";
    "Sony PlayStation Portable" = "Sony PSP";
    "IBM PC" = "PC";
    "Sega Master System Mark III" = "Sega Master System";
    "Sega Mega Drive Genesis" = "Sega Genesis";
    "Sega Mega CD Sega CD" = "Sega CD";
    "PC 98" = "NEC PC-9801";
    "NEC PC Engine TurboGrafx 16" = "NEC TurboGrafx 16";
    "NEC PC Engine CD TurboGrafx CD" = "NEC TurboGrafx-CD";
    "NEC PC Engine SuperGrafx" = "NEC SuperGrafx";
    "NEC PC FX" = "NEC PC-FX";
    "Nintendo Wii (Digital)" = "Nintendo Wii"
}

$profileTemplate = '
    - Name: {0}
      DefaultArguments: ''-L ".\cores\{1}.dll" "{{ImagePath}}"''
      Platforms: [{2}]
      ImageExtensions: [{3}]
      RequiredFiles: [''cores\{4}.dll'']
      ExecutableLookup: ^retroarch\.exe$
'
$profiles = ""
$infoPath = Join-Path $RetroArchDir "info"
$infoFiles = Get-ChildItem $infoPath -Filter "*.info"
foreach ($infoFile in $infoFiles)
{
    if ($ignoreList.Contains($infoFile.Name))
    {
        continue
    }
    
    $profile = ParseInfoFile $infoFile.FullName
    $name = $profile.corename
    if ($profile.database)
    {
        $platforms = $profile.database.Split("|", [System.StringSplitOptions]::RemoveEmptyEntries) | Foreach {
            $platform = ($_ -replace "\s*\-\s*", " ").Trim()
            if ($platform.StartsWith($profile.manufacturer))
            {
                $platform = $platform -replace "^\s*$($profile.manufacturer)", ""
                $platform = $platform -replace "^\s*$($profile.manufacturer)", ""
                $platform = $profile.manufacturer + " " + $platform.Trim()
            }

            $platform = $platform.Trim()
            if ($platformsTranslate.ContainsKey($platform))
            {
                $platform = $platformsTranslate[$platform]
            }
            
            if ($platform -match "various")
            {
                $platform = "Various"
            }

            $platform
        } | Select-Object -Unique

        $platforms = [System.String]::Join(", ", $platforms)
    }
    else
    {
        $platforms = ($profile.manufacturer + " " + ($profile.systemname -replace "^$($profile.manufacturer)", "")) -replace "\s+", " "
        if ($platforms -match "various")
        {
            $platforms = "Various"
        }
    }

    if ($profile.supported_extensions)
    {
        $extensions = [System.String]::Join(", ", $profile.supported_extensions.Split("|", [System.StringSplitOptions]::RemoveEmptyEntries))
        if ($extensions -notmatch "zip")
        {
            $extensions += ", zip"
        }

        if ($extensions -notmatch "7z")
        {
            $extensions += ", 7z"
        }
    }

    $profileString = $profileTemplate -f $name, $infoFile.BaseName, $platforms, $extensions, $infoFile.BaseName
    $profiles += $profileString
}

$profiles