param (
    [string]$Version = "1.0.0",
    [string]$OutputDir = "$PSScriptRoot\..\release"
)

$ErrorActionPreference = "Stop"
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  DroidBox - Creating Release Package    " -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

$publishDir = "$PSScriptRoot\..\publish"
& "$PSScriptRoot\publish.ps1" -Configuration "Release" -OutputDir $publishDir

if (!(Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

$zipName = "DroidBox-v$Version-win-x64.zip"
$zipPath = Join-Path $OutputDir $zipName

if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Write-Host "Compressing release archive to $zipPath..." -ForegroundColor Yellow
Compress-Archive -Path "$publishDir\*" -DestinationPath $zipPath -Force

& "$PSScriptRoot\generate-checksums.ps1" -Directory $OutputDir

Write-Host "Release package v$Version created successfully at $OutputDir" -ForegroundColor Green
