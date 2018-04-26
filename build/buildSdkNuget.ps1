param(
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",
    [string]$OutputPath = (Join-Path $PWD "$($Configuration)SDK"),
    [switch]$SkipBuild = $false,
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
#            Compile SDK
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

    StartAndWait "nuget.exe" "restore ..\source\PlayniteSDK\packages.config -PackagesDirectory ..\source\packages"
    $project = Join-Path $pwd "..\source\PlayniteSDK\PlayniteSDK.csproj"
    $msbuildPath = "c:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe";
    $arguments = "`"$project`" /p:OutputPath=`"$outputPath`";Configuration=$configuration /t:Build";
    $compilerResult = StartAndWait $msbuildPath $arguments
    if ($compilerResult -ne 0)
    {
        throw "Build failed."
    }
    else
    {
        if ($Sign)
        {
            Join-Path $OutputPath "PlayniteSDK.dll" | SignFile
        }
    }
}

# -------------------------------------------
#            Create NUGET
# -------------------------------------------
$version = (Get-ChildItem (Join-Path $OutputPath "PlayniteSDK.dll")).VersionInfo.ProductVersion
$version = $version -replace "\.0$", ""
$spec = Get-Content "PlayniteSDK.nuspec"
$spec = $spec -replace "{Version}", $version
$spec = $spec -replace "{OutDir}", $OutputPath
$specFile = "nuget.nuspec"

try
{
    $spec | Out-File $specFile
    $packageRes = StartAndWait "nuget.exe" "pack $specFile"
    if ($packageRes -ne 0)
    {
        throw "Nuget packing failed."
    }
}
finally
{
    Remove-Item $specFile -EA 0
}

return $true