param(
    # Build configuration
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",
    
    # Target platform
    [ValidateSet("x86", "x64")]
    [string]$Platform = "x86",

    # Target directory for build files    
    [string]$OutputDir,

    # Target directory for installer files
    [string]$InstallerDir,

    # Package build output into zip file
    [switch]$Package = $false,

    # Skip build process
    [switch]$SkipBuild = $false,

    # Temp directory for build process
    [string]$TempDir = (Join-Path $env:TEMP "PlayniteBuild"),

    [string]$LicensedDependenciesUrl,

    [switch]$SdkNuget,

    [string]$OnlineInstallerConfig
)

$ErrorActionPreference = "Stop"
if (!(Get-InstalledModule "powershell-yaml" -EA 0))
{
    Install-Module powershell-yaml
}

Set-Location $PSScriptRoot
& .\common.ps1

if (!$OutputDir)
{
    $OutputDir = Join-Path $PWD $Configuration
}

if (!$InstallerDir)
{
    $InstallerDir = $PWD
}

function PackExtensionTemplate()
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$TemplateRootName,        
        [Parameter(Mandatory = $true)]
        [string]$OutputDir
    )

    $templatesDir = Join-Path $OutputDir "Templates\Extensions\"
    $templateOutDir = Join-Path $templatesDir $TemplateRootName
    $tempFiles = Get-Content "..\source\Tools\Playnite.Toolbox\Templates\Extensions\$TemplateRootName\BuildInclude.txt" | Where { ![string]::IsNullOrEmpty($_) }
    $targetZip = Join-Path $templatesDir "$TemplateRootName.zip"
    foreach ($file in $tempFiles)
    {
        $target = Join-Path $templateOutDir $file
        New-FolderFromFilePath $target
        Copy-Item (Join-Path "..\source\Tools\Playnite.Toolbox\Templates\Extensions\$TemplateRootName" $file) $target        
    }

    New-ZipFromDirectory $templateOutDir $targetZip
    Remove-Item $templateOutDir -Recurse -Force
} 

# -------------------------------------------
#            Verify various non-build files
# -------------------------------------------
.\VerifyLanguageFiles.ps1

$platforms = Get-Content "..\source\Playnite\Emulation\Platforms.yaml" -Raw | ConvertFrom-Yaml
if (!($platforms.Count -gt 0))
{
    throw "Platforms definition file is not valid."
}

Write-OperationLog "Platforms definitions are OK"

$regions = Get-Content "..\source\Playnite\Emulation\Regions.yaml" -Raw | ConvertFrom-Yaml
if (!($regions.Count -gt 0))
{
    throw "Regions definition file is not valid."
}

Write-OperationLog "Regions definitions are OK"

Get-ChildItem "..\source\Playnite\Emulation\" -Filter "*.yaml" -Recurse | ForEach {
    $emuDef = Get-Content $_.FullName -Raw | ConvertFrom-Yaml
    if (!$emuDef.Id)
    {
        throw "$($_.FullName) is not valid emulator definition."
    }

    foreach ($profile in $emuDef.Profiles)
    {
        foreach ($platId in $profile.Platforms)
        {
            if (!($platforms | Where { $_.Id -eq $platId } ))
            {
                throw "Platform $platId not found, $($_.FullName)."
            }
        }
    }
}

Write-OperationLog "Emulator definitions are OK"

# -------------------------------------------
#            Compile application 
# -------------------------------------------
if (!$SkipBuild)
{
    if (Test-Path $OutputDir)
    {
        Remove-Item $OutputDir -Recurse -Force
    }
    
    if ($LicensedDependenciesUrl)
    {
        $depArchive = Join-Path $env:TEMP "deps.zip"
        Invoke-WebRequest $LicensedDependenciesUrl -OutFile $depArchive
        Expand-ZipToDirectory $depArchive (Resolve-Path "..").Path
    }

    if ($OnlineInstallerConfig)
    {
        Write-OperationLog "Updating online installer config..."
        $locaConfigPath = "..\source\Tools\PlayniteInstaller\installer_mirrors.txt"
        if ($OnlineInstallerConfig.StartsWith("http"))
        {
            $configFile = Join-Path $env:TEMP "installer_mirrors.txt"
            Invoke-WebRequest $OnlineInstallerConfig -OutFile $locaConfigPath
        }
        else
        {
            Copy-Item $OnlineInstallerConfig $locaConfigPath -Force
        }
    }

    $solutionDir = Join-Path $pwd "..\source"
    Invoke-Nuget "restore ..\source\Playnite.sln"
    $msbuildpath = Get-MsBuildPath
    $arguments = "build.xml /p:SolutionDir=`"$solutionDir\\`" /p:OutputPath=`"$OutputDir`";Configuration=$configuration /property:Platform=$Platform /t:Build"
    $compilerResult = StartAndWait $msbuildPath $arguments
    if ($compilerResult -ne 0)
    {
        throw "Build failed."
    }

    # Copy extension templates
    PackExtensionTemplate "CustomLibraryPlugin" $OutputDir
    PackExtensionTemplate "CustomMetadataPlugin" $OutputDir
    PackExtensionTemplate "GenericPlugin" $OutputDir
    PackExtensionTemplate "PowerShellScript" $OutputDir
}

New-Folder $InstallerDir

# -------------------------------------------
#            SDK nuget
# -------------------------------------------
if ($SdkNuget)
{
    & .\buildSdkNuget.ps1 -SkipBuild -OutputPath $OutputDir | Out-Null
}

# -------------------------------------------
#            Build zip package
# -------------------------------------------
if ($Package)
{
    Write-OperationLog "Building zip package..."
    $packageName = Join-Path $InstallerDir "Playnite.zip"
    New-ZipFromDirectory $OutputDir $packageName
}

(Get-ChildItem (Join-Path $OutputDir "Playnite.dll")).VersionInfo.FileVersion | Write-Host -ForegroundColor Green
return $true