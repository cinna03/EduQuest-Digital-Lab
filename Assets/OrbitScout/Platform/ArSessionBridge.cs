using OrbitScout.Platform;
using OrbitScout.Tapping;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace OrbitScout.Platform
{
    /// <summary>
    /// Optional: add to AR rig later. Disabled until play mode is AugmentedReality.
    /// </summary>
    public class ArSessionBridge : MonoBehaviour
    {
        public ARRaycastManager raycastManager;
        public ARPlaneManager planeManager;

        bool placementActive;
        static readonly System.Collections.Generic.List<ARRaycastHit> Hits =
            new System.Collections.Generic.List<ARRaycastHit>();

        void Awake()
        {
            if (raycastManager == null)
                raycastManager = GetComponent<ARRaycastManager>();
            if (planeManager == null)
                planeManager = GetComponent<ARPlaneManager>();
        }

        public void BeginPlacement()
        {
            placementActive = true;
            if (planeManager != null)
                OrbitScoutArPlanePresentation.ShowForPlacementScan(planeManager);
        }

        void Update()
        {
            if (!placementActive || SolarBootstrap.Instance == null)
                return;

            if (planeManager != null && planeManager.trackables.count == 0)
                return;

            if (!PointerPressReader.TryGetPressThisFrame(out Vector2 screenPosition, out _))
                return;

            if (raycastManager == null ||
                !raycastManager.Raycast(screenPosition, Hits, TrackableType.PlaneWithinPolygon))
                return;

            placementActive = false;
            SolarBootstrap.Instance.StartArSessionAt(Hits[0].pose);
        }
    }
}

