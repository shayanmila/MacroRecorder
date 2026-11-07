# Masoom Macro Recorder

Native Windows desktop macro recorder built using **C# / .NET 8 / WPF**. No Python and no Flutter.

Features: global mouse and keyboard recording, event log, JSON save/load, replay with adjustable speed, repeat count or timed replay, F8 emergency stop and F9 playback pause/resume.

Build locally on Windows with `dotnet run --project src/MacroRecorder/MacroRecorder.csproj`.

The **Windows Release (Manual Only)** workflow is launched from Actions > Run workflow. It creates a self-contained Windows x64 portable ZIP and Inno Setup EXE installer, then publishes GitHub Release assets. There are **no push or pull_request triggers**.

**Privacy:** recordings can include typed sensitive text; avoid recording passwords. Use only on devices and apps you control. Playback in protected/elevated windows can be restricted by Windows.

Original idea and branding: MasoomDev.