param(
    # Build configuration
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",
    
    # Target platform
    [ValidateSet("x86", "x64")]
    [string]$Platform = "x86",

    # File path with list of values for PlayniteUI.exe.config
    [string]$ConfigUpdatePath,

    # Target directory for build files    
    [string]$OutputDir = (Join-Path $PWD $Configuration),

    # Build installers
    [switch]$Installers = $false,

    # Target directory for installer files
    [string]$InstallerDir = $PWD,

    # Installer technology
    [ValidateSet("nsis", "inno")]
    [string]$InstallerType = "inno",

    # Playnite version dirs used for diff installers
    [array]$UpdateDiffs,

    # Directory containing build files for $UpdateDiffs
    [string]$BuildsStorageDir,

    # Build portable package
    [switch]$Portable = $false,

    # Skip build process
    [switch]$SkipBuild = $false,

    # Sign binary files
    [switch]$Sign = $false,

    # Temp directory for build process
    [string]$TempDir = (Join-Path $env:TEMP "PlayniteBuild")
)

$ErrorActionPreference = "Stop"
$NugetUrl = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"

function StartAndWait()
{
    param(
        [string]$Path,
        [string]$Arguments,
        [string]$WorkingDir
    )

    if ($WorkingDir)
    {
        $proc = Start-Process $Path $Arguments -PassThru -NoNewWindow -WorkingDirectory $WorkingDir
    }
    else
    { 
        $proc = Start-Process $Path $Arguments -PassThru -NoNewWindow
    }

    $handle = $proc.Handle # cache proc.Handle http://stackoverflow.com/a/23797762/1479211
    $proc.WaitForExit()
    return $proc.ExitCode
}

function SignFile()
{
    param(
        [Parameter(Position = 0, Mandatory = $true, ValueFromPipeline = $true)]
        [string]$Path        
    )

    process
    {
        Write-Host "Signing file `"$Path`"" -ForegroundColor Green
        $signToolPath = "c:\Program Files (x86)\Windows Kits\10\bin\10.0.16299.0\x86\signtool.exe"
        $res = StartAndWait $signToolPath ('sign /n "Open Source Developer, Josef Němec" /t http://time.certum.pl /v ' + "`"$Path`"")
        if ($res -ne 0)
        {        
            throw "Failed to sign file."
        }
    }
}

function BuildNsisInstaller()
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourceDir,
        [Parameter(Mandatory = $true)]
        [string]$DestinationFile,
        [Parameter(Mandatory = $true)]
        [string]$Version
    )

    Write-Host "Building NSIS setup..." -ForegroundColor Green
        
    $nsisCompiler = "c:\Program Files (x86)\NSIS\makensis.exe"
    $installerScript = "NsisSetup.nsi"
    $installerTempScript = "NsisSetup.temp.nsi"

    $destinationDir = Split-Path $DestinationFile -Parent
    if (!(Test-Path $destinationDir))
    {
        mkdir $destinationDir | Out-Null
    }

    $scriptContent = Get-Content $installerScript
    $files = Get-ChildItem $SourceDir -Recurse
    foreach ($file in $files)
    {        
        $name = $file.FullName.Replace($SourceDir, "").TrimStart("\")

        if (Test-Path $file.FullName -PathType Container)
        {
            $filesString += "`$`{CreateDirectory} `"`$INSTDIR\$($name)`"`r`n"
        }
        else
        {
            $name = $file.FullName.Replace($SourceDir, "").TrimStart("\")
            $filesString += "`$`{FileOname} `"$($name)`" `"$($file.FullName)`"`r`n"
        }        
    }

    $scriptContent = $scriptContent -replace ";{files_here}", $filesString
    $scriptContent = $scriptContent -replace ";{out_file_name}", "`"$DestinationFile`""
    $scriptContent | Out-File $installerTempScript "utf8"

    $arguments = '/DVERSION="{0}" {1}' -f $Version, $installerTempScript
    $res = StartAndWait $nsisCompiler $arguments -WorkingDir $PWD
    if ($res -ne 0)
    {        
        throw "NSIS build failed."
    }

    Remove-Item $installerTempScript
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
        [switch]$IncludeVcredist
    )

    $innoCompiler = "C:\Program Files (x86)\Inno Setup 5\ISCC.exe"
    $innoScript = "InnoSetup.iss"
    $innoTempScript = "InnoSetup.temp.iss"
    $destinationExe = Split-Path $DestinationFile -Leaf
    $destinationDir = Split-Path $DestinationFile -Parent

    Write-Host "Building Inno Setup $destinationExe..." -ForegroundColor Green
    if ($IncludeVcredist)
    {
        Write-Host "Including vcredist into install package." -ForegroundColor Gray
    }

    if (!(Test-Path $destinationDir))
    {
        mkdir $destinationDir | Out-Null
    }

    $scriptContent = Get-Content $innoScript
    $scriptContent = $scriptContent -replace "{source_path}", $SourceDir
    $scriptContent = $scriptContent -replace "{version}", $Version
    $scriptContent = $scriptContent -replace "{out_dir}", $destinationDir
    $scriptContent = $scriptContent -replace "{out_file_name}", ($destinationExe -replace "\..+`$", "")
    if ($IncludeVcredist)
    {
        $scriptContent = $scriptContent -replace ";{vcredist}", ""
    }

    $scriptContent | Out-File $innoTempScript "utf8"
   
    $res = StartAndWait $innoCompiler "/Q $innoTempScript" -WorkingDir $PWD    
    if ($res -ne 0)
    {        
        throw "Inno build failed."
    }

    (Get-FileHash $DestinationFile -Algorithm MD5).Hash + " $destinationExe" | Out-File ($DestinationFile + ".md5")
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
    
    $baseDirFiles = Get-ChildItem $BaseDir -Recurse | ForEach { Get-FileHash -Path $_.FullName -Algorithm MD5 }
    $targetDirFiles = Get-ChildItem $TargetDir -Recurse | ForEach { Get-FileHash -Path $_.FullName -Algorithm MD5 }
    $diffs = Compare-Object -ReferenceObject $baseDirFiles -DifferenceObject $targetDirFiles -Property Hash -PassThru | Where { $_.SideIndicator -eq "=>" } | Select-Object Path
    
    if (Test-Path $OutPath)
    {
        Remove-Item $OutPath -Force -Recurse
    }
    
    mkdir $OutPath | Out-Null
    foreach ($file in $diffs)
    {
        $target = [Regex]::Replace($file.Path, [Regex]::Escape($TargetDir), $OutPath, "IgnoreCase")
        $targetFileDir = (Split-Path $target -Parent)
        if (!(Test-Path $targetFileDir))
        {
            mkdir $targetFileDir | Out-Null
        }
    
        Copy-Item $file.Path $target
    }

    $tempPath = Join-Path $TempDir (Split-Path $OutPath -Leaf)
    if (Test-Path $tempPath)
    {
        Remove-Item $tempPath -Recurse
    }

    mkdir $tempPath | Out-Null
    Copy-Item (Join-Path $BaseDir "*") $tempPath -Recurse -Force
    Copy-Item (Join-Path $OutPath "*")  $tempPath -Recurse -Force
    $tempPathFiles = Get-ChildItem $tempPath -Recurse | ForEach { Get-FileHash -Path $_.FullName -Algorithm MD5 }
    $tempDiff = Compare-Object -ReferenceObject $targetDirFiles -DifferenceObject $tempPathFiles -Property Hash -PassThru

    if ($tempDiff -ne $null)
    {
        $tempDiff | ForEach { Write-Host "Diff fail: $($_.Path)" -ForegroundColor Red }
        throw "Diff build failed, some files are not included (or different) in diff package."
    }    

    Remove-Item $tempPath -Recurse -Force
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

    # Restore NuGet packages
    if (-not (Test-Path "nuget.exe"))
    {
        Invoke-WebRequest -Uri $NugetUrl -OutFile "nuget.exe"
    }

    $nugetProc = Start-Process "nuget.exe" "restore ..\source\Playnite.sln" -PassThru -NoNewWindow
    $handle = $nugetProc.Handle
    $nugetProc.WaitForExit()

    $solutionDir = Join-Path $pwd "..\source"
    $msbuildPath = "c:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe";
    $arguments = "build.xml /p:SolutionDir=`"$solutionDir`" /p:OutputPath=`"$OutputDir`";Configuration=$configuration /property:Platform=$Platform /t:Build";
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
            Join-Path $OutputDir "PlayniteSDK.dll" | SignFile
            Join-Path $OutputDir "PlayniteUI.exe" | SignFile
        }
    }
}

# -------------------------------------------
#            Set config values
# -------------------------------------------
if ($ConfigUpdatePath)
{
    Write-Host "Updating config values..." -ForegroundColor Green
    $configPath = Join-Path $OutputDir "PlayniteUI.exe.config"
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

        Write-Host "Settings config value $proName : $proValue" -ForegroundColor Gray

        if ($configXml.configuration.appSettings.add.key -contains $proName)
        {
            $node = $configXml.configuration.appSettings.add | Where { $_.key -eq $proName }
            $node.value = $proValue
        }
        else
        {            
            $node = $configXml.CreateElement("add")
            $node.SetAttribute("key", $proName)
            $node.SetAttribute("value", $proValue)
            $configXml.configuration.appSettings.AppendChild($node) | Out-Null
        }
    }

    $configXml.Save($configPath)
}

$buildNumber = (Get-ChildItem (Join-Path $OutputDir "PlayniteUI.exe")).VersionInfo.ProductVersion
$buildNumber = $buildNumber -replace "\.0\.\d+$", ""
$buildNumberPlain = $buildNumber.Replace(".", "")

# -------------------------------------------
#            Build installer
# -------------------------------------------
if ($Installers)
{
    $installerPath = Join-Path $InstallerDir "Playnite$buildNumberPlain.exe"
    
    if ($InstallerType -eq "nsis")
    {
        BuildNsisInstaller $OutputDir $installerPath $buildNumber
    }
    else
    {        
        BuildInnoInstaller $OutputDir $installerPath $buildNumber -IncludeVcredist
    }

    if ($Sign)
    {
        SignFile $installerPath
    }
}

# -------------------------------------------
#            Build update installers
# -------------------------------------------
if ($UpdateDiffs)
{
    foreach ($diffVersion in $UpdateDiffs)
    {
        Write-Host "Building diff package from version $diffVersion..." -ForegroundColor Green

        $diffString = "{0}to{1}" -f $diffVersion.Replace(".", ""), $buildNumberPlain.Replace(".", "")
        $diffDir = Join-Path $InstallerDir $diffString
        CreateDirectoryDiff (Join-Path $BuildsStorageDir $diffVersion) $OutputDir $diffDir

        $includeVcredist = (Get-ChildItem $diffDir | Where { $_.Name -match "CefSharp|libcef" }) -ne $null
        $installerPath = Join-Path $InstallerDir "$diffString.exe"
        BuildInnoInstaller $diffDir $installerPath $buildNumber -IncludeVcredist:$includeVcredist

        if ($Sign)
        {
            SignFile $installerPath
        }
    }
}

# -------------------------------------------
#            Build portable package
# -------------------------------------------
if ($Portable)
{
    Write-Host "Building portable package..." -ForegroundColor Green

    $packageName = "PlaynitePortable.zip"

    if (Test-path $packageName)
    {
        Remove-Item $packageName
    }

    Add-Type -assembly "System.IO.Compression.Filesystem" | Out-Null
    [IO.Compression.ZipFile]::CreateFromDirectory($OutputDir, $packageName, "Optimal", $false) 
}

return $true