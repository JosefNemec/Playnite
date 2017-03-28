function global:Get-TestProperties()
{
    param(
        $path = "testConfig.json"
    )

    if (-not (Test-Path $path))
    {
        throw "Test config not found."
    }

    return ConvertFrom-Json (Get-Content $path -Raw)
}

function global:Get-NewTempFolder()
{
    return Join-Path $env:TEMP ([Guid]::NewGuid())
}

function global:WaitFor
{
    param
    (
        [Parameter(Mandatory=$true)]
        [ScriptBlock]$Condition,
        [int]$Timeout = 10000,
        [int]$CheckPeriod = 1000    
    )

    $watch = New-Object "System.Diagnostics.Stopwatch"
    $watch.Start()

    $tempErrPref = $ErrorActionPreference
    $ErrorActionPreference = "SilentlyContinue"

    try
    {
        while ($watch.ElapsedMilliseconds -lt $Timeout)
        {
            $result = $null

            try
            {
                $result = & $Condition
            }
            catch
            {
                $result = $false
            }

            if (-not $result)
            {
                Start-Sleep -Milliseconds $CheckPeriod
            }
            else
            {
                $result = $true
                break
            }
        }
    }
    finally
    {
        $ErrorActionPreference = $tempErrPref
        $watch.Stop()
    }

    if (-not $result)
    {
        throw "Waiting was not successful: $Condition"
    }
}

function global:Invoke-OnLocation()
{
    param
    (
        [Parameter(Mandatory=$true)]
        [string]$Path,
        [Parameter(Mandatory=$true)]
        [ScriptBlock]$Script  
    )
    
    Push-Location

    try
    {
        Set-Location $Path
        [System.IO.Directory]::SetCurrentDirectory((Get-Location))
        & $Script
    }
    finally
    {
        Pop-Location
        [System.IO.Directory]::SetCurrentDirectory((Get-Location))
    }
}

function global:Remove-FolderClean()
{
    param
    (
        [Parameter(Mandatory=$true)]
        [string]$Path
    )

    if (Test-Path $Path)
    {
        Remove-Item $Path -Recurse
    }

    New-Item $Path -ItemType Directory | Out-Null
}
