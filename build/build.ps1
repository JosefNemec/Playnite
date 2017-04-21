param(
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",
    [string]$OutputPath = (Join-Path $PWD $Configuration),
    [switch]$Setup = $false,
    [switch]$Portable = $false,
    [switch]$SkipBuild = $false
)

$ErrorActionPreference = "Stop"
$NugetUrl = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"

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
    $arguments = "build.xml /p:SolutionDir=`"$solutionDir`" /p:OutputPath=`"$outputPath`";Configuration=$configuration /property:Platform=x86 /t:Build";
    $compiler = Start-Process $msbuildPath $arguments -PassThru -NoNewWindow
    $handle = $compiler.Handle # cache proc.Handle http://stackoverflow.com/a/23797762/1479211
    $compiler.WaitForExit()

    if ($compiler.ExitCode -ne 0)
    {
        $appCompileSuccess = $false
        Write-Host "Build failed." -ForegroundColor "Red"
    }
    else
    {
        $appCompileSuccess = $true
    }
}
else
{
    $appCompileSuccess = $true
}

# -------------------------------------------
#            Build installer
# -------------------------------------------
if ($Setup -and $appCompileSuccess)
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
    $buildProc = Start-Process $nsisCompiler $arguments -NoNewWindow -WorkingDirectory $PWD -PassThru
    $buildProc.WaitForExit()
    Remove-Item $installerTempScript
}

# -------------------------------------------
#            Build portable package
# -------------------------------------------
if ($Portable -and $appCompileSuccess)
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

return $appCompileSuccess