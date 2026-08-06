# EduQuest Digital Lab — Solar Quiz (Orbit Scout)

Educational **AR solar-system quiz** built in Unity. Place a tabletop solar system on a real surface, read mission clues, and answer by **tapping 3D planets**. The app includes scoring, level unlocks, and four mission styles.

**Unity:** 6000.5.0f1  
**Author:** Cannelle Mwiza · African Leadership University  
**Repository:** https://github.com/cinna03/EduQuest-Digital-Lab

---

## How the game works

1. **Main menu** — Start a journey or reset progress.
2. **Level select** — Pick Mission I–IV (later missions unlock after passing earlier ones).
3. **Briefing** — Short mission intro, then play begins.
4. **Play**
   - **On device (AR):** Scan a flat surface → tap to place the solar system → read the clue → tap the matching planet.
   - **In the editor:** The system spawns for fast testing without a phone; hover planets for names, click to answer.
5. **Feedback** — Correct/wrong responses update score and mission rules (timers, greyscale restore, multi-select, gauntlet windows).
6. **Results** — Stars, score, and summary; retry, level select, or main menu.

### Mission styles

| Mission | Focus | Rules (summary) |
|---------|--------|------------------|
| I — First Orbit | Planet basics | Fact clues; tap the matching planet |
| II — Save the Planets | Visual restore | Greyscale planets; correct taps restore color |
| III — Shared Traits | Multi-select | Timed; select all planets that match a trait |
| IV — Gauntlet | Final exam | Short read + answer windows; strict pass threshold |

---

## Requirements

- Unity **6000.5.0f1** (Unity 6, URP)
- **iOS/Android builds:** AR Foundation with ARKit or ARCore enabled under **Project Settings → XR Plug-in Management**

---

## How to run — Editor (desktop)

1. Clone the repo and open the project in Unity Hub.
2. Open **`Assets/Scenes/OrbitScout_EditorTest.unity`**.  
   If it is missing: **Orbit Scout → Create Editor Test Scene**, then save.
3. Press **Play** → **Play** on the menu → choose a mission.
4. Hover planets for names; click/tap to answer. Finish the mission to see the results screen.

---

## How to run — AR on device

1. Open **`Assets/Scenes/SampleScene.unity`**.
2. **Orbit Scout → Setup AR In Active Scene** → save the scene.
3. **File → Build Settings** — add **SampleScene** first, switch to iOS or Android, **Build and Run**.
4. On device: allow camera → pick a mission → scan a lit floor or table → tap to place → tap planets to answer.

**iOS signing:** Use a bundle ID already registered on your Apple Personal Team if you hit the free App ID limit (reuse an existing ID from a prior Unity build).

---

## Project structure

| Path | Role |
|------|------|
| `Assets/OrbitScout/Core/` | `MissionController`, levels, quiz bank, `GameProgress` |
| `Assets/OrbitScout/View/` | `SolarSystemView`, planet bodies, matcap materials, orbits |
| `Assets/OrbitScout/Tapping/` | `PlanetTapInput`, pointer helpers |
| `Assets/OrbitScout/UI/` | HUD prefab, `MissionHud`, menus, glass UI theme |
| `Assets/OrbitScout/Platform/` | `SolarBootstrap`, AR session bridge, scene entry |
| `Assets/Editor/` | Orbit Scout menu tools (setup AR, HUD, editor test scene) |
| `Assets/Resources/OrbitScout/` | Runtime HUD prefab, planet matcaps, UI sprites |
| `Assets/Scenes/` | `OrbitScout_EditorTest` (desktop), `SampleScene` (AR build) |

---

## Architecture

Responsibilities are split so gameplay, presentation, and platform code stay testable.

| Component | Responsibility |
|-----------|----------------|
| `SolarBootstrap` | Editor vs AR play mode; starts/ends play sessions |
| `ArSessionBridge` | AR plane raycasts and placement on device |
| `MissionController` | Level rules, timers, score, questions, end-of-run results |
| `LevelCatalog` / quiz bank | Clue content and level configuration |
| `GameProgress` | Unlocks and high scores (persisted) |
| `SolarSystemView` | Builds sun, planets, rings at runtime |
| `PlanetTapInput` | Raycasts taps to `PlanetBody` |
| `MissionHud` / `OrbitScoutHudView` | Menus, play HUD, results; wires buttons to flow |

**Design patterns**

- **Single responsibility** — rules live in `MissionController`; UI does not mutate score directly.
- **Observer** — `MissionHud` subscribes to mission events (`OnQuestionChanged`, `OnScoreChanged`, `OnLevelEnded`, …).
- **Factory-style build** — `SolarSystemView.Build()` creates the full tabletop system when a session starts.

**Typical data flow**

`SolarBootstrap` → (AR) `ArSessionBridge` placement → `MissionController` starts level → `SolarSystemView` spawns bodies → player taps via `PlanetTapInput` → controller updates score/state → HUD refreshes from events.

**Rendering / performance (prototype)**

- Matcap shaders (`OrbitScout/PlanetSurface`, `PlanetRings`) with textures under `Resources/OrbitScout/Planets/`.
- Materials reused per body type; per-planet tint/saturation via **MaterialPropertyBlock**.
- Lightweight primitive meshes and small AR tabletop scale.
- Mobile builds use **IL2CPP** on iOS; custom shaders are included for player builds so planets and UI glass render on device.

---

## Editor menu shortcuts

| Menu item | Purpose |
|-----------|---------|
| **Orbit Scout → Create Editor Test Scene** | Desktop test scene with HUD |
| **Orbit Scout → Setup AR In Active Scene** | AR components on `SampleScene` |
| **Orbit Scout → Create Editable HUD In Scene** | Rebuild/edit HUD prefab in scene |

---

## License

Unity template and sample assets remain under Unity’s license. Coursework scripts under `Assets/OrbitScout/` are student project work.
