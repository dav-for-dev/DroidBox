param (
    [string]$TargetSdk = "34",
    [string]$SystemImage = "system-images;android-34;google_apis;x86_64"
)

$ErrorActionPreference = "Stop"
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  DroidBox - Android Runtime Provisioner " -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

$localAppData = [Environment]::GetFolderPath([Environment+SpecialFolder]::LocalApplicationData)
$sdkPath = Join-Path $localAppData "Android\Sdk"
if ($env:ANDROID_HOME) { $sdkPath = $env:ANDROID_HOME }

Write-Host "Target SDK Directory: $sdkPath" -ForegroundColor Yellow

# Ensure directory exists
if (!(Test-Path $sdkPath)) {
    New-Item -ItemType Directory -Path $sdkPath -Force | Out-Null
}

$sdkManager = Join-Path $sdkPath "cmdline-tools\latest\bin\sdkmanager.bat"
if (Test-Path $sdkManager) {
    Write-Host "Accepting Android SDK licenses..." -ForegroundColor Yellow
    cmd.exe /c "echo y | `"$sdkManager`" --licenses"

    Write-Host "Installing platform-tools, emulator, and system image..." -ForegroundColor Yellow
    & $sdkManager "platform-tools" "emulator" "platforms;android-$TargetSdk" $SystemImage
    Write-Host "Android runtime components installed successfully!" -ForegroundColor Green
} else {
    Write-Host "Android SDK command-line tools not found at $sdkManager." -ForegroundColor Yellow
    Write-Host "Please download Android command-line tools or run 'droidbox doctor' for guidance." -ForegroundColor Yellow
}
