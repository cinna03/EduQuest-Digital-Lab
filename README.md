# EduQuest Digital Lab

Virtual science lab for youth — run safe, interactive experiments without physical reagents.

**Author:** Cannelle Mwiza  
**Engine:** Unity 6 · Universal 3D (URP)  
**Main scene:** `Assets/Scenes/EduQuestLab.unity` (create via menu)

## Experiments

1. **Science — Germination** — water + warmth + days → seed sprouts  
2. **Physics — Pendulum** — length vs mass; period ≈ `2π√(L/g)`  
3. **Chemistry — Dancing blue flame** — simulated Al + HCl → H₂ bubbles → ignited pale blue flame  

> Chemistry station is a **simulation** with an on-screen safety note. Never attempt real HCl reactions unsupervised.

## Quick start

1. Open this project in Unity Hub.  
2. Menu **EduQuest → Build Digital Lab Scene**.  
3. Press **Play**.  
4. Pick a lab → move sliders → **Reflect**.

## Project structure

```
Assets/Scripts/EduQuest/
  LabHub.cs
  ReflectionUI.cs
  ILabExperiment.cs
  Experiments/
    GerminationExperiment.cs
    PendulumExperiment.cs
    BlueFlameExperiment.cs
  Editor/LabSceneBuilder.cs
```

## GitHub Desktop

This folder is the Unity project. In GitHub Desktop:
1. **Add** → **Add Existing Repository** → choose `EduQuest Digital Lab`  
   (or publish the local `main` branch to your empty GitHub repo)  
2. Commit the lab scripts after Unity imports them  
3. Push to `origin`

## Next (AR video)

Desktop Play Mode is for building. For the graded AR demo video we will add AR Foundation + place-lab-on-table, then record on phone.

## License / course

ALU AR/VR specialization summative portfolio project.
