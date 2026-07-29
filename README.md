<div align="center">

# 👁️ ScreenVigil

**A tray-resident, in-memory time tracker for Windows.**

Watches which app has your focus — and, in the browser, which site — for as long as it's running. No database, no history, no account. Close it and the slate is clean.

[![Download](https://img.shields.io/github/v/release/mrkanber/ScreenVigil?label=Download&style=for-the-badge)](https://github.com/mrkanber/ScreenVigil/releases/latest)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg?style=for-the-badge)](LICENSE)
![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge)
![Windows](https://img.shields.io/badge/Windows-11-0078D6?style=for-the-badge)

</div>

---

## Why this exists

Tools like ManicTime or RescueTime track history and want a database for it. Sometimes you just want to know "what have I actually been doing this session" without any of that sticking around. Built end-to-end with [Claude Code](https://claude.com/claude-code), released free and open source.

## Features

- **Foreground tracking** — event-driven (`SetWinEventHook`, not polling), so it costs ~0% CPU while idle
- **Per-site breakdown in the browser** — a small Chrome/Edge extension reports the active tab's domain, so "chrome.exe" splits into `twitter.com`, `jira.yourcompany.com`, etc.
- **Zero persistence, by design** — everything lives in memory only; close the app and every number is gone. No history, no database, nothing written to disk
- **Live dashboard** — open it from the tray anytime; rows re-sort and animate in real time as durations change
- **Tray-resident** — no window on launch, single instance enforced, optional launch at startup

## Download

Grab the latest build from the [**Releases**](https://github.com/mrkanber/ScreenVigil/releases/latest) page — no installer, just run the executable.

## Browser extension (optional)

For per-site breakdown in Chrome or Edge:

1. Open `chrome://extensions` (or `edge://extensions`)
2. Enable **Developer mode**
3. **Load unpacked** → select the `Extension/` folder from this repo
4. ScreenVigil must be running locally for it to receive updates (it talks to `127.0.0.1:51823`, nothing leaves your machine)

## Build from source

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download) on Windows.

```
git clone git@github.com:mrkanber/ScreenVigil.git
cd ScreenVigil
dotnet build
dotnet run
```

## Quitting

Right-click the tray icon → **Exit**.

## Contributing

Issues and PRs are welcome. This is a small hobby project, so keep changes focused and simple.

## License

[MIT](LICENSE)
