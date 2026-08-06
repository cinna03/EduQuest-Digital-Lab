# Solar Quiz (Orbit Scout)

Educational **AR solar-system quiz** for Unity — place a tabletop solar system on a real surface, answer planet clues by tapping 3D planets, with scoring, unlocks, and four mission styles.

**Unity:** 6000.5.0f1  
**Student:** Cannelle Mwiza · African Leadership University  

This vertical slice implements the scope in the Game Design Document: AR placement, mission loop, tap-to-answer, and editor testing path.

---

## Requirements

- Unity **6000.5.0f1** (or compatible Unity 6)  
- For device builds: AR Foundation + ARKit (iOS) or ARCore (Android) enabled in XR Plug-in Management  

---

## How to run — Editor (desktop)

1. Clone this repository and open the folder in Unity Hub.  
2. Open **`Assets/Scenes/OrbitScout_EditorTest.unity`**.  
   - If missing: menu **Orbit Scout → Create Editor Test Scene**, then save.  
3. Press **Play**.  
4. **Play** on the main menu → choose a mission → **hover** planets for names → **click/tap** to answer.  
5. Finish a level to see the end screen (retry / level select / menu).

---

## How to run — AR on device

1. Open **`Assets/Scenes/SampleScene.unity`**.  
2. Menu: **Orbit Scout → Setup AR In Active Scene** → **save the scene**.  
3. **File → Build Settings**: enable **SampleScene** (first in list) → switch platform to iOS or Android → Build and Run.  
4. On device: allow camera → choose mission → scan a well-lit floor/table → **tap** to place the solar system → tap planets to answer.

---

## Project structure

| Path | Role |
|------|------|
| `Assets/OrbitScout/Core/` | Mission rules, levels, progress, quiz content |
| `Assets/OrbitScout/View/` | Solar system build, planets, hover labels |
| `Assets/OrbitScout/Tapping/` | Planet pick / pointer input |
| `Assets/OrbitScout/UI/` | HUD / menus |
| `Assets/OrbitScout/Platform/` | Editor vs AR bootstrap, AR bridge |
| `Assets/Editor/` | Orbit Scout setup menus |
| `Assets/Scenes/` | `OrbitScout_EditorTest`, `SampleScene` |
| `docs/` | Submission hub, presentation cheat sheet, performance notes |

---

## Architecture (for graders / presentation)

- **Single responsibility:** `MissionController` (rules), `MissionHud` (UI), `SolarSystemView` (3D), `PlanetTapInput` (input), `SolarBootstrap` (session).  
- **Observer:** HUD listens to mission events.  
- **Factory-style:** `SolarSystemView.Build()` creates the tabletop system at runtime.  

**Performance:** shared URP material with **GPU instancing** + **MaterialPropertyBlock** colors in `SolarSystemView` (see `docs/GDD_PERFORMANCE.md`).

---

## Docs for summative

- [docs/SUMMATIVE_SUBMISSION_HUB.md](docs/SUMMATIVE_SUBMISSION_HUB.md) — links template  
- [docs/LIVE_PRESENTATION_CHEATSHEET.md](docs/LIVE_PRESENTATION_CHEATSHEET.md) — live Q&A  
- [docs/DEMO_VIDEO_SCRIPT.md](docs/DEMO_VIDEO_SCRIPT.md) — video recording script  
- [docs/GDD_PERFORMANCE.md](docs/GDD_PERFORMANCE.md) — paste into GDD  

---

## License

Unity template / sample assets remain under Unity’s license. Coursework scripts in `Assets/OrbitScout/` are student project work.
