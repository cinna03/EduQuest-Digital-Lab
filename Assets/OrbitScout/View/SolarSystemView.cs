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
            public float StartAngle;
        }

        static readonly PlanetSpec[] Specs =
        {
            new PlanetSpec { Id = PlanetId.Mercury, Radius = 0.04f, Orbit = 0.22f, Speed = 38f, StartAngle = 10f },
            new PlanetSpec { Id = PlanetId.Venus, Radius = 0.05f, Orbit = 0.30f, Speed = 30f, StartAngle = 55f },
            new PlanetSpec { Id = PlanetId.Earth, Radius = 0.052f, Orbit = 0.38f, Speed = 26f, StartAngle = 120f },
            new PlanetSpec { Id = PlanetId.Mars, Radius = 0.045f, Orbit = 0.46f, Speed = 22f, StartAngle = 200f },
            new PlanetSpec { Id = PlanetId.Jupiter, Radius = 0.11f, Orbit = 0.56f, Speed = 14f, StartAngle = 260f },
            new PlanetSpec { Id = PlanetId.Saturn, Radius = 0.095f, Orbit = 0.66f, Speed = 11f, StartAngle = 310f },
            new PlanetSpec { Id = PlanetId.Uranus, Radius = 0.07f, Orbit = 0.76f, Speed = 9f, StartAngle = 15f },
            new PlanetSpec { Id = PlanetId.Neptune, Radius = 0.068f, Orbit = 0.86f, Speed = 7f, StartAngle = 140f }
        };

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

                Renderer renderer = planet.GetComponent<Renderer>();
                PlanetMaterials.ApplyBody(renderer, spec.Id);

                SphereCollider collider = planet.GetComponent<SphereCollider>();
                collider.radius = 0.55f;

                Color fallback = PlanetMaterials.FallbackColor(spec.Id);
                PlanetBody body = planet.AddComponent<PlanetBody>();
                body.planetId = spec.Id;
                body.Initialize(fallback);
                PlanetRegistry.Register(body);

                PlanetOrbit orbit = planet.AddComponent<PlanetOrbit>();
                orbit.orbitCenter = sun.transform;
                orbit.orbitRadius = spec.Orbit;
                orbit.degreesPerSecond = spec.Speed;
                orbit.startAngleDegrees = spec.StartAngle;

                if (spec.Id == PlanetId.Saturn)
                    AddRing(planet.transform, spec.Radius * 2.85f);
            }

            PlanetRegistry.ResetAllForLevel(visualMode);
            return root.transform;
        }

        static void AddRing(Transform planet, float diameter)
        {
            GameObject ring = new GameObject("Ring", typeof(MeshFilter), typeof(MeshRenderer));
            ring.transform.SetParent(planet, false);
            ring.transform.localPosition = Vector3.zero;
            ring.transform.localRotation = Quaternion.Euler(28f, 0f, 0f);
            ring.transform.localScale = Vector3.one * diameter;

            MeshFilter filter = ring.GetComponent<MeshFilter>();
            filter.sharedMesh = PlanetMaterials.GetRingDiscMesh();

            MeshRenderer renderer = ring.GetComponent<MeshRenderer>();
            PlanetMaterials.ApplyRings(renderer);
        }
    }
}
