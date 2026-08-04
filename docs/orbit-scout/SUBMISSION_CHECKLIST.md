# Orbit Scout — submission guide

**No live presentation:** your **demo video must answer all rubric and presentation questions.** Use **[DEMO_VIDEO_SCRIPT.md](DEMO_VIDEO_SCRIPT.md)** (8–12 min, full narration).

Use this alongside **README.md** for your summative hand-in.

---

## Deliverables checklist

| Item | Status / action |
|------|-----------------|
| **Public GitHub repo** | Push `Solar Quiz` with `.gitignore`, README, source |
| **README** | How to run editor + AR (below) |
| **Gameplay video** | Record using **DEMO_VIDEO_SCRIPT.md** (includes OOP, Profiler, AR, scope cuts) |
| **GDD link + Performance section** | Paste from `docs/GDD_PERFORMANCE.md` + screenshots + **filled FPS table** |

---

## Record your video

Follow **[DEMO_VIDEO_SCRIPT.md](DEMO_VIDEO_SCRIPT.md)** — one recording covers gameplay, AR, OOP/patterns, Profiler, performance techniques, FPS benchmarks, and scope not achieved.

---

## Presentation cheat sheet (for video narration — not a separate session)

**OOP / patterns**

- **Single responsibility:** `MissionController` = rules/score; `MissionHud` = UI; `SolarSystemView` = 3D build; `PlanetTapInput` = picking  
- **Observer:** HUD subscribes to `MissionController` events (`OnQuestionChanged`, `OnAnswerCorrect`, …)  
- **State:** Menu → (AR placement) → Playing → End  
- **Factory-style:** `SolarSystemView.Build()` creates the whole tabletop solar system at runtime  

**AR interactions**

- Plane detection (`ARPlaneManager`)  
- Raycast placement (`ARRaycastManager` + `ArSessionBridge`)  
- Tap planets in 3D on device; hover labels while finger/cursor over planet  

**Profiler / performance**

- Target: **60 FPS** editor desktop, **30+ FPS** on mid-range phone (your measured number goes in GDD)  
- Techniques: **GPU instancing** + one shared URP material with **MaterialPropertyBlock** colors; optional **IL2CPP** on mobile builds  

**Scope not finished (honest)**

- e.g. “Learn mode with paused orbits”, “NASA textures”, “voice-over clues” — if you didn’t ship them, say so clearly  

---

## GDD

Copy **Performance Considerations** from [docs/GDD_PERFORMANCE.md](GDD_PERFORMANCE.md) into your GDD and add **2 Profiler screenshots** (CPU Usage + Rendering) while playing a mission.

---

## GitHub push (after commit)

```bash
cd "/Users/cikirezi/Solar Quiz"
git checkout -b main  # or merge your feature branch
git add -A
git commit -m "Orbit Scout summative vertical slice"
git remote add origin <your-repo-url>
git push -u origin main
```

---

**Student:** Mwiza Cannelle · **Unity:** 6000.5.0f1
