<div align="center">

<img src="src/NetPulse/Assets/NetPulse.ico" width="96" height="96" alt="NetPulse logo" />

# NetPulse

**A lightweight, transparent Windows desktop widget that shows real-time upload and download speed next to your taskbar — single EXE, no installer, no admin rights, 100% offline.**

[![Latest release](https://img.shields.io/github/v/release/dipak-katariya/NetPulse?label=latest%20release&color=0a7bbb&style=for-the-badge)](https://github.com/dipak-katariya/NetPulse/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/dipak-katariya/NetPulse/total?style=for-the-badge&color=0a7bbb)](https://github.com/dipak-katariya/NetPulse/releases)
[![Build](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
[![Platform](https://img.shields.io/badge/Windows-10%20%7C%2011-0078D6?style=for-the-badge&logo=windows)](#system-requirements)
[![License](https://img.shields.io/github/license/dipak-katariya/NetPulse?style=for-the-badge&color=success)](#license)

[**Download NetPulse**](#install-netpulse) &nbsp;·&nbsp;
[**Features**](#features) &nbsp;·&nbsp;
[**Build from source**](#build-from-source) &nbsp;·&nbsp;
[**Privacy**](#privacy)

</div>

<!-- VERSION:START -->
**Current release: v0.1.0** &nbsp;&middot;&nbsp; Released 2026-05-28 &nbsp;&middot;&nbsp; [Direct download](#direct-download-from-repository)
<!-- VERSION:END -->

---

## About

NetPulse is a tiny, beautifully simple Windows desktop utility that pins a live network speed indicator to the area next to your system tray. It samples local network adapter counters every second and renders the result in a frameless, transparent widget that stays out of your way.

It is built specifically for users who want a **glanceable, always-on**, no-frills speedometer — without bundled bloatware, telemetry, network calls, or an installer that requires elevation.

- **Single portable EXE** — drop it anywhere and run.
- **Self-contained** — end users do **not** need .NET installed.
- **Zero network activity** — verifiable with Wireshark / Process Monitor.
- **User-scope only** — no admin rights, no registry tampering outside `HKCU\...\Run`.
- **Theme-aware** — follows Windows light/dark mode, or pick your own.

---

## Install NetPulse

### Recommended: Download the latest release

Grab the **latest signed EXE** for your architecture from the buttons below — these always point at the newest published release on GitHub.

<div align="center">

| Architecture | Download | Notes |
| :---: | :---: | :--- |
| **Windows x64** (Intel / AMD) | [**Download NetPulse-win-x64.exe**](https://github.com/dipak-katariya/NetPulse/releases/latest/download/NetPulse-win-x64.exe) | Most desktops & laptops |
| **Windows ARM64** | [**Download NetPulse-win-arm64.exe**](https://github.com/dipak-katariya/NetPulse/releases/latest/download/NetPulse-win-arm64.exe) | Snapdragon / Surface Pro X |

</div>

> The links above use GitHub's `releases/latest/download/<asset>` redirect, so they always serve the most recent published release — you never need to update the link.

You can also browse **all releases, changelogs, and SHA-256 checksums** on the [Releases page](https://github.com/dipak-katariya/NetPulse/releases).

<!-- DIRECT_DOWNLOAD:START -->
### Direct download from repository

Files in this repo's `releases/` folder are refreshed by `Scripts\release.bat` and always reflect the latest committed build (currently **v0.1.0**).

| Architecture | Direct download | Approx. size | SHA-256 |
| :---: | :---: | :---: | :--- |
| **Windows x64** | [NetPulse-win-x64.exe](releases/NetPulse-win-x64.exe) | 90.1 MB | `85e45f32b20cfb4d87ed50ae706424da3f5011a63e561ba887e46ea0c461bdb5` |
| **Windows ARM64** | [NetPulse-win-arm64.exe](releases/NetPulse-win-arm64.exe) | 85.1 MB | `99696a42a49e5f6f430aa4d78fa7cca08a2e32e27ff2669f1568e3aaf6103f53` |

Checksum files are committed beside each EXE as `*.sha256`. Older versions live on the [GitHub Releases page](https://github.com/dipak-katariya/NetPulse/releases).
<!-- DIRECT_DOWNLOAD:END -->

### First run

1. Save `NetPulse-win-x64.exe` to a stable location — `%LOCALAPPDATA%\NetPulse\` is recommended.
2. Double-click to launch.
3. On first launch you will be prompted whether to **start automatically with Windows**.
4. Right-click the floating widget (or the system tray icon) and choose **Settings…** to customize layout, units, theme, font, and autostart behavior.

### Other ways to install

| Method | Command / Action |
| :--- | :--- |
| **GitHub CLI** | `gh release download --repo dipak-katariya/NetPulse --pattern "NetPulse-win-x64.exe"` |
| **PowerShell (one-liner)** | `iwr https://github.com/dipak-katariya/NetPulse/releases/latest/download/NetPulse-win-x64.exe -OutFile NetPulse.exe` |
| **Build it yourself** | See [Build from source](#build-from-source) below |

### Uninstall

NetPulse is fully portable — to uninstall, simply:

1. Close the widget from the tray icon → **Exit**.
2. Delete `NetPulse.exe`.
3. (Optional) Delete `%APPDATA%\NetPulse\` to clear your settings.
4. (Optional) Remove the autostart entry: `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` → `NetPulse`.

---

## System requirements

| Requirement | Minimum |
| :--- | :--- |
| **OS** | Windows 10 version 1809 (build 17763) or later — Windows 10 / 11 |
| **Architecture** | x64 *or* ARM64 |
| **RAM** | ~40 MB working set |
| **Disk** | ~80 MB for the self-contained EXE |
| **.NET runtime** | **Not required** — the EXE is self-contained |
| **Privileges** | Standard user — no administrator rights |
| **Network** | None — fully offline |

---

## Features

NetPulse focuses on doing one thing well — showing your real-time network speed — with a thoughtful set of customizations.

### Live speed widget

A small frameless window that hovers next to the taskbar tray and shows live **upload / download** speed of the active network adapter, updated every second.

- Auto-detects the active adapter (Wi-Fi, Ethernet, VPN, mobile broadband).
- Aggregates across multiple adapters when applicable.
- Smoothed sampling using a short rolling window to avoid jitter.
- Idle-friendly: rendering pauses when the desktop is locked.

### Flexible display options

| Option | Choices |
| :--- | :--- |
| **Layout** | Single row (compact)  ·  Two rows (stacked up/down) |
| **Units** | Bytes (KB/s, MB/s, GB/s)  ·  Bits (Kbps, Mbps, Gbps) |
| **Pin location** | Pinned next to the taskbar tray  ·  Free-float anywhere |
| **Transparency** | Click-through transparent background with subtle glass blur |

### Appearance

- **Theme**: Follow Windows  ·  Dark  ·  Light
- **Font**: Use Windows system font, or pick any installed font family
- **Font size & color**: Fully configurable
- **DPI-aware**: Renders crisply on 4K / 200% scaling

### Startup behavior

| Mode | Behavior |
| :--- | :--- |
| **Always start with Windows** | Adds an `HKCU\...\Run` entry (user-scope only) |
| **One-time run** | Runs now, does not register for autostart |
| **Disabled** | Removes any existing autostart entry |
| **Start hidden** | Skip showing the widget at launch — only the tray icon appears |

### Tray integration

- Lightweight tray icon with **Settings…**, **Show / Hide widget**, **About**, and **Exit**.
- Right-click context menu on both the widget and the tray icon.

### Settings persistence

- Settings are stored as plain JSON at `%APPDATA%\NetPulse\settings.json`.
- Hand-editable — corrupt or missing files fall back to defaults silently.
- Roams cleanly across machines if you sync `%APPDATA%`.

---

## Screenshots

> Screenshots will appear here once attached to the release. For now, run the EXE — the widget appears next to the system tray within a second.

---

## Build from source

### Prerequisites

- **.NET 8 SDK** ([download](https://dotnet.microsoft.com/en-us/download/dotnet/8.0))
- Windows 10 / 11 x64 or ARM64
- *(Optional)* Visual Studio 2022 17.8+ with the **.NET Desktop Development** workload

### Clone & build

```powershell
git clone https://github.com/dipak-katariya/NetPulse.git
cd NetPulse

dotnet test  .\NetPulse.sln

dotnet publish .\src\NetPulse\NetPulse.csproj `
    -c Release -r win-x64 --self-contained true `
    -o .\publish\win-x64
```

For ARM64 devices, repeat the `publish` step with `-r win-arm64`.

The output is a single self-contained EXE at `publish\win-x64\NetPulse.exe` (~70–80 MB compressed).

### Optional: Authenticode-sign the EXE

The build automatically signs the published EXE if you provide an Authenticode certificate via environment variables. Signing uses `signtool.exe` from the Windows SDK — nothing new is added to the project.

| Variable | Required | Purpose |
| :--- | :---: | :--- |
| `NETPULSE_SIGN_PFX` | yes | Absolute path to your `.pfx` certificate file |
| `NETPULSE_SIGN_PFX_PASSWORD` | optional | PFX password (omit if the PFX has none) |
| `NETPULSE_SIGN_TIMESTAMP_URL` | optional | Default: `http://timestamp.digicert.com` |
| `NETPULSE_SIGN_TOOL_PATH` | optional | Defaults to `signtool.exe` on `PATH` |
| `NETPULSE_SIGN_DESCRIPTION` | optional | Default: `NetPulse` |

```powershell
$env:NETPULSE_SIGN_PFX = "C:\certs\netpulse-signing.pfx"
$env:NETPULSE_SIGN_PFX_PASSWORD = "your-pfx-password"
dotnet publish .\src\NetPulse\NetPulse.csproj -c Release -r win-x64 --self-contained true -o .\publish\win-x64

signtool verify /pa /v .\publish\win-x64\NetPulse.exe
```

If `NETPULSE_SIGN_PFX` is not set, the EXE is still produced — just unsigned. The Settings → About panel shows the signature subject, or "Signature: not signed" if absent.

#### Getting a real code-signing certificate

For external distribution you need a certificate from a CA trusted by SmartScreen:

- **Azure Trusted Signing** — cloud-hosted, simplest, ~$10/month.
- **EV Code Signing Certificate** (DigiCert, Sectigo, GlobalSign) — $200–$500/year, hardware token, instant SmartScreen reputation.
- **Standard OV Code Signing Certificate** — cheaper, but reputation accumulates slowly.

Self-signed PFX (via `New-SelfSignedCertificate`) is fine for internal use but will not satisfy SmartScreen on other PCs.

### Repository layout

```
NetPulse/
├── src/NetPulse/              # WPF application
│   ├── Core/                  # SpeedSampler, ByteRateFormatter, Settings, Autostart
│   ├── Native/                # DWM / Shell / User32 / WindowComposition P/Invokes
│   ├── UI/                    # FloatingWidget, SettingsWindow, TrayIconHost
│   └── Sign.targets           # Optional Authenticode signing (env-var driven)
├── tests/NetPulse.Tests/      # xUnit tests (Core + Settings + Autostart)
└── NetPulse.sln
```

---

## How reverse-engineering-resistant is the binary?

Be honest: .NET 8 WPF binaries compile to IL (intermediate language), and any decompiler (dnSpy, ILSpy, dotPeek) can recover the C# source-level structure of the EXE. **Authenticode signing proves authenticity and tamper-evidence**, but it does not prevent decompilation.

Realistic protection layers, in order of cost vs benefit:

1. **Code signing** (built into this project's `Sign.targets`) — proves the EXE came from you and hasn't been modified. Removes the SmartScreen warning over time.
2. **IL obfuscation** with a tool like [ConfuserEx](https://github.com/mkaring/ConfuserEx) — free, open-source, post-publish step. Slows down casual decompilation; not bundled here because it can break WPF apps if misconfigured.
3. **NativeAOT** — would compile to true native code, but **is not compatible with WPF** as of .NET 8/9.

If you need stronger protection than (1) + (2), you've outgrown a WPF utility — rewrite speed-sensitive parts in C++.

---

## Privacy

NetPulse is **completely offline**:

- It reads **only** local `NetworkInterface` byte counters via `GetIPStatistics()`.
- It writes **only** to `%APPDATA%\NetPulse\settings.json` (user-scope, no admin).
- The autostart toggle writes to `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` — user-scope only.
- There are **no outbound HTTP calls**, **no telemetry**, **no analytics**, **no auto-update**.
- You can verify with a network sniffer (Wireshark, Process Monitor) that the EXE makes zero network connections.

---

## Contributing

Bug reports and pull requests are welcome.

- **Bug or feature request?** Open an issue: <https://github.com/dipak-katariya/NetPulse/issues>
- **Submitting a PR?** Please run `dotnet test .\NetPulse.sln` before pushing, and target the `main` branch.
- **Coding style** — follow the conventions already in `src/NetPulse/`. Keep public APIs small; user-facing strings should be culture-invariant.

---

## Roadmap

Ideas being considered for future releases (no commitments):

- Per-adapter selection in Settings (currently auto-aggregates).
- Optional daily / monthly totals graph.
- Localized UI (currently English / culture-invariant).
- WinGet manifest for `winget install NetPulse`.

If any of these matter to you, open an issue and say so — that is how priority gets set.

---

## License

See [LICENSE](LICENSE) for details. If no license file is present in the repository yet, the project is provided as-is for personal use pending a formal license decision.

---

<div align="center">

**Made for Windows users who want a clean, honest speedometer.**

<sub>Latest release: <a href="https://github.com/dipak-katariya/NetPulse/releases/latest">github.com/dipak-katariya/NetPulse/releases/latest</a></sub>

</div>
