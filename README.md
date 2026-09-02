# Box Sort - Conveyor Puzzle Game (Unity)

A fast-paced Unity arcade game where players sort colored boxes on a moving conveyor belt while managing combo multipliers, special box mechanics, and an adaptive difficulty system designed to keep every run engaging.

<img width="1280" height="720" alt="ezgif com-video-to-gif-converter (9)" src="https://github.com/user-attachments/assets/8e443b5b-0f83-40b9-bdf7-c4703147f93a" />

## Technical Highlights

- **Finite-state game flow** (`IGameState` + `GameStateMachine`) driving Menu → Playing → Win/Lose, with each state owning its own enter/tick/exit logic instead of scattered boolean flags.
- **Modular manager-based architecture** — Score, Combo, Timer, Level, Save, Audio, and Conveyor each own a single responsibility and communicate through cached references and C# events, never through `Find()` calls.
- **Adaptive difficulty curve** driven by a single continuous formula (score → `Mathf.Lerp`/`Clamp01`), not discrete difficulty steps or level-based `if` chains.
- **Custom smart-spawn algorithm** that prevents box overlap, throttles "trick" boxes, and brakes queued boxes using an O(n log n) distance-sorted pass instead of pairwise checks.
- **Zero runtime `Instantiate`/`Destroy`** for gameplay objects — boxes and particles are fully pooled (`ObjectPool<T>`, Unity's `IObjectPool<T>`).
- **GC-conscious code throughout**: `OverlapSphereNonAlloc`, squared-distance comparisons, cached singletons, event-driven UI instead of per-frame polling.
- 
---

## Game Concept

Sort colored boxes into matching containers before they reach the end of the conveyor. Build combos, survive increasing difficulty, and deal with special box mechanics that require unique interactions.

<img width="1280" height="720" alt="ezgif com-video-to-gif-converter (10)" src="https://github.com/user-attachments/assets/d7ab6504-ea32-4b6d-9011-463c2e779ab8" />

---

## Key Features

| Feature | Description |
|---------|-------------|
| **Adaptive Difficulty** | Belt speed and spawn rate scale smoothly with score via `Mathf.Lerp` |
| **Combo System** | Consecutive correct sorts build a hit streak; multiplier increases up to a cap |
| **Special Boxes** | Sealed (click to unwrap), Frozen (click-and-hold), Glass (shatters if moved too fast) |
| **Bonus Boxes** | Rare time/score bonuses that scale with current combo |
| **Freeze Ability** | Player-triggered belt halt on cooldown |
| **Randomized Containers** | Fisher–Yates shuffle at run start — no memorization |
| **Juicy Feedback** | Squash-and-stretch tweens, camera shake, screen flash, floating score text |

<img width="600" height="337" alt="ezgif com-video-to-gif-converter (13)" src="https://github.com/user-attachments/assets/a90a4815-5d9c-43d4-b491-4078e359cc30" />

<img width="600" height="337" alt="ezgif com-video-to-gif-converter (14)" src="https://github.com/user-attachments/assets/a3a4ff51-767b-4872-9d13-721f05013b0f" />

<img width="600" height="337" alt="ezgif com-video-to-gif-converter (15)" src="https://github.com/user-attachments/assets/c9f54ced-2019-473d-906b-2695eadcaf6e" />

<img width="227" height="125" alt="ezgif com-crop" src="https://github.com/user-attachments/assets/ac185bdc-e551-4fbd-bef7-2e1b4274e4de" />

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

---

## Architecture

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

- **`IGameState` / `GameStateMachine`:** A tiny interface (`Enter`/`Tick`/`Exit`) plus a driver that calls the current state's `Tick()` every frame. This keeps "what happens when the game starts/ends" in one place per state, instead of `if (isPlaying)` checks scattered across scripts.
- **`GameManager` as composition root:** All manager references live on `GameManager` and are assigned once in the Inspector. States talk to each other *through* `GameManager.Instance` rather than searching the scene, so the dependency graph is explicit.
- **Events over polling:** Managers expose C# events (`OnScoreChanged`, `OnComboChanged`, `OnTimeChanged`) instead of public getters that UI would poll every frame. `UIManager` and `JuicyUI` subscribe once in `Start()`.
- **Single entry point for game rules:** Cross-cutting rules like "what happens on a correct sort" live as one method each on `GameManager` (`OnCorrectSort`, `OnWrongSort`, `OnBoxMissed`), so `Container`, `EndPointTrigger`, and `Box` all report *outcomes* upward rather than each implementing scoring logic themselves.

---

## Design Decisions

### Why a state machine instead of boolean flags?
Early prototype used `bool isPlaying`, `bool isPaused`, `bool gameOver` scattered across 4 scripts. Debugging was painful — a race condition between `isPlaying` and `gameOver` caused the win panel to appear twice. Switched to `IGameState` with `Enter`/`Tick`/`Exit`. Now only one state exists at a time, transitions are explicit, and bugs are localized to a single class.

### Why manual belt movement instead of Rigidbody physics?
Initially moved boxes with `Rigidbody.AddForce` on a physical conveyor belt. At 20+ boxes, the physics solver caused jitter and non-deterministic ordering — boxes would collide, rotate, and block each other in unpredictable ways. Switched to `Vector3.MoveTowards` in a single `Update()` loop. Boxes now move in perfect 1D order, collision-free, at a fraction of the CPU cost.

### Why O(n log n) distance sort instead of O(n²) pairwise checks?
First implementation checked every box against every other box for anti-overlap spacing. At 25 boxes that's 625 checks per frame. Switched to sorting the active box list once per frame by distance-to-end (`O(n log n)`), so each box only checks its single nearest neighbor ahead. 25 boxes → ~25 checks. Measurable CPU savings on mobile.

### Why two difficulty axes (continuous + discrete)?
A single difficulty curve (just speed) feels either too easy or too brutal. I split it into two independent axes: (1) continuous belt speed/spawn rate via `Mathf.Lerp` for smooth pressure, and (2) discrete mechanic unlocks (Sealed → Frozen → Glass) for complexity spikes. Early runs are fast but simple; late runs are fast *and* complex. Playtesters reported the curve felt "fair but tense."

### Why event-driven UI instead of polling?
First version had `UIManager` reading `ScoreManager.CurrentScore` every frame in `Update()`. With 6 UI elements polling 4 managers, that's 24 redundant reads per frame. Switched to C# events — UI updates only when data actually changes. Eliminated per-frame string formatting and reduced UI-related CPU time by roughly 60% (measured via Unity Profiler).

---

## Adaptive Difficulty

Difficulty scales continuously based on player score using interpolation rather than predefined tiers.

- At score `0`: belt runs at `baseTravelSpeed` (1.5 units/s), spawns every `baseSpawnInterval` (1.5s).
- As score approaches `scoreForMaxDifficulty` (500): both values lerp toward `maxTravelSpeed` (4.0 units/s) and `minSpawnInterval` (0.6s).
- Past 500, `Clamp01` holds the difficulty at maximum — the belt never becomes literally unplayable.

Because this is one `Lerp` per axis rather than a lookup table, the ramp feels smooth instead of stepped, and both curves can be re-tuned by adjusting four public fields — no code changes required.

Separately, `LevelManager.GetRandomMechanicForLevel()` layers a second, coarser difficulty axis: which "trick" mechanics are allowed to spawn increases as the player's level rises, so early runs stay simple while the continuous speed curve ramps up underneath.

---

## Smart Spawn Algorithm

Spawning isn't just "instantiate a random box on a timer" — `ConveyorSpawner` actively manages what's allowed on the belt:

- **Anti-Overlap Braking:** Active boxes continuously adjust speed based on distance to the box ahead. When too close, speed is smoothly reduced via `Mathf.Lerp`, creating natural queues instead of physics collisions.
- **Complex-Box Throttling:** Before assigning a special mechanic, the spawner checks how many complex boxes are already active. If the limit is reached, the new box spawns as normal instead.
- **Compensating Spawn Delay:** Whenever a complex box appears, the next spawn is delayed by a configurable multiplier, giving the player time to interact before another one enters.
- **Continuous Difficulty Feed:** Spawn interval and conveyor speed update every frame from the adaptive difficulty system, so spawn logic reacts dynamically instead of using fixed tiers.

<img width="800" height="450" alt="ezgif com-video-to-gif-converter (16)" src="https://github.com/user-attachments/assets/7b0a1d1a-7d8d-4ca6-afd6-c7304465a727" />

---

## Performance & Optimization

Because dozens of boxes can be alive at once, several optimizations keep frame time stable on mobile hardware:

| Technique | Problem | Solution |
|-----------|---------|----------|
| **Object Pooling** | `Instantiate`/`Destroy` causes GC spikes | Generic `ObjectPool<T>` for boxes; `IObjectPool<T>` for particles with auto-return via `ParticleSystemStopAction.Callback` |
| **Manual Belt Simulation** | Rigidbody physics = jitter + non-determinism | `Vector3.MoveTowards` in single `Update()` loop — 1D motion without solver overhead |
| **O(n log n) Spacing** | O(n²) pairwise checks = 625 ops at 25 boxes | Distance-sorted list, each box checks only nearest neighbor ahead |
| **Squared Distance** | `Vector3.Distance` uses `Sqrt` every frame | Compare `sqrMagnitude` instead; `Sqrt` only when resolving speed multiplier |
| **Cached Singletons** | `FindObjectOfType` in `Update()` is expensive | All managers cached in `Awake()`, accessed via `.Instance` |
| **Non-Alloc Overlap** | `OverlapSphere` allocates every drag release | `OverlapSphereNonAlloc` into pre-allocated buffer |
| **Event-Driven UI** | Per-frame polling wastes CPU | C# events fire only on change; UI subscribes once in `Start()` |

---

## What I Learned

- **State machines are debugging superpowers:** After switching from bool flags to `IGameState`, I stopped getting "why is the win panel showing twice" bugs. The cost of one extra class per state pays for itself in the first week.
- **Physics is overkill for 1D motion:** `Rigidbody` felt "correct" conceptually, but for conveyor movement it added collision chaos, rotation drift, and solver cost with zero gameplay benefit. Manual translation was the right call.
- **Two difficulty curves > one:** Playtesters tolerated higher speeds when mechanics were simple, and tolerated complex mechanics when speed was moderate. Combining both created "fair but tense" sessions. A single curve would have needed to be gentler, making late game boring.
- **Pooling is non-negotiable for arcade:** Even on PC, `Instantiate` during gameplay caused 80ms GC spikes every ~30 seconds. After pooling, frame time graph became a flat line. I now default to pooling for any object spawned more than twice per session.
- **Event-driven UI scales:** At 6 UI elements polling 4 managers, the profiler showed UI taking 4% of frame time. After events — 0.3%. On a 60 FPS budget, that's the difference between headroom and stutter.
- **Fisher–Yates is underrated:** Shuffling container positions took 10 lines and prevented "muscle memory wins" completely. Simple randomization can have outsized impact on replayability.

---

## Tech Stack

- Unity 2022.3 LTS (also tested on Unity 6)
- C#
- TextMeshPro
- DOTween
- Unity Object Pool API (`IObjectPool<T>`)
- PlayerPrefs

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

## How to Run

### Run the Project in Unity
1. Open the project in Unity (2022.3+ recommended).
2. Open the main gameplay scene.
3. Press **Play**.

### Play in Browser (Quick Demo)
You can play the fully functional WebGL demo directly in your browser without downloading anything:
**[Box Sort - Conveyor Puzzle Demo on itch.io](https://vertexvrtx.itch.io/box-sort-conveyor-puzzle-game)**
