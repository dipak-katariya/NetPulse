# SignPath Foundation — OSS Code-Signing Application (NetPulse)

This document holds the prepared answers for the free OSS certificate
application at <https://signpath.org/apply>. Fields marked **⚠️ CONFIRM** contain
inferred values you must verify before submitting.

> Eligibility note: SignPath Foundation signs artifacts built by a **CI pipeline**
> (origin verification), not local builds. The included
> `.github/workflows/release.yml` satisfies this. SignPath also favors projects
> with demonstrable usage; strengthen the **Reputation** answer with real numbers
> (GitHub stars, release download counts) before applying.

---

## Form answers

**Project Name**
```
NetPulse
```

**Repository URL**
```
https://github.com/dipak-katariya/NetPulse
```

**Homepage URL**
```
https://github.com/dipak-katariya/NetPulse
```

**Download URL**
```
https://github.com/dipak-katariya/NetPulse#install-netpulse
```
> This README section names SignPath Foundation as the code-signing provider, as
> required ("This page must mention that the project uses the SignPath Foundation
> for code signing.").

**Privacy Policy URL**
```
https://github.com/dipak-katariya/NetPulse/blob/main/docs/PRIVACY.md
```

**Wikipedia URL** (optional)
```
(none)
```

**Tagline**
```
A lightweight, transparent Windows desktop widget that shows your real-time
network upload and download speed next to the taskbar.
```

**Description**
```
NetPulse is a free, open-source Windows desktop utility that displays live
network upload and download speed in a small, transparent, frameless widget
pinned next to the system tray. It ships as a single self-contained executable
with no installer, requires no administrator rights, and runs entirely offline —
it reads only local network-adapter counters and makes no network connections,
no telemetry, and no auto-update. NetPulse is designed for users who want a
clean, glanceable, always-on speed indicator without bundled bloatware.
```

**Reputation** — ⚠️ CONFIRM / STRENGTHEN
```
NetPulse is a publicly developed open-source project on GitHub
(https://github.com/dipak-katariya/NetPulse), released under the MIT license,
built and tested via GitHub Actions CI. <Add concrete evidence before
submitting, e.g.: "N GitHub stars, M total release downloads, featured in
<blog/forum>, discussed at <link>.">
```
> This is the weakest field for a young project. SignPath wants proof the project
> is used/trusted. If you have no stars/downloads yet, consider publishing a few
> releases and gathering some traction first, or be candid that it is an
> early-stage project seeking signing to enable safe public distribution.

**Maintainer Type**
```
Individual
```
> Keep this as **Individual** unless a legal "NetPulse" entity actually exists.
> Being an individual maintainer is normal for OSS and does not reduce approval
> chances; claiming an organization that cannot be verified can cause rejection
> or later revocation.

**Build System**
```
GitHub Actions
```

**First Name** — ⚠️ CONFIRM (real person — this creates the SignPath account)
```
Dipak
```

**Last Name** — ⚠️ CONFIRM (real person — this creates the SignPath account)
```
Katariya
```
> First/Last name and email identify the **account holder** that SignPath
> verifies and contacts. These must be a real person — they cannot be "NetPulse".
> They are not printed on the certificate (Foundation certs are issued in
> "SignPath Foundation"'s name, associated with the project).

**Email** — ⚠️ CONFIRM
```
<your project/contact email>
```

**Company Name** (optional)
```
NetPulse
```
> Optional, free-text metadata. "NetPulse" here reads as the project/brand, which
> is fine. Do **not** rely on this field to present the project as a registered
> company — that role belongs to **Maintainer Type**, which should stay
> "Individual" (see above).

**Primary Discovery Channel** — ⚠️ CONFIRM
```
Search engine
```

**Please specify the exact source** (optional) — ⚠️ CONFIRM
```
Google search for "free code signing certificate for open source projects"
```

---

## Consent checkboxes

- ☑ I have read and agree to the SignPath Foundation Code of Conduct, and I
  understand certificates are issued in SignPath Foundation's name and may be
  revoked if terms are violated. — **required, tick it**
- ☐ I agree to receive other communications from SignPath. — optional
- ☑ I agree to allow SignPath to store and process my personal data. —
  **required to submit**

---

## Pre-submission checklist

- [x] OSI-approved license in repo — `LICENSE` (MIT)
- [x] Privacy policy — `docs/PRIVACY.md`
- [x] Code of Conduct — `CODE_OF_CONDUCT.md`
- [x] Security policy — `SECURITY.md`
- [x] CI build pipeline — `.github/workflows/release.yml` (GitHub Actions)
- [x] README names SignPath Foundation as signing provider (Download section)
- [ ] Commit & push all of the above to the **default branch** (`main`)
- [ ] After approval: set SignPath Org ID / project / policy + add
      `SIGNPATH_API_TOKEN` repo secret, then fill the three TODOs in the workflow
- [ ] Strengthen the **Reputation** answer with real metrics
- [ ] Confirm all ⚠️ CONFIRM fields above
