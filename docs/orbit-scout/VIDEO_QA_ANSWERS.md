# Solar Quiz — Video Q&A sheet (exact answers)

Use in **Part C** of the video. Put the **question title on screen**, then read the answer.

---

## Q1 — What OOP principles and design patterns did you use to ensure code quality?

**Answer:**

I structured Solar Quiz under `Assets/OrbitScout` with clear responsibilities.

**OOP / SOLID in practice**
- **Single responsibility:** `MissionController` owns rules, score, and timers. `MissionHud` only shows UI. `SolarSystemView` builds the 3D solar system. `PlanetTapInput` handles picking. `SolarBootstrap` chooses editor vs AR and starts the session.
- **Encapsulation:** the HUD does not write the score directly; it reacts to the controller.

**Design patterns**
- **Observer:** `MissionController` raises events (`OnQuestionChanged`, `OnScoreChanged`, `OnLevelEnded`, …). `MissionHud` subscribes, so UI can change without rewriting mission logic.
- **Factory-style construction:** `SolarSystemView.Build()` creates the full tabletop system from data at runtime.
- **State flow:** Menu → level select → AR placement → playing → end screen.

That keeps the code modular and easy to extend.

---

## Q2 — Demonstrate the use of Unity Profiler to identify a performance roadblock

**Answer (while Profiler is open):**

I’m profiling during active gameplay — Window → Analysis → Profiler — CPU and Rendering modules.

**Roadblock I designed against:** creating a unique material per planet, which increases CPU/GPU cost and breaks batching.

**What we do instead:** in `SolarSystemView`, all planets share **one URP material** with **GPU instancing**, and colors use a **MaterialPropertyBlock**.

Planets are **lightweight primitives** (no heavy NASA textures this slice).

**FPS benchmark:** Editor about **____ FPS**. On **________** (device) about **____ FPS** during AR. Those numbers are in my GDD Performance section with screenshots.

Also: small **tabletop scale**, **IL2CPP** on mobile. I did **not** use occlusion culling — the system is small and always visible. LOD isn’t needed for low-poly spheres.

---

## Q3 — Which part of the scope identified earlier were you not able to achieve?

**Answer:**

From ideation and GDD, **not** shipped this trimester:

- Photoreal **NASA textures** / high-fidelity models  
- **Voice narration** / rich audio  
- **Multiplayer**  
- Full **EduQuest** multi-subject product + **LMS integration**  
- **App Store** release + full **accessibility** audit  

**Shipped:** AR plane placement, four mission levels with unlock rules, tap-to-answer, full UI flow, editor test path, performance-aware materials. Cuts stay for capstone / EduQuest expansion.

---

## Q4 — What AR/VR specific interactions were included?

**Answer:**

1. **Plane detection** — `ARPlaneManager` finds real floors/tables.  
2. **Raycast placement** — `ARRaycastManager` + `ArSessionBridge`; tap the plane to place the solar system.  
3. **World-space tabletop content** — walk around the anchored model.  
4. **Touch selection** of 3D planets to answer clues.  
5. **Live camera during play** with HUD; solid backdrop on menus for readability.

---

## Also say once (not numbered, but rubric cares)

**Passing from one level to the next**

| Level | How you win / unlock next |
|-------|---------------------------|
| I First Orbit | Need **5/8** correct → unlocks Level 2 |
| II Save the Planets | **Save 3** planets fully → unlocks Level 3 |
| III Shared Traits | Need **7/10** within **10 min** → unlocks Level 4 |
| IV Gauntlet | Need **5/5** (10s read + 10s answer each) → mastered |

Fail = end screen shows what you needed; previous level stays unlocked; next stays locked until you pass.

---

## Short “feature pass” lines for Part A (phone demo)

While playing, you can say:

- “Level cards tell you the fantasy and the pass rule.”  
- “During play, the HUD shows the win condition for this mission.”  
- “On the end screen, if I passed, it says the next level is unlocked.”
