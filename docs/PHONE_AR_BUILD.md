# Phone AR build — Photographic Crystal

Uses **AR Foundation 6.5** (required for Unity **6000.5**). Older 6.0.x breaks URP compile.

## One-time Unity setup
1. Open project — let packages install (`arfoundation`, `arcore`, `arkit` 6.5.0)
2. If compile errors from old cache: close Unity, delete `Library/PackageCache/com.unity.xr.arfoundation*`, reopen
3. **EduQuest → Build Photographic Crystal Lab**
4. **Edit → Project Settings → XR Plug-in Management**
   - **Android** tab → enable **ARCore**
   - **iOS** tab → enable **ARKit** (Mac + Xcode)
5. **Player Settings**
   - Android: Minimum API 24+, **Internet Access** optional, camera permission via ARCore
   - iOS: Camera Usage Description = `Needed for AR table scanning and light sensing`

## Build & Run
1. Connect phone (USB debugging on Android)
2. **File → Build Settings → Android** (or iOS) → Switch Platform
3. **Build And Run**

## In-app phone flow
1. Point at a **flat table** — translucent planes appear
2. **Tap the plane** → Griffin beaker anchors
3. Reagent glassware spawns around the beaker
4. Tap bottles → measure → pour (keep dark)
5. Fixer → bright light → glow

## Desktop still works
Editor Play Mode uses the desktop table preview (forced in editor). Same puzzle logic.
