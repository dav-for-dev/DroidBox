$ErrorActionPreference = "Stop"
$env:PATH = "C:\Users\hafiz\AppData\Local\Microsoft\dotnet;$env:PATH"
$env:DOTNET_CLI_HOME = "E:\code-from-haris\DroidBox\.dotnet_home"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:DOTNET_NOLOGO = "1"
$env:NUGET_PACKAGES = "E:\code-from-haris\DroidBox\.nuget_packages"

Set-Location "E:\code-from-haris\DroidBox"

# Ensure directories
New-Item -ItemType Directory -Path "src" -Force | Out-Null
New-Item -ItemType Directory -Path "tests" -Force | Out-Null
New-Item -ItemType Directory -Path "installer\assets" -Force | Out-Null
New-Item -ItemType Directory -Path "installer\scripts" -Force | Out-Null
New-Item -ItemType Directory -Path "docs\offline" -Force | Out-Null
New-Item -ItemType Directory -Path "docs\getting-started" -Force | Out-Null
New-Item -ItemType Directory -Path "docs\cli" -Force | Out-Null
New-Item -ItemType Directory -Path "docs\api" -Force | Out-Null
New-Item -ItemType Directory -Path "docs\python" -Force | Out-Null
New-Item -ItemType Directory -Path "docs\flutter" -Force | Out-Null
New-Item -ItemType Directory -Path "docs\react-native" -Force | Out-Null
New-Item -ItemType Directory -Path "docs\android" -Force | Out-Null
New-Item -ItemType Directory -Path "docs\architecture" -Force | Out-Null
New-Item -ItemType Directory -Path "docs\contributing" -Force | Out-Null
New-Item -ItemType Directory -Path "docs\troubleshooting" -Force | Out-Null
New-Item -ItemType Directory -Path "assets" -Force | Out-Null
New-Item -ItemType Directory -Path "samples\python" -Force | Out-Null
New-Item -ItemType Directory -Path ".github\workflows" -Force | Out-Null
New-Item -ItemType Directory -Path ".github\ISSUE_TEMPLATE" -Force | Out-Null

Write-Host "Created folder structure."
