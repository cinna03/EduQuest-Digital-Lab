using UnityEngine;

namespace OrbitScout.View
{
    public static class SolarSystemVisuals
    {
        static Material orbitLineMaterial;

        public static void SetupSun(GameObject sun)
        {
            Renderer renderer = sun.GetComponent<Renderer>();
            if (renderer != null)
                PlanetMaterials.ApplySun(renderer);

            EnsureSunGlow(sun.transform);
            EnsureSunLight(sun);
        }

        static void EnsureSunGlow(Transform sun)
        {
            Transform existing = sun.Find("SunGlow");
            if (existing != null)
                return;

            GameObject glow = GameObject.CreatePrimitive(PrimitiveType.Quad);
            glow.name = "SunGlow";
            glow.transform.SetParent(sun, false);
            glow.transform.localPosition = Vector3.zero;
            glow.transform.localScale = Vector3.one * 2.35f;

            Collider col = glow.GetComponent<Collider>();
            if (col != null)
                Object.Destroy(col);

            Renderer glowRenderer = glow.GetComponent<Renderer>();
            PlanetMaterials.ApplySunGlow(glowRenderer);

            // Billboard so the corona faces the camera
            glow.AddComponent<SunGlowBillboard>();
        }

        static void EnsureSunLight(GameObject sun)
        {
            Light light = sun.GetComponent<Light>();
            if (light == null)
                light = sun.AddComponent<Light>();

            light.type = LightType.Point;
            light.color = new Color(1f, 0.88f, 0.62f);
            light.intensity = 2.6f;
            light.range = 8f;
            light.shadows = LightShadows.Soft;
        }

        public static void AddOrbitRing(Transform root, Transform center, float radius)
        {
            GameObject orbitObject = new GameObject("Orbit_" + radius.ToString("0.00"));
            orbitObject.transform.SetParent(root, false);

            LineRenderer line = orbitObject.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.loop = true;
            line.widthMultiplier = 0.004f;
            line.numCornerVertices = 4;
            line.numCapVertices = 4;
            line.material = GetOrbitLineMaterial();
            line.startColor = new Color(0.35f, 0.65f, 0.95f, 0.22f);
            line.endColor = new Color(0.35f, 0.65f, 0.95f, 0.08f);

            const int segments = 72;
            line.positionCount = segments;
            Vector3 origin = center != null ? center.position : Vector3.zero;

            for (int i = 0; i < segments; i++)
            {
                float angle = i / (float)segments * Mathf.PI * 2f;
                Vector3 point = origin + new Vector3(Mathf.Cos(angle) * radius, 0.012f, Mathf.Sin(angle) * radius);
                line.SetPosition(i, point);
            }
        }

        static Material GetOrbitLineMaterial()
        {
            if (orbitLineMaterial != null)
                return orbitLineMaterial;

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Unlit/Color")
                ?? Shader.Find("Sprites/Default");
            orbitLineMaterial = new Material(shader);
            return orbitLineMaterial;
        }

        public static void ConfigurePlanetRenderer(Renderer renderer, PlanetId id)
        {
            PlanetMaterials.ApplyBody(renderer, id);
        }
    }

    /// <summary>
    /// Keeps the sun corona quad facing the active camera.
    /// </summary>
    public sealed class SunGlowBillboard : MonoBehaviour
    {
        void LateUpdate()
        {
            Camera cam = Camera.main;
            if (cam == null)
                return;

            transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position, Vector3.up);
        }
    }
}
