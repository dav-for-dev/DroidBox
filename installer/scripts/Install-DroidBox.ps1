<#
.SYNOPSIS
    One-line PowerShell installer for DroidBox on Windows.
.DESCRIPTION
    Downloads the latest release of DroidBox from GitHub, extracts it to the local user directory,
    and configures the DroidBox CLI in the user PATH.
#>

param (
    [string]$InstallDir = "$env:LOCALAPPDATA\DroidBox\bin",
    [string]$Version = "latest"
)

$ErrorActionPreference = "Stop"
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  Installing DroidBox for Windows        " -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

$releaseUrl = "https://github.com/dav-for-dev/DroidBox/releases/latest/download/DroidBox-win-x64.zip"
$tempZip = Join-Path $env:TEMP "DroidBox.zip"

Write-Host "Downloading DroidBox from GitHub Releases..." -ForegroundColor Yellow
Invoke-WebRequest -Uri $releaseUrl -OutFile $tempZip -UseBasicParsing

if (!(Test-Path $InstallDir)) {
    New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null
}

Write-Host "Extracting to $InstallDir..." -ForegroundColor Yellow
Expand-Archive -Path $tempZip -DestinationPath $InstallDir -Force
Remove-Item $tempZip -Force

# Add to user PATH
$currentPath = [Environment]::GetEnvironmentVariable("Path", "User")
if ($currentPath -notlike "*$InstallDir*") {
    Write-Host "Adding $InstallDir to user PATH..." -ForegroundColor Yellow
    [Environment]::SetEnvironmentVariable("Path", "$currentPath;$InstallDir", "User")
    $env:Path += ";$InstallDir"
}

Write-Host "DroidBox installed successfully!" -ForegroundColor Green
Write-Host "Run 'droidbox doctor' in a new terminal to verify your Android SDK environment." -ForegroundColor Cyan
