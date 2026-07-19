# Flappy Bird Clone (Unity101)

A faithful Flappy Bird clone built with **Unity 6** (URP) as a learning project, targeting **PC, macOS, iOS, and Android** from a single codebase. One button, pooled pipes, pixel-perfect classic art, parallax background, and an original chiptune loop.

> ⚠️ **Asset license note:** the sprites and sound effects are the classic Flappy Bird assets (© Dong Nguyen / .GEARS), used here for private educational purposes only. **Do not distribute builds publicly** without replacing them — see [Docs/GDD.md §2](Docs/GDD.md). The background music is an original, license-free generated track.

## How to play

1. Open the project in Unity 6 (built with `6000.3.12f1`).
2. Open `Assets/Scenes/Game.unity` and press **Play** (portrait 9:16 Game View recommended; landscape works too).
3. Flap: **tap** (mobile) / **left click, Space, Enter, W, ↑** (desktop) / **A button** (gamepad).

Pass through pipe gaps to score. One hit ends the run — tap to restart instantly. Best score is saved locally.

## Project structure

| Path | What |
|---|---|
| `Assets/Scenes/Game.unity` | The single game scene (only scene in Build Settings) |
| `Assets/Game/Scripts/` | All runtime code (state machine, bird physics, pooled pipe spawner, scrolling/parallax tiles, bitmap-digit UI, audio) |
| `Assets/Game/Editor/GameSetup.cs` | Regenerates the entire scene: **Tools ▸ Flappy Bird ▸ Build Game Scene** |
| `Assets/Game/GameConfig.asset` | Every gameplay tuning value (gravity, flap, speeds, gaps) — edit in the Inspector, no code needed |
| `Assets/Game/Sprites/`, `Assets/Game/Audio/` | Classic art & SFX + generated music loop |
| `Docs/GDD.md` | Full game design document |
| `Docs/ImplementationNotes.md` | Implementation status, script map, dev notes |

## Design highlights

- **Classic feel**: flap *replaces* vertical velocity (never adds), fixed scroll speed, no difficulty ramp, deterministic 50 Hz physics with interpolation.
- **Cross-platform by construction**: aspect-aware camera framing, UI scales with screen height, safe portrait lock on mobile, separate URP pipeline assets for PC and mobile, focus-loss auto-freeze (tap to resume).
- **Zero allocations during play**: pipes and ground/background tiles are pooled and recycled.
- **Dev bot**: attach `AutoPilot` to the Bird at runtime for automated playtests (current bot best: 18).

## Credits

- Classic art & SFX: Dong Nguyen / .GEARS (educational use), mirrored via [samuelcust/flappy-bird-assets](https://github.com/samuelcust/flappy-bird-assets)
- Music: original generated chiptune loop
- Built with Unity 6 + Claude Code driving the [Unity-MCP](https://github.com/IvanMurzak/Unity-MCP) editor bridge
