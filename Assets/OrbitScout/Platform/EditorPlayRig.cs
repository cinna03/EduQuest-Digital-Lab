using OrbitScout.View;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OrbitScout.Platform
{
    public static class EditorPlayRig
    {
        public static void Ensure()
        {
            EnsureEventSystem();
            SpaceEnvironment.Ensure();
            EnsureLight();
            EnsureArenaPad();
            EnsureCamera();
        }

        public static void FocusCameraOn(Transform target)
        {
            Camera cam = Camera.main;
            if (cam == null || target == null)
                return;

            cam.transform.position = target.position + new Vector3(0f, 1.05f, -1.65f);
            cam.transform.LookAt(target.position + Vector3.up * 0.15f);
        }

        static void EnsureEventSystem()
        {
            OrbitScoutUiInputSetup.EnsureEventSystem();
        }

        static void EnsureLight()
        {
            if (Object.FindAnyObjectByType<Light>() != null)
                return;

            GameObject lightObject = new GameObject("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        static void EnsureArenaPad()
        {
            if (GameObject.Find("EditorFloor") != null)
                return;

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            floor.name = "EditorFloor";
            floor.transform.position = new Vector3(0f, -0.02f, 0f);
            floor.transform.localScale = new Vector3(0.45f, 0.004f, 0.45f);

            Renderer renderer = floor.GetComponent<Renderer>();
            if (renderer != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                Material mat = new Material(shader);
                mat.color = new Color(0.06f, 0.08f, 0.14f, 1f);
                if (mat.HasProperty("_Smoothness"))
                    mat.SetFloat("_Smoothness", 0.85f);
                if (mat.HasProperty("_Metallic"))
                    mat.SetFloat("_Metallic", 0.35f);
                renderer.material = mat;
            }
        }

        public static void EnsureCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObject = new GameObject("Main Camera");
                camObject.tag = "MainCamera";
                cam = camObject.AddComponent<Camera>();
                camObject.AddComponent<AudioListener>();
            }

            if (cam.GetComponent<OrbitScout.Tapping.PlanetTapInput>() == null)
                cam.gameObject.AddComponent<OrbitScout.Tapping.PlanetTapInput>();

            if (cam.GetComponent<OrbitScout.View.PlanetNameHoverLabel>() == null)
                cam.gameObject.AddComponent<OrbitScout.View.PlanetNameHoverLabel>();
        }
    }
}
