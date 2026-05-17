# NetPulse

A modern, transparent Windows desktop widget that shows real-time internet upload / download speed near the taskbar tray. Single EXE, no installer, no admin rights, runs entirely offline.

## Quick start

1. Drop `NetPulse.exe` anywhere (e.g. `%LOCALAPPDATA%\NetPulse\`).
2. Double-click to launch.
3. On first launch you'll be asked whether to start automatically with Windows.
4. Right-click the floating widget (or the tray icon) for **Settings…** — layout, units, theme, fonts, autostart mode, and more.

## Build from source

Requires the **.NET 8 SDK** (build-time only — end users don't need .NET installed). The published EXE is self-contained.

```powershell
dotnet test  D:\Projects\katard\Tools\NetPulse\NetPulse.sln
dotnet publish D:\Projects\katard\Tools\NetPulse\src\NetPulse\NetPulse.csproj `
    -c Release -r win-x64 --self-contained true `
    -o D:\Projects\katard\Tools\NetPulse\publish\win-x64
```

For ARM64 laptops, repeat with `-r win-arm64`.

## Signing the EXE (optional, recommended)

The build automatically signs the published EXE if you provide an Authenticode PFX via environment variables. No extra tooling is added to the project — this uses `signtool.exe` from the Windows SDK that ships with Visual Studio / Windows 10/11 SDK.

Required:

| Variable | Purpose |
|---|---|
| `NETPULSE_SIGN_PFX` | Absolute path to your `.pfx` certificate file. |
| `NETPULSE_SIGN_PFX_PASSWORD` | PFX password (omit if your PFX has none). |

Optional:

| Variable | Default |
|---|---|
| `NETPULSE_SIGN_TIMESTAMP_URL` | `http://timestamp.digicert.com` |
| `NETPULSE_SIGN_TOOL_PATH` | uses `signtool.exe` from `PATH` |
| `NETPULSE_SIGN_DESCRIPTION` | `NetPulse` |

Example PowerShell setup before publish:

```powershell
$env:NETPULSE_SIGN_PFX = "C:\certs\netpulse-signing.pfx"
$env:NETPULSE_SIGN_PFX_PASSWORD = "your-pfx-password"
dotnet publish src\NetPulse\NetPulse.csproj -c Release -r win-x64 --self-contained true -o publish\win-x64
```

After publishing, verify manually:

```powershell
signtool verify /pa /v publish\win-x64\NetPulse.exe
```

If the PFX env var is **not** set, the EXE is still produced — just unsigned. The Settings window shows the signature subject ("Signature: not signed" if absent).

### Obtaining a code-signing certificate

For external distribution, you need a real Code Signing certificate from a CA trusted by SmartScreen. Practical options:

- **Azure Trusted Signing** (~$10/month) — cloud-hosted, simplest.
- **EV (Extended Validation) Code Signing Certificate** from DigiCert, Sectigo, GlobalSign — $200–$500/year, comes on a hardware token, instant SmartScreen reputation.
- **Standard OV (Organization Validation) Code Signing Certificate** — cheaper but accumulates reputation slowly.

For internal testing, you can sign with a self-signed PFX produced via `New-SelfSignedCertificate` — this proves authenticity on machines where you've installed the certificate as Trusted, but won't satisfy SmartScreen on other PCs.

## What gets reverse-engineered, and what doesn't

**Be honest:** .NET 8 WPF binaries compile to IL (intermediate language), and any decompiler (dnSpy, ILSpy, dotPeek) can recover the C# source-level structure of the EXE. **Authenticode signing proves authenticity and tamper-evidence**, but it does not prevent decompilation. The realistic protection layers are, in order of cost vs benefit:

1. **Code signing (built into this project's `Sign.targets`)** — proves the EXE came from you and hasn't been modified. Removes the SmartScreen warning over time.
2. **IL obfuscation with a tool like [ConfuserEx](https://github.com/mkaring/ConfuserEx)** — free, open-source, run as a post-publish step. Significantly slows down casual decompilation, breaks naive string-grep. Not bundled in this repo because (a) it's a separate tool, (b) it can break WPF apps if misconfigured.
3. **NativeAOT** — would compile to true native code, but **is not compatible with WPF** as of .NET 8/9. Not an option for this project.

If you need stronger protection than (1) + (2), you've outgrown a WPF utility — consider rewriting the speed-sensitive parts in C++ as a native EXE.

## Privacy / data-handling

NetPulse is **completely offline**:

- It reads **only** local `NetworkInterface` byte counters (`GetIPStatistics()`).
- It writes **only** to `%APPDATA%\NetPulse\settings.json` (user-scope, no admin).
- The autostart toggle writes to `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` — user-scope only.
- There are no outbound HTTP calls, no telemetry, no analytics, no auto-update.
- You can verify with a network sniffer (Wireshark, Process Monitor) that the EXE makes zero network connections.

## Configuration surface

All settings are exposed in the Settings window (right-click the widget or the tray icon → **Settings…**):

| Section | Setting |
|---|---|
| Display | Layout (single row / two rows), Units (Bytes — KB/s / Bits — Kbps), Pin next to taskbar |
| Appearance | Theme (Follow Windows / Dark / Light), Use system font, Font family, Font size, Font color |
| Startup | Always start with Windows / One-time run / Disabled, Start hidden |
| About | Version, signature subject |

Settings are stored as JSON at `%APPDATA%\NetPulse\settings.json`. You can edit by hand; corrupt files fall back to defaults silently.

## File layout

```
src/NetPulse/        - WPF application
  Core/               - SpeedSampler, ByteRateFormatter, Settings, Autostart...
  Native/             - DWM, Shell, User32, WindowComposition P/Invokes
  UI/                 - FloatingWidget, SettingsWindow, TrayIconHost
  Sign.targets        - optional Authenticode signing (env-var driven)
tests/NetPulse.Tests - xUnit tests (Core + Settings + Autostart)
```

## License

(unspecified)
