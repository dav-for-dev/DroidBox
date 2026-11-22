# DroidBox - Run in Development Mode
$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir

Push-Location $rootDir
try {
    # 1. Setup environment paths (.NET SDK & NuGet)
    . "$scriptDir\setup-env.ps1"
    $env:DOTNET_ROOT = "C:\Users\hafiz\AppData\Local\Microsoft\dotnet"

    Write-Host "=========================================" -ForegroundColor Cyan
    Write-Host "  DroidBox - Launching in Dev Mode       " -ForegroundColor Cyan
    Write-Host "=========================================" -ForegroundColor Cyan

    dotnet run --project "$rootDir\src\DroidBox.Desktop\DroidBox.Desktop.csproj"
}
finally {
    Pop-Location
}
