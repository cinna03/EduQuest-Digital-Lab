# EduQuest · Timed Science Riddle Campaign

AR evaluation game: kids **fight short waves**, then solve **science riddles** by finding real-world clues (sky/clouds, flowers, light). Finish all **3 levels before time runs out** — the faster you finish, the higher your score.

**Author:** Cannelle Mwiza  
**GitHub:** https://github.com/cinna03/EduQuest-Digital-Lab

## How it works

| Stage | What happens | Learning check |
|-------|----------------|----------------|
| **Level 1** | Easy combat (frogs/rats) → riddle | Infer **sky / clouds** |
| **Level 2** | Harder combat (enemies attack) → riddle | Infer **flowers** (germination) |
| **Level 3** | Final riddle | Infer **light** (physics) |

- During riddle hunts there are **no enemies**
- Combat stays on a **shared horizontal ground** (living-room floor / table)
- Riddles **do not name the object** — the player must figure it out
- **3:00** total time limit; remaining time boosts **score + stars**
- Time out = failed evaluation → RESET to retry

## Editor test (desktop — no phone needed)

1. Open this Unity project  
2. Menu **EduQuest → Editor Test → Build Editor Test Scene**  
3. Press **Play** ▶  
   Scene: `Assets/Scenes/EduQuestLab_EditorTest.unity`

Or: **EduQuest → Editor Test → Open & Play Ready**

**Editor stubs:** `WIN WAVE` clears combat; `SOLVED RIDDLE` stands in for AR finds until phone detection is wired.

## Phone AR

1. **EduQuest → Build Clean AR Campaign** → `Assets/Scenes/EduQuestLab.unity`  
2. **File → Build Settings → iOS/Android → Build And Run**  
3. Scan a flat surface → tap to place the arena → play the timed campaign  

## Project layout (what matters)

- `Assets/Scripts/EduQuest/Runtime/` — campaign flow, combat, HUD, AR place  
- `Assets/Resources/EduQuest/Enemies/` — Quaternius CC0 enemy meshes  
- `Assets/Scenes/` — editor test + AR campaign scenes  

## Credits

- Enemy models: [Quaternius — Easy Animated Enemy Pack](https://quaternius.itch.io/animated-easy-enemies) (CC0)
