# MASTER DEVELOPMENT PROMPT — DROIDBOX

## 1. PROJECT OVERVIEW

You are a Senior Windows Desktop Application Architect, C#/.NET Engineer, Android Emulator Integration Engineer, DevOps Engineer, and Open Source Maintainer.

Your task is to build **DroidBox**, a lightweight, free, open-source Android application testing environment for Windows.

GitHub Repository:

https://github.com/dav-for-dev/DroidBox.git

All development must happen inside this repository.

First, clone and inspect the repository, understand the existing files, and then implement the complete application architecture.

Do not create an unrelated project or a separate repository.

The application must allow developers to install, manage, launch, and test Android APK applications inside a virtual Android environment directly on Windows.

A physical Android device must never be required.

### Product Vision

DroidBox — Your Lightweight Android Testing Environment.

Tagline:

Install. Run. Test. Without a Physical Device.

Core principles:

- Free and Open Source.
- Apache License 2.0.
- No advertisements.
- No mandatory account.
- No unnecessary background processes.
- No telemetry by default.
- No physical Android device requirement.
- Developer-first CLI.
- Offline documentation.
- Modular and community-friendly architecture.

---

# 2. TECHNOLOGY STACK

Use the following technologies.

### Windows Desktop Application

- C#
- .NET 10 LTS
- WPF
- MVVM architecture
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Hosting
- CommunityToolkit.Mvvm
- SQLite
- Entity Framework Core
- Serilog

### Android Runtime

Primary provider:

- Official Android Emulator.
- Android SDK Command-line Tools.
- Android Platform Tools.
- ADB.
- Android Virtual Device.
- WHPX hardware acceleration.

Secondary experimental provider:

- QEMU.
- Android-x86.

The runtime architecture must support additional providers without modifying the core application.

### Distribution

- Inno Setup EXE bootstrapper.
- Optional MSI for future enterprise distribution.
- GitHub Releases.
- Windows Package Manager (WinGet) manifest.
- PowerShell installation scripts.
- GitHub Actions CI/CD.

### Developer Tools

- Standalone droidbox.exe CLI.
- Windows Named Pipes for local IPC.
- Optional Python SDK.
- Offline HTML documentation.

Do not use Electron, Tauri, Flutter Desktop, React Desktop, or a browser-based shell for the primary Windows application.

The main application must be a genuine native Windows WPF application.

---

# 3. FIRST TASK: REPOSITORY INSPECTION

Clone:

https://github.com/dav-for-dev/DroidBox.git

Inspect:

- Existing repository structure.
- Existing README.
- Branches.
- Existing files.
- Git history.
- Repository configuration.

Do not overwrite existing work blindly.

Create an implementation plan before modifying the repository.

Use the existing main branch and repository identity.

Do not create a new repository.

Do not publish an untested application or claim that a component works before validating it.

---

# 4. PROJECT ARCHITECTURE

Create a modular monorepo.

Required structure:

DroidBox/
│
├── src/
│   ├── DroidBox.Desktop/
│   ├── DroidBox.Core/
│   ├── DroidBox.Application/
│   ├── DroidBox.Infrastructure/
│   ├── DroidBox.Cli/
│   ├── DroidBox.Bootstrapper/
│   ├── DroidBox.Shared/
│   ├── DroidBox.Runtime.AndroidSdk/
│   ├── DroidBox.Runtime.Qemu/
│   └── DroidBox.Display/
│
├── tests/
│   ├── DroidBox.Core.Tests/
│   ├── DroidBox.Application.Tests/
│   ├── DroidBox.Infrastructure.Tests/
│   ├── DroidBox.Cli.Tests/
│   └── DroidBox.Integration.Tests/
│
├── installer/
│   ├── DroidBox.iss
│   ├── assets/
│   └── scripts/
│
├── docs/
│   ├── offline/
│   ├── getting-started/
│   ├── cli/
│   ├── api/
│   ├── python/
│   ├── flutter/
│   ├── react-native/
│   ├── android/
│   ├── architecture/
│   ├── contributing/
│   └── troubleshooting/
│
├── scripts/
│   ├── build.ps1
│   ├── publish.ps1
│   ├── release.ps1
│   ├── setup-dev.ps1
│   ├── setup-runtime.ps1
│   ├── generate-docs.ps1
│   ├── generate-checksums.ps1
│   └── verify-sdk.ps1
│
├── .github/
│   ├── workflows/
│   ├── ISSUE_TEMPLATE/
│   ├── PULL_REQUEST_TEMPLATE.md
│   └── CODEOWNERS
│
├── assets/
├── samples/
│
├── LICENSE
├── NOTICE
├── CONTRIBUTING.md
├── GOVERNANCE.md
├── CODE_OF_CONDUCT.md
├── SECURITY.md
├── SUPPORT.md
├── THIRD-PARTY-NOTICES.md
├── ROADMAP.md
├── CHANGELOG.md
├── README.md
├── DroidBox.sln
├── Directory.Build.props
└── Directory.Packages.props

Create every file individually and organize implementation by responsibility.

Do not create giant source files containing unrelated functionality.

---

# 5. WPF DESKTOP APPLICATION

Build a polished, minimalist, modern Windows utility application.

Design principles:

- Windows-native appearance.
- Light theme as default.
- Dark theme optional.
- Minimal navigation.
- Compact layout.
- Clear typography.
- Consistent spacing.
- No unnecessary animations.
- No unnecessary decorative components.
- Keyboard and mouse accessibility.

Main navigation:

1. Home
2. Applications
3. Emulator
4. Documentation
5. Settings

### Dashboard

Display:

- Emulator status.
- Android version.
- Active device profile.
- Installed applications count.
- Memory allocation.
- Quick APK installation.
- Recently installed applications.
- Start/Stop emulator actions.

### APK Manager

Required functionality:

- Browse APK.
- Drag and drop APK.
- APK installation.
- APK update.
- Uninstall.
- Launch.
- Application icon.
- Application name.
- Package name.
- Version name.
- Version code.
- Minimum SDK.
- Target SDK.
- CPU architecture.
- Installation history.

Support:

- Single APK.
- Split APK installation.
- Multiple APK installation where supported.

Use proper Android package metadata extraction.

### Emulator Screen

The Android screen must be accessible inside the main WPF application window.

Provide:

- Embedded screen.
- Mouse interaction.
- Touch simulation.
- Keyboard input.
- Home.
- Back.
- Recent Apps.
- Screenshot.
- Zoom.
- Portrait and Landscape.
- Tablet and Mobile modes.

IMPORTANT:

The official Android Emulator does not provide a universally stable public API for embedding its native graphical window in WPF.

Therefore, build a display abstraction.

First investigate the official Emulator gRPC/headless interface for screenshot streaming and input injection on Windows.

Evaluate native-window integration as an experimental fallback.

Do not implement a fake static screenshot and describe it as a functional emulator.

Do not use unsupported tricks as the only production display implementation.

If interactive embedding is not yet feasible, provide a functional separate-window fallback and clearly document the limitation.

---

# 6. HYBRID ANDROID RUNTIME ARCHITECTURE

Implement runtime providers.

Primary:

DroidBox.Runtime.AndroidSdk

Secondary:

DroidBox.Runtime.Qemu

Define common interfaces:

IRuntimeProvider
IAndroidRuntime
IRuntimeInstaller
IDeviceProfileManager
IDisplayAdapter
IInputController
IApkAnalyzer
IAdbClient
IScreenshotService
IRuntimeHealthChecker

Each provider must implement common lifecycle and capability contracts.

Primary engine requirements:

- Detect Android SDK installation.
- Install SDK Command-line Tools.
- Install Platform Tools.
- Install Android Emulator.
- Install selected System Image.
- Create AVD.
- Start Emulator.
- Stop Emulator.
- Connect through local ADB.
- Install APK.
- Launch applications.
- List installed applications.
- Capture screenshots.

Secondary engine is experimental in the first release.

Do not claim two engines are available until both are actually implemented and validated.

Use runtime capability discovery.

---

# 7. TABLET AND MOBILE PROFILES

Create two default virtual device profiles.

Tablet:

- Initial resolution: 1280 × 800.
- Landscape.
- Configurable density.

Mobile:

- Initial resolution: 412 × 915.
- Portrait.
- Configurable density.

Allow developers to:

- Switch profiles.
- Change resolution.
- Change density.
- Rotate display.
- Create custom profiles.

Important:

Changing the WPF screen container size alone is not sufficient.

The actual Android device configuration must be updated.

Preserve application data when switching configurations.

When an application is not compatible with a selected profile, show a clear warning.

Never assume APK compatibility purely from its filename.

Use manifest data, ABI requirements, screen compatibility and runtime results.

---

# 8. DEVELOPER CLI

Create a separate standalone CLI project.

Executable:

droidbox.exe

The CLI must operate independently of the WPF application.

Use Windows Named Pipes to communicate with a local DroidBox process/service where necessary.

Required commands:

droidbox setup

droidbox doctor

droidbox status

droidbox install app-debug.apk

droidbox install app-debug.apk --mode tablet

droidbox install app-debug.apk --mode mobile

droidbox launch com.example.app

droidbox uninstall com.example.app

droidbox list

droidbox devices

droidbox device tablet

droidbox device mobile

droidbox screenshot output.png

droidbox logs

droidbox docs

droidbox update

droidbox stop

Implement:

- Proper exit codes.
- Structured output.
- Useful error messages.
- Cancellation handling.
- Process timeouts.
- Logging.
- Help system.
- Version command.

Commands must be documented.

Never rely on GUI-only state to execute a CLI command.

---

# 9. COMPLETE WINDOWS INSTALLER

This is a critical requirement.

Developers must be able to install DroidBox through PowerShell or CMD.

Provide three installation methods.

### Method A: GUI Installer

Filename:

DroidBox-Setup-x64.exe

Use Inno Setup to build the Windows installer.

### Method B: WinGet

Provide a valid WinGet package manifest.

Target installation command:

winget install --id DroidBox.DroidBox -e

This command must only be advertised as available after the package has actually been published to a WinGet source.

### Method C: PowerShell Bootstrapper

Provide a secure downloadable installation script.

Example:

Install-DroidBox.ps1

It must:

1. Download the correct installer.
2. Verify SHA256.
3. Validate Authenticode signature if available.
4. Install the application.
5. Configure user PATH.
6. Register CLI.
7. Optionally run initial setup.
8. Show completion information.

Do not use unverified remote script execution through `irm | iex`.

Provide an inspect-before-execute installation method.

### Installer modes

Support:

- Complete GUI Installation.
- CLI Developer Installation.
- Minimal Installation.

CLI-only installation must not require the WPF application.

The installer should be small.

Do not bundle massive Android System Images unnecessarily.

Instead, provide a first-run runtime downloader.

---

# 10. AUTOMATIC ANDROID ENVIRONMENT SETUP

The user must be able to run:

droidbox setup

This command prepares the complete Android environment.

Steps:

1. Check Windows compatibility.
2. Check CPU virtualization.
3. Check available RAM.
4. Check free disk space.
5. Check .NET dependencies.
6. Download Android SDK Command-line Tools.
7. Verify downloaded packages.
8. Install Platform Tools.
9. Install Android Emulator.
10. Display SDK licenses and request explicit consent.
11. Download selected Android System Image.
12. Create default AVD.
13. Start the virtual device.
14. Verify ADB.
15. Verify runtime.
16. Verify display capabilities.
17. Install offline documentation.
18. Print final environment report.

Make downloads resumable.

Use a package manifest to pin compatible versions.

Do not download all Android SDK platforms and system images.

Only download the components required for the selected runtime.

The application must clearly distinguish:

- Application installation.
- Runtime installation.
- System image installation.
- AVD creation.

---

# 11. OFFLINE DOCUMENTATION

All documentation must be available offline after installation.

Build a local documentation package.

Required documentation:

### Getting Started

- Installation.
- First run.
- APK installation.
- Emulator setup.
- Device profiles.

### CLI Documentation

- Every command.
- Parameters.
- Exit codes.
- Examples.
- Automation.

### API Documentation

- Runtime APIs.
- APK APIs.
- Device APIs.
- Screenshot APIs.
- Error contracts.

### Python Documentation

- Python integration.
- subprocess examples.
- CLI automation.
- Optional Python SDK.
- Error handling.
- Example scripts.

### Framework Documentation

- Flutter.
- React Native.
- Kotlin.
- Java.
- Android Gradle builds.
- CI/CD.

### Contributor Documentation

- Development environment.
- Architecture.
- Build.
- Tests.
- Runtime providers.
- Pull requests.
- Coding standards.

Generate local HTML documentation.

All navigation, CSS, JavaScript, code examples and images must be local assets.

Do not require an online documentation website to use the documentation.

Add a Documentation section inside the WPF application.

Provide a local documentation path that developers can also open from the CLI.

---

# 12. PYTHON INTEGRATION

Python must be supported as an optional developer integration.

The core application remains C#.

First implement stable CLI automation.

Example:

import subprocess

subprocess.run(
    ["droidbox", "install", "app-debug.apk"],
    check=True
)

subprocess.run(
    ["droidbox", "launch", "com.example.app"],
    check=True
)

Optionally publish a Python package:

droidbox-sdk

Potential API:

from droidbox import DroidBox

device = DroidBox()

device.install("app-debug.apk")
device.launch("com.example.app")
device.screenshot("result.png")

Do not claim that the Python package exists until it has been implemented, tested and published.

---

# 13. DATABASE AND LOCAL STORAGE

Use SQLite.

Tables:

- InstalledApps.
- ApkHistory.
- DeviceProfiles.
- EmulatorSettings.
- SdkPackages.
- AppSettings.
- RecentApks.

Application binaries:

%LOCALAPPDATA%\Programs\DroidBox\

User data:

%LOCALAPPDATA%\DroidBox\

Keep application files, SDK files, AVD data and user data separate.

Updating DroidBox must not automatically delete installed Android applications or virtual device data.

Include backup and recovery support for user configuration.

The Android virtual device's package manager is the source of truth for installed applications. Reconcile SQLite records with actual Android runtime state.

---

# 14. SECURITY AND PRIVACY

Mandatory:

- No advertisements.
- No telemetry by default.
- No hidden analytics.
- No mandatory account.
- No remote ADB exposure.
- No unnecessary administrator privileges.
- No hardcoded secrets.
- Secure package downloads.
- SHA256 verification.
- Authenticode verification where applicable.
- Process argument validation.
- Shell injection prevention.
- Local-only IPC.
- Secure logging.
- Safe uninstallation.

Show clear warnings when APK files are not trusted.

Do not describe an emulator as a guaranteed sandbox for malicious applications.

For potentially malicious APK analysis, explain the additional isolation requirements.

Only download Android SDK components from approved sources.

Keep third-party licensing and notices accurate.

---

# 15. GITHUB OPEN SOURCE STRUCTURE

Repository:

https://github.com/dav-for-dev/DroidBox

License:

Apache License 2.0.

Build a proper Open Source contribution system.

Required:

README.md
LICENSE
NOTICE
CONTRIBUTING.md
GOVERNANCE.md
CODE_OF_CONDUCT.md
SECURITY.md
SUPPORT.md
ROADMAP.md
CHANGELOG.md
THIRD-PARTY-NOTICES.md
CODEOWNERS

GitHub issue templates:

- Bug report.
- Feature request.
- Runtime compatibility report.

Pull request template.

GitHub Discussions support.

DCO sign-off validation.

Beginner-friendly labels:

- good first issue
- help wanted
- bug
- enhancement
- documentation
- performance
- runtime
- display
- cli
- security

Use a public and transparent contribution workflow.

Do not automatically merge unreviewed external contributions.

---

# 16. GITHUB ACTIONS

Create automated CI/CD.

Build workflow:

- Checkout repository.
- Install .NET 10 SDK.
- Restore dependencies.
- Build solution.
- Run unit tests.
- Run integration tests where suitable.
- Publish WPF.
- Publish CLI.
- Validate documentation.
- Validate installer scripts.

Release workflow:

Triggered by version tags:

v0.1.0

Generate:

- DroidBox-Setup-x64.exe
- DroidBox-CLI-win-x64.zip
- DroidBox-Offline-Docs.zip
- Source archive.
- SHA256SUMS.txt
- SPDX SBOM.

Create GitHub Release and upload artifacts.

Do not publish an installer unless the build and verification steps pass.

Do not distribute Android SDK components in the repository or release unless their applicable licenses explicitly permit it.

---

# 17. PERFORMANCE REQUIREMENTS

DroidBox must be lightweight.

Priorities:

- Lazy-load UI pages.
- Avoid unnecessary background workers.
- Start Android runtime on demand.
- Support a single running virtual device initially.
- Make RAM allocation configurable.
- Avoid redundant SDK downloads.
- Implement download caching.
- Avoid polling aggressively.
- Limit display frame rendering.
- Use efficient SQLite queries.
- Keep logs bounded.
- Provide an explicit stop command.

Initial engineering targets:

- WPF UI memory target: under 100 MB idle.
- Desktop startup target: under 3 seconds.
- APK parsing target: under 2 seconds for ordinary APKs.
- 30 FPS target for embedded display when hardware permits.

These targets must be benchmarked and reported. Do not fabricate performance results.

---

# 18. BUILD AND DEVELOPMENT WORKFLOW

Create these scripts:

scripts/setup-dev.ps1
scripts/build.ps1
scripts/test.ps1
scripts/publish.ps1
scripts/release.ps1
scripts/setup-runtime.ps1
scripts/generate-docs.ps1
scripts/verify-sdk.ps1

Expected commands:

dotnet restore DroidBox.sln

dotnet build DroidBox.sln -c Release

dotnet test DroidBox.sln -c Release

pwsh ./scripts/build.ps1

pwsh ./scripts/publish.ps1

The repository must be easy for community developers to build.

Document required development tools and exact supported versions.

---

# 19. IMPLEMENTATION PHASES

Do not attempt a giant implementation without validation.

Phase 1: Foundation

- .NET solution.
- WPF application.
- MVVM.
- Shared contracts.
- Database.
- Logging.
- README.

Phase 2: Runtime prototype

- SDK installer.
- AVD manager.
- ADB.
- Emulator process manager.
- Hardware acceleration verification.

Phase 3: APK manager

- APK parser.
- Installation.
- Launch.
- Uninstall.
- Installed app listing.

Phase 4: Display

- Headless/gRPC feasibility.
- Embedded rendering.
- Touch input.
- Keyboard input.
- Tablet/mobile profiles.

Phase 5: CLI

- Standalone CLI.
- Local IPC.
- Commands.
- Exit codes.
- Automation documentation.

Phase 6: Distribution

- EXE bootstrapper.
- PowerShell installer.
- CLI-only installation.
- Offline docs package.
- WinGet manifest preparation.

Phase 7: Open Source Release

- GitHub Actions.
- License and notices.
- Security workflow.
- Documentation.
- Public alpha release.

Each phase must build and pass its relevant tests before progressing.

---

# 20. DEVELOPMENT RULES — IMPORTANT

1. Work directly in the supplied repository.
2. Preserve existing work.
3. Do not create unnecessary folders or files.
4. Keep each class and responsibility in its own file.
5. Use meaningful names.
6. Add XML documentation to public APIs.
7. Use dependency injection.
8. Use async/await for I/O.
9. Use CancellationToken for long operations.
10. Do not block the WPF UI thread.
11. Implement proper error handling.
12. Never hardcode local paths.
13. Support Windows user-level installation.
14. Do not require physical Android hardware.
15. Do not claim incomplete modules are working.
16. Do not use mock implementations in production paths.
17. Keep experimental features explicitly labelled.
18. Pin package versions and audit dependencies.
19. Keep all installation and runtime operations idempotent.
20. Provide recovery for incomplete setup.

When a component cannot be completed because of platform limitations, document the limitation and implement an honest fallback.

---

# 21. ACCEPTANCE CRITERIA

DroidBox MVP is complete only when the following have been verified:

[ ] WPF application builds.

[ ] CLI builds independently.

[ ] Windows EXE installer builds.

[ ] PowerShell setup works.

[ ] Android SDK bootstrap works.

[ ] SDK licenses are handled correctly.

[ ] AVD creation works.

[ ] Emulator boots without a physical device.

[ ] ADB communicates with the local emulator.

[ ] APK installation works.

[ ] Split APK installation works.

[ ] Installed app listing works.

[ ] Application launch works.

[ ] Application uninstall works.

[ ] Tablet and mobile configurations work.

[ ] Interactive display works or a documented fallback is provided.

[ ] CLI can install and launch APKs.

[ ] Offline documentation is available.

[ ] Python integration examples execute.

[ ] GitHub Actions build and tests pass.

[ ] Third-party notices are complete.

[ ] No advertisements.

[ ] No telemetry by default.

[ ] No mandatory account.

[ ] User data survives application updates.

[ ] Release artifacts have checksums and appropriate signatures.

---

# 22. FINAL INSTRUCTION TO ANTIGRAVITY

Start by inspecting the repository and presenting a short technical implementation plan.

Then begin implementing the actual source code.

Do not stop after creating an architecture document or mock UI.

Prioritize a compiling, testable, real application.

Implement files individually and maintain a clean project structure.

After every major phase:

- Build the solution.
- Run tests.
- Fix errors.
- Update documentation.
- Report completed and incomplete features.

At the end, provide:

1. Complete repository structure.
2. Implemented source files.
3. Successful build results.
4. Test results.
5. Installer build instructions.
6. CLI usage.
7. Offline documentation location.
8. Known limitations.
9. Remaining implementation tasks.

The final objective is a real, production-oriented open-source Windows Android testing tool.

Repository:

https://github.com/dav-for-dev/DroidBox.git

Project:

DroidBox

License:

Apache-2.0

Platform:

Windows 10/11 x64

Main Technology:

C# / .NET 10 / WPF

Primary Android Runtime:

Official Android Emulator

Secondary Runtime:

QEMU + Android-x86

Distribution:

EXE Bootstrapper + PowerShell + CLI + Offline Documentation

Build it for developers. Keep it lightweight. Keep it open source.