# EduQuest Anatomy Atlas — import a real model (optional upgrade)

The atlas already runs with a **built-in layered torso** (skin → muscles → organs → nerves).  
Use this if you download a better FBX/OBJ.

## Recommended free sources
- HuBMAP CCF reference organs (GLB): https://hubmapconsortium.github.io/ccf/pages/ccf-3d-reference-library.html  
- Rhode Island Hospital open anatomic FBX/OBJ repository (verify license on each item)

Paid (only if you already own): layered anatomy packs on CGTrader / Unity Asset Store.

## Steps in Unity
1. Download model → put files in `Assets/Models/Anatomy/`  
2. Open `Assets/Scenes/EduQuestLab.unity` (after **EduQuest → Build Anatomy Lab Scene**)  
3. Drag the model under `AnatomyExplorer/ImportRoot`  
4. Menu **EduQuest → Map Imported Anatomy Model**  
5. Rename children to include keywords when possible:  
   `skin`, `muscle`, `heart`, `lung`, `liver`, `stomach`, `nerve`, `spine`, `brain`  
6. Press Play → peel layers → click parts  

## Scope note for graders
This vertical slice teaches **layer exploration + structure facts**, not a medical-grade atlas of every nerve. Full-body complete systems = capstone expansion.
