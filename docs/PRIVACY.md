# NetPulse Privacy Policy

_Last updated: 2026-05-29_

NetPulse is a local Windows desktop utility that displays your real-time network
upload and download speed. Protecting your privacy is simple for us, because
**NetPulse does not collect, transmit, or share any data whatsoever.**

## What data NetPulse processes

| Data | Where it stays | Sent anywhere? |
| :--- | :--- | :---: |
| Local network adapter byte counters (read via the Windows `NetworkInterface` API) | In memory only, for live display | No |
| Your display preferences (layout, units, theme, font, autostart mode) | `%APPDATA%\NetPulse\settings.json` on your own machine | No |
| Autostart preference | `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` (user scope) | No |

## What NetPulse does **not** do

- No telemetry, analytics, crash reporting, or usage tracking.
- No accounts, sign-in, ads, or in-app purchases.
- No outbound network connections of any kind — no auto-update, no "phone home".
- No reading of packet contents, browsing history, or personal files.
- No access to data outside the user-scope locations listed above.

## Verifiability

Because NetPulse makes zero network connections, you can confirm this yourself
with a packet sniffer such as **Wireshark** or **Process Monitor** — you will
observe no outbound traffic originating from `NetPulse.exe`.

## Third parties

NetPulse bundles no third-party SDKs that collect data. The published Windows
binaries are code-signed; signing is provided by the
[SignPath Foundation](https://signpath.org/). SignPath only verifies the origin
and integrity of the build artifact — it does not receive any end-user data.

## Data removal

NetPulse is fully portable. To remove all traces:

1. Exit NetPulse from its tray icon.
2. Delete `NetPulse.exe`.
3. Delete the settings folder: `%APPDATA%\NetPulse\`.
4. Remove the autostart entry (if set): `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` → `NetPulse`.

## Contact

Questions about this policy can be raised as a GitHub issue:
<https://github.com/dipak-katariya/NetPulse/issues>
