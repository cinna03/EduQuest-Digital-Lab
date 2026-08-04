# Orbit Scout (Solar Quiz)

Educational **AR planet quiz** — timed clues, scoring, streaks, facts, hover names, and AR plane placement.

**Unity:** 6000.5.0f1 · Copied into **EduQuest Digital Lab** for GitHub workflow.

**Summative:** [SUBMISSION_CHECKLIST.md](SUBMISSION_CHECKLIST.md) · [DEMO_VIDEO_SCRIPT.md](DEMO_VIDEO_SCRIPT.md) · [GDD_PERFORMANCE.md](GDD_PERFORMANCE.md)

---

## Quick start (editor)

1. **Orbit Scout → Create Editor Test Scene** (once).  
2. Open **`Assets/Scenes/OrbitScout_EditorTest.unity`**.  
3. **Play** → **Play** → choose level → hover planets → tap to answer.

---

## AR on device

1. Open **`Assets/Scenes/SampleScene.unity`**.  
2. **Orbit Scout → Setup AR In Active Scene** → save scene.  
3. **Build Settings** — **SampleScene** first → build iOS/Android.  
4. On device: choose mission → scan → tap to place → quiz.

---

## Code

All game logic: **`Assets/OrbitScout/`** (Core, View, UI, Platform, Tapping).
