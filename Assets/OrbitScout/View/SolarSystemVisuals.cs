using UnityEngine;

namespace OrbitScout.View
{
    public static class SolarSystemVisuals
    {
        static Material sunMaterial;
        static Material orbitLineMaterial;

        public static void SetupSun(GameObject sun)
        {
            Renderer renderer = sun.GetComponent<Renderer>();
            if (renderer == null)
                return;

            if (sunMaterial == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                sunMaterial = new Material(shader);
                sunMaterial.EnableKeyword("_EMISSION");
                sunMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }

            renderer.sharedMaterial = sunMaterial;
            Color baseSun = new Color(1f, 0.78f, 0.25f);
            renderer.material.color = baseSun;
            renderer.material.SetColor("_EmissionColor", baseSun * 2.2f);

            Light light = sun.GetComponent<Light>();
            if (light != null)
            {
                light.type = LightType.Point;
                light.color = new Color(1f, 0.85f, 0.65f);
                light.intensity = 2.4f;
                light.range = 8f;
            }
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

        public static void ConfigurePlanetRenderer(Renderer renderer, Color color)
        {
            if (renderer == null)
                return;

            renderer.material.color = color;
            if (renderer.material.HasProperty("_Smoothness"))
                renderer.material.SetFloat("_Smoothness", 0.65f);
            if (renderer.material.HasProperty("_Metallic"))
                renderer.material.SetFloat("_Metallic", 0.08f);
        }
    }
}
