# Solar Quiz — Live presentation cheat sheet (5 pts)

Keep this beside you during the live presentation. Aim **6–8 minutes** demo + Q&A.

---

## Opening (30 s)
“I’m Cannelle Mwiza. **Solar Quiz** is my AR vertical slice for the specialization. It matches my GDD: place a tabletop solar system in the room and answer planet clues by tapping 3D planets — discovery learning toward my EduQuest mission.”

---

## Demo order (show, don’t only talk)
1. **Editor** (`OrbitScout_EditorTest`): menu → level select → Level 1 clue → hover → wrong tap (optional) → correct tap → end screen.  
2. **AR** (phone or recording): scan → place → one question.  
3. **Code peek:** Hierarchy `OrbitScout` + one script (`MissionController` or `SolarSystemView`).  
4. **Profiler:** Play + Profiler CPU/Rendering; point at FPS and one technique.

---

## Rubric → what to prove

| Rubric area | Prove by… |
|-------------|-----------|
| Implementation / GDD | Demo matches flowchart: menu → place → quiz → end |
| Code quality | Name SRP + Observer + Build factory |
| GitHub / README | Flash repo + README run steps |
| Video | Already submitted; mention it covers full walkthrough |
| Performance | GDD FPS number + Profiler + GPU instancing / MPB |
| Live presentation | Clear demo + answer the 4 questions below |

---

## Exact answers to likely questions

**Q: OOP principles and design patterns?**  
“SOLID focus on single responsibility: rules, UI, view, and input are separate classes. Observer pattern: the HUD listens to `MissionController` events instead of polling. Factory-style creation: `SolarSystemView.Build` constructs the solar system at runtime. That keeps the code modular and testable.”

**Q: Unity Profiler roadblock?**  
“I profiled during an active mission. [Say what you saw — e.g. Rendering or Scripts.] To keep cost down I use one shared URP material with GPU instancing and MaterialPropertyBlock colors so eight planets don’t create eight materials. Editor target is **60 FPS**; on device I measured **[YOUR NUMBER]** FPS.”

**Q: Scope not achieved?**  
“From the GDD out-of-scope list I did not ship NASA textures, voice clues, multiplayer, LMS integration, or store release. Those stay for capstone / EduQuest expansion. The vertical slice prioritizes AR place + four mission styles + tap answers.”

**Q: AR/VR interactions?**  
“Plane detection with ARPlaneManager, placement with ARRaycastManager through ArSessionBridge, then world-space taps on PlanetBody objects. On phone the live camera shows during play; menus use a solid backdrop so UI stays readable.”

---

## Closing (15 s)
“Source is on GitHub with README. GDD link includes Performance Considerations with Profiler screenshots and FPS benchmarks. Happy to take questions.”
