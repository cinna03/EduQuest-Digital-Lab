using UnityEngine;

namespace OrbitScout.View
{
    public static class SolarSystemView
    {
        struct PlanetSpec
        {
            public PlanetId Id;
            public float Radius;
            public float Orbit;
            public float Speed;
            public Color Color;
            public float StartAngle;
        }

        static readonly PlanetSpec[] Specs =
        {
            new PlanetSpec { Id = PlanetId.Mercury, Radius = 0.04f, Orbit = 0.22f, Speed = 38f, Color = new Color(0.75f, 0.73f, 0.7f), StartAngle = 10f },
            new PlanetSpec { Id = PlanetId.Venus, Radius = 0.05f, Orbit = 0.30f, Speed = 30f, Color = new Color(0.95f, 0.78f, 0.42f), StartAngle = 55f },
            new PlanetSpec { Id = PlanetId.Earth, Radius = 0.052f, Orbit = 0.38f, Speed = 26f, Color = new Color(0.25f, 0.52f, 0.98f), StartAngle = 120f },
            new PlanetSpec { Id = PlanetId.Mars, Radius = 0.045f, Orbit = 0.46f, Speed = 22f, Color = new Color(0.88f, 0.38f, 0.22f), StartAngle = 200f },
            new PlanetSpec { Id = PlanetId.Jupiter, Radius = 0.11f, Orbit = 0.56f, Speed = 14f, Color = new Color(0.85f, 0.65f, 0.45f), StartAngle = 260f },
            new PlanetSpec { Id = PlanetId.Saturn, Radius = 0.095f, Orbit = 0.66f, Speed = 11f, Color = new Color(0.9f, 0.82f, 0.55f), StartAngle = 310f },
            new PlanetSpec { Id = PlanetId.Uranus, Radius = 0.07f, Orbit = 0.76f, Speed = 9f, Color = new Color(0.55f, 0.85f, 0.9f), StartAngle = 15f },
            new PlanetSpec { Id = PlanetId.Neptune, Radius = 0.068f, Orbit = 0.86f, Speed = 7f, Color = new Color(0.28f, 0.38f, 0.98f), StartAngle = 140f }
        };

        static Material sharedBodyMaterial;
        static readonly MaterialPropertyBlock ColorBlock = new MaterialPropertyBlock();
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int SmoothnessId = Shader.PropertyToID("_Smoothness");
        static readonly int MetallicId = Shader.PropertyToID("_Metallic");

        public static Transform Build(Transform parent, LevelVisualMode visualMode = LevelVisualMode.FullColor)
        {
            PlanetRegistry.Clear();

            GameObject root = new GameObject("SolarSystemView");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            GameObject sun = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sun.name = "Sun";
            sun.transform.SetParent(root.transform, false);
            sun.transform.localScale = Vector3.one * 0.18f;
            SolarSystemVisuals.SetupSun(sun);
            sun.AddComponent<SunTapReject>();

            foreach (PlanetSpec spec in Specs)
                SolarSystemVisuals.AddOrbitRing(root.transform, sun.transform, spec.Orbit);

            foreach (PlanetSpec spec in Specs)
            {
                GameObject planet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                planet.name = spec.Id.ToString();
                planet.transform.SetParent(root.transform, false);
                planet.transform.localScale = Vector3.one * (spec.Radius * 2f);
                ApplyPlanetColor(planet, spec.Color);

                SphereCollider collider = planet.GetComponent<SphereCollider>();
                collider.radius = 0.55f;

                PlanetBody body = planet.AddComponent<PlanetBody>();
                body.planetId = spec.Id;
                body.Initialize(spec.Color);
                PlanetRegistry.Register(body);

                PlanetOrbit orbit = planet.AddComponent<PlanetOrbit>();
                orbit.orbitCenter = sun.transform;
                orbit.orbitRadius = spec.Orbit;
                orbit.degreesPerSecond = spec.Speed;
                orbit.startAngleDegrees = spec.StartAngle;

                if (spec.Id == PlanetId.Saturn)
                    AddRing(planet.transform, spec.Radius * 2.8f);
            }

            PlanetRegistry.ResetAllForLevel(visualMode);
            return root.transform;
        }

        static void AddRing(Transform planet, float size)
        {
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "Ring";
            ring.transform.SetParent(planet, false);
            ring.transform.localScale = new Vector3(size, 0.0025f, size);
            ring.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            ApplyPlanetColor(ring, new Color(0.82f, 0.76f, 0.58f, 0.75f));
            RemoveCollider(ring);
        }

        static Material GetSharedBodyMaterial()
        {
            if (sharedBodyMaterial != null)
                return sharedBodyMaterial;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            sharedBodyMaterial = new Material(shader);
            sharedBodyMaterial.enableInstancing = true;
            if (sharedBodyMaterial.HasProperty("_Smoothness"))
                sharedBodyMaterial.SetFloat("_Smoothness", 0.62f);
            return sharedBodyMaterial;
        }

        static void ApplyPlanetColor(GameObject go, Color color)
        {
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer == null)
                return;

            renderer.sharedMaterial = GetSharedBodyMaterial();
            ColorBlock.SetColor(BaseColorId, color);
            ColorBlock.SetColor("_Color", color);
            ColorBlock.SetFloat(SmoothnessId, 0.62f);
            ColorBlock.SetFloat(MetallicId, 0.06f);
            renderer.SetPropertyBlock(ColorBlock);
        }

        static void RemoveCollider(GameObject go)
        {
            Collider col = go.GetComponent<Collider>();
            if (col != null)
                Object.Destroy(col);
        }
    }
}
