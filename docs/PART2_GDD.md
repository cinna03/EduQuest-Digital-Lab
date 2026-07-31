# EduQuest Photographic Crystal Puzzle — GDD
## AR chemistry lab (AgCl-inspired simulation)

**Author:** Cannelle Mwiza  
**Program:** ALU — AR/VR Specialization (Individual)  
**App:** **EduQuest Photographic Crystal Puzzle**  
**Version:** 5.0  
**Date:** 31 July 2026  

**Publishing:** Google Doc / Medium → Anyone with the link.

---

## Concept

Students solve a lab puzzle: in **darkness**, mix measured **silver nitrate** and **sodium chloride** to form light-sensitive **silver chloride** (white precipitate), **stabilize** with sodium thiosulfate, then use **real light** to activate a stable silver-blue crystal. Six bottles (3 correct, 3 traps), measurements, timing, and light/dark create fair failure states. **Simulation only** — AgNO₃ is hazardous in reality.

## USPs

1. Puzzle chemistry (not “mix two and win”)  
2. Real light/dark via camera sensor  
3. Measurements + timing matter  
4. Distinct failure visuals + lab journal  
5. Score /100 (chem, measure, light, timing)  
6. 60 FPS performance target  

## Platform

| Item | Decision |
|------|----------|
| Engine | Unity 6 URP |
| AR | Desktop camera preview (table place + light sensor); phone AR Foundation optional later (6.5+) |
| Scope | One guided puzzle + Reflect + safety |

## Core loop

Place beaker → Dark → 10 ml AgNO₃ → 10 ml NaCl → wait 5s → 5 ml fixer → Bright light → Glow / Fail → Reflect

## Cause → effect (summary)

| Mistake | Outcome |
|---------|---------|
| Light ON while forming AgCl | Burnt silver residue |
| Wrong bottle (D/E/F) | Contaminated solution |
| Bad 10:10 ratio | Incomplete / weak / unstable mix |
| Fixer too early | No crystal |
| Fixer too much | Dissolved crystal |
| Fixer too little + light | Grey unstable crystal |
| Correct path + light last | Stable photographic crystal |

## Scoring

Correct chemicals 30 · Measurements 30 · Light condition 20 · Timing 20 = **100**

## Performance

Target **60 FPS**. Cap particles; document Profiler in submission.
