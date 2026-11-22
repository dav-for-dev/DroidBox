Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  DroidBox - Developer Environment Setup " -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir

# 1. Verify .NET SDK
Write-Host "Checking .NET SDK installation..." -ForegroundColor Yellow
$dotnetVersion = & dotnet --version
Write-Host "[OK] .NET SDK version: $dotnetVersion" -ForegroundColor Green

# 2. Restore NuGet Packages
Write-Host "Restoring solution NuGet packages..." -ForegroundColor Yellow
dotnet restore "$rootDir\DroidBox.sln"

# 3. Build Solution
Write-Host "Building DroidBox solution..." -ForegroundColor Yellow
dotnet build "$rootDir\DroidBox.sln" -c Debug

# 4. Check Android SDK
& "$scriptDir\verify-sdk.ps1"

Write-Host "Developer environment setup complete! You are ready to develop DroidBox." -ForegroundColor Green
