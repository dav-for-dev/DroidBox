# DroidBox — Implementation Plan

## Repository State

- **Remote**: `https://github.com/dav-for-dev/DroidBox.git`
- **Branch**: `main` (single commit: "Initial commit")
- **Existing files**: Only `README.md` (10 bytes: `# DroidBox`)
- **Environment**: Windows, .NET 6 runtime installed, .NET 10 SDK being installed

---

## Phase 1: Foundation (Current)

Build the core .NET solution with MVVM WPF app, shared contracts, database, and logging.

### Deliverables

| # | Task | Project |
|---|------|---------|
| 1 | Create `DroidBox.sln` with `Directory.Build.props` and `Directory.Packages.props` | Root |
| 2 | `DroidBox.Core` — Domain entities, interfaces, enums | `src/DroidBox.Core/` |
| 3 | `DroidBox.Shared` — Shared DTOs, constants, helpers | `src/DroidBox.Shared/` |
| 4 | `DroidBox.Application` — Services, CQRS-like patterns | `src/DroidBox.Application/` |
| 5 | `DroidBox.Infrastructure` — SQLite, EF Core, Serilog, file system | `src/DroidBox.Infrastructure/` |
| 6 | `DroidBox.Desktop` — WPF app, MVVM, navigation, themes | `src/DroidBox.Desktop/` |
| 7 | Unit test projects | `tests/` |
| 8 | Build scripts | `scripts/` |
| 9 | README, LICENSE, community files | Root |

### Architecture Notes

- **Central Package Management** via `Directory.Packages.props`
- **Shared build properties** via `Directory.Build.props`
- **DI** via `Microsoft.Extensions.Hosting` in Desktop and CLI
- **MVVM** via `CommunityToolkit.Mvvm`
- **Database** via EF Core + SQLite
- **Logging** via Serilog

## Phase 2: Runtime Prototype

| # | Task | Project |
|---|------|---------|
| 1 | `DroidBox.Runtime.AndroidSdk` — SDK detection, download, AVD, emulator | `src/DroidBox.Runtime.AndroidSdk/` |
| 2 | `DroidBox.Runtime.Qemu` — Stub/experimental QEMU provider | `src/DroidBox.Runtime.Qemu/` |
| 3 | ADB client wrapper | Infrastructure |
| 4 | Hardware acceleration (WHPX) detection | Runtime |
| 5 | Integration tests | `tests/DroidBox.Integration.Tests/` |

## Phase 3: APK Manager

| # | Task |
|---|------|
| 1 | APK binary parser (AndroidManifest.xml, resources.arsc) |
| 2 | APK metadata extraction (package name, version, SDK levels, ABIs) |
| 3 | Install/Update/Uninstall via ADB |
| 4 | Split APK support |
| 5 | WPF APK Manager page with drag-drop |

## Phase 4: Display

| # | Task |
|---|------|
| 1 | Research Android Emulator gRPC console API |
| 2 | Screenshot streaming service |
| 3 | Input injection (touch, keyboard) |
| 4 | WPF display control with embedded rendering |
| 5 | Fallback: separate window mode |

## Phase 5: CLI

| # | Task |
|---|------|
| 1 | `DroidBox.Cli` — System.CommandLine-based CLI |
| 2 | Named pipe IPC with Desktop service |
| 3 | All commands: setup, doctor, install, launch, etc. |
| 4 | Exit codes, structured output, help |

## Phase 6: Distribution

| # | Task |
|---|------|
| 1 | Inno Setup installer script |
| 2 | PowerShell bootstrapper (`Install-DroidBox.ps1`) |
| 3 | WinGet manifest |
| 4 | Offline documentation generator |
| 5 | Checksum generation |

## Phase 7: Open Source Release

| # | Task |
|---|------|
| 1 | GitHub Actions CI/CD workflows |
| 2 | Issue/PR templates |
| 3 | SBOM generation |
| 4 | Release automation |

---

> [!IMPORTANT]
> Each phase must build and pass tests before proceeding to the next.
