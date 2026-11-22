param (
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  DroidBox - Running Test Suite          " -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir

Push-Location $rootDir
try {
    Write-Host "Executing tests in $Configuration mode..." -ForegroundColor Yellow
    dotnet test DroidBox.sln -c $Configuration --nologo --verbosity normal
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Test execution failed with exit code $LASTEXITCODE"
    }
    Write-Host "All tests passed successfully!" -ForegroundColor Green
}
finally {
    Pop-Location
}
