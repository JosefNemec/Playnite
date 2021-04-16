param(
    # Build configuration
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",
    
    # Target platform
    [ValidateSet("x86", "x64")]
    [string]$Platform = "x86",

    # File path with list of values for Common.config
    [string]$PlayniteConfigUpdate,

    # Target directory for build files    
    [string]$OutputDir,

    # Target directory for installer files
    [string]$InstallerDir,

    # Build portable package
    [switch]$Package = $false,

    # Skip build process
    [switch]$SkipBuild = $false,

    # Sign binary files
    [string]$Sign,

    # Temp directory for build process
    [string]$TempDir = (Join-Path $env:TEMP "PlayniteBuild"),

    [string]$LicensedDependenciesUrl,

    [switch]$SdkNuget,

    [string]$OnlineInstallerConfig
)

$ErrorActionPreference = "Stop"
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

function BuildInnoInstaller()
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourceDir,
        [Parameter(Mandatory = $true)]
        [string]$DestinationFile,
        [Parameter(Mandatory = $true)]
        [string]$Version,
        [Parameter(Mandatory = $false)]
        [switch]$Update = $false
    )

    $innoCompiler = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
    $innoScript = "InnoSetup.iss"    
    $innoTempScript = "InnoSetup.temp.iss"
    $destinationExe = Split-Path $DestinationFile -Leaf
    $destinationDir = Split-Path $DestinationFile -Parent
    if ($Update)
    {
        $innoScript = "InnoSetupUpdate.iss"
    }

    Write-OperationLog "Building Inno Setup $destinationExe..."
    New-Folder $destinationDir
    $scriptContent = Get-Content $innoScript
    $scriptContent = $scriptContent -replace "{source_path}", $SourceDir
    $scriptContent = $scriptContent -replace "{version}", $Version
    $scriptContent = $scriptContent -replace "{out_dir}", $destinationDir
    $scriptContent = $scriptContent -replace "{out_file_name}", ($destinationExe -replace "\..+`$", "")
    $scriptContent | Out-File $innoTempScript "utf8"
   
    $res = StartAndWait $innoCompiler "/Q $innoTempScript" -WorkingDir $PWD    
    if ($res -ne 0)
    {        
        throw "Inno build failed."
    }

    Remove-Item $innoTempScript
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

.\VerifyLanguageFiles.ps1

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
        $locaConfigPath = "..\source\Tools\PlayniteInstaller\config.json"
        if ($OnlineInstallerConfig.StartsWith("http"))
        {
            $configFile = Join-Path $env:TEMP "onlineconfig.json"
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
    else
    {
        if ($Sign)
        {
            Join-Path $OutputDir "Playnite.dll" | SignFile
            Join-Path $OutputDir "Playnite.Common.dll" | SignFile
            Join-Path $OutputDir "Playnite.SDK.dll" | SignFile
            Join-Path $OutputDir "Playnite.DesktopApp.exe" | SignFile
            Join-Path $OutputDir "Playnite.FullscreenApp.exe" | SignFile
        }
    }

    # Copy extension templates
    PackExtensionTemplate "CustomLibraryPlugin" $OutputDir
    PackExtensionTemplate "CustomMetadataPlugin" $OutputDir
    PackExtensionTemplate "GenericPlugin" $OutputDir
    PackExtensionTemplate "IronPythonScript" $OutputDir
    PackExtensionTemplate "PowerShellScript" $OutputDir
}

# -------------------------------------------
#            Set config values
# -------------------------------------------
if ($PlayniteConfigUpdate)
{
    Write-OperationLog "Updating config values..."
    if ($PlayniteConfigUpdate.StartsWith("http"))
    {
        $configFile = Join-Path $env:TEMP "config.cfg"
        Invoke-WebRequest $PlayniteConfigUpdate -OutFile $configFile
        $PlayniteConfigUpdate = $configFile
    }

    $configPath = Join-Path $OutputDir "Common.config"
    [xml]$configXml = Get-Content $configPath
    $customConfigContent = Get-Content $PlayniteConfigUpdate

    foreach ($line in $customConfigContent)
    {
        $proName = $line.Split(">")[0]
        $proValue = $line.Split(">")[1]

        if ([string]::IsNullOrEmpty($proName))
        {
            continue
        }

        Write-DebugLog "Settings config value $proName : $proValue"

        if ($configXml.appSettings.add.key -contains $proName)
        {
            $node = $configXml.appSettings.add | Where { $_.key -eq $proName }
            $node.value = $proValue
        }
        else
        {            
            $node = $configXml.CreateElement("add")
            $node.SetAttribute("key", $proName)
            $node.SetAttribute("value", $proValue)
            $configXml.appSettings.AppendChild($node) | Out-Null
        }
    }

    $configXml.Save($configPath)
}

$buildNumber = (Get-ChildItem (Join-Path $OutputDir "Playnite.dll")).VersionInfo.ProductVersion
$buildNumber = $buildNumber -replace "\.0\.\d+$", ""
$buildNumberPlain = $buildNumber.Replace(".", "")
New-Folder $InstallerDir

# -------------------------------------------
#            SDK nuget
# -------------------------------------------
if ($SdkNuget)
{
    & .\buildSdkNuget.ps1 -SkipBuild -OutputPath $OutputDir | Out-Null
}

# -------------------------------------------
#            Merge online installer
# -------------------------------------------
Write-OperationLog "Merging online installer..."
$ilMerge = (Get-ChildItem "..\source\packages" -Filter "ILMerge.exe" -Recurse | Select -First 1).FullName
$installerOutPath = Join-Path $OutputDir "PlayniteInstaller.exe"
$onlineInstallerDir = Join-Path $OutputDir "Installer"
$mergeRes = StartAndWait $ilMerge "PlayniteInstaller.exe *.dll /out:`"$installerOutPath`" /ndebug /wildcards" -WorkingDir $onlineInstallerDir
if ($mergeRes -ne 0)
{        
    throw "ILMerge of installer files failed."
}

Remove-Item $onlineInstallerDir -Recurse

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