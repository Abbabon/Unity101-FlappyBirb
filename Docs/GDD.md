# Game Design Document — *Flappy Bird Clone*

| | |
|---|---|
| **Working Title** | Flappy Bird Clone (Unity101-FlappyBird) |
| **Genre** | Arcade / Endless obstacle-dodger / "One-button" score-chaser |
| **Platforms** | PC (Windows), macOS, iOS, Android |
| **Engine** | Unity 6 (URP — separate PC and Mobile render pipeline assets already configured) |
| **Orientation** | Portrait-first (9:16 reference); letterboxed/pillarboxed on desktop |
| **Target Audience** | Casual players, all ages; also a learning project (Unity101) |
| **Session Length** | 10 seconds – 5 minutes |
| **Monetization** | None (educational/portfolio project) |
| **Document Version** | 1.0 — 2026-07-19 |

---

## 1. High Concept

The player controls a small bird that constantly falls under gravity. A single input — tap, click, or key press — makes the bird flap upward. The world scrolls from right to left at a constant speed, presenting an endless series of pipe pairs with a fixed-size vertical gap at randomized heights. Passing through a gap scores one point. Touching a pipe, the ground, or (optionally) the ceiling ends the run instantly. The entire game loop — die, see score, restart — takes under three seconds, which is the core of its addictiveness.

**Design pillars:**

1. **One input, total depth** — the entire skill ceiling lives in the timing of a single button.
2. **Instant restart** — failure costs nothing but pride; the retry loop must be frictionless (< 2 s from death to new run).
3. **Absolute fairness** — deterministic physics, fixed gap size, no difficulty ramp. Every death is the player's fault and feels like it.
4. **Pixel-perfect retro charm** — faithful recreation of the classic 2013 look and feel.

---

## 2. Legal Note on "Classic Assets"

The original Flappy Bird art, audio, and name are the property of Dong Nguyen / .GEARS. The well-known classic sprite sheet (bird, green pipes, day/night city backgrounds, ground strip, medals, number fonts) and sound set (`sfx_wing`, `sfx_point`, `sfx_hit`, `sfx_die`, `sfx_swoosh`) are widely mirrored for educational use and **may be used in this private learning project**, but:

- **Do not publish** builds containing the original assets to any store, itch.io, or public web host.
- For any public release, swap in original or CC0 replacements (e.g., "Tappy Plane" style kits, Kenney.nl assets) and rename the game. The architecture below treats all art/audio as data-driven so a reskin is a drop-in replacement.

---

## 3. Core Gameplay

### 3.1 Game Loop

```
 ┌────────────┐   tap    ┌──────────┐   collision   ┌───────────┐
 │  GetReady  │ ───────► │ Playing  │ ─────────────►│ GameOver  │
 │ (bird idle │          │ (physics │               │ (score +  │
 │  hovering) │          │  active) │               │  medal UI)│
 └────────────┘          └──────────┘               └─────┬─────┘
       ▲                                                  │ tap "OK"
       └──────────────────────────────────────────────────┘
```

- **Boot/Splash → Title:** logo, animated bird, "Play" button, high score display.
- **GetReady:** world scrolls, bird bobs on a sine wave at start position, "Get Ready" graphic + tap hint shown. Pipes do **not** spawn yet. First tap starts physics and spawning.
- **Playing:** the core loop (see 3.2).
- **GameOver:** flash frame + hit/die sounds, bird tumbles to the ground, score panel slides up (swoosh), medal awarded, buttons for **Restart** and **Menu**.

### 3.2 Moment-to-Moment Rules

- The bird has **no horizontal movement**; the world moves left at constant speed. (Implementation: bird's `x` is fixed; pipes and ground scroll.)
- Every flap sets vertical velocity to a fixed upward impulse (velocity is **replaced**, not added — this is essential to the classic feel).
- Bird rotation is cosmetic but iconic: pitch up quickly to ~ +25° on flap, then after a short hold, pitch down toward −90° as it falls.
- Score +1 the moment the bird's leading edge passes a pipe pair's trailing edge (one score trigger per pair).
- **Death conditions:** overlap with any pipe collider or the ground. The top of the screen is not lethal, but the bird cannot fly above it far enough to skip pipes (clamp `y`, and pipes extend above the visible area).
- On death: gameplay input is ignored, scrolling stops instantly, bird keeps gravity and tumbles to the ground, then the results panel appears.

### 3.3 Tuning Values (initial — expect iteration)

All values in Unity world units, 1 unit = 100 px of the reference art (PPU = 100), reference resolution 288 × 512 (classic native, scaled ×2 on modern screens).

| Parameter | Value | Notes |
|---|---|---|
| Gravity | −9.8 u/s² × **1.8** scale ≈ −17.7 u/s² | Softened from 2.5 after playtest feedback (fall felt too fast) |
| Flap impulse (set velocity) | +5.0 u/s | Softened from 6.5 after playtest feedback (jump felt too high) |
| Terminal fall velocity | −10 u/s (clamped) | Prevents unrecoverable dives |
| Scroll speed | 1.6 u/s | Constant; never ramps |
| Pipe gap height | 1.2 u (≈ 120 px ref) | Fixed for the whole run |
| Pipe horizontal spacing | 1.8 u | Constant spawn cadence ≈ every 1.125 s |
| Gap center range | 25 %–75 % of playfield height | Uniform random; optional max delta between consecutive gaps of 40 % of playfield to avoid impossible jumps |
| Bird collider | Circle, radius ≈ 0.12 u | Slightly **smaller** than the sprite — deaths must never feel cheap |
| Bird x-position | 28 % of screen width from left | |
| Rotation | +25° on flap; lerp to −90° when v < −4 u/s | Purely visual |

**Tuning philosophy:** the game should feel *hard but honest*. Playtest gate: a first-time player scores ≥ 1 within five attempts; a practiced player can reach 10+.

### 3.4 Scoring & Medals

- **Score:** +1 per pipe pair. Displayed live top-center in the classic big number font.
- **High score:** best score persisted locally (`PlayerPrefs` initially; JSON save file if we later add settings).
- **Medals** (shown on the results panel, classic thresholds):

| Medal | Score |
|---|---|
| Bronze | 10 |
| Silver | 20 |
| Gold | 30 |
| Platinum | 40 |

- **"New" label** appears next to the high score when beaten this run.
- Results panel counts the score up from 0 (ticking), then reveals the medal with a sparkle animation.

---

## 4. Controls & Input

Single logical action: **Flap**. Implemented via the already-present **Unity Input System** (`InputSystem_Actions.inputactions`) with one action map:

| Platform | Flap | Pause | UI Navigation |
|---|---|---|---|
| PC / Mac | `Space`, `Left Mouse Button`, `Enter`, `W`/`↑` | `Esc` / `P` | Mouse + keyboard |
| Mobile (iOS/Android) | Tap anywhere on screen | OS back button (Android) pauses; pause button on screen | Touch |
| Gamepad (nice-to-have) | `A` / South button | `Start` | D-pad |

Input rules:

- Flap is read on **press** (not release), buffered in `Update`, applied in `FixedUpdate` — no dropped inputs.
- Input over UI buttons must **not** flap (use `EventSystem.IsPointerOverGameObject` / UI Toolkit picking).
- On GetReady, the first Flap input both starts the run **and** counts as the first flap.
- On GameOver, gameplay input is locked for 0.5 s so panic-taps don't instantly restart.

---

## 5. Game Screens & UI

Reference layout is portrait 9:16. On desktop the playfield renders pillarboxed at 9:16 in a resizable window (default 480 × 854); background side bars use a subtle dark fill.

### 5.1 Screen Inventory

1. **Title** — game logo (custom, not the trademarked logo for any public build), animated flapping bird, `Play`, `Scores` (optional), mute toggle, version string.
2. **GetReady** — "Get Ready!" graphic, tap-hint icon (finger/click), live score `0`.
3. **HUD (Playing)** — score only. Nothing else. Optional tiny pause button top-right (mobile).
4. **Pause** — dimmed overlay: `Resume`, `Restart`, `Menu`. Game time frozen (`Time.timeScale = 0`).
5. **GameOver / Results** — "Game Over" graphic, panel with Score / Best / Medal / "New" tag, `OK` (menu) and `Restart` buttons.

### 5.2 UI Style

- Classic bitmap number fonts: large (score HUD) and small (results panel).
- Panels and buttons from the classic sheet (9-sliced where needed).
- All UI on a Screen Space camera canvas, `CanvasScaler` in *Scale With Screen Size*, reference 288 × 512, match = 0.5.
- Transitions: results panel slides up with `sfx_swoosh`; screen wipes use the classic black fade (0.3 s).

---

## 6. Art Direction

### 6.1 Asset Manifest (classic sprite sheet)

| Asset | Frames / Variants | Use |
|---|---|---|
| Bird | 3 colors (yellow, red, blue) × 3 flap frames | Random color per run; 3-frame flap loop at ~10 fps, ping-pong |
| Pipe | Green (day) — top + bottom | Single sprite flipped vertically for the top pipe |
| Background | Day (city + clouds), Night (stars) | Random per run or day/night by local clock; static (no parallax in the classic) |
| Ground | Repeating dirt/grass strip | Scrolls at world speed, in front of pipes |
| "Get Ready", "Game Over", logo | 1 each | Screen graphics |
| Medals | Bronze / Silver / Gold / Platinum | Results panel |
| Number fonts | Big (0–9), small (0–9) | HUD / results |
| Buttons | Play, Pause, OK/Menu, mute | UI |
| Tap hint | Finger + arrows | GetReady |

### 6.2 Technical Art Rules

- **Pixel-perfect:** import with `Point (no filter)`, no compression, PPU = 100, sprites packed in one `SpriteAtlas`.
- Use Unity's **Pixel Perfect Camera** (URP) — reference 288 × 512, upscale render texture ON.
- Draw order (back → front): background → pipes → ground → bird → UI. Use sorting layers, not z.
- Ground and pipe scrolling must be perfectly synchronized (same scroll component/speed) or the world visibly "slides."
- The camera never moves; everything else does.

---

## 7. Audio

Classic five-sound set, played through a simple `AudioManager` (single `AudioSource` pool, no music — the classic has none):

| Event | Sound |
|---|---|
| Flap | `sfx_wing` |
| Score | `sfx_point` |
| Collision | `sfx_hit` |
| Death (after hit, while tumbling) | `sfx_die` |
| Panel slide / screen transition | `sfx_swoosh` |

- Master mute toggle persisted in `PlayerPrefs`.
- Mobile: audio session set to *ambient/mix-with-others* so it doesn't kill the user's music (optional setting).
- Import mono, `Decompress On Load` for these tiny clips; force to load on scene start to avoid first-play hitch.

---

## 8. Technical Design

### 8.1 Architecture Overview

Single scene (`Game.unity`) with state-driven flow — no scene loads between runs (restart = reset, for the instant-retry pillar).

```
GameManager (state machine: Title → GetReady → Playing → GameOver → Paused)
 ├── BirdController      (input buffer, physics in FixedUpdate, rotation, animation)
 ├── PipeSpawner         (timer-based spawn, gap randomization, object pool)
 ├── Scroller            (shared scroll speed for ground segments & pipes)
 ├── ScoreManager        (current, best, medal calc, PlayerPrefs persistence)
 ├── UIManager           (screen panels, HUD, transitions)
 └── AudioManager        (SFX playback, mute state)
GameConfig : ScriptableObject   (every tuning value from §3.3 — designers tune without code)
```

Key implementation decisions:

- **Physics:** `Rigidbody2D` (Dynamic) on the bird with custom gravity scale; pipes/ground are static colliders with `IsTrigger` score zones between pipe pairs. `Physics2D` simulation only — no 3D physics module needed.
- **Object pooling:** pipes and ground segments are pooled (≈ 6 pipe pairs, 3 ground segments); nothing is instantiated during play — zero GC pressure, essential for consistent frame time on mobile.
- **Determinism/feel:** all movement in `FixedUpdate` at 50 Hz; visual interpolation ON for the bird so 60/120 Hz displays look smooth.
- **State machine:** plain C# enum + switch is sufficient at this scope; no framework.
- **Config:** every number in §3.3 lives in one `GameConfig` ScriptableObject.
- **Saves:** `PlayerPrefs` keys: `HighScore` (int), `Muted` (int). No cloud, no accounts.

### 8.2 Platform Matrix

| Concern | PC / macOS | iOS / Android |
|---|---|---|
| Render pipeline asset | `PC_RPAsset` (already in `Assets/Settings`) | `Mobile_RPAsset` (already in `Assets/Settings`) |
| Resolution | Resizable window, default 480×854; fullscreen toggle (`F11` / `Cmd+Ctrl+F`) | Native, portrait locked |
| Safe area | n/a | UI panels respect `Screen.safeArea` (notches) |
| Frame rate | vSync on | `targetFrameRate = 60` (120 optional on ProMotion later) |
| Quit | `Esc` from Title / window close | OS-managed; auto-pause on `OnApplicationPause` |
| Input | Keyboard + mouse | Touch |

- **Auto-pause:** losing focus (mobile background, desktop minimize) during Playing switches to Paused — never let the player die off-screen.
- **Aspect handling:** camera shows a fixed playfield height; extra-tall phones see more sky/ground (both extend safely), desktop pillarboxes.

### 8.3 Performance Budgets (mobile floor: 2018-era device)

- 60 fps sustained; < 1 ms script time per frame.
- Zero allocations during Playing state (verify with Profiler).
- Draw calls ≤ 10 (single atlas + UI batch).
- Build size < 30 MB Android / < 50 MB iOS.
- Cold start to Title < 3 s.

---

## 9. Content & Feature Scope

### 9.1 MVP (v0.1 — "it's Flappy Bird")

- [ ] Bird physics + flap + rotation + animation
- [ ] Pipe spawning with random gaps, pooled
- [ ] Scrolling ground, static background
- [ ] Collision → GameOver flow, instant restart
- [ ] Score + high score persistence, HUD
- [ ] All five classic SFX
- [ ] GetReady / GameOver screens
- [ ] Desktop (Win/macOS) + one mobile build working

### 9.2 v1.0 polish

- [ ] Title screen with logo + menu
- [ ] Medals + score count-up + "New" tag on results panel
- [ ] Random bird color & day/night background per run
- [ ] Pause (Esc/button + auto-pause on focus loss)
- [ ] White flash frame on death, screen fades, swooshes
- [ ] Mute toggle, safe-area handling, icons/splash for all platforms

### 9.3 Nice-to-have (post-1.0, only if desired)

- Gamepad support; screenshot-share of results panel (mobile); alternate CC0 skin for public release; local "last 10 runs" stats; 120 Hz support.

### 9.4 Explicitly Out of Scope

Ads, IAP, leaderboards/online services, difficulty ramping, power-ups, multiple game modes, level progression. The classic's purity is the product.

---

## 10. Milestones

| Milestone | Contents | Estimate |
|---|---|---|
| **M1 — Greybox** | Bird physics + placeholder pipes, death & restart, tuned feel | 1–2 days |
| **M2 — MVP** | §9.1 complete with classic assets | +2–3 days |
| **M3 — Polish** | §9.2 complete, mobile builds tested on device | +3–4 days |
| **M4 — Release candidate** | Perf pass vs. §8.3 budgets, playtest tuning, bug fix | +2 days |

---

## 11. Risks & Mitigations

| Risk | Impact | Mitigation |
|---|---|---|
| Classic assets in a public build | Legal/takedown | §2 policy: private use only; data-driven art so a reskin is trivial |
| Feel doesn't match the classic | Core pillar fails | `GameConfig` SO for live tuning; reference-video side-by-side comparison; velocity-set (not additive) flap from day one |
| Frame hitches on mobile → unfair deaths | Fairness pillar fails | Object pooling, zero-alloc gameplay, atlas, profiler gate in M4 |
| Input latency on mobile browsers/devices | Feels mushy | Read input on press, buffer to FixedUpdate; test on lowest-end target device |
| Aspect-ratio extremes (tablets, ultrawide) | Gaps or unfair view | Fixed playfield height + extendable sky/ground; pillarbox on desktop |

---

## 12. Open Questions

1. Should the top of the screen be lethal like the ground, or clamped (classic behavior: clamped)? **Current decision: clamped.**
2. Day/night background: random per run or based on device clock? **Current lean: random.**
3. Do we want a WebGL build for easy sharing? (Would raise the asset-licensing question immediately — only with a reskin.)
