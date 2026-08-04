# GDD — Performance Considerations (paste into your Game Design Document)

## Performance goals

- **Editor (desktop test):** sustain **~60 FPS** while eight planets orbit and UI updates each frame.  
- **Mobile AR:** target **≥ 30 FPS** on a mid-range iOS/Android device during placement + quiz (record your actual device model and FPS in this section).

## How we measured

1. Unity **Window → Analysis → Profiler**  
2. Enter Play Mode (editor scene) or deploy a **Development Build** to device with **Autoconnect Profiler**  
3. Capture 10–20 seconds during active gameplay (orbiting planets + tapping answers)  
4. Note **CPU Main Thread** and **GPU** time; record average **FPS** from the Stats overlay or Profiler  

*Insert screenshot 1: Profiler CPU module during mission.*  
*Insert screenshot 2: Profiler Rendering / GPU module during mission.*

## Techniques used

### 1. GPU instancing + shared material (batching awareness)

Planets are built at runtime in `SolarSystemView`. Instead of creating a unique `Material` per sphere, we use:

- One **shared URP Lit material** with **`enableInstancing = true`**  
- Per-planet color via **`MaterialPropertyBlock`** (`_BaseColor`)

This reduces material variants and helps the GPU batch draw calls for similarly shaded primitives—important when all eight planets are on screen in AR.

### 2. Lightweight geometry (no heavy assets)

- Primitives only (spheres/cylinder ring)—low polygon count  
- No texture downloads; solid colors only—reduces memory and fill rate  

### 3. AR-specific scope control

- Tabletop scale (`arenaAnchor` ~0.55 in AR) keeps the play area small  
- Point light on sun with limited range—avoids excessive lighting cost  

### 4. Mobile build settings (recommended)

- **Scripting backend: IL2CPP** (iOS required; improves performance on many Android devices)  
- **Managed stripping:** Low or Medium (test AR after changing)  
- Disable unused **Sample/XRI** packages in future trims if build size matters  

### Not used (and why)

- **Occlusion culling:** not justified—the scene is a single small solar system always in view; setup cost outweighs benefit.  
- **Custom texture atlas:** no custom textures in this vertical slice; course requirement acknowledged via shared material strategy above.  
- **LOD groups:** planet meshes are already low-poly primitives.

## Known bottlenecks to mention in presentation

- **UI rebuild:** HUD is built once at runtime (acceptable)  
- **AR plane visualization:** template plane meshes add cost during scanning—acceptable for assignment demo  

## FPS benchmark (fill in after profiling)

| Platform | Scene | Avg FPS | Notes |
|----------|--------|---------|--------|
| Unity Editor | OrbitScout_EditorTest | _____ | e.g. 60 on M1 Mac |
| Device | SampleScene AR | _____ | e.g. iPhone 12, 35 FPS |

---

*Project: Orbit Scout (Solar Quiz) · Unity 6000.5.0f1*
