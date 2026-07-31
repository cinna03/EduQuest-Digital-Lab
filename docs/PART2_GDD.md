# EduQuest Anatomy Atlas — Game Design Document  
## EduQuest Digital Lab · Biology vertical slice

**Author:** Cannelle Mwiza  
**Program:** African Leadership University — AR/VR Specialization (Individual)  
**App name:** **EduQuest Anatomy Atlas**  
**Version:** 3.1 — EduQuest Anatomy Atlas branding  
**Date:** 31 July 2026  

**Publishing:** Google Doc or Medium → **Anyone with the link can view** (incognito-test).

---

## Working title

**EduQuest Anatomy Atlas**

---

# HIGH-LEVEL CONCEPT / DESIGN

## Concept statement

**EduQuest Anatomy Atlas** is a student-friendly biology laboratory where learners peel a human body from the outer skin inward through muscles, organs, and nerves, click structures for short facts, and reflect on what they discovered — discovery learning instead of memorizing a flat diagram.

## Genre(s)

Educational simulation / interactive anatomy explorer (desktop 3D; AR placement optional later).

## Target audience

| Dimension | Detail |
|-----------|--------|
| Primary | University / upper-secondary students in developing contexts |
| Secondary | Educators wanting safe digital dissection-style exploration |
| Evaluators | Graders on desktop Unity without headset |
| Age | ~15–24 |
| Motivation | Curiosity about “what’s under the skin,” systems thinking |

## Game Persona and POV

- **Persona:** Amina, 19 — wants to understand body systems for health class without access to a physical cadaver lab.  
- **POV:** Young investigator exploring a portable digital anatomy station.  
- **Emotions:** Curiosity, clarity, calm confidence.  
- **Engagement:** Peel layers → click → short fact → reflect.

## Unique Selling Points

1. Layer peel (Skin → Muscles → Organs → Nerves)  
2. Clickable teaching hotspots with plain-language facts  
3. Orbit/zoom inspection (spatial learning)  
4. Reflection beat for explicit learning  
5. Import path for higher-fidelity anatomy FBX without rewriting code  

## Visual and Audio Style

Clean dark lab backdrop; readable colors per layer (skin tone, muscle red, organ tones, nerve yellow). Soft UI panels. Optional ambient; usable muted.

**Figures:** `docs/diagrams/flowchart.png`, `uml.png`, `ux_wireframe.png`

## Game World Fiction

You open **EduQuest Anatomy Atlas**, EduQuest’s biology station. A human form waits. Peel the outer layer to reveal muscles, then organs, then nerve pathways. Click a structure to learn its role. Reflect before you leave.

## Monetization

Academic prototype this trimester. Future: free learner labs + optional institutional anatomy packs.

## Platform / Tech / Scope

| Item | Decision |
|------|----------|
| Platform | Desktop Unity 6 (URP) primary |
| Engine | Unity 6 |
| In scope | 4 layers, clickable parts, orbit camera, reflection, import hook |
| Out of scope | Medical-grade full atlas, every nerve, multiplayer, LMS |

---

# DETAILED DESIGN

## Core loop

Select layer → Orbit/inspect → Click structure → Read fact → Reflect → Retry  

## Objectives

Understand that the body is layered systems; name a few structures; connect structure → function in one sentence reflection.

## Game systems

`AnatomyExplorer` (lab), `AnatomyPartHotspot` (selectables), `AnatomyOrbitCamera`, layer slider UI, `ReflectionUI`, optional imported model mapper.

## Interactivity

| Type | How |
|------|-----|
| Action/Feedback | Layer slider peels body; click highlights + fact |
| ST Cog | Predict what’s under skin; verify by peeling |
| LT Cog | Systems view of anatomy |
| Emotional | Curiosity / “aha” |
| Social | Classroom share of reflection later |
| Cultural | Accessible STEM without physical lab reagents/cadavers |

## Flowchart / UML

Insert `docs/diagrams/flowchart.png` and `uml.png` (update mentally: AnatomyExplorer replaces multi-lab hub).

---

# PERFORMANCE CONSIDERATIONS

## FPS benchmark

| Item | Value |
|------|--------|
| **Target FPS** | **60 FPS** |
| Platform | Unity Editor / Mac standalone |
| Floor | ≥ 50 FPS while orbiting + switching all 4 layers |

## Techniques

1. **Static batching** on non-interactive shells where marked  
2. **Mipmaps** on imported textures  
3. **MaterialPropertyBlock** highlight (no material clone spam)  
4. **Layer visibility culling** — inactive layers `SetActive(false)` (not drawn)  
5. **IL2CPP** for future device/AR builds  
6. **Occlusion culling** — examined, **not used** (single small lab volume)

**Insert Profiler screenshots P1–P3 when publishing.**  
Summary card: `docs/diagrams/performance_summary.png`

## Scope not achieved

- Full medical atlas of every muscle/nerve  
- Optional AR table placement (deferred)  
- Photoreal cadaver rendering  

---

# RUBRIC NOTES

Aligns with vertical slice: interactive, immersive 3D biology lab matching this GDD. Creativity via layered exploration + importable assets path.
