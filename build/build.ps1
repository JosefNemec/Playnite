param(
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",
    [string]$OutputPath = (Join-Path $PWD $Configuration),
    [switch]$Setup = $false,
    [switch]$Portable = $false,
    [switch]$SkipBuild = $false,
    [ValidateSet("x86", "x64")]
    [string]$Platform = "x86",
    [string]$UpdateBranch = "stable",
    [switch]$Sign = $false
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

# -------------------------------------------
#            Compile application 
# -------------------------------------------
if (!$SkipBuild)
{
    if (Test-Path $OutputPath)
    {
        Remove-Item $OutputPath -Recurse -Force
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
    $arguments = "build.xml /p:SolutionDir=`"$solutionDir`" /p:OutputPath=`"$outputPath`";Configuration=$configuration /property:Platform=$Platform /t:Build";
    $compilerResult = StartAndWait $msbuildPath $arguments
    if ($compilerResult -ne 0)
    {
        throw "Build failed."
    }
    else
    {
        if ($Sign)
        {
            Join-Path $OutputPath "Playnite.dll" | SignFile
            Join-Path $OutputPath "PlayniteUI.exe" | SignFile
        }
    }
}

# -------------------------------------------
#            Set update branch 
# -------------------------------------------
$configPath = Join-Path $OutputPath "PlayniteUI.exe.config"
(Get-Content $configPath) -replace '.*key="UpdateBranch".*', "<add key=`"UpdateBranch`" value=`"$UpdateBranch`" />" | Out-File $configPath -Encoding utf8

# -------------------------------------------
#            Build installer
# -------------------------------------------
if ($Setup)
{
    Write-Host "Building setup..." -ForegroundColor Green
    
    $nsisCompiler = "c:\Program Files (x86)\NSIS\makensis.exe"
    $installerScript = "setup.nsi"
    $installerTempScript = "setup.temp.nsi"

    $buildNumber = (Get-ChildItem (Join-Path $OutputPath "PlayniteUI.exe")).VersionInfo.ProductVersion
    $buildNumber = $buildNumber -replace "\.0\.0", ""

    $scriptContent = Get-Content $installerScript
    $files = Get-ChildItem $OutputPath -Recurse
    foreach ($file in $files)
    {        
        $name = $file.FullName.Replace($OutputPath, "").TrimStart("\")

        if (Test-Path $file.FullName -PathType Container)
        {
            $filesString += "`$`{CreateDirectory} `"`$INSTDIR\$($name)`"`r`n"
        }
        else
        {
            $name = $file.FullName.Replace($OutputPath, "").TrimStart("\")
            $filesString += "`$`{FileOname} `"$($name)`" `"$($file.FullName)`"`r`n"
        }        
    }

    $scriptContent = $scriptContent -replace ";{files_here}", $filesString
    $scriptContent | Out-File $installerTempScript "utf8"

    $arguments = '/DVERSION="{0}" /DFOLDER="{1}" {2}' -f $buildNumber, $OutputPath, $installerTempScript
    StartAndWait $nsisCompiler $arguments -WorkingDir $PWD
    Remove-Item $installerTempScript

    if ($Sign)
    {
        SignFile "PlayniteInstaller.exe"
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
    [IO.Compression.ZipFile]::CreateFromDirectory($OutputPath, $packageName, "Optimal", $false) 
}

return $true