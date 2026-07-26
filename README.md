# Box Sort — Conveyor Puzzle Game

A fast-paced Unity arcade game where players sort colored boxes on a moving conveyor belt while managing combo multipliers, special box mechanics, and an adaptive difficulty system designed to keep every run engaging.

<!--
GIF #1 — HERO SHOT (top of README, right after the description)
Record ~8-10 seconds of normal, fast gameplay: several boxes coming down the belt,
the player dragging 2-3 of them into the correct containers back-to-back, combo
counter ticking up. This is the "first impression" gif, so keep it snappy.
-->
![gameplay hero gif placeholder](docs/gifs/hero-gameplay.gif)

## Technical Highlights

A 30-second summary of what's actually going on under the hood, for anyone skimming before reading the rest:

- **Finite-state game flow** (`IGameState` + `GameStateMachine`) driving Menu → Playing → Win/Lose, with each state owning its own enter/tick/exit logic instead of scattered boolean flags.
- **Modular manager-based architecture** — Score, Combo, Timer, Level, Save, Audio, and Conveyor each own a single responsibility and communicate through cached references and C# events, never through `Find()` calls.
- **Adaptive difficulty curve** driven by a single continuous formula (score → `Mathf.Lerp`/`Clamp01`), not discrete difficulty steps or level-based `if` chains.
- **Custom smart-spawn algorithm** that prevents box overlap, throttles "trick" boxes, and brakes queued boxes using an O(n log n) distance-sorted pass instead of pairwise checks.
- **Zero runtime `Instantiate`/`Destroy`** for gameplay objects — boxes and particles are fully pooled (`ObjectPool<T>`, Unity's `IObjectPool<T>`).
- **GC-conscious code throughout**: `OverlapSphereNonAlloc`, squared-distance comparisons, cached singletons, event-driven UI instead of per-frame polling.

---

## Game Concept

Sort colored boxes into matching containers before they reach the end of the conveyor. Build combos, survive increasing difficulty, and deal with special box mechanics that require unique interactions.

<!--
GIF #2 — BUILDING VS. RUSH COMPARISON (right after "Game Concept")
Two short side-by-side style clips (or one gif that cuts between them):
  1) Early game — slow belt, only Normal boxes, plenty of reaction time.
  2) Late game (high score) — fast belt, tight box spacing, Sealed/Frozen/Glass
     boxes mixed in. This shows off the adaptive difficulty curve.
-->
![difficulty ramp gif placeholder](docs/gifs/difficulty-ramp.gif)

---

## Key Features

- **Adaptive Difficulty:** Belt speed and spawn rate scale smoothly with the player's score.
- **Combo & Multiplier System:** Consecutive correct sorts build a hit streak; every few hits the score multiplier increases (up to a configurable cap), rewarding sustained accuracy over lucky single sorts.
- **Special "Trick" Boxes:** Sealed (taped), Frozen (iced), and Glass boxes each demand a different micro-interaction before — or instead of — a normal drag-and-drop.
- **Bonus Boxes:** Rare boxes grant either extra time or bonus score (scaled by the current combo multiplier) instead of needing to match a color.
- **Freeze Ability:** A player-triggered ability that briefly halts the belt on a cooldown, useful for surviving a crowded moment.
- **Randomized Container Layout:** Container positions are shuffled at the start of each run so the color-to-position mapping can't be memorized.
- **Full Run Loop:** Menu → Playing → Win/Lose, driven by a small finite-state machine, with best-score persistence between sessions.
- **Juicy Feedback:** Squash-and-stretch tweens, camera shake, screen flash on mistakes, floating score text, and audio feedback (with pitch variation) on every interaction.

<!--
GIF #3 — TRICK BOXES SHOWCASE (right after "Key Features")
Best as three quick back-to-back clips (or one combined gif):
  - Sealed box: click to peel the tape off, then drag it normally.
  - Frozen box: hold it in place until the ice cracks/melts, then drag.
  - Glass box: drag it slowly into a container vs. dragging too fast and
    watching it shatter (shows both the success and failure case).
-->
![trick boxes gif placeholder](docs/gifs/trick-boxes.gif)

---

## Controls

| Action                      | Control                            |
| --------------------------- | ----------------------------------- |
| **Pick Up / Drag Box**      | `Left Click` / `Touch` + Drag        |
| **Peel Sealed Box**         | `Click` the box once                 |
| **Break Frozen Box's Ice**  | `Click and Hold` on the box           |
| **Drop Box in Container**   | Release over a container             |
| **Freeze Belt (Ability)**   | Click the **Freeze** UI button        |
| **Start Run**                | `Play` button (Menu screen)          |
| **Restart**                  | `Restart` button (Win/Lose screen)   |

---

## Interactive Objects & Box Mechanics

The belt can spawn **6 box types**, split between color boxes and bonus boxes, and any color box may additionally carry one of **3 mechanic states** that changes how it must be handled.

- **Color Box (Red / Blue / Green / Yellow):** Standard box — drag it into the container of the same color.
- **Bonus Score Box:** Grants a flat score bonus (multiplied by the current combo) when dropped into *any* container.
- **Bonus Time Box:** Adds extra seconds to the run timer when dropped into *any* container.
- **Sealed (Taped) Box:** Can't be dragged until the player clicks it once to unwrap the tape; briefly slows the belt as a small breather.
- **Frozen (Iced) Box:** Must be clicked-and-held for a short duration to melt the ice before it becomes draggable.
- **Glass Box:** Draggable immediately, but if it's moved faster than a set speed threshold it shatters mid-drag, counting as a miss.

Which mechanic (if any) a spawned box gets is decided by `LevelManager.GetRandomMechanicForLevel()` — the odds of Sealed/Frozen/Glass appearing increase as the player levels up, so early runs are all Normal boxes and later runs mix in all three.

<!--
GIF #4 — CONTAINER FEEDBACK (near "Interactive Objects" or "Key Features")
Capture a correct sort (container pulses/scales up, floating "+score" text,
particle/sound) immediately followed by a wrong sort (container shakes,
red particle burst, camera shake, screen flash). Having both in one gif
sells the "juice" of the feedback system well.
-->
![container feedback gif placeholder](docs/gifs/container-feedback.gif)

---

## Architecture

The project is split into two clear layers: a **state machine** that owns the high-level flow of a run, and a set of small, single-responsibility **manager** components that `GameManager` wires together. The project follows a modular manager-based architecture centered around a lightweight state machine.

```
GameStateMachine
 ├─ MenuState     → shows/hides the menu panel
 ├─ PlayingState   → starts the timer + belt, ticks the timer every frame
 ├─ WinState       → saves best score, shows win panel, stops the belt
 └─ LoseState      → saves best score, shows lose panel, stops the belt

GameManager (Instance)
 ├─ ConveyorSpawner   → spawning, belt movement, difficulty scaling
 ├─ ScoreManager       → current score + OnScoreChanged event
 ├─ ComboManager        → hit streak + multiplier + OnComboChanged event
 ├─ TimerManager         → countdown + OnTimeChanged / OnTimeUp events
 ├─ LevelManager          → level thresholds + per-level box mechanic odds
 ├─ SaveManager            → best-score persistence (PlayerPrefs)
 ├─ UIManager               → HUD + panels, subscribed to manager events
 └─ InputManager             → raycasting, drag handling, container drops
```

- **`IGameState` / `GameStateMachine`:** A tiny interface (`Enter`/`Tick`/`Exit`) plus a driver class that calls the current state's `Tick()` every frame and swaps states on `ChangeState()`. This keeps "what should happen when the game starts/ends" in one place per state, instead of `if (isPlaying)` checks scattered across multiple scripts.
- **`GameManager` as composition root:** All manager references live on `GameManager` and are assigned once in the Inspector. States and other managers talk to each other *through* `GameManager.Instance` rather than searching the scene, so the dependency graph is explicit and easy to trace from one file.
- **Events over polling:** Managers that hold state (`ScoreManager`, `ComboManager`, `TimerManager`) expose C# events (`OnScoreChanged`, `OnComboChanged`, `OnTimeChanged`) instead of public getters that UI would need to poll every frame. `UIManager` and `JuicyUI` simply subscribe once in `Start()`/`OnEnable()`.
- **Single entry point for game rules:** Cross-cutting rules like "what happens on a correct sort" or "what happens when a box is missed" live as one method each on `GameManager` (`OnCorrectSort`, `OnWrongSort`, `OnBoxMissed`), so `Container`, `EndPointTrigger`, and `Box` all report *outcomes* upward rather than each implementing scoring/combo/life logic themselves.

---

## Project Structure

```text
Game
├── State Machine
│   ├── MenuState
│   ├── PlayingState
│   ├── WinState
│   └── LoseState
│
├── Managers
│   ├── GameManager
│   ├── ScoreManager
│   ├── ComboManager
│   ├── TimerManager
│   ├── LevelManager
│   └── SaveManager
│
├── Gameplay
│   ├── ConveyorSpawner
│   ├── Box
│   ├── Container
│   ├── EndPointTrigger
│   └── ContainerShuffler
│
├── Pooling
│   ├── ObjectPool<T>
│   └── ParticlePoolManager
│
└── UI & Feedback
    ├── UIManager
    ├── JuicyUI
    ├── FloatingScore
    ├── CameraShaker
    └── AudioManager
```

---

## Adaptive Difficulty

Difficulty scales continuously based on player score using interpolation rather than predefined difficulty tiers.

- At score `0`, the belt runs at `baseTravelSpeed` (1.5 units/s) and spawns a box every `baseSpawnInterval` (1.5s).
- As score approaches `scoreForMaxDifficulty` (500 by default), both values are linearly interpolated toward their extremes: `maxTravelSpeed` (4.0 units/s) and `minSpawnInterval` (0.6s).
- Past that score, `Clamp01` holds the difficulty at its maximum instead of overshooting, so the belt never becomes literally unplayable no matter how long a skilled run continues.

Because this is one `Lerp` per axis rather than a lookup table of hand-tuned "level 1 / level 2 / level 3" values, the ramp feels smooth instead of stepped, and both curves (speed and spawn rate) can be independently re-tuned by adjusting four public fields — no code changes required.

Separately, `LevelManager.GetRandomMechanicForLevel()` layers a *second*, coarser difficulty axis on top of the continuous one: which "trick" mechanics (Sealed/Frozen/Glass) are even allowed to spawn increases in variety as the player's level rises, so early runs stay simple while the continuous speed/spawn curve is still ramping up underneath.

---

## Smart Spawn Algorithm

Spawning isn't just "instantiate a random box on a timer" — `ConveyorSpawner` actively manages what's allowed on the belt at once so the difficulty curve never turns into an unfair pile-up:

- **Anti-Overlap Braking:** Active boxes continuously adjust their movement based on the distance to the box ahead. When they get too close, their speed is smoothly reduced using `Mathf.Lerp`, creating natural queues instead of overlapping objects or relying on physics collisions.
- **Complex-Box Throttling:** Before assigning a special mechanic (Sealed, Frozen, or Glass), the spawner checks how many complex boxes are already active. If the configured limit has been reached, the new box is spawned as a normal box instead, preventing overwhelming gameplay.
- **Compensating Spawn Delay:** Whenever a complex box appears, the next spawn is delayed by a configurable multiplier. This gives the player extra time to interact with mechanic-heavy boxes before another one enters the belt.
- **Continuous Difficulty Feed:** Spawn interval and conveyor speed are continuously updated by the adaptive difficulty system, allowing the spawn logic to react dynamically to the current game state instead of fixed difficulty tiers.

<!--
GIF #5 — SMART SPAWN / BRAKING IN ACTION (right after "Smart Spawn Algorithm")
A gif showing several boxes queued close together on the belt, visibly slowing
down as they approach the box ahead of them instead of overlapping, is the
clearest way to show this system working rather than just describing it.
-->
![smart spawn gif placeholder](docs/gifs/smart-spawn-braking.gif)

---

## Performance & Optimization

Performance was a major focus during development. The game avoids unnecessary allocations and minimizes runtime overhead through pooling, cached references, and allocation-free APIs. Because dozens of boxes can be alive on the belt at once, several optimizations keep frame time stable on mobile-class hardware:

- **Object Pooling for Boxes and Particles:** Boxes are never instantiated or destroyed during gameplay. A generic `ObjectPool<T>` pre-warms a batch of boxes at scene load and recycles them via `Get()`/`Release()` as they're sorted or missed. One-shot VFX (correct/wrong particles) go through a second pool (`ParticlePoolManager`, built on Unity's own `IObjectPool<T>`) that returns particles to the pool automatically via `ParticleSystemStopAction.Callback`, so no `Instantiate`/`Destroy` calls happen mid-run.
- **Manual Belt Simulation Instead of Physics:** Boxes on the conveyor are moved with `Vector3.MoveTowards` in a single `Update()` loop rather than relying on Rigidbody forces or physics joints, avoiding per-box physics solver overhead for what is fundamentally 1D motion.
- **O(n log n) Spacing Instead of O(n²):** As described under Smart Spawn Algorithm, the active box list is sorted once per frame by distance-to-end so each box only ever needs to check its single nearest neighbor ahead, rather than every box checking every other box.
- **Squared-Distance Comparisons:** Anti-overlap spacing and drag-speed checks compare squared distances (`sqrMagnitude`) instead of taking square roots every frame, and the square root is only computed once spacing actually needs to be resolved into a speed multiplier.
- **No Per-Frame `Find()` Calls:** Manager references (`GameManager.Instance`, `AudioManager.Instance`, `CameraShaker.Instance`, etc.) are cached singletons assigned once in `Awake()`, so gameplay code never calls `GameObject.Find` or `FindObjectOfType` inside `Update()`.
- **Non-Allocating Overlap Queries:** Container detection under a dropped box uses `Physics.OverlapSphereNonAlloc` into a pre-allocated buffer instead of the allocating `OverlapSphere`, avoiding GC pressure on every single drag release.
- **Event-Driven UI Instead of Polling:** Score, combo, and timer displays subscribe to C# events (`OnScoreChanged`, `OnComboChanged`, `OnTimeChanged`) fired only when a value actually changes, rather than reading and formatting state every frame in `Update()`.

<!--
GIF #6 — OPTIMIZATION SHOWCASE (optional, right after "Performance & Optimization")
If you have the Unity Profiler open, a short gif showing a busy belt (10+ boxes)
running at a stable frame time is a strong, concrete way to back up this section
visually.
-->

---

## Additional Systems

### Save System

`SaveManager` persists the player's best score via `PlayerPrefs` and only overwrites it when the current run actually beats the record, loading it once on startup to display alongside the live score.

### Randomized Container Placement

`ContainerShuffler` randomizes the position of each container at the start of a run (Fisher–Yates style shuffle), so the physical layout of colors can't be memorized between runs.

---

## Technologies

- Unity 6
- C#
- TextMeshPro
- DOTween
- Unity Object Pool API
- PlayerPrefs

---

## How to Run

### Run the Project in Unity

1. Open the project in Unity (2022.3+ recommended).
2. Open the main gameplay scene.
3. Press **Play**.

### Play in Browser (Quick Demo)

*()*

---

## Skills Demonstrated

- Gameplay Programming
- Game Architecture
- State Machines
- Object Pooling
- Performance Optimization
- Event-driven Programming
- UI Programming
- Save Systems
- Design Patterns
