# Solar Quiz — Summative submission hub

**Student:** Cannelle Mwiza · African Leadership University · AR/VR Specialization  
**App:** Solar Quiz (in-engine folder name: Orbit Scout)  
**Unity:** 6000.5.0f1  

Paste this into a **Google Doc**, fill the links, share as **Anyone with the link → Viewer**, submit that URL.

---

## Deliverable links (fill these)

| Deliverable | Link |
|-------------|------|
| **GitHub repository (public)** | [PASTE] |
| **Gameplay / experience video** | [PASTE YouTube unlisted/public or Drive] |
| **Game Design Document (updated)** | [PASTE — must include Performance Considerations + FPS + Profiler screenshots] |

---

## GDD alignment (what we committed → what shipped)

| Ideation / GDD commitment | Status in prototype |
|---------------------------|---------------------|
| AR plane place solar system | ✅ `SampleScene` + `ArSessionBridge` |
| Mission levels (clues, score, timers) | ✅ Levels I–IV via `MissionController` / `LevelCatalog` |
| Tap planets in 3D to answer | ✅ `PlanetTapInput` |
| UI: menu → level select → play → results | ✅ `MissionHud` / `OrbitScoutHudView` |
| Editor test path | ✅ `OrbitScout_EditorTest` |
| Educational / EduQuest mission link | ✅ Astronomy discovery lab vertical slice |
| NASA textures / voice / LMS / store | ❌ Deliberate out of scope (say in presentation) |

---

## How to run (also in README)

### Editor (desktop)
1. Open project in **Unity 6000.5.0f1**.  
2. Open `Assets/Scenes/OrbitScout_EditorTest.unity` (or **Orbit Scout → Create Editor Test Scene** once).  
3. **Play** → **Play** → choose level → hover planets → tap to answer.

### AR (phone)
1. Open `Assets/Scenes/SampleScene.unity`.  
2. **Orbit Scout → Setup AR In Active Scene** → save.  
3. Build Settings: SampleScene first → iOS/Android.  
4. On device: choose mission → scan → tap to place → quiz.

---

## Performance (must match GDD section)

**FPS benchmark target: 60 FPS** (editor); **≥ 30 FPS** (phone AR — fill measured value).

Techniques implemented / documented:
1. **GPU instancing** + shared URP material (`SolarSystemView`)  
2. **MaterialPropertyBlock** per-planet color (no material spam)  
3. Lightweight primitive meshes  
4. **IL2CPP** recommended for mobile builds  
5. Occlusion culling / LOD / texture atlas — **not used**; justified in GDD  

*Attach Profiler CPU + Rendering screenshots in the GDD.*

---

## Presentation question cheat sheet

### 1. OOP / design patterns
- **Single responsibility:** `MissionController` (rules), `MissionHud` (UI), `SolarSystemView` (3D), `PlanetTapInput` (input), `SolarBootstrap` (session/mode).  
- **Observer:** HUD subscribes to mission events (`OnQuestionChanged`, `OnScoreChanged`, `OnLevelEnded`, …).  
- **Factory-style:** `SolarSystemView.Build()` creates the tabletop system at runtime.  
- **State flow:** Menu → Level select → (AR place) → Playing → End.

### 2. Profiler roadblock
Open **Window → Analysis → Profiler** during Play. Call out e.g. Rendering/CPU spikes from AR planes or UI; show that shared instanced materials keep draw cost manageable.

### 3. Scope not achieved
- Photoreal NASA textures, voice-over clues  
- Full EduQuest multi-subject product / LMS  
- App Store release / full accessibility audit  

### 4. AR/VR interactions
- Plane detection (`ARPlaneManager`)  
- Raycast placement (`ARRaycastManager` + `ArSessionBridge`)  
- World-space planet taps; hover name labels (editor)  
- Live camera during play; solid menu presentation on device  

---

## Pre-submit checklist

- [ ] Repo public + `.gitignore` excludes `Library/`  
- [ ] README explains editor + AR  
- [ ] Video shows menu, at least one full level (editor), and AR place + tap  
- [ ] GDD Performance section: techniques + **FPS numbers filled** + 2 screenshots  
- [ ] Practice 4 presentation answers out loud (~2 min each)
