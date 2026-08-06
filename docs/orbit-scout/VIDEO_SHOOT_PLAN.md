# Solar Quiz — VIDEO STRUCTURE (final)

**Order graders must see:**

1. **Mobile AR gameplay** — full feature showcase on phone  
2. **Code walkthrough** — architecture of the app  
3. **Q&A answers** — all evaluation questions on camera  

There is **no live presentation**. This one video covers everything.

---

## Part A — Phone AR demo (show ALL features)

**Record on the device** (iOS Screen Recording / Android recorder) or film the phone clearly.

### Feature checklist (hit every row)

| # | Feature | What to do on camera |
|---|---------|----------------------|
| 1 | Launch / main menu | Open app → show **Play** / menu art |
| 2 | Level select | Open missions → show Level I–IV cards |
| 3 | AR plane detect | Move phone over floor/table until planes appear |
| 4 | Place solar system | **Tap** surface → 8 planets + sun appear |
| 5 | Walk-around immersion | Slowly circle the tabletop system |
| 6 | Clue / HUD | Read one clue aloud; show score/timer if visible |
| 7 | Tap wrong planet | Wrong feedback once |
| 8 | Tap correct planet | Correct feedback + score |
| 9 | Second mission style (optional but strong) | Start Level II or III briefly if time |
| 10 | End / retry or menu | Show end screen or return to menu |

**Narration:** short and demo-led — “here’s the menu… placing… answering…”  
Save deep technical talk for Part B and C.

**Target length:** 3–5 minutes.

---

## Part B — Code walkthrough (architecture)

**Record Unity Editor** (QuickTime screen recording).

### Show in this order

1. Project window → `Assets/OrbitScout/` folders (Core, View, UI, Tapping, Platform)  
2. Hierarchy: `OrbitScout` + HUD / AR objects in SampleScene if useful  
3. Open briefly:
   - `SolarBootstrap.cs` — editor vs AR session  
   - `ArSessionBridge.cs` — place on plane  
   - `MissionController.cs` — rules / events  
   - `SolarSystemView.cs` — Build + shared material  
   - `MissionHud.cs` / `OrbitScoutHudView` — UI  
   - `PlanetTapInput.cs` — taps  

**Say:** how data flows: Bootstrap → (AR place) → Mission start → View builds planets → Tap → Controller → HUD events.

**Target length:** 2–3 minutes.

---

## Part C — Answer ALL questions (on-screen titles)

Put a **big title** before each answer, then read the teleprompter block.

| Title on screen | Question |
|-----------------|----------|
| **Q1 — OOP & design patterns** | What OOP principles and design patterns did you use? |
| **Q2 — Unity Profiler** | Demonstrate Profiler / performance roadblock (+ say FPS) |
| **Q3 — Scope not achieved** | What from earlier scope did you not achieve? |
| **Q4 — AR/VR interactions** | What AR/VR interactions were included? |

**Note:** You already *showed* AR in Part A. In Q4, **name** the interactions explicitly (plane detect, raycast place, world taps, etc.) so the rubric question is unmistakably answered.

For **Q2**, switch to Profiler in Editor (Play mode) — show CPU + Rendering.

**Target length:** 4–5 minutes.

---

## Optional 20 s bookends

- **Start:** “Solar Quiz — Cannelle Mwiza — this video is gameplay, architecture, and all evaluation answers.”  
- **End:** GitHub README scroll + GDD link + thank you.

---

## Full timeline (~10–12 min)

| Part | Time | Content |
|------|------|---------|
| A | 0:00–4:30 | Phone AR — all features |
| B | 4:30–7:00 | Code architecture |
| C | 7:00–11:00 | Q1 → Q2 → Q3 → Q4 |
| Outro | 11:00–11:30 | Repo + GDD |

Open **`docs/VIDEO_TELEPROMPTER.md`** and read Parts A → B → C in order.
