$global:NugetUrl = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"

function global:StartAndWait()
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

function global:Invoke-Nuget()
{
    param(
        [string]$NugetArgs
    ) 

    $nugetCommand = Get-Command -Name "nuget" -Type Application -ErrorAction Ignore
    if (-not $nugetCommand)
    {
        if (-not (Test-Path "nuget.exe"))
        {
            Invoke-WebRequest -Uri $NugetUrl -OutFile "nuget.exe"
        }
    }

    if ($nugetCommand)
    {
        return StartAndWait "nuget" $NugetArgs
    }
    else
    {      
        return StartAndWait ".\nuget.exe" $NugetArgs
    }
}

function global:Get-MsBuildPath()
{
    $VSWHERE_CMD = "vswhere"

    if (-not (Get-Command -Name $VSWHERE_CMD -Type Application -ErrorAction Ignore))
    {
        $VSWHERE_CMD = "..\source\packages\vswhere.2.6.7\tools\vswhere.exe"
        if (-not (Get-Command -Name $VSWHERE_CMD -Type Application -ErrorAction Ignore))
        {
            Invoke-Nuget "install vswhere -Version 2.6.7 -SolutionDirectory `"$solutionDir`"" | Out-Null
        }
    }

    $path = & $VSWHERE_CMD -version "[15.0,16.0)" -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" -latest | Select-Object -First 1
    if ($path -and (Test-Path $path))
    {
        return $path
    }

    $path = & $VSWHERE_CMD -version "[16.0,17.0)" -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" -latest | Select-Object -First 1
    if ($path -and (Test-Path $path))
    {
        return $path
    }

    throw "MS Build not found."
}

function global:New-Folder()
{
    param(
        [Parameter(Position = 0, Mandatory = $true, ValueFromPipeline = $true)]
        [string]$Path        
    )

    if (Test-Path $Path)
    {
        return
    }

    mkdir $Path | Out-Null
}

function global:New-FolderFromFilePath()
{
    param(
        [Parameter(Position = 0, Mandatory = $true, ValueFromPipeline = $true)]
        [string]$FilePath        
    )

    $dirPath = Split-Path $FilePath
    if (Test-Path $dirPath)
    {
        return
    }

    mkdir $dirPath | Out-Null
}

function global:New-EmptyFolder()
{
    param(
        [Parameter(Position = 0, Mandatory = $true, ValueFromPipeline = $true)]
        [string]$Path        
    )

    if (Test-Path $Path)
    {
        Remove-Item $Path -Recurse -Force
    }

    mkdir $Path | Out-Null
}

function global:New-ZipFromDirectory()
{
    param(
        [string]$directory,
        [string]$resultZipPath,
        [bool]$includeBaseDirectory = $false
    )

    if (Test-path $resultZipPath)
    {
        Remove-Item $resultZipPath
    }

    Add-Type -assembly "System.IO.Compression.Filesystem" | Out-Null
    [IO.Compression.ZipFile]::CreateFromDirectory($directory, $resultZipPath, "Optimal", $includeBaseDirectory) 
}

function global:Expand-ZipToDirectory()
{
    param(
        [string]$zipPath,
        [string]$directory
    )

    Add-Type -assembly "System.IO.Compression.Filesystem" | Out-Null
    [IO.Compression.ZipFile]::ExtractToDirectory($zipPath, $directory, $true)
}

function global:Write-OperationLog()
{
    param(
        [Parameter(Position = 0, Mandatory = $true, ValueFromPipeline = $true)]
        [string]$Message
    )

    Write-Host $Message -ForegroundColor Green
}

function global:Write-ErrorLog()
{
    param(
        [Parameter(Position = 0, Mandatory = $true, ValueFromPipeline = $true)]
        [string]$Message
    )

    Write-Host $Message -ForegroundColor Red -BackgroundColor Black
}


function global:Write-WarningLog()
{
    param(
        [Parameter(Position = 0, Mandatory = $true, ValueFromPipeline = $true)]
        [string]$Message
    )

    Write-Host $Message -ForegroundColor Yellow
}

function global:Write-InfoLog()
{
    param(
        [Parameter(Position = 0, Mandatory = $true, ValueFromPipeline = $true)]
        [string]$Message
    )

    Write-Host $Message -ForegroundColor White
}

function global:Write-DebugLog()
{
    param(
        [Parameter(Position = 0, Mandatory = $true, ValueFromPipeline = $true)]
        [string]$Message
    )

    Write-Host $Message -ForegroundColor DarkGray
}