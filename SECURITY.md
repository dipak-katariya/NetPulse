# Security Policy

## Supported versions

NetPulse is distributed as a self-contained single-file executable. Only the
**latest released version** receives security fixes. Always download the most
recent build from the
[Releases page](https://github.com/dipak-katariya/NetPulse/releases/latest).

| Version | Supported |
| :--- | :---: |
| Latest release | ✅ |
| Older releases | ❌ |

## Reporting a vulnerability

Please report security issues **privately** — do not open a public issue for
an unpatched vulnerability.

- Preferred: open a [GitHub private security advisory](https://github.com/dipak-katariya/NetPulse/security/advisories/new).
- Alternatively, reach the NetPulse maintainers through the project's
  [GitHub repository](https://github.com/dipak-katariya/NetPulse).

Please include:

- A description of the issue and its impact.
- Steps to reproduce (proof-of-concept if available).
- The NetPulse version and your Windows version / architecture.

You can expect an initial acknowledgement within **5 business days**. Once a fix
is available, a new signed release will be published and the advisory disclosed.

## Integrity of downloads

- Every released EXE is published with a matching `*.sha256` checksum.
- Windows builds are **Authenticode code-signed**; signing is provided by the
  [SignPath Foundation](https://signpath.org/) free OSS program.
- NetPulse performs **no auto-update** and makes **no network connections**, so
  a downloaded binary never changes itself after install.

## Security posture (by design)

- Runs entirely in **user scope** — no administrator rights required.
- Reads only local network-adapter counters; sends no data anywhere.
- Writes only to `%APPDATA%\NetPulse\` and the user-scope `Run` registry key.
