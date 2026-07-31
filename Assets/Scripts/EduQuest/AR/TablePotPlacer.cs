using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace EduQuest.AR
{
    /// <summary>
    /// Tap/click a detected table plane to place a prefab once (pot, beaker, etc.).
    /// Desktop/camera preview: Physics raycast onto the table collider.
    /// </summary>
    public class TablePotPlacer : MonoBehaviour
    {
        [SerializeField] Camera rayCamera;
        [SerializeField] GameObject potPrefab;
        [SerializeField] LayerMask tableMask = ~0;
        [SerializeField] bool placementEnabled;

        GameObject m_Placed;
        bool m_HasPot;

        public bool HasPot => m_HasPot;
        public GameObject PlacedObject => m_Placed;
        /// <summary>Legacy accessor for germination lab.</summary>
        public GerminationPot Pot => m_Placed != null ? m_Placed.GetComponent<GerminationPot>() : null;
        public bool PlacementEnabled
        {
            get => placementEnabled;
            set => placementEnabled = value;
        }

        public void Configure(Camera cam, GameObject prefab)
        {
            rayCamera = cam;
            potPrefab = prefab;
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
            if (!placementEnabled || m_HasPot || rayCamera == null) return;

            var mouse = Mouse.current;
            var touch = Touchscreen.current;

            Vector2 screenPos;
            bool pressed = false;

            if (touch != null && touch.primaryTouch.press.wasPressedThisFrame)
            {
                screenPos = touch.primaryTouch.position.ReadValue();
                pressed = true;
            }
            else if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                screenPos = mouse.position.ReadValue();
                pressed = true;
            }
            else return;

            if (!pressed) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            var ray = rayCamera.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out var hit, 40f, tableMask))
            {
                var pose = new Pose(hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
                PlaceOnPose(pose);
            }
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
