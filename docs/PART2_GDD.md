# EduQuest Light & Life — Game Design Document
## AR multi-factor biology growth lab (vertical slice)

**Author:** Cannelle Mwiza
**Program:** African Leadership University — AR/VR Specialization (Individual)
**App name:** **EduQuest Light & Life**
**Version:** 2.0 — Hypothesis + water-at-table + light/dark growth lab
**Date:** 31 July 2026

**Publishing:** Google Doc or Medium → **Anyone with the link can view** (incognito-test).

---

# HIGH-LEVEL CONCEPT / DESIGN

## Concept statement

**EduQuest Light & Life** is an AR-first biology growth lab. Students form a hypothesis, place a seedling on their table, water it only when facing the table (not a lamp), then use **real bright light** and **real darkness** to drive photosynthesis, oxygen production, night mode, and failure states (wilt / flood / scorch). A UI “Add light” button cannot finish the experiment — the room is the controller.

## Genre(s)

Educational AR simulation / inquiry-based biology lab.

## Unique Selling Points

1. Hypothesis → trial → result
2. Multi-factor controls: moisture + real light + real dark
3. Table-view constraint for watering
4. Science meters (light, water, energy, O₂, stage)
5. Failure states (wilt / flood / scorch)
6. **60 FPS** performance target documented

## Platform / Scope

| Item | Decision |
|------|----------|
| Platform | Unity 6 URP; webcam light sensing |
| In scope | Full guided growth trial + Reflect + performance notes |
| Out of scope | Production AR Foundation plane anchors this slice |

---

# DETAILED DESIGN

## Core loop

Hypothesis → Place → Water at table → Seek bright light → Grow → Seek dark → Night mode → Hypothesis check → Reflect

## Cause → effect

| Action | Result |
|--------|--------|
| Water while facing lamp | Blocked |
| Balanced water + bright light | Energy ↑, O₂ ↑, seedling |
| Bright + dry soil | Scorch risk |
| Flood + dark | Flood / rot risk |
| Real darkness after growth | Night mode |

## Systems

`WorldLightSensor`, `GuidedLightLabExperiment` (`ILabExperiment`), `LightLabBuilder`, `ReflectionUI`, `LabPerformanceSettings`.

---

# PERFORMANCE CONSIDERATIONS

## FPS benchmark

| Item | Value |
|------|--------|
| **Target FPS** | **60 FPS** |
| Floor | ≥ 50 FPS with camera preview + particles |

## Techniques

1. Single lab active
2. MaterialPropertyBlock for soil/leaf colors
3. Static table props
4. Downsampled camera luminance sampling
5. Simple meshes
6. Occlusion culling unused (tiny volume)

**Insert Profiler screenshots when publishing.**
