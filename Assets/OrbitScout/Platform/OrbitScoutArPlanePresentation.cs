using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Samples.ARStarterAssets;
using UnityEngine.XR.Templates.AR;

namespace OrbitScout.Platform
{
    /// <summary>
    /// Hides AR plane / surface debug meshes after the solar system is placed.
    /// </summary>
    public static class OrbitScoutArPlanePresentation
    {
        public static void FadeOutAfterPlacement(ARPlaneManager planeManager)
        {
            if (planeManager == null)
                return;

            OrbitScoutPlaneVisibilityController controller =
                planeManager.GetComponent<OrbitScoutPlaneVisibilityController>();
            if (controller == null)
                controller = planeManager.gameObject.AddComponent<OrbitScoutPlaneVisibilityController>();

            controller.HideAllSurfaces(planeManager);
        }

        public static void ShowForPlacementScan(ARPlaneManager planeManager)
        {
            if (planeManager == null)
                return;

            OrbitScoutPlaneVisibilityController controller =
                planeManager.GetComponent<OrbitScoutPlaneVisibilityController>();
            if (controller == null)
                controller = planeManager.gameObject.AddComponent<OrbitScoutPlaneVisibilityController>();

            controller.ShowSurfacesForScanning(planeManager);
        }
    }

    sealed class OrbitScoutPlaneVisibilityController : MonoBehaviour
    {
        const float EnforceHideSeconds = 3f;

        ARPlaneManager planeManager;
        bool hideSurfaces;
        GameObject savedPlanePrefab;
        PlaneDetectionMode savedDetectionMode = PlaneDetectionMode.Horizontal;

        public void HideAllSurfaces(ARPlaneManager manager)
        {
            planeManager = manager;
            hideSurfaces = true;

            if (savedPlanePrefab == null && manager.planePrefab != null)
                savedPlanePrefab = manager.planePrefab;

            savedDetectionMode = manager.requestedDetectionMode;
            manager.requestedDetectionMode = PlaneDetectionMode.None;
            manager.planePrefab = null;

            SuppressTemplatePlaneUi();
            manager.trackablesChanged.RemoveListener(OnPlanesChanged);
            manager.trackablesChanged.AddListener(OnPlanesChanged);

            foreach (ARPlane plane in manager.trackables)
                HidePlaneVisuals(plane);

            StopAllCoroutines();
            StartCoroutine(EnforceHideRoutine());
        }

        public void ShowSurfacesForScanning(ARPlaneManager manager)
        {
            planeManager = manager;
            hideSurfaces = false;
            StopAllCoroutines();

            manager.trackablesChanged.RemoveListener(OnPlanesChanged);

            if (savedPlanePrefab != null)
                manager.planePrefab = savedPlanePrefab;

            manager.requestedDetectionMode = savedDetectionMode == PlaneDetectionMode.None
                ? PlaneDetectionMode.Horizontal
                : savedDetectionMode;

            foreach (ARPlane plane in manager.trackables)
                ShowPlaneVisuals(plane);
        }

        void OnDestroy()
        {
            if (planeManager != null)
                planeManager.trackablesChanged.RemoveListener(OnPlanesChanged);
        }

        void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> changes)
        {
            if (!hideSurfaces)
                return;

            foreach (ARPlane added in changes.added)
                HidePlaneVisuals(added);

            foreach (ARPlane updated in changes.updated)
                HidePlaneVisuals(updated);
        }

        IEnumerator EnforceHideRoutine()
        {
            float elapsed = 0f;
            while (elapsed < EnforceHideSeconds && hideSurfaces && planeManager != null)
            {
                foreach (ARPlane plane in planeManager.trackables)
                    HidePlaneVisuals(plane);

                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        void LateUpdate()
        {
            if (!hideSurfaces || planeManager == null)
                return;

            foreach (ARPlane plane in planeManager.trackables)
                HidePlaneVisuals(plane);
        }

        static void SuppressTemplatePlaneUi()
        {
            foreach (ARTemplateMenuManager menu in Object.FindObjectsByType<ARTemplateMenuManager>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                menu.enabled = false;
            }

            foreach (GoalManager goal in Object.FindObjectsByType<GoalManager>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                goal.enabled = false;
            }

            GameObject debugMenu = GameObject.Find("DebugMenu");
            if (debugMenu != null)
                debugMenu.SetActive(false);
        }

        static void HidePlaneVisuals(ARPlane plane)
        {
            if (plane == null)
                return;

            ARPlaneMeshVisualizer meshVisualizer = plane.GetComponent<ARPlaneMeshVisualizer>();
            if (meshVisualizer != null)
                meshVisualizer.enabled = false;

            ARFeatheredPlaneMeshVisualizer feathered = plane.GetComponent<ARFeatheredPlaneMeshVisualizer>();
            if (feathered != null)
                feathered.enabled = false;

            ARPlaneMeshVisualizerFader fader = plane.GetComponent<ARPlaneMeshVisualizerFader>();
            if (fader != null)
            {
                fader.enabled = false;
                fader.SetVisualsImmediate(0f);
            }

            foreach (Renderer renderer in plane.GetComponentsInChildren<Renderer>(true))
                renderer.enabled = false;

            foreach (LineRenderer line in plane.GetComponentsInChildren<LineRenderer>(true))
                line.enabled = false;
        }

        static void ShowPlaneVisuals(ARPlane plane)
        {
            if (plane == null)
                return;

            ARPlaneMeshVisualizer meshVisualizer = plane.GetComponent<ARPlaneMeshVisualizer>();
            if (meshVisualizer != null)
                meshVisualizer.enabled = true;

            ARFeatheredPlaneMeshVisualizer feathered = plane.GetComponent<ARFeatheredPlaneMeshVisualizer>();
            if (feathered != null)
                feathered.enabled = true;

            ARPlaneMeshVisualizerFader fader = plane.GetComponent<ARPlaneMeshVisualizerFader>();
            if (fader != null)
            {
                fader.enabled = true;
                fader.visualizeSurfaces = true;
            }

            foreach (Renderer renderer in plane.GetComponentsInChildren<Renderer>(true))
                renderer.enabled = true;
        }
    }
}
