# Orbit Scout — demo video script (no live presentation)

Record **one video** (recommended **8–12 minutes**) that replaces the live presentation. Use **voiceover** or **on-screen text cards** for the technical sections. Show the **Game view** or **device screen** for gameplay; switch to **Unity Editor** for code, Profiler, and Project Settings.

**Before recording**

- [ ] `OrbitScout_EditorTest` works: Play → Start Mission → hover → tap → hint → finish  
- [ ] `SampleScene` saved after **Orbit Scout → Setup AR In Active Scene**  
- [ ] Profiler open in second monitor (or record Editor segments separately and edit)  
- [ ] Fill FPS numbers in `docs/GDD_PERFORMANCE.md` and match what you say in the video  

---

## Suggested structure (copy this order)

| Time | Section | What graders see |
|------|---------|-------------------|
| 0:00–0:45 | Intro + GDD alignment | Title, learning goal, matches GDD |
| 0:45–3:30 | Core gameplay (Editor) | Interactive, immersive, polished |
| 3:30–5:30 | AR on device (or Editor AR rig) | AR/VR interactions |
| 5:30–7:00 | Code & architecture | OOP + patterns |
| 7:00–8:30 | Profiler + performance | Roadblock + techniques + FPS |
| 8:30–9:30 | Scope not achieved | Honest GDD gaps |
| 9:30–10:00 | Outro | Repo, GDD link, thank you |

---

## 0:00 — Title card (on screen)

**Text:**  
**Orbit Scout** — Educational AR Solar System Quiz  
Mwiza Cannelle · Unity 6000.5.0f1  
GitHub: `[your public repo URL]`

**Say (or text):**  
“This is my AR vertical slice for the summative assignment. The experience matches my Game Design Document: a timed, clue-based quiz where players identify planets in a tabletop solar system, with scoring, learning facts, and AR placement on a real surface.”

---

## 0:45 — GDD alignment (30 s)

**Show:** GDD open in browser/Docs (objectives paragraph only — blur personal info if needed).

**Say:**  
“My GDD goals were: make space science interactive, use AR to place content in the learner’s environment, and combine game feel—timer, streaks, stars—with short factual feedback. Orbit Scout implements that loop: read a clue, find the planet, get a fact, complete the mission for a star rating.”

---

## 1:00 — Editor gameplay walkthrough (~2.5 min)

**Show:** `OrbitScout_EditorTest` → Play.

1. **Start Mission** — menu and instructions.  
2. Read **Question 1/6** clue aloud briefly.  
3. **Hover** a planet — name pill appears.  
4. **Tap wrong** once (optional) — penalty message / red flash.  
5. **Tap correct** — green flash, fact line, score/streak.  
6. Press **Hint (1)** — read hint text.  
7. Finish mission (or skip ahead if long) — **Mission Over**, score, **stars**, **Play Again** / **Main Menu**.

**Say:**  
“Interactions are tap-to-answer on 3D planets, hover labels so players know what they’re selecting, one hint per mission, wrong answers cost time, and streaks reward consecutive correct answers. The HUD is driven by game events, not hard-wired in every script.”

---

## 3:30 — AR segment (~2 min) — **required for AR/VR criterion**

**Preferred:** Phone/tablet recording (screen mirror or camera over shoulder).

**Fallback:** Unity Editor with `SampleScene` + device simulator — say clearly: “Device build; this segment shows the same flow on phone.”

**Show:**

1. App launch → **Start Mission**.  
2. **Move device** — planes / surface detection visible.  
3. **Tap floor/table** — solar system appears at tabletop scale.  
4. One full question: clue → hover/tap → feedback.  
5. Briefly **walk around** the anchor (immersion).

**Say — AR/VR interactions (answers presentation question 4):**

“In AR I used **plane detection** so the app finds real horizontal surfaces. **Raycast placement** anchors the solar system on the user’s table or floor. The player uses **6-DoF viewing** to look around the model. **Touch input** selects planets for the quiz, and **touch-with-hover labels** show planet names while the finger is over a sphere. Content is **tabletop-scaled** so it stays comfortable and readable in arm’s reach.”

---

## 5:30 — Code quality: OOP & patterns (~1.5 min)

**Show:** Project window → `Assets/OrbitScout/` folders.

**Say — presentation question 1:**

“I organized code by responsibility:

- **Core** — `MissionController` owns rules, timer, score, hints; `SolarQuizBank` holds data. That’s **single responsibility** and **encapsulation**—UI doesn’t mutate score directly.  
- **View** — `SolarSystemView` builds the 3D scene; `PlanetBody` handles visuals.  
- **UI** — `MissionHud` only displays and forwards button clicks.  
- **Tapping / Platform** — input and AR bootstrap stay separate from quiz rules.

**Patterns:**  
- **Observer** — `MissionController` raises events like `OnQuestionChanged` and `OnAnswerCorrect`; `MissionHud` subscribes. Adding UI doesn’t require changing mission logic.  
- **State flow** — Menu → optional **AR placement** → **Playing** → **End screen**.  
- **Factory-style builder** — `SolarSystemView.Build()` creates the whole system at runtime from data specs.

This follows **SOLID** ideas in practice: one place for rules, one for presentation, dependencies pointing inward toward Core.”

**Show (5–10 s each):**  
`MissionController.cs` (events + `SubmitPlanet`) → `MissionHud.cs` (subscriptions) → `SolarSystemView.cs` (Build + shared material).

---

## 7:00 — Profiler & performance (~1.5 min)

**Show:** Play mode → **Window → Analysis → Profiler** → record 10 s during orbiting planets + taps.

**Say — presentation question 2 + performance rubric:**

“I used the **Unity Profiler** during active gameplay. In **CPU**, most time stays on Main Thread scripts and UI; nothing spikes every frame from per-planet material creation because planets share one instanced material with **MaterialPropertyBlock** colors in `SolarSystemView`—that’s **GPU instancing / batching awareness**. In **Rendering**, draw calls stay low for primitive planets.

**FPS benchmark:** In the Editor test scene I measured **[SAY YOUR NUMBER, e.g. ~60] FPS**. On **[device model]** in AR I measured **[SAY YOUR NUMBER, e.g. ~35] FPS** during placement and quiz—documented in my GDD Performance section.

**Other techniques:** lightweight **primitive meshes**, no heavy textures, **tabletop scale** in AR, **IL2CPP** on mobile builds in Player Settings. I did **not** use **occlusion culling** because the whole solar system is small and always visible—setup cost isn’t justified. **LOD** isn’t needed on low-poly spheres. For **mipmap / texture awareness**, UI uses TextMesh Pro and default assets that import with mipmaps; planets use solid colors to avoid texture memory and sampling cost.”

**Show:** Profiler CPU graph + Rendering; optional **Player Settings → IL2CPP** screenshot.

---

## 8:30 — Scope not achieved (~1 min)

**Say — presentation question 3:**

“From my original GDD scope, these were **not** fully delivered in this vertical slice:

- **Learn mode** with paused orbits and free exploration without a timer.  
- **High-fidelity planet models or NASA textures**—I used procedural primitives for speed and performance.  
- **Advanced AR features** like image tracking or sky questions pointing at real clouds.  
- **Audio** for correct/wrong feedback.  
- Trimming unused **template sample packages** from the repo for a minimal build.

The **core summative loop**—AR placement, timed quiz, education facts, scoring, and stars—is complete and testable.”

Adjust bullets if your GDD listed different features—**keep what you say identical to your updated GDD**.

---

## 9:30 — Outro (30 s)

**Show:** README on GitHub (quick scroll: Quick start, AR, Architecture).

**Say:**  
“Source is public on GitHub with README instructions for editor test and AR SampleScene setup. The GDD link includes an updated **Performance Considerations** section with Profiler screenshots and FPS tables. Thanks for watching.”

**On screen:** Repo URL · Video already submitted · GDD URL

---

## Rubric coverage checklist (verify before upload)

| Rubric area | Covered in video? |
|-------------|-------------------|
| Functionality & GDD alignment | Intro + gameplay + AR |
| Interactivity & immersion | Quiz, hover, streaks, walk-around AR |
| Code quality | Section 5:30 folders + patterns |
| GitHub/README | Outro + optional README scroll |
| Video quality | Follow script; stable audio |
| Performance | Profiler + 2+ techniques + **spoken FPS** |
| All 4 presentation questions | Sections 3:30, 5:30, 7:00, 8:30 |

---

## Recording tips

- **1080p**, landscape for Editor; portrait OK for phone AR if you label it.  
- If nervous on voiceover: record gameplay first, then record narration while watching the capture.  
- **One wrong answer + one hint** in editor proves feedback systems.  
- Do **not** skip AR entirely—graders need to see plane placement or a clear device recording.

---

*After recording, upload to YouTube (unlisted/public) or course portal and paste the link in your submission document next to the GDD and GitHub URLs.*
