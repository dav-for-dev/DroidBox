Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  DroidBox - Verifying Android SDK Tools " -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

$localAppData = [Environment]::GetFolderPath([Environment+SpecialFolder]::LocalApplicationData)
$sdkPath = Join-Path $localAppData "Android\Sdk"

if ($env:ANDROID_HOME) { $sdkPath = $env:ANDROID_HOME }
elseif ($env:ANDROID_SDK_ROOT) { $sdkPath = $env:ANDROID_SDK_ROOT }

Write-Host "Checking Android SDK at: $sdkPath" -ForegroundColor Yellow

$adb = Join-Path $sdkPath "platform-tools\adb.exe"
$emulator = Join-Path $sdkPath "emulator\emulator.exe"
$avdmanager = Join-Path $sdkPath "cmdline-tools\latest\bin\avdmanager.bat"

$allValid = $true

if (Test-Path $adb) {
    Write-Host "[OK] ADB found: $adb" -ForegroundColor Green
    & $adb version | Select-Object -First 1 | Write-Host -ForegroundColor Gray
} else {
    Write-Host "[MISSING] ADB not found" -ForegroundColor Yellow
    $allValid = $false
}

if (Test-Path $emulator) {
    Write-Host "[OK] Emulator found: $emulator" -ForegroundColor Green
} else {
    Write-Host "[MISSING] Android Emulator not found" -ForegroundColor Yellow
    $allValid = $false
}

# Check WHPX
$whpx = (Get-WindowsOptionalFeature -Online -FeatureName "HypervisorPlatform" -ErrorAction SilentlyContinue).State
Write-Host "Windows Hypervisor Platform (WHPX) State: $whpx" -ForegroundColor Cyan

if ($allValid) {
    Write-Host "All Android SDK components are verified!" -ForegroundColor Green
} else {
    Write-Host "Run 'scripts/setup-runtime.ps1' to automatically install missing SDK components." -ForegroundColor Yellow
}
