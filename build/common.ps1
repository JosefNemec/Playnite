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

function global:Start-SigningWatcher()
{
    param(
        [string]$Pass
    )

    Write-OperationLog "Starting signing watcher..."
    $global:SigningWatcherJob = Start-Job {
        Import-Module PSNativeAutomation
        while ($true)
        {
            $window = Get-UIWindow -ControlType Dialog -Name "Common profile login" -EA 0
            if ($window)
            {
                $window | Get-UIEdit | Set-UIValue $args[0]
                $window | Get-UIButton -AutomationId 1010 | Invoke-UIInvokePattern
            }

            Start-Sleep -Seconds 1
        }
    } -ArgumentList $Pass
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
            Invoke-Nuget "install vswhere -Version 2.6.7 -SolutionDirectory `"$solutionDir`""
        }
    }

    $path = & $VSWHERE_CMD -version "[15.0,16.0)" -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe"
    if (Test-Path $path)
    {
        return $path
    }
    else
    {
        throw "MS Build not found."
    }
}

function global:Stop-SigningWatcher()
{
    Write-OperationLog "Stopping signing watcher..."
    if ($SigningWatcherJob)
    {
        $SigningWatcherJob.StopJob()
    }
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
        $res = StartAndWait $signToolPath ('sign /n "Open Source Developer, Josef Němec" /t http://time.certum.pl /v /sha1 FE916C2B41F1DB83F0C972274CB8CD03BF79B0DA ' + "`"$Path`"")
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