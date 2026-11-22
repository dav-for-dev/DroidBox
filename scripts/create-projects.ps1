$ErrorActionPreference = "Stop"

Set-Location "E:\code-from-haris\DroidBox"

$projects = @(
    "src\DroidBox.Core",
    "src\DroidBox.Shared",
    "src\DroidBox.Application",
    "src\DroidBox.Infrastructure",
    "src\DroidBox.Display",
    "src\DroidBox.Runtime.AndroidSdk",
    "src\DroidBox.Runtime.Qemu",
    "src\DroidBox.Cli",
    "src\DroidBox.Bootstrapper",
    "src\DroidBox.Desktop",
    "tests\DroidBox.Core.Tests",
    "tests\DroidBox.Application.Tests",
    "tests\DroidBox.Infrastructure.Tests",
    "tests\DroidBox.Cli.Tests",
    "tests\DroidBox.Integration.Tests"
)

foreach ($p in $projects) {
    if (-not (Test-Path $p)) {
        New-Item -ItemType Directory -Path $p -Force | Out-Null
    }
}

# 1. DroidBox.Core.csproj
@'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
</Project>
'@ | Set-Content -Path "src\DroidBox.Core\DroidBox.Core.csproj"

# 2. DroidBox.Shared.csproj
@'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\DroidBox.Core\DroidBox.Core.csproj" />
  </ItemGroup>
</Project>
'@ | Set-Content -Path "src\DroidBox.Shared\DroidBox.Shared.csproj"

# 3. DroidBox.Application.csproj
@'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\DroidBox.Core\DroidBox.Core.csproj" />
    <ProjectReference Include="..\DroidBox.Shared\DroidBox.Shared.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
  </ItemGroup>
</Project>
'@ | Set-Content -Path "src\DroidBox.Application\DroidBox.Application.csproj"

# 4. DroidBox.Infrastructure.csproj
@'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\DroidBox.Core\DroidBox.Core.csproj" />
    <ProjectReference Include="..\DroidBox.Shared\DroidBox.Shared.csproj" />
    <ProjectReference Include="..\DroidBox.Application\DroidBox.Application.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" />
    <PackageReference Include="Serilog" />
    <PackageReference Include="Serilog.Sinks.File" />
    <PackageReference Include="Serilog.Sinks.Console" />
    <PackageReference Include="Serilog.Extensions.Hosting" />
    <PackageReference Include="Microsoft.Extensions.Hosting" />
  </ItemGroup>
</Project>
'@ | Set-Content -Path "src\DroidBox.Infrastructure\DroidBox.Infrastructure.csproj"

# 5. DroidBox.Display.csproj
@'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\DroidBox.Core\DroidBox.Core.csproj" />
    <ProjectReference Include="..\DroidBox.Shared\DroidBox.Shared.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
  </ItemGroup>
</Project>
'@ | Set-Content -Path "src\DroidBox.Display\DroidBox.Display.csproj"

# 6. DroidBox.Runtime.AndroidSdk.csproj
@'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\DroidBox.Core\DroidBox.Core.csproj" />
    <ProjectReference Include="..\DroidBox.Shared\DroidBox.Shared.csproj" />
    <ProjectReference Include="..\DroidBox.Application\DroidBox.Application.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
  </ItemGroup>
</Project>
'@ | Set-Content -Path "src\DroidBox.Runtime.AndroidSdk\DroidBox.Runtime.AndroidSdk.csproj"

# 7. DroidBox.Runtime.Qemu.csproj
@'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\DroidBox.Core\DroidBox.Core.csproj" />
    <ProjectReference Include="..\DroidBox.Shared\DroidBox.Shared.csproj" />
    <ProjectReference Include="..\DroidBox.Application\DroidBox.Application.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
  </ItemGroup>
</Project>
'@ | Set-Content -Path "src\DroidBox.Runtime.Qemu\DroidBox.Runtime.Qemu.csproj"

# 8. DroidBox.Cli.csproj
@'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <AssemblyName>droidbox</AssemblyName>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\DroidBox.Core\DroidBox.Core.csproj" />
    <ProjectReference Include="..\DroidBox.Shared\DroidBox.Shared.csproj" />
    <ProjectReference Include="..\DroidBox.Application\DroidBox.Application.csproj" />
    <ProjectReference Include="..\DroidBox.Infrastructure\DroidBox.Infrastructure.csproj" />
    <ProjectReference Include="..\DroidBox.Runtime.AndroidSdk\DroidBox.Runtime.AndroidSdk.csproj" />
    <ProjectReference Include="..\DroidBox.Runtime.Qemu\DroidBox.Runtime.Qemu.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="System.CommandLine" />
    <PackageReference Include="Microsoft.Extensions.Hosting" />
  </ItemGroup>
</Project>
'@ | Set-Content -Path "src\DroidBox.Cli\DroidBox.Cli.csproj"

# 9. DroidBox.Bootstrapper.csproj
@'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\DroidBox.Core\DroidBox.Core.csproj" />
    <ProjectReference Include="..\DroidBox.Shared\DroidBox.Shared.csproj" />
    <ProjectReference Include="..\DroidBox.Application\DroidBox.Application.csproj" />
    <ProjectReference Include="..\DroidBox.Infrastructure\DroidBox.Infrastructure.csproj" />
    <ProjectReference Include="..\DroidBox.Runtime.AndroidSdk\DroidBox.Runtime.AndroidSdk.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Hosting" />
  </ItemGroup>
</Project>
'@ | Set-Content -Path "src\DroidBox.Bootstrapper\DroidBox.Bootstrapper.csproj"

# 10. DroidBox.Desktop.csproj
@'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\DroidBox.Core\DroidBox.Core.csproj" />
    <ProjectReference Include="..\DroidBox.Shared\DroidBox.Shared.csproj" />
    <ProjectReference Include="..\DroidBox.Application\DroidBox.Application.csproj" />
    <ProjectReference Include="..\DroidBox.Infrastructure\DroidBox.Infrastructure.csproj" />
    <ProjectReference Include="..\DroidBox.Display\DroidBox.Display.csproj" />
    <ProjectReference Include="..\DroidBox.Runtime.AndroidSdk\DroidBox.Runtime.AndroidSdk.csproj" />
    <ProjectReference Include="..\DroidBox.Runtime.Qemu\DroidBox.Runtime.Qemu.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Hosting" />
    <PackageReference Include="CommunityToolkit.Mvvm" />
  </ItemGroup>
</Project>
'@ | Set-Content -Path "src\DroidBox.Desktop\DroidBox.Desktop.csproj"

# 11. DroidBox.Core.Tests.csproj
@'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\DroidBox.Core\DroidBox.Core.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="FluentAssertions" />
    <PackageReference Include="coverlet.collector" />
  </ItemGroup>
</Project>
'@ | Set-Content -Path "tests\DroidBox.Core.Tests\DroidBox.Core.Tests.csproj"

# 12. DroidBox.Application.Tests.csproj
@'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\DroidBox.Core\DroidBox.Core.csproj" />
    <ProjectReference Include="..\..\src\DroidBox.Shared\DroidBox.Shared.csproj" />
    <ProjectReference Include="..\..\src\DroidBox.Application\DroidBox.Application.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="Moq" />
    <PackageReference Include="FluentAssertions" />
    <PackageReference Include="coverlet.collector" />
  </ItemGroup>
</Project>
'@ | Set-Content -Path "tests\DroidBox.Application.Tests\DroidBox.Application.Tests.csproj"

# 13. DroidBox.Infrastructure.Tests.csproj
@'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\DroidBox.Core\DroidBox.Core.csproj" />
    <ProjectReference Include="..\..\src\DroidBox.Infrastructure\DroidBox.Infrastructure.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" />
    <PackageReference Include="FluentAssertions" />
    <PackageReference Include="coverlet.collector" />
  </ItemGroup>
</Project>
'@ | Set-Content -Path "tests\DroidBox.Infrastructure.Tests\DroidBox.Infrastructure.Tests.csproj"

# 14. DroidBox.Cli.Tests.csproj
@'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\DroidBox.Core\DroidBox.Core.csproj" />
    <ProjectReference Include="..\..\src\DroidBox.Shared\DroidBox.Shared.csproj" />
    <ProjectReference Include="..\..\src\DroidBox.Cli\DroidBox.Cli.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="FluentAssertions" />
    <PackageReference Include="coverlet.collector" />
  </ItemGroup>
</Project>
'@ | Set-Content -Path "tests\DroidBox.Cli.Tests\DroidBox.Cli.Tests.csproj"

# 15. DroidBox.Integration.Tests.csproj
@'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\DroidBox.Core\DroidBox.Core.csproj" />
    <ProjectReference Include="..\..\src\DroidBox.Shared\DroidBox.Shared.csproj" />
    <ProjectReference Include="..\..\src\DroidBox.Application\DroidBox.Application.csproj" />
    <ProjectReference Include="..\..\src\DroidBox.Infrastructure\DroidBox.Infrastructure.csproj" />
    <ProjectReference Include="..\..\src\DroidBox.Runtime.AndroidSdk\DroidBox.Runtime.AndroidSdk.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="FluentAssertions" />
    <PackageReference Include="coverlet.collector" />
  </ItemGroup>
</Project>
'@ | Set-Content -Path "tests\DroidBox.Integration.Tests\DroidBox.Integration.Tests.csproj"

# Create DroidBox.sln
$env:PATH = "C:\Users\hafiz\AppData\Local\Microsoft\dotnet;$env:PATH"
$env:DOTNET_CLI_HOME = "E:\code-from-haris\DroidBox\.dotnet_home"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:DOTNET_NOLOGO = "1"

if (Test-Path "DroidBox.sln") {
    Remove-Item "DroidBox.sln" -Force
}
dotnet new sln -n DroidBox

foreach ($p in $projects) {
    $c = Get-ChildItem -Path $p -Filter "*.csproj"
    if ($c) {
        dotnet sln DroidBox.sln add $c.FullName
    }
}

Write-Host "Projects and DroidBox.sln created successfully!"
