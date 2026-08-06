using OrbitScout.Core;
using OrbitScout.Platform;
using OrbitScout.UI;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// Wires Orbit Scout into SampleScene (AR) or EditorTest (desktop).
/// Menus use the same Overlay HUD; AR mode keeps the live camera feed available.
/// </summary>
public class OrbitScoutSceneEntry : MonoBehaviour
{
    void Awake()
    {
        if (GetComponent<MissionController>() == null)
            gameObject.AddComponent<MissionController>();

        SolarBootstrap bootstrap = GetComponent<SolarBootstrap>();
        if (bootstrap == null)
            bootstrap = gameObject.AddComponent<SolarBootstrap>();

        ARRaycastManager raycastManager = FindAnyObjectByType<ARRaycastManager>();
        if (raycastManager != null)
        {
            bootstrap.playMode = SolarPlayMode.AugmentedReality;

            ArSessionBridge bridge = raycastManager.GetComponent<ArSessionBridge>();
            if (bridge == null)
                bridge = raycastManager.gameObject.AddComponent<ArSessionBridge>();
            bridge.raycastManager = raycastManager;
            bridge.planeManager = raycastManager.GetComponent<ARPlaneManager>();

            // Keep AR session + camera background running for live feed
            EnsureArCameraLive();
            OrbitScoutArTemplateSuppress.Apply();
            // Menu still shows branded panels; camera feed is behind / used after Start
            OrbitScoutArCameraPresentation.ApplyMenuPresentation();
        }
        else
        {
            bootstrap.playMode = SolarPlayMode.EditorDesktop;
        }

        if (GetComponent<MissionHud>() == null)
            gameObject.AddComponent<MissionHud>();
    }

    static void EnsureArCameraLive()
    {
        foreach (ARSession session in FindObjectsByType<ARSession>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            session.enabled = true;

        Camera cam = Camera.main;
        if (cam == null)
        {
            foreach (Camera c in FindObjectsByType<Camera>(FindObjectsSortMode.None))
            {
                if (c != null && c.GetComponent<ARCameraBackground>() != null)
                {
                    cam = c;
                    break;
                }
            }
        }

        if (cam == null)
            return;

        ARCameraBackground background = cam.GetComponent<ARCameraBackground>();
        if (background != null)
            background.enabled = true;
    }
}
