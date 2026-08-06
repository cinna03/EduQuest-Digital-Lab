# Solar Quiz — Demo & code walkthrough video script  
### Replaces live presentation — every exam question answered on camera

Record **one video (recommended 10–12 minutes)**. There is **no live presentation**, so this recording must answer **all** rubric presentation questions out loud (use the **on-screen question titles** below so graders cannot miss them).

**Before recording**

- [ ] `OrbitScout_EditorTest`: Play → menu → level → hover → tap wrong → tap correct → end screen  
- [ ] `SampleScene` saved after **Orbit Scout → Setup AR In Active Scene** (or show phone AR recording)  
- [ ] Profiler ready (CPU + Rendering modules)  
- [ ] FPS numbers filled in GDD Performance section — **say the same numbers** in the video  
- [ ] GitHub URL + GDD URL ready for title/outro cards  

---

## Question → timestamp map (must hit all four)

| # | Presentation question | Video section | Approx. time |
|---|------------------------|---------------|--------------|
| **Q1** | What OOP principles and design patterns did you use? | Code walkthrough | ~5:30 |
| **Q2** | Demonstrate Unity Profiler to identify a performance roadblock | Profiler segment | ~7:00 |
| **Q3** | Which part of the earlier scope were you not able to achieve? | Scope gaps | ~8:45 |
| **Q4** | What AR/VR specific interactions were included? | AR segment | ~3:30 |

Also cover without a numbered “Q”: gameplay/GDD alignment, GitHub/README, performance techniques + **FPS benchmark**.

---

## Suggested timeline

| Time | Section | Rubric / Q |
|------|---------|------------|
| 0:00–0:50 | Title + GDD alignment | Implementation |
| 0:50–3:20 | Editor gameplay | Implementation / interactivity |
| 3:20–5:20 | **Q4 — AR interactions** | AR + immersion |
| 5:20–7:00 | **Q1 — OOP & patterns** | Code quality |
| 7:00–8:40 | **Q2 — Profiler + performance** | Performance (5 pts) |
| 8:40–9:40 | **Q3 — Scope not achieved** | Honesty / GDD |
| 9:40–10:30 | GitHub README + outro | Repo docs |

---

## 0:00 — Title card

**On screen:**  
**Solar Quiz** — Educational AR Solar System Quiz  
Cannelle Mwiza · Unity 6000.5.0f1  
GitHub: `[URL]` · GDD: `[URL]`  
*This video replaces the live presentation and answers all evaluation questions.*

**Say:**  
“Hi, I’m Cannelle Mwiza. This is my summative AR vertical slice, **Solar Quiz**. It matches my Game Design Document: place a tabletop solar system in the learner’s space and answer planet clues by tapping 3D planets. Because there is no live presentation, this walkthrough also answers the course evaluation questions on OOP, Profiler, scope gaps, and AR interactions.”

---

## 0:50 — GDD alignment + editor gameplay (~2.5 min)

**Show:** GDD objectives (brief) → then `OrbitScout_EditorTest` → Play.

**Do:**
1. Main menu → **Play** → level select → start **Level 1** (or another short level).  
2. Read one clue aloud.  
3. **Hover** a planet — name label.  
4. **Wrong tap** once — show feedback.  
5. **Correct tap** — score / feedback.  
6. Reach **end screen** (or show end after a few questions if long).

**Say:**  
“The GDD core loop is menu, mission select, optional AR place, clue, tap planet, feedback, then results. In the editor I can test the full quiz without a phone. Interactions are spatial: hover to identify, tap to answer, with UI driven by game events.”

---

## 3:20 — Q4 card + AR segment (~2 min)

**On screen big title (3 seconds):**  
**Q4 — What AR/VR specific interactions were included?**

**Show:** Phone recording preferred (or SampleScene on device / clear phone mirror).

**Do:**
1. Launch → choose mission.  
2. Show **plane detection** (move device over floor/table).  
3. **Tap** to **place** solar system.  
4. One clue → **tap a planet**.  
5. Move around the placed system (immersion).

**Say (read clearly):**  
“Answering Q4 — AR/VR interactions in Solar Quiz:  
First, **plane detection** with AR Foundation’s plane manager finds real horizontal surfaces.  
Second, **raycast placement** via the raycast manager and my `ArSessionBridge` lets the player tap the plane to anchor the solar system.  
Third, the experience is **world-space** and **tabletop-scaled** so you can walk around it.  
Fourth, **touch input** selects planets for answers; in editor I also use hover labels for readability.  
Fifth, during play the **live camera** stays visible behind a translucent HUD, while menus use a solid backdrop so UI stays readable.  
Those are the AR-specific interactions for this vertical slice.”

---

## 5:20 — Q1 card + code walkthrough (~1.5–2 min)

**On screen big title:**  
**Q1 — What OOP principles and design patterns did you use?**

**Show:** `Assets/OrbitScout/` folders, then briefly open:
1. `MissionController.cs` — events / level start  
2. `MissionHud.cs` — subscribe to events  
3. `SolarSystemView.cs` — `Build()` + shared material  

**Say (read clearly):**  
“Answering Q1 — OOP and design patterns.  

**Single responsibility:** `MissionController` owns rules, score, and timers. `MissionHud` only shows UI and forwards buttons. `SolarSystemView` builds the 3D solar system. `PlanetTapInput` handles picking. `SolarBootstrap` chooses editor versus AR and starts the session.  

**Encapsulation:** UI does not write score directly; it reacts to controller events.  

**Observer pattern:** `MissionController` raises events such as `OnQuestionChanged`, `OnScoreChanged`, and `OnLevelEnded`. The HUD subscribes, so I can change UI without rewriting mission logic.  

**Factory-style construction:** `SolarSystemView.Build` creates the full tabletop system from data at runtime.  

**Clear state flow:** Menu → level select → optional AR placement → playing → end screen.  

Together that follows SOLID ideas in practice: modular folders, one owner per concern, and dependencies pointing toward Core.”

---

## 7:00 — Q2 card + Profiler & performance (~1.5–2 min)

**On screen big title:**  
**Q2 — Demonstrate the Unity Profiler to identify a performance roadblock**

**Show:** Enter Play → **Window → Analysis → Profiler** → capture ~10 seconds while planets orbit and you tap. Switch **CPU** then **Rendering**. Optionally flash **Player Settings → Scripting Backend → IL2CPP**.

**Say (read clearly):**  
“Answering Q2 — I am demonstrating the Unity Profiler during an active mission.  

Looking at **CPU**, time is spent on the main thread for scripts, UI, and the scene update. A potential roadblock in a naive design would be creating a unique material per planet every frame or every spawn, which increases cost and break batching.  

In Solar Quiz I avoided that: in `SolarSystemView` all planet bodies share **one URP material** with **GPU instancing** enabled, and each planet gets its color through a **MaterialPropertyBlock**. That is batching awareness and fewer material variants.  

On **Rendering**, draw cost stays manageable because planets are **lightweight primitive meshes** with solid colors—no heavy NASA textures in this slice.  

**FPS benchmark:** In the Editor test scene I measured **[FILL, e.g. ~60] FPS**. On **[device name]** in AR I measured **[FILL] FPS** during placement and quiz. Those numbers are written in my GDD Performance Considerations section with Profiler screenshots.  

Other techniques: small **tabletop scale** in AR, and **IL2CPP** for mobile builds. I did **not** use **occlusion culling**—the solar system is small and always in view, so the setup cost is not justified. **LOD** is unnecessary for low-poly spheres. UI fonts and default assets use normal import settings with mipmaps where textures exist; planets themselves avoid texture sampling cost by using colors.”

---

## 8:40 — Q3 card + scope not achieved (~1 min)

**On screen big title:**  
**Q3 — Which part of the scope identified earlier were you not able to achieve?**

**Say (must match your GDD out-of-scope list):**  
“Answering Q3 — from my ideation and GDD, these were **not** achieved in this trimester’s vertical slice:  

- Photoreal **NASA textures** and high-fidelity planet models  
- **Voice narration** / rich audio feedback  
- **Multiplayer**  
- Full **EduQuest** multi-subject product and **LMS / Canvas integration**  
- **App Store** release and a full **accessibility** audit  

What **is** complete is the committed core: AR plane placement, four mission styles, tap-to-answer on 3D planets, menu-to-results UI, editor testing, and performance-aware materials. The cut items remain for capstone or EduQuest expansion.”

---

## 9:40 — GitHub / README + outro (~45 s)

**Show:** Public GitHub → open **README.md** → scroll Quick start (editor) + AR on device.

**Say:**  
“The public GitHub repository includes a `.gitignore` that excludes Library and build folders, and a README that explains how to run the editor test scene and how to set up SampleScene for AR on a phone. My submission document links this repo, this video, and the GDD with the Performance section. Thank you for watching.”

**On screen:**  
Repo URL · Video URL · GDD URL  

---

## Final checklist before upload

| Must be audible / visible | Done? |
|---------------------------|-------|
| Q1 OOP + patterns spoken with on-screen title | ☐ |
| Q2 Profiler shown + roadblock explained + FPS spoken | ☐ |
| Q3 Scope gaps spoken (match GDD) | ☐ |
| Q4 AR interactions spoken with on-screen title | ☐ |
| Gameplay + AR placement shown | ☐ |
| README / GitHub shown | ☐ |
| Performance techniques ≥ 2 named (instancing + MPB, etc.) | ☐ |

---

## Recording tips

- Put **Q1–Q4 titles on screen** even if you ad-lib slightly — graders scan for those answers.  
- If voice shakes: record gameplay silent first, then narrate over it.  
- Do **not** skip AR or Profiler — those map directly to Q4 and Q2.  
- Upload YouTube **unlisted** or Drive with link access for graders.

*Paste the video link into `docs/SUMMATIVE_SUBMISSION_HUB.md` / your Canvas submission Doc next to GitHub and GDD.*
