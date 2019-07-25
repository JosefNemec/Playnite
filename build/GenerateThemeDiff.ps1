param(
    [Parameter(Mandatory=$true)]
    [string]$FirstCommit,
    [Parameter(Mandatory=$true)]
    [string]$SecondCommit
)

$gitOutPath = "gitout.txt"
try
{
    Start-Process "git" "diff --name-only $FirstCommit $SecondCommit" -NoNewWindow -Wait -RedirectStandardOutput $gitOutPath
    Get-Content $gitOutPath | Where {$_ -match "(DesktopApp|FullscreenApp)/Themes/(Desktop|Fullscreen)/Default"}
}
finally
{
    Remove-item $gitOutPath -Force -EA 0
}