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

function global:SignFile()
{
    param(
        [Parameter(Position = 0, Mandatory = $true, ValueFromPipeline = $true)]
        [string]$Path        
    )
    
    process
    {
        Write-Host "Signing file `"$Path`"" -ForegroundColor Green
        $signToolPath = (Resolve-Path "c:\Program Files*\Windows Kits\*\bin\*\x86\signtool.exe").Path
        $res = StartAndWait $signToolPath ('sign /n "Open Source Developer, Josef Němec" /t http://time.certum.pl /v ' + "`"$Path`"")
        if ($res -ne 0)
        {        
            throw "Failed to sign file."
        }
    }
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

function global:BuildInnoInstaller()
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourceDir,
        [Parameter(Mandatory = $true)]
        [string]$DestinationFile,
        [Parameter(Mandatory = $true)]
        [string]$Version
    )

    $innoCompiler = "C:\Program Files (x86)\Inno Setup 5\ISCC.exe"
    $innoScript = "InnoSetup.iss"    
    $innoTempScript = "InnoSetup.temp.iss"
    $destinationExe = Split-Path $DestinationFile -Leaf
    $destinationDir = Split-Path $DestinationFile -Parent
        
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

    (Get-FileHash $DestinationFile -Algorithm md5).Hash
    Remove-Item $innoTempScript
}