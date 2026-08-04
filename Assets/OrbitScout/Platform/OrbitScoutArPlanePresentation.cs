using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit.Samples.ARStarterAssets;
using UnityEngine.XR.Templates.AR;

namespace OrbitScout.Platform
{
    /// <summary>
    /// After Orbit Scout placement, fade out AR plane / surface debug meshes so only the quiz content remains.
    /// </summary>
    public static class OrbitScoutArPlanePresentation
    {
        public static void FadeOutAfterPlacement(ARPlaneManager planeManager)
        {
            if (planeManager == null)
                return;

            OrbitScoutPlaneFadeDriver driver = planeManager.GetComponent<OrbitScoutPlaneFadeDriver>();
            if (driver == null)
                driver = planeManager.gameObject.AddComponent<OrbitScoutPlaneFadeDriver>();

            driver.Begin(planeManager);
        }

        public static void ShowForPlacementScan(ARPlaneManager planeManager)
        {
            OrbitScoutPlaneFadeDriver.RestoreScanningVisuals(planeManager);
        }
    }

    sealed class OrbitScoutPlaneFadeDriver : MonoBehaviour
    {
        const float FadeSeconds = 0.85f;

        GameObject savedPlanePrefab;

        public void Begin(ARPlaneManager planeManager)
        {
            if (savedPlanePrefab == null && planeManager.planePrefab != null)
                savedPlanePrefab = planeManager.planePrefab;

            StopAllCoroutines();
            StartCoroutine(FadeRoutine(planeManager));
        }

        public static void RestoreScanningVisuals(ARPlaneManager planeManager)
        {
            if (planeManager == null)
                return;

            OrbitScoutPlaneFadeDriver driver = planeManager.GetComponent<OrbitScoutPlaneFadeDriver>();
            if (driver != null && driver.savedPlanePrefab != null)
                planeManager.planePrefab = driver.savedPlanePrefab;

            foreach (ARPlane plane in planeManager.trackables)
            {
                MeshRenderer renderer = plane.GetComponent<MeshRenderer>();
                if (renderer != null)
                    renderer.enabled = true;

                ARPlaneMeshVisualizer meshVisualizer = plane.GetComponent<ARPlaneMeshVisualizer>();
                if (meshVisualizer != null)
                    meshVisualizer.enabled = true;

                ARPlaneMeshVisualizerFader fader = plane.GetComponent<ARPlaneMeshVisualizerFader>();
                if (fader != null)
                {
                    fader.enabled = true;
                    fader.visualizeSurfaces = true;
                }

                ARFeatheredPlaneMeshVisualizer feathered = plane.GetComponent<ARFeatheredPlaneMeshVisualizer>();
                if (feathered != null)
                    feathered.enabled = true;
            }
        }

        IEnumerator FadeRoutine(ARPlaneManager planeManager)
        {
            foreach (ARPlane plane in planeManager.trackables)
                BeginPlaneFade(plane);

            planeManager.planePrefab = null;

            GameObject debugMenu = GameObject.Find("DebugMenu");
            if (debugMenu != null)
                debugMenu.SetActive(false);

            yield return new WaitForSeconds(FadeSeconds);

            foreach (ARPlane plane in planeManager.trackables)
                DisablePlaneVisuals(plane);
        }

        static void BeginPlaneFade(ARPlane plane)
        {
            if (plane == null)
                return;

            ARFeatheredPlaneMeshVisualizer feathered = plane.GetComponent<ARFeatheredPlaneMeshVisualizer>();
            if (feathered != null)
                feathered.enabled = false;

            MeshRenderer renderer = plane.GetComponent<MeshRenderer>();
            ARPlaneMeshVisualizerFader fader = plane.GetComponent<ARPlaneMeshVisualizerFader>();
            if (fader != null)
            {
                fader.fadeSpeed = 1.15f;
                fader.visualizeSurfaces = false;
            }
            else if (renderer != null)
            {
                OrbitScoutSimpleRendererFade fade = renderer.GetComponent<OrbitScoutSimpleRendererFade>();
                if (fade == null)
                    fade = renderer.gameObject.AddComponent<OrbitScoutSimpleRendererFade>();
                fade.Run(FadeSeconds);
            }
        }

        static void DisablePlaneVisuals(ARPlane plane)
        {
            if (plane == null)
                return;

            ARPlaneMeshVisualizer meshVisualizer = plane.GetComponent<ARPlaneMeshVisualizer>();
            if (meshVisualizer != null)
                meshVisualizer.enabled = false;

            ARPlaneMeshVisualizerFader fader = plane.GetComponent<ARPlaneMeshVisualizerFader>();
            if (fader != null)
                fader.enabled = false;

            MeshRenderer renderer = plane.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.enabled = false;
        }
    }

    sealed class OrbitScoutSimpleRendererFade : MonoBehaviour
    {
        MeshRenderer targetRenderer;
        Material material;
        int alphaId;
        float duration;
        float elapsed;

        public void Run(float seconds)
        {
            targetRenderer = GetComponent<MeshRenderer>();
            if (targetRenderer == null)
                return;

            duration = seconds;
            elapsed = 0f;
            enabled = true;
            material = targetRenderer.material;
            alphaId = Shader.PropertyToID("_PlaneAlpha");
            if (!material.HasProperty(alphaId))
                alphaId = Shader.PropertyToID("_BaseColor");
        }

        void Update()
        {
            if (material == null || duration <= 0f)
                return;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float alpha = 1f - t;

            if (material.HasProperty("_PlaneAlpha"))
                material.SetFloat("_PlaneAlpha", alpha);
            else if (material.HasProperty("_BaseColor"))
            {
                Color color = material.GetColor("_BaseColor");
                color.a = alpha;
                material.SetColor("_BaseColor", color);
            }

            if (t >= 1f)
            {
                targetRenderer.enabled = false;
                enabled = false;
            }
        }
    }
}
