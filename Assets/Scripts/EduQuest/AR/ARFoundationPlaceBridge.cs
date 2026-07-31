using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace EduQuest.AR
{
    /// <summary>
    /// Phone AR: tap a detected plane to place the beaker once.
    /// </summary>
    [RequireComponent(typeof(ARRaycastManager))]
    public class ARFoundationPlaceBridge : MonoBehaviour
    {
        [SerializeField] TablePotPlacer placer;
        [SerializeField] ARPlaneManager planeManager;
        [SerializeField] bool hidePlanesAfterPlace = true;

        static readonly List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
        ARRaycastManager m_Raycasts;

        public event Action<GameObject> Placed;

        public void Configure(TablePotPlacer potPlacer, ARPlaneManager planes)
        {
            placer = potPlacer;
            planeManager = planes;
        }

        public void ResetPlanes()
        {
            if (planeManager == null) return;
            planeManager.enabled = true;
            foreach (var plane in planeManager.trackables)
                plane.gameObject.SetActive(true);
        }

        void Awake()
        {
            m_Raycasts = GetComponent<ARRaycastManager>();
            if (planeManager == null)
                planeManager = GetComponent<ARPlaneManager>();
        }

        void Update()
        {
            if (!isActiveAndEnabled) return;
            if (placer == null || !placer.PlacementEnabled || placer.HasPot)
                return;

            if (!TryGetPress(out var screenPos))
                return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (!m_Raycasts.Raycast(screenPos, s_Hits, TrackableType.PlaneWithinPolygon))
                return;

            var pose = s_Hits[0].pose;
            // Keep upright on horizontal tables
            pose.rotation = Quaternion.Euler(0f, pose.rotation.eulerAngles.y, 0f);

            var go = placer.PlaceOnPose(pose);
            if (go == null) return;

            if (hidePlanesAfterPlace && planeManager != null)
            {
                foreach (var plane in planeManager.trackables)
                    plane.gameObject.SetActive(false);
                planeManager.enabled = false;
            }

            Placed?.Invoke(go);
        }

        static bool TryGetPress(out Vector2 screenPos)
        {
            screenPos = default;
            var touch = Touchscreen.current;
            var mouse = Mouse.current;
            if (touch != null && touch.primaryTouch.press.wasPressedThisFrame)
            {
                screenPos = touch.primaryTouch.position.ReadValue();
                return true;
            }
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                screenPos = mouse.position.ReadValue();
                return true;
            }
            return false;
        }
    }
}
