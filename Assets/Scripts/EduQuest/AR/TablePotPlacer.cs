using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace EduQuest.AR
{
    /// <summary>
    /// Places a prefab once on a table.
    /// Desktop: Physics raycast. Phone AR: ARFoundationPlaceBridge → PlaceOnPose.
    /// </summary>
    public class TablePotPlacer : MonoBehaviour
    {
        [SerializeField] Camera rayCamera;
        [SerializeField] GameObject potPrefab;
        [SerializeField] LayerMask tableMask = ~0;
        [SerializeField] bool placementEnabled;
        [SerializeField] bool usePhysicsPlacement = true;

        GameObject m_Placed;
        bool m_HasPot;

        public bool HasPot => m_HasPot;
        public GameObject PlacedObject => m_Placed;
        public GerminationPot Pot => m_Placed != null ? m_Placed.GetComponent<GerminationPot>() : null;
        public bool UsePhysicsPlacement
        {
            get => usePhysicsPlacement;
            set => usePhysicsPlacement = value;
        }

        public bool PlacementEnabled
        {
            get => placementEnabled;
            set => placementEnabled = value;
        }

        public void Configure(Camera cam, GameObject prefab)
        {
            if (cam != null) rayCamera = cam;
            if (prefab != null) potPrefab = prefab;
        }

        public void SetCamera(Camera cam)
        {
            if (cam != null) rayCamera = cam;
        }

        public GameObject PlaceOnPose(Pose pose)
        {
            if (m_HasPot || potPrefab == null) return m_Placed;

            m_Placed = Instantiate(potPrefab, pose.position, pose.rotation);
            m_Placed.name = potPrefab.name + "_Placed";
            m_HasPot = true;
            placementEnabled = false;
            return m_Placed;
        }

        void Update()
        {
            if (!usePhysicsPlacement) return;
            if (!placementEnabled || m_HasPot || rayCamera == null) return;

            if (!TryGetPress(out var screenPos)) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            var ray = rayCamera.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out var hit, 40f, tableMask))
            {
                var pose = new Pose(hit.point, Quaternion.Euler(0f, 0f, 0f));
                PlaceOnPose(pose);
            }
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

        public void ResetPlacement()
        {
            if (m_Placed != null)
                Destroy(m_Placed);
            m_Placed = null;
            m_HasPot = false;
        }
    }
}
