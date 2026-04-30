# CupkekGames Input

Unity Input System integration for CupkekGames packages. Code is gated behind a `versionDefine` — only compiles if `com.unity.inputsystem` is installed.

## What's inside

**Runtime** (`CupkekGames.Input.asmdef`)

- `InputDeviceManager` — tracks the currently active input device and exposes change events
- `InputEscapeManager` + `EscapeAction` / `EscapeActionListener` — per-screen escape-key/back-button stack
- `InputIcons` (`InputIconDatabaseSO`, `InputIconControlScheme`) — control-scheme-aware glyph database for in-game prompts

**Editor** (`CupkekGames.Input.Editor.asmdef`)

- `InputIconResultDrawer` — inspector drawer for input-icon lookup results

## Dependencies

- `com.unity.inputsystem` (optional — gated by versionDefine)
