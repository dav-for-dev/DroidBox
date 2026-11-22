param (
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  DroidBox - Building Solution           " -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir

Push-Location $rootDir
try {
    Write-Host "Compiling DroidBox.sln in $Configuration configuration..." -ForegroundColor Yellow
    dotnet build DroidBox.sln -c $Configuration --nologo
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed with exit code $LASTEXITCODE"
    }
    Write-Host "Build completed successfully!" -ForegroundColor Green
}
finally {
    Pop-Location
}
