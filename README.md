# DroidBox

> **Your Lightweight Android Testing Environment for Windows**  
> *Install. Run. Test. Without a Physical Device.*

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](#)
[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-blue)](#)
[![Framework](https://img.shields.io/badge/.NET-10.0%20LTS-purple)](#)

---

## Overview

**DroidBox** is a fast, free, open-source Android application testing environment designed for Windows developers, QA engineers, and automated test environments. It provides native desktop and command-line interfaces to install, launch, test, and debug Android APKs without requiring a physical smartphone or heavy third-party emulator bloatware.

### Core Principles

- **100% Free & Open Source**: Licensed under the Apache License 2.0.
- **Zero Advertising or Telemetry**: No tracking, analytics daemons, or telemetry sent to third parties.
- **No Mandatory Account**: Use immediately without creating an account or logging in.
- **Native Windows WPF UI**: Clean, responsive desktop user experience built with modern .NET 10.
- **Developer-First CLI**: Standalone `droidbox.exe` for seamless automation in PowerShell, Command Prompt, CI/CD, and Python scripts.
- **Offline Documentation**: Fully accessible local documentation packaged with the installer.

---

## Architecture

DroidBox uses a modular monorepo architecture:

```
DroidBox/
├── src/
│   ├── DroidBox.Core/               # Domain models, entities, and runtime interfaces
│   ├── DroidBox.Shared/             # Cross-cutting utilities, path and process helpers
│   ├── DroidBox.Application/        # Use-case services (Emulator, APK, Device, Capture)
│   ├── DroidBox.Infrastructure/     # EF Core SQLite storage and Serilog logging
│   ├── DroidBox.Runtime.AndroidSdk/ # Official Google Android Emulator & ADB provider
│   ├── DroidBox.Runtime.Qemu/       # Experimental QEMU + Android-x86 provider
│   ├── DroidBox.Display/            # Screen capture & input streaming controls
│   ├── DroidBox.Bootstrapper/       # Local Named Pipe IPC daemon
│   ├── DroidBox.Cli/                # Standalone droidbox.exe developer tool
│   └── DroidBox.Desktop/            # Native WPF desktop application
├── tests/                           # Unit and integration test suites
├── installer/                       # Inno Setup and WinGet distribution manifests
├── docs/                            # Offline HTML manuals and integration guides
└── scripts/                         # Build, test, provision, and packaging automation
```

---

## Quick Start

### 1. Requirements

- Windows 10 (version 1903+) or Windows 11 (64-bit).
- .NET 10 SDK or Desktop Runtime.
- Windows Hypervisor Platform (WHPX) enabled for hardware acceleration.

### 2. Verify System Health

Run the diagnostic doctor tool from any terminal:

```powershell
droidbox doctor
```

### 3. CLI Usage

```powershell
# List available device profiles
droidbox devices

# Install an APK file
droidbox install "C:\path\to\application.apk"

# List installed packages
droidbox apps

# Launch an app
droidbox launch "com.example.testapp"

# Take a screenshot
droidbox screenshot --output "screenshot.png"

# Stop running emulator
droidbox stop
```

---

## Developing & Building

Clone the repository and run the automated build script:

```powershell
# Run the complete test suite
.\scripts\test.ps1

# Build all projects in Release mode
.\scripts\build.ps1

# Publish binaries
.\scripts\publish.ps1
```

---

## Testing Framework Integrations

- **Flutter**: Run `flutter devices` to detect DroidBox via standard ADB connection.
- **React Native**: Execute `npx react-native run-android` directly.
- **Python**: Use the Python client in `samples/python/droidbox.py`.

---

## License

DroidBox is released under the **Apache License 2.0**. See [LICENSE](LICENSE) for details.
