param(
    [Parameter(Mandatory=$true)]
    [string]$RetroArchDir,
    [Parameter(Mandatory=$true)]
    [string]$PlayniteSourceDirectory
)

$ErrorActionPreference = "Stop"
if (!(Get-InstalledModule "powershell-yaml" -EA 0))
{
    Install-Module powershell-yaml
}

function Get-IsYamlValid
{
    param (
        [Parameter(Mandatory=$true)]
        [string] $yamlContent
    )

    try {
        $yamlContent | ConvertFrom-Yaml | Out-Null
        return $true
    } catch {
        return $false
    }
}

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

$emulationDirPath = [System.IO.Path]::Combine($PlayniteSourceDirectory, "Playnite", "Emulation")
$platformsDefinitionPath = [System.IO.Path]::Combine($emulationDirPath, "Platforms.yaml")
$retroArchEmuDefinitionPath = [System.IO.Path]::Combine($emulationDirPath, "Emulators", "RetroArch", "emulator.yaml")

if (!(Test-Path $platformsDefinitionPath -Type Leaf))
{
    Write-Host "Playnite platforms definition file not found in $platformsDefinitionPath" -ForegroundColor Yellow
    return
}

$ignoreList = @( 
    "00_example_libretro.info",
    "2048_libretro.info",
    "3dengine_libretro.info",
    "advanced_tests_libretro.info",
    "bk_libretro.info",
    "boom3_libretro.info",
    "boom3_xp_libretro.info",
    "cannonball_libretro.info",
    "chaigame_libretro.info",
    "chailove_libretro.info",
    "craft_libretro.info",
    "cruzes_libretro.info",
    "dhewm3_libretro.info",
    "dinothawr_libretro.info",
    "ecwolf_libretro.info",
    "ffmpeg_libretro.info",
    "freechaf_libretro.info",
    "freej2me_libretro.info",
    "gme_libretro.info",
    "hbmame_libretro.info",
    "imageviewer_libretro.info",
    "lutro_libretro.info",
    "mojozork_libretro.info",
    "mpv_libretro.info",
    "mrboom_libretro.info",
    "mu_libretro.info",
    "nxengine_libretro.info",
    "oberon_libretro.info",
    "openlara_libretro.info",
    "opentyrian_libretro.info",
    "pascal_pong_libretro.info",
    "pocketcdg_libretro.info",
    "pokemini_libretro.info",
    "prboom_libretro.info",
    "quasi88_libretro.info",
    "redbook_libretro.info",
    "reminiscence_libretro.info",
    "remotejoy_libretro.info",
    "rvvm_libretro.info",
    "scummvm_libretro.info",
    "simcp_libretro.info",
    "squirreljme_libretro.info",
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
    "thepowdertoy_libretro.info",
    "tic80_libretro.info",
    "tyrquake_libretro.info",
    "ume2015_libretro.info",
    "uw8_libretro.info",
    "vaporspec_libretro.info",
    "vitaquake2-rogue_libretro.info",
    "vitaquake2-xatrix_libretro.info",
    "vitaquake2-zaero_libretro.info",
    "vitaquake2_libretro.info",
    "vitaquake3_libretro.info",
    "vitavoyager_libretro.info",
    "wasm4_libretro.info",
    "x1_libretro.info",
    "xrick_libretro.info"
)

$retroarchArcadeSystems = @(
    "16-bit (Various)",
    "16-bit + 32X (Various)",
    "Arcade (various)",
    "CP System I",
    "CP System II",
    "CP System III",
    "Multi (various)",
    "Neo Geo"
)

$retroarchMiscSystems = @(
    "2003 Game Engine",
    "4",
    "CHIP-8",
    "Handheld Electronic", #Handheld Electronic (GW)
    "LowRes NX", #LowRes NX
    "Magnavox Odyssey2", #133 	Philips Videopac G7000
    "Mega Duck", # Mega Duck
    "Moonlight", # Moonlight,
    "PICO8", # PICO-8
    "Pong Game Clone", # Gong,
    "RPG Maker 2000",
    "SEGA Visual Memory Unit", #VeMUlator / Sega VMU
    "Uzebox" # Uzebox (Uzem)
)

$raCoreNameToPlatformIdsTranslate = @{
    "Gearsystem" = @("sega_mastersystem", "sega_gamegear", "sega_sg1000", "coleco_vision");
    "Genesis Plus GX" = @("sega_mastersystem", "sega_gamegear", "sega_genesis", "sega_cd");
    "Genesis Plus GX Wide" = @("sega_mastersystem", "sega_gamegear", "sega_genesis", "sega_cd");
    "nSide (Super Famicom Accuracy)" = @("nintendo_super_nes", "nintendo_gameboy", "nintendo_gameboycolor");
    "PicoDrive" = @("sega_mastersystem", "sega_genesis", "sega_cd", "sega_32x");
    "SMS Plus GX" = @("sega_mastersystem", "sega_gamegear", "sega_sg1000", "coleco_vision");
}

$raSystemIdToPlatformIdsTranslate = @{
    "msx" = @("microsoft_msx", "microsoft_msx2");
    "neo_geo_pocket" = @("snk_neogeopocket", "snk_neogeopocket_color");
}

$raSystemNameToPlatformIdTranslate = @{
    "3DO" = "3do";
    "3DS" = "nintendo_3ds";
    "Amiga" = "commodore_amiga";
    "Atari 2600" = "atari_2600";
    "Atari 5200" = "atari_5200";
    "Atari 7800" = "atari_7800";
    "Atari ST" = "atari_st";
    "C128" = "commodore_64";
    "C64" = "commodore_64";
    "C64 SuperCPU" = "commodore_64";
    "CBM-5x0" = "commodore_cbm5x0";
    "CBM-II" = "commodore_cbm2";
    "CD" = "nec_turbografx_cd";
    "ColecoVision" = "coleco_vision";
    "Color" = "bandai_wonderswan_color";
    "Commodore Amiga" = "commodore_amiga";
    "CPC" = "amstrad_cpc";
    "DOS" = "pc_dos";
    "DS" = "nintendo_ds";
    "Falcon" = "atari_falcon030";
    "Game Boy" = "nintendo_gameboy";
    "Game Boy Advance" = "nintendo_gameboyadvance";
    "Game Boy Color" = "nintendo_gameboycolor";
    "GameCube" = "nintendo_gamecube";
    "GG" = "sega_gamegear";
    "Intellivision" = "mattel_intellivision";
    "Jaguar" = "atari_jaguar";
    "Lynx" = "atari_lynx";
    "Nintendo 64" = "nintendo_64";
    "Nintendo DS" = "nintendo_ds";
    "Nintendo Entertainment System" = "nintendo_nes";
    "PC" = "pc_dos";
    "PC Engine" = "nec_turbografx_16";
    "PC Engine SuperGrafx" = "nec_supergrafx";
    "PC-98" = "nec_pc98";
    "PC-FX" = "nec_pcfx";
    "PCE-CD" = "nec_turbografx_cd";
    "PET" = "commodore_pet";
    "PlayStation" = "sony_playstation";
    "PLUS" = "commodore_plus4";
    "PSP" = "sony_psp";
    "Saturn" = "sega_saturn";
    "Sega Dreamcast" = "sega_dreamcast";
    "Sega Genesis" = "sega_genesis";
    "Sega Master System" = "sega_mastersystem";
    "Sharp X68000" = "sharp_x68000";
    "SNK Neo Geo CD" = "snk_neogeo_cd";
    "Sony PlayStation 2" = "sony_playstation2";
    "STE" = "atari_st";
    "Super Nintendo Entertainment System" = "nintendo_super_nes";
    "SuperGrafx" = "nec_supergrafx";
    "Supervision" = "watara_supervision";
    "SVI" = "microsoft_msx";
    "Thomson MO" = "thomson_mo5";
    "TO" = "thomson_to7";
    "TT" = "atari_st"; # The Atari TT030 is a member of the Atari ST family
    "Vectrex" = "vectrex";
    "VIC-20" = "commodore_vci20";
    "Virtual Boy" = "nintendo_virtualboy";
    "Wii" = "nintendo_wii";
    "WonderSwan" = "bandai_wonderswan";
    "Xbox" = "xbox";
    "ZX Spectrum (various)" = "sinclair_zxspectrum";
    "ZX81" = "sinclair_zx81";
}

$emuDefinitionTemplate = "Id: retroarch
Name: RetroArch
Website: 'http://www.retroarch.com/'
Profiles:
{0}"

$profileTemplate = '  - Name: {0}
    StartupArguments: ''-L ".\cores\{1}.dll" "{{ImagePath}}"''
    Platforms: [{2}]
    ImageExtensions: [{3}]
    ProfileFiles: [''cores\{4}.dll'']
    StartupExecutable: ^retroarch\.exe$'

$existingProfileReaddTemplate = '  - Name: {0}
    StartupArguments: ''{1}''
    Platforms: [{2}]
    ImageExtensions: [{3}]
    ProfileFiles: [''{4}'']
    StartupExecutable: ^retroarch\.exe$'

$existingEmuDefinition = Get-Content $retroArchEmuDefinitionPath -Raw | ConvertFrom-Yaml
$usedProfileNames = @()
$existingProfiles = @{}
foreach ($existingProfile in $existingEmuDefinition.Profiles) {
    $existingProfiles[$existingProfile.ProfileFiles[0]] = $existingProfile
    $usedProfileNames += $existingProfile.Name
}

$foundCoreNames = @()
$foundPlatformIds = @()
$profiles = @()
$infoPath = Join-Path $RetroArchDir "info"
$infoFiles = Get-ChildItem $infoPath -Filter "*.info"
foreach ($infoFile in $infoFiles)
{
    if ($ignoreList.Contains($infoFile.Name))
    {
        continue
    }
    
    $coreInfo = ParseInfoFile $infoFile.FullName
    $coreFile = "cores\{0}.dll" -f $infoFile.BaseName
    $platformIds = @()
    if ($existingProfiles.ContainsKey($coreFile))
    {
        $coreName = $existingProfiles[$coreFile].Name
        $platformIds = $existingProfiles[$coreFile].Platforms
    }
    else
    {
        if ($usedProfileNames.Contains($coreInfo.corename))
        {
            $coreName = "{0} - {1}" -f $coreInfo.corename, $coreInfo.display_name
        }
        else
        {
            $coreName = $coreInfo.corename
        }
    }
    
    if (!($coreInfo.ContainsKey("corename")))
    {
        Write-Host "$($infoFile.Name) does not contain the core name" -ForegroundColor Yellow # Some cores like anarch_libretro.info don't contain a core name
        continue
    }
    
    $coreInfoCoreName = $coreInfo.corename
    if ($raCoreNameToPlatformIdsTranslate.ContainsKey($coreInfoCoreName))
    {
        $platformIds = $raCoreNameToPlatformIdsTranslate[$coreInfoCoreName]
    }
    elseif ($null -ne $coreInfo.systemid -and $raSystemIdToPlatformIdsTranslate.ContainsKey($coreInfo.systemid.Trim()))
    {
        $platformIds = $raSystemIdToPlatformIdsTranslate[$coreInfo.systemid.Trim()]
    }
    else
    {
        if (!($coreInfo.ContainsKey("systemname")))
        {
            Write-Host "$($infoFile.Name) does not contain systemname" -ForegroundColor Yellow # galaksija_libretro.info
            continue
        }

        $coreInfo.systemname.Split("/", [System.StringSplitOptions]::RemoveEmptyEntries) | ForEach-Object {
            $system = $_.Trim()
            if (($retroarchArcadeSystems -contains $system) -or ($retroarchMiscSystems -contains $system))
            {
                Continue
            }

            if ($raSystemNameToPlatformIdTranslate.ContainsKey($system))
            {
                $platformId = $raSystemNameToPlatformIdTranslate[$system]
                if ($platformIds -notcontains $platformId)
                {
                    $platformIds += $platformId
                }
            }
            else
            {
                Write-Host "System to platform translate not found. System: $system, CoreName: $($coreInfo.corename), DisplayName: $($coreInfo.display_name)" -ForegroundColor Yellow
            }
        }
    }

    if ($null -eq $platformIds -or $platformIds.Count -eq 0)
    {
        Write-Host "PlatformIds not found. System: $system, CoreName: $($coreInfo.corename), DisplayName: $($coreInfo.display_name)" -ForegroundColor Yellow
        continue
    }

    if ($coreInfo.supported_extensions)
    {
        $extensions = @()
        $coreInfo.supported_extensions.Split("|", [System.StringSplitOptions]::RemoveEmptyEntries) | ForEach-Object {
            $extensions += $_
        }

        if ($extensions -notcontains "zip")
        {
            $extensions += "zip"
        }

        if ($extensions -notcontains "7z")
        {
            $extensions += "7z"
        }
        
        $extensions = $extensions | Sort-Object
        $extensionsString = [System.String]::Join(", ", $extensions)
    }
    else
    {
        Write-Host "Profile did not have extensions. System: $system, CoreName: $($coreInfo.corename), DisplayName: $($coreInfo.display_name)" -ForegroundColor Yellow
        continue
    }

    foreach ($platformId in $platformIds) {
        if ($foundPlatformIds -notcontains $platformId)
        {
            $foundPlatformIds += $platformId
        }
    }

    $platformIds = $platformIds | Sort-Object
    $extensions = $extensions | Sort-Object
    $platformIdsString = [System.String]::Join(", ", $platformIds)
    $profileString = $profileTemplate -f $coreName, $infoFile.BaseName, $platformIdsString, $extensionsString, $infoFile.BaseName
    $profiles += $profileString
    $foundCoreNames += $coreName
    $usedProfileNames += $coreName
}

# Previous profiles need to be kept or existing emulator configuration will break
# Here emulators not generated will be added back
foreach ($existingProfile in $existingEmuDefinition.Profiles) {
    if ($foundCoreNames -notcontains $existingProfile.Name)
    {
        foreach ($platformId in $existingProfile.Platforms) {
            if ($foundPlatformIds -notcontains $platformId)
            {
                $foundPlatformIds += $platformId
            }
        }

        $platformIds = [System.String]::Join(", ", ($existingProfile.Platforms | Sort-Object))
        $extensionsString = [System.String]::Join(", ", ($existingProfile.ImageExtensions | Sort-Object))
        $profileFilesString = [System.String]::Join(", ", ($existingProfile.ProfileFiles | Sort-Object))
        $profileString = $existingProfileReaddTemplate -f  $existingProfile.Name, $existingProfile.StartupArguments, $platformIds, $extensionsString, $profileFilesString
        $profiles += $profileString
        Write-Host "Existing profile $($existingProfile.Name) not found in newly generated definition and was added back from previous definition" -ForegroundColor Yellow
    }
}

# Join generated profiles and validated created profile yaml content
$profiles = $profiles | Sort-Object
$profilesString = [System.String]::Join("`n`n", $profiles)
$emuDefinitionContent = $emuDefinitionTemplate -f $profilesString
if ((Get-IsYamlValid $emuDefinitionContent) -eq $false)
{
    Write-Host "Newly generated profile definition file is not valid!" -ForegroundColor Red
    Set-Clipboard $emuDefinitionContent
    return
}

# Add RetroArch to emulators list in Playnite platforms definitions
$platformDefinitionTemplate = '- Name: {0}
{1}'

$newPlatformsDefinitions = @()
$existingPlatformsDefinition = Get-Content $platformsDefinitionPath -Raw | ConvertFrom-Yaml
foreach ($platform in $existingPlatformsDefinition) {
    $platformData = @()
    $platformData += "  Id: {0}" -f $platform.Id
    if ($platform.IgdbId)
    {
        $platformData += "IgdbId: {0}" -f $platform.IgdbId
    }

    if ($platform.Databases)
    {
        $platformData +=  "Databases: [{0}]" -f [System.String]::Join(", ", ($platform.Databases | Sort-Object))
    }

    $isSupportedByRetroArch = $foundPlatformIds -contains $platform.Id
    if ($platform.Emulators)
    {
        if ($isSupportedByRetroArch -and ($platform.Emulators -notcontains "retroarch"))
        {
            $platform.Emulators += "retroarch"
            $platform.Emulators = $platform.Emulators | Sort-Object
            Write-Host "Added retroarch emulator to `"$($platform.Name)`" platform" -ForegroundColor Blue
        }
        $platformData +=  "Emulators: [{0}]" -f [System.String]::Join(", ", ($platform.Emulators | Sort-Object))
    }
    elseif ($isSupportedByRetroArch)
    {
        $platformData +=  "Emulators: [{0}]" -f "retroarch"
    }
    
    $platformDataString = $platformDefinitionTemplate -f $platform.Name, [System.String]::Join("`n  ", $platformData)
    $newPlatformsDefinitions += $platformDataString
}

$newPlatformsDefinitionsContent = [System.String]::Join("`n  `n", $newPlatformsDefinitions)

# Validate created platforms definition yaml content
if ((Get-IsYamlValid $newPlatformsDefinitionsContent) -eq $false)
{
    Write-Host "Newly generated platforms definition file is not valid!" -ForegroundColor Red
    Set-Clipboard $newPlatformsDefinitionsContent
    return
}

# Save generated yaml files. New lines are replaced to maintain CRLF endings
[System.IO.File]::WriteAllLines($retroArchEmuDefinitionPath, $emuDefinitionContent.Replace("`n","`r`n"), [System.Text.Encoding]::UTF8)
[System.IO.File]::WriteAllLines($platformsDefinitionPath, $newPlatformsDefinitionsContent.Replace("`n","`r`n"), [System.Text.Encoding]::UTF8)
Write-Host "RetroArch emulator and Playnite platforms definitions updated and saved successfully!" -ForegroundColor Green