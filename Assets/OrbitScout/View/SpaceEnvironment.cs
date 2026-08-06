using OrbitScout.Platform;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.ARFoundation;

namespace OrbitScout.View
{
    /// <summary>
    /// Editor / desktop presentation: starfield, ambient, camera, optional post FX hookup.
    /// Skips camera override when running AR on device.
    /// </summary>
    public static class SpaceEnvironment
    {
        const string StarfieldName = "OrbitScoutStarfield";

        public static void Ensure()
        {
            EnsureAmbient();
            if (!IsAugmentedRealitySession())
            {
                EnsureCamera();
                EnsureStarfield();
            }
        }

        static bool IsAugmentedRealitySession()
        {
            SolarBootstrap bootstrap = SolarBootstrap.Instance;
            if (bootstrap != null && bootstrap.playMode == SolarPlayMode.AugmentedReality)
                return true;

            return Object.FindAnyObjectByType<ARRaycastManager>() != null
                && Application.isMobilePlatform;
        }

        static void EnsureCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
                return;

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.015f, 0.02f, 0.045f, 1f);
            cam.allowHDR = true;
        }

        static void EnsureAmbient()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.12f, 0.14f, 0.22f);
            RenderSettings.fog = false;
        }

        static void EnsureStarfield()
        {
            if (GameObject.Find(StarfieldName) != null)
                return;

            GameObject starfield = new GameObject(StarfieldName);
            ParticleSystem ps = starfield.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.startLifetime = 120f;
            main.startSpeed = 0.02f;
            main.startSize = 0.025f;
            main.maxParticles = 800;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.85f, 0.9f, 1f, 0.9f));

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 800) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(12f, 6f, 12f);

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.y = 0.005f;

            ParticleSystemRenderer renderer = starfield.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit")
                ?? Shader.Find("Particles/Standard Unlit"));
        }
    }
}
