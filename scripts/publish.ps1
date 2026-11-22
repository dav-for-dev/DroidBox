param (
    [string]$Configuration = "Release",
    [string]$OutputDir = "$PSScriptRoot\..\publish"
)

$ErrorActionPreference = "Stop"
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  DroidBox - Publishing Binaries         " -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir

$desktopOut = Join-Path $OutputDir "desktop"
$cliOut = Join-Path $OutputDir "cli"

Write-Host "Publishing Desktop application to: $desktopOut" -ForegroundColor Yellow
dotnet publish "$rootDir\src\DroidBox.Desktop\DroidBox.Desktop.csproj" -c $Configuration -r win-x64 --self-contained false -o $desktopOut

Write-Host "Publishing CLI tool to: $cliOut" -ForegroundColor Yellow
dotnet publish "$rootDir\src\DroidBox.Cli\DroidBox.Cli.csproj" -c $Configuration -r win-x64 --self-contained false -o $cliOut

Write-Host "Publishing complete!" -ForegroundColor Green
