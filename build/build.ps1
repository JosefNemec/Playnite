param(
    # Build configuration
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",
    
    # Target platform
    [ValidateSet("x86", "x64")]
    [string]$Platform = "x86",

    # File path with list of values for Common.config
    [string]$ConfigUpdatePath,

    # Target directory for build files    
    [string]$OutputDir = (Join-Path $PWD $Configuration),

    # Build installers
    [switch]$Installers = $false,

    # Target directory for installer files
    [string]$InstallerDir = $PWD,

    # Playnite version dirs used for diff installers
    [array]$UpdateDiffs,

    # Directory containing build files for $UpdateDiffs
    [string]$BuildsStorageDir = ".\",

    # Build portable package
    [switch]$Portable = $false,

    # Skip build process
    [switch]$SkipBuild = $false,

    # Sign binary files
    [string]$Sign,

    # Temp directory for build process
    [string]$TempDir = (Join-Path $env:TEMP "PlayniteBuild")
)

$ErrorActionPreference = "Stop"
& .\common.ps1

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

    $innoCompiler = "C:\Program Files (x86)\Inno Setup 5\ISCC.exe"
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

function CreateDirectoryDiff()
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$BaseDir,
        [Parameter(Mandatory = $true)]
        [string]$TargetDir,
        [Parameter(Mandatory = $true)]
        [string]$OutPath
    )
    
    $baseDirFiles = Get-ChildItem $BaseDir -Recurse -File | ForEach { Get-FileHash -Path $_.FullName -Algorithm MD5 }
    $targetDirFiles = Get-ChildItem $TargetDir -Recurse -File | ForEach { Get-FileHash -Path $_.FullName -Algorithm MD5 }
    $diffs = Compare-Object -ReferenceObject $baseDirFiles -DifferenceObject $targetDirFiles -Property Hash -PassThru | Where { $_.SideIndicator -eq "=>" } | Select-Object Path        
    New-EmptyFolder $OutPath

    foreach ($file in $diffs)
    {
        $target = [Regex]::Replace($file.Path, [Regex]::Escape($TargetDir), $OutPath, "IgnoreCase")
        $targetFileDir = (Split-Path $target -Parent)
        New-Folder $targetFileDir    
        Copy-Item $file.Path $target
    }

    $tempPath = Join-Path $TempDir (Split-Path $OutPath -Leaf)
    New-EmptyFolder $tempPath
    Copy-Item (Join-Path $BaseDir "*") $tempPath -Recurse -Force
    Copy-Item (Join-Path $OutPath "*")  $tempPath -Recurse -Force
    $tempPathFiles = Get-ChildItem $tempPath -Recurse -File | ForEach { Get-FileHash -Path $_.FullName -Algorithm MD5 }
    $tempDiff = Compare-Object -ReferenceObject $targetDirFiles -DifferenceObject $tempPathFiles -Property Hash -PassThru
    
    # Ignore removed files
    $tempDiff = $tempDiff | Where { Test-Path ([Regex]::Replace($_.Path, [Regex]::Escape($tempPath), $TargetDir, "IgnoreCase")) }
    if ($tempDiff -ne $null)
    {
        $tempDiff | ForEach { Write-ErrorLog "Diff fail: $($_.Path)" }
        throw "Diff build failed, some files are not included (or different) in diff package."
    }    

    Remove-Item $tempPath -Recurse -Force
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

if ($Sign)
{
    Start-SigningWatcher $Sign
}

# -------------------------------------------
#            Compile application 
# -------------------------------------------
if (!$SkipBuild)
{
    if (Test-Path $OutputDir)
    {
        Remove-Item $OutputDir -Recurse -Force
    }
    
    $solutionDir = Join-Path $pwd "..\source"
    Invoke-Nuget "restore ..\source\Playnite.sln"
    $msbuildpath = Get-MsBuildPath
    $arguments = "build.xml /p:SolutionDir=`"$solutionDir`" /p:OutputPath=`"$OutputDir`";Configuration=$configuration /property:Platform=$Platform /t:Build"
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
            Join-Path $OutputDir "PlayniteUI.exe" | SignFile
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
if ($ConfigUpdatePath)
{
    Write-OperationLog "Updating config values..."
    $configPath = Join-Path $OutputDir "Common.config"
    [xml]$configXml = Get-Content $configPath
    $customConfigContent = Get-Content $ConfigUpdatePath

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
#            Build installer
# -------------------------------------------
if ($Installers)
{
    $installerPath = Join-Path $InstallerDir "Playnite$buildNumberPlain.exe"          
    BuildInnoInstaller $OutputDir $installerPath $buildNumber   

    if ($Sign)
    {
        SignFile $installerPath
    }
            
    $infoFile = Join-Path $InstallerDir "Playnite$buildNumberPlain.exe.info"
    $buildNumber | Out-File $infoFile
    (Get-FileHash $installerPath -Algorithm MD5).Hash | Out-File $infoFile -Append
}

# -------------------------------------------
#            Build update installers
# -------------------------------------------
if ($UpdateDiffs)
{
    foreach ($diffVersion in $UpdateDiffs)
    {
        Write-OperationLog "Building diff package from version $diffVersion..."

        $diffString = "{0}to{1}" -f $diffVersion.Replace(".", ""), $buildNumberPlain.Replace(".", "")
        $diffDir = Join-Path $InstallerDir $diffString
        CreateDirectoryDiff (Join-Path $BuildsStorageDir $diffVersion) $OutputDir $diffDir

        $installerPath = Join-Path $InstallerDir "$diffString.exe"
        BuildInnoInstaller $diffDir $installerPath $buildNumber -Update
        Remove-Item $diffDir -Recurse -Force
        
        if ($Sign)
        {
            SignFile $installerPath
        }        
        
        $infoFile = Join-Path $InstallerDir "$diffString.exe.info"
        $diffVersion | Out-File $infoFile
        (Get-FileHash $installerPath -Algorithm MD5).Hash | Out-File $infoFile -Append
    }
}

# -------------------------------------------
#            Build portable package
# -------------------------------------------
if ($Portable)
{
    Write-OperationLog "Building portable package..."
    $packageName = Join-Path $BuildsStorageDir "Playnite$buildNumberPlain.zip"
    New-ZipFromDirectory $OutputDir $packageName
}

if ($Sign)
{
    Stop-SigningWatcher
}

return $true