using UnityEngine;

namespace OrbitScout.View
{
    /// <summary>
    /// Resolves Orbit Scout shaders for player builds.
    /// Custom shaders are not found by Shader.Find unless included (Always Included / Resources mats).
    /// </summary>
    public static class OrbitScoutShaderLibrary
    {
        const string PlanetSurfaceName = "OrbitScout/PlanetSurface";
        const string PlanetRingsName = "OrbitScout/PlanetRings";
        const string GlassPillName = "OrbitScout/UI/GlassPill";

        const string PlanetSurfaceMatPath = "OrbitScout/Materials/PlanetSurface";
        const string PlanetRingsMatPath = "OrbitScout/Materials/PlanetRings";
        const string GlassPillMatPath = "OrbitScout/Materials/GlassPill";

        static Shader planetSurface;
        static Shader planetRings;
        static Shader glassPill;
        static bool warmedUp;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void WarmUp()
        {
            if (warmedUp)
                return;
            warmedUp = true;
            _ = PlanetSurface;
            _ = PlanetRings;
            _ = GlassPill;
        }

        public static Shader PlanetSurface => planetSurface ??= Resolve(PlanetSurfaceName, PlanetSurfaceMatPath);
        public static Shader PlanetRings => planetRings ??= Resolve(PlanetRingsName, PlanetRingsMatPath);
        public static Shader GlassPill => glassPill ??= Resolve(GlassPillName, GlassPillMatPath);

        static Shader Resolve(string shaderName, string resourcesMaterialPath)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader != null)
                return shader;

            Material mat = Resources.Load<Material>(resourcesMaterialPath);
            if (mat != null && mat.shader != null && mat.shader.name != "Hidden/InternalErrorShader")
                return mat.shader;

            return Shader.Find(shaderName);
        }
    }
}
