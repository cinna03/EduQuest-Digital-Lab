# Presentation answers — EduQuest Anatomy Atlas

**Pitch:** “EduQuest Anatomy Atlas lets students peel Skin → Muscles → Organs → Nerves, click structures for facts, and reflect — a digital biology lab slice of EduQuest.”

**OOP / patterns:** `ILabExperiment`; `AnatomyExplorer` orchestrates layers; `AnatomyPartHotspot` encapsulates name/fact/highlight; import mapper uses name heuristics.

**Profiler:** Window → Profiler → Play → peel all layers + orbit → show frame time vs **16.6 ms (60 FPS)**.

**Scope not achieved:** Full medical atlas of every muscle/nerve; AR table place (deferred); photoreal cadaver mesh (optional FBX import).

**AR/VR interactions:** Spatial 3D orbit/zoom inspection + layered reveal. AR Foundation placement is next milestone.

**Performance:** Layer `SetActive` culling, MaterialPropertyBlock highlights, static shells, mipmaps on imports, IL2CPP for device builds; occlusion unused (tiny scene).
