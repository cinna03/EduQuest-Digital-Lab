# Orbit Scout (Solar Quiz)

Educational **AR planet quiz** — timed clues, scoring, streaks, facts, hover names, and AR plane placement.

**Unity:** 6000.5.0f1 · Copied into **EduQuest Digital Lab** for GitHub workflow.

**Summative:** [SUBMISSION_CHECKLIST.md](SUBMISSION_CHECKLIST.md) · [DEMO_VIDEO_SCRIPT.md](DEMO_VIDEO_SCRIPT.md) · [GDD_PERFORMANCE.md](GDD_PERFORMANCE.md)

---

## Quick start (editor)

1. Open **`OrbitScout_EditorTest.unity`** or **`SampleScene`** — HUD/menu stay aligned via **Orbit Scout → Sync HUD And Menu Background (Both Game Scenes)**.  
2. **Play** → **Play** → choose level → hover planets → tap to answer.

---

## AR on device

1. Open **`Assets/Scenes/SampleScene.unity`**.  
2. **Orbit Scout → Setup AR In Active Scene** — saves SampleScene and updates **OrbitScout_EditorTest** to match.  
3. **Build Settings** — **SampleScene** first → build iOS/Android.  
4. On device: choose mission → scan → tap to place → quiz.

---

## Code

All game logic: **`Assets/OrbitScout/`** (Core, View, UI, Platform, Tapping).

---

## Edit the start menu in the Hierarchy (fonts, layout, backgrounds)

The HUD lives under **`UI (Edit Here)` → OrbitScoutHud** in game scenes (World Space in edit mode so Scene view shows the menu).

1. Open **`OrbitScout_EditorTest.unity`** or **`SampleScene`**.  
2. **Orbit Scout → UI Editing → Prepare Scene For UI Hierarchy** (once per scene, or after pulling changes).  
3. Use **Orbit Scout → UI Editing → Edit Main Menu / Level Select / …** to select the right panel and hide the others while you work.  
4. Hierarchy rows under the HUD show **Panel / Btn / Text** badges.  
5. On **OrbitScoutHud**, use inspector **UI hierarchy shortcuts**.  
6. **Save the scene**; run **Sync HUD And Menu Background (Both Game Scenes)** if you edited only one scene.

**Default menu art:** `Assets/OrbitScout/UI/Visuals/MenuPanel_background.png` on **MenuPanel → Image → Source Image** (saved on the HUD prefab; visible in Scene view). Re-apply with **Orbit Scout → Assign Menu Panel Background (Scene View)** if needed.

**Prefab path:** `Assets/OrbitScout/UI/Prefabs/OrbitScoutHud.prefab`  
**Shortcut:** **Orbit Scout → Open HUD Prefab For Editing**

`MissionHud` on **OrbitScout** must reference **Hud View** (wired automatically when the HUD is created).
