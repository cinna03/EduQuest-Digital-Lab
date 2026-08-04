using OrbitScout.Core;
using OrbitScout.Platform;
using OrbitScout.UI;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// Ensures the scene works even if MonoBehaviours were not saved on the OrbitScout object.
/// Auto-detects AR rig and wires placement.
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
            OrbitScoutArTemplateSuppress.Apply();
            OrbitScoutArCameraPresentation.ApplyMenuPresentation();
        }
        else
        {
            bootstrap.playMode = SolarPlayMode.EditorDesktop;
        }

        if (GetComponent<MissionHud>() == null)
            gameObject.AddComponent<MissionHud>();
    }
}
