Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  DroidBox - Offline Docs Generator     " -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir
$docsDir = Join-Path $rootDir "docs\offline"

if (!(Test-Path $docsDir)) {
    New-Item -ItemType Directory -Path $docsDir -Force | Out-Null
}

Write-Host "Offline HTML documentation is available at: $docsDir\index.html" -ForegroundColor Green
