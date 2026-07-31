using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace EduQuest.Lab
{
    /// <summary>Raycasts taps/clicks onto ChemicalBottle / CrystalBeaker colliders.</summary>
    public class LabTapSelector : MonoBehaviour
    {
        [SerializeField] Camera rayCamera;
        [SerializeField] LayerMask mask = ~0;

        public event Action<ChemicalBottle> BottleTapped;
        public event Action BeakerTapped;

        ChemicalBottle m_Selected;

        public ChemicalBottle Selected => m_Selected;

        public void Configure(Camera cam) => rayCamera = cam;

        public void ClearSelection()
        {
            if (m_Selected != null) m_Selected.SetSelected(false);
            m_Selected = null;
        }

        void Update()
        {
            if (rayCamera == null) rayCamera = Camera.main;
            if (rayCamera == null) return;

            Vector2 screen;
            bool pressed = false;
            var touch = Touchscreen.current;
            var mouse = Mouse.current;

            if (touch != null && touch.primaryTouch.press.wasPressedThisFrame)
            {
                screen = touch.primaryTouch.position.ReadValue();
                pressed = true;
            }
            else if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                screen = mouse.position.ReadValue();
                pressed = true;
            }
            else return;

            if (!pressed) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            var ray = rayCamera.ScreenPointToRay(screen);
            if (!Physics.Raycast(ray, out var hit, 40f, mask))
                return;

            var bottle = hit.collider.GetComponentInParent<ChemicalBottle>();
            if (bottle != null)
            {
                if (m_Selected != null && m_Selected != bottle)
                    m_Selected.SetSelected(false);
                m_Selected = bottle;
                m_Selected.SetSelected(true);
                BottleTapped?.Invoke(bottle);
                return;
            }

            if (hit.collider.GetComponentInParent<EduQuest.Experiments.CrystalBeaker>() != null)
                BeakerTapped?.Invoke();
        }
    }
}
