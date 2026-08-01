using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace EduQuest
{
    /// <summary>Phone AR: find a horizontal plane, then place the campaign arena once.</summary>
    [RequireComponent(typeof(ARRaycastManager))]
    public class ArTablePlacer : MonoBehaviour
    {
        [SerializeField] ARPlaneManager planeManager;
        [SerializeField] bool armed;

        static readonly List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
        ARRaycastManager m_Raycast;
        bool m_ReportedPlane;

        public event Action PlanesFound;
        public event Action<Pose> Placed;

        public bool HasPlanes => planeManager != null && planeManager.trackables.count > 0;
        public bool Armed { get => armed; set => armed = value; }

        public void Configure(ARPlaneManager planes) => planeManager = planes;

        void Awake()
        {
            m_Raycast = GetComponent<ARRaycastManager>();
            if (planeManager == null) planeManager = GetComponent<ARPlaneManager>();
            if (planeManager != null)
                planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
        }

        void Update()
        {
            if (!isActiveAndEnabled) return;

            if (!m_ReportedPlane && HasPlanes)
            {
                m_ReportedPlane = true;
                PlanesFound?.Invoke();
            }

            if (!armed) return;
            if (!TryPress(out var screen)) return;
            if (IsOverUi(screen)) return;

            var mask = TrackableType.PlaneWithinPolygon
                       | TrackableType.PlaneWithinBounds
                       | TrackableType.PlaneEstimated;
            if (!m_Raycast.Raycast(screen, s_Hits, mask)) return;

            var pose = s_Hits[0].pose;
            pose.rotation = Quaternion.Euler(0f, pose.rotation.eulerAngles.y, 0f);
            Place(pose);
        }

        public bool TryPlaceOnBestPlane()
        {
            if (planeManager == null || !HasPlanes) return false;
            ARPlane best = null;
            float bestArea = -1f;
            foreach (var p in planeManager.trackables)
            {
                float area = p.size.x * p.size.y;
                if (area > bestArea)
                {
                    bestArea = area;
                    best = p;
                }
            }
            if (best == null) return false;
            Place(new Pose(best.center, Quaternion.Euler(0f, best.transform.eulerAngles.y, 0f)));
            return true;
        }

        void Place(Pose pose)
        {
            armed = false;
            if (planeManager != null)
            {
                foreach (var p in planeManager.trackables)
                    p.gameObject.SetActive(false);
                planeManager.enabled = false;
            }
            Placed?.Invoke(pose);
        }

        public void ResetTracking()
        {
            m_ReportedPlane = false;
            armed = false;
            if (planeManager == null) return;
            planeManager.enabled = true;
            planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
            foreach (var p in planeManager.trackables)
                p.gameObject.SetActive(true);
        }

        static bool TryPress(out Vector2 screen)
        {
            screen = default;
            var touch = Touchscreen.current;
            var mouse = Mouse.current;
            if (touch != null && touch.primaryTouch.press.wasPressedThisFrame)
            {
                screen = touch.primaryTouch.position.ReadValue();
                return true;
            }
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                screen = mouse.position.ReadValue();
                return true;
            }
            return false;
        }

        static bool IsOverUi(Vector2 screen)
        {
            if (EventSystem.current == null) return false;
            var ped = new PointerEventData(EventSystem.current) { position = screen };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(ped, results);
            return results.Count > 0;
        }
    }
}
