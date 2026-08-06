# Solar Quiz — TELEPROMPTER (read in order)

**Structure:** A) Phone AR demo → B) Code architecture → C) All questions  

Fill before Part C / Q2: Editor FPS **____** · Phone FPS **____** · Device **________**

---

# PART A — MOBILE AR GAMEPLAY (3–5 min)

*(Screen-record the phone. Keep talking short while you play.)*

---

Hi, I’m **Cannelle Mwiza**. This is **Solar Quiz**, my AR educational vertical slice.

This video has three parts: first a **full demo on the phone**, then a **code architecture walkthrough**, then answers to **all evaluation questions**.

---

### Menu + levels

Here’s the **main menu**. I’ll start and open **level select** so you can see the four missions from my GDD.

I’m choosing a mission to play in AR.

---

### Place the solar system

Now the AR flow. I’m scanning a real surface — that’s **plane detection**.

I’ll **tap** the table to **place** the solar system…  
Here are the planets in my room at tabletop scale.

I’ll move around it so you can see it’s anchored in the real world.

---

### Play the quiz

Here’s the **HUD** with the clue.  

I’ll tap a **wrong** planet once — you can see the feedback…  
Now the **correct** planet — score and feedback update.

The core loop is: place the system, read the clue, tap the planet in 3D, get feedback.

*(If time: open one more level briefly — “Level II / III uses different rules, same place-and-tap flow.”)*

*(Show end screen or return to menu if you finish a short run.)*

That completes the feature demo on device: menu, levels, AR place, walk-around, quiz taps, and feedback.

---

# PART B — CODE WALKTHROUGH / ARCHITECTURE (2–3 min)

*(Switch to Unity Editor screen recording.)*

---

Now the **architecture** of Solar Quiz. Code lives under `Assets/OrbitScout`, split by responsibility.

**Platform** — `SolarBootstrap` chooses editor versus AR and starts a session.  
`ArSessionBridge` handles **plane raycast placement** on the phone.

**Core** — `MissionController` owns mission rules, scoring, timers, and unlocks.  
`LevelCatalog` holds the clue content.  
`GameProgress` stores unlocks and high scores.

**View** — `SolarSystemView.Build` creates the sun and eight planets at runtime.  
`PlanetBody` is each planet.

**Tapping** — `PlanetTapInput` raycasts the player’s touch onto a planet and sends the answer to the controller.

**UI** — `MissionHud` and `OrbitScoutHudView` show menu, level select, play HUD, and end screen.

**Data flow:** Bootstrap starts the session → on phone we place via AR → MissionController starts the level → SolarSystemView builds the system → the player taps → Controller updates score → HUD listens to events and refreshes.

That separation keeps AR setup, game rules, 3D view, input, and UI from becoming one giant script.

*(While talking: click folders, then open Bootstrap → ArSessionBridge → MissionController → SolarSystemView → PlanetTapInput → MissionHud briefly.)*

---

# PART C — ALL EVALUATION QUESTIONS (4–5 min)

*(Big on-screen titles before each answer.)*

---

## Q1 — OOP & design patterns

**On screen:** `Q1 — What OOP principles and design patterns did you use?`

Answering **question one**.

**Single responsibility:**  
MissionController = rules.  
MissionHud = UI only.  
SolarSystemView = 3D build.  
PlanetTapInput = picking.  
SolarBootstrap = session and mode.

**Encapsulation:** UI does not write the score directly; it reacts to the controller.

**Observer pattern:** MissionController raises events such as OnQuestionChanged, OnScoreChanged, and OnLevelEnded. The HUD subscribes — so I can change UI without rewriting mission logic.

**Factory-style construction:** SolarSystemView.Build creates the full tabletop system from data at runtime.

**State flow:** Menu → level select → AR placement → playing → end screen.

That is how I kept the code modular and aligned with SOLID ideas in practice.

---

## Q2 — Unity Profiler

**On screen:** `Q2 — Demonstrate the Unity Profiler / performance roadblock`

Answering **question two**.

I’m entering Play mode and opening **Window → Analysis → Profiler**.

I’ll capture several seconds during gameplay.

On **CPU**, work is on the main thread — scripts, UI, updates.  
A likely roadblock in a weaker design would be a **unique material per planet**, which increases cost and hurts batching.

In Solar Quiz, planets share **one URP material** with **GPU instancing**, and colors use a **MaterialPropertyBlock** in SolarSystemView.

On **Rendering**, cost stays manageable with **lightweight primitive meshes** and solid colors.

**FPS benchmark:** In the editor I measured about **____ FPS**.  
On my phone, **________**, I measured about **____ FPS** during AR play.  
Those numbers are in my GDD Performance Considerations section with Profiler screenshots.

I also use small **tabletop scale**, and **IL2CPP** for mobile builds.  
I did **not** use occlusion culling — the system is small and always visible. LOD isn’t needed for low-poly spheres.

---

## Q3 — Scope not achieved

**On screen:** `Q3 — Scope not achieved`

Answering **question three**.

From my ideation and GDD, these were **not** achieved in this trimester’s slice:

- Photoreal **NASA textures** and high-fidelity models  
- **Voice narration** / rich audio  
- **Multiplayer**  
- Full **EduQuest** multi-subject product and **LMS integration**  
- **App Store** release and a full **accessibility** audit  

What **is** complete is the committed core: AR placement, four mission styles, tap-to-answer, full UI flow, editor testing, and performance-aware materials. The rest stays for capstone or EduQuest expansion.

---

## Q4 — AR/VR interactions

**On screen:** `Q4 — What AR/VR specific interactions were included?`

Answering **question four** — naming the AR interactions you saw in the phone demo:

1. **Plane detection** — ARPlaneManager finds real surfaces.  
2. **Raycast placement** — ARRaycastManager + ArSessionBridge to tap and place.  
3. **World-space tabletop content** — walk around the anchored solar system.  
4. **Touch selection** of 3D planets to answer clues.  
5. **Live camera during play** with a readable HUD; solid backdrop on menus.

Those are the AR/VR-specific interactions in Solar Quiz.

---

# OUTRO (~30 s)

The project is on **public GitHub** with a README for editor and AR setup.  
My submission document links the **repo**, this **video**, and the **GDD** with Performance screenshots and FPS.

Thank you for watching.

*(Scroll README: How to run Editor + AR.)*
