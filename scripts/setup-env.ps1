$root = (Get-Item "$PSScriptRoot\..").FullName
$env:PATH = "C:\Users\hafiz\AppData\Local\Microsoft\dotnet;$env:PATH"
$env:APPDATA = "$root\.dotnet_home\AppData\Roaming"
$env:DOTNET_CLI_HOME = "$root\.dotnet_home"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:DOTNET_NOLOGO = "1"
$env:NUGET_PACKAGES = "$root\.nuget_packages"

if (-not (Test-Path "$env:APPDATA\NuGet")) {
    New-Item -ItemType Directory -Path "$env:APPDATA\NuGet" -Force | Out-Null
}
Copy-Item "$root\nuget.config" "$env:APPDATA\NuGet\NuGet.Config" -Force
