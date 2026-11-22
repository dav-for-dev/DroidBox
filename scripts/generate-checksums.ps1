param (
    [string]$Directory = "$PSScriptRoot\..\release"
)

$ErrorActionPreference = "Stop"
Write-Host "Generating SHA-256 checksums for files in: $Directory" -ForegroundColor Cyan

if (!(Test-Path $Directory)) {
    Write-Warning "Directory $Directory does not exist."
    return
}

$files = Get-ChildItem -Path $Directory -File -Exclude "*.sha256"
$checksumFile = Join-Path $Directory "SHA256SUMS.txt"

$hashes = @()
foreach ($file in $files) {
    $hash = (Get-FileHash -Path $file.FullName -Algorithm SHA256).Hash.ToLower()
    $line = "$hash  $($file.Name)"
    Write-Host $line -ForegroundColor Gray
    $hashes += $line
}

$hashes | Set-Content -Path $checksumFile -Encoding UTF8
Write-Host "Checksums written to $checksumFile" -ForegroundColor Green
