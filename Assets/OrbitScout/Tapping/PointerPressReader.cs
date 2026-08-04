using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace OrbitScout.Tapping
{
    /// <summary>
    /// Reads a single press this frame from touch or mouse, compatible with
    /// Input System-only and legacy Input Manager project settings.
    /// </summary>
    public static class PointerPressReader
    {
        public static bool TryGetPressThisFrame(out Vector2 screenPosition, out int pointerId)
        {
            screenPosition = default;
            pointerId = -1;

#if ENABLE_INPUT_SYSTEM
            if (TryReadTouchPress(out screenPosition, out pointerId))
                return true;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                screenPosition = Mouse.current.position.ReadValue();
                pointerId = -1;
                return true;
            }

            return false;
#else
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    screenPosition = touch.position;
                    pointerId = touch.fingerId;
                    return true;
                }

                return false;
            }

            if (Input.GetMouseButtonDown(0))
            {
                screenPosition = Input.mousePosition;
                pointerId = -1;
                return true;
            }

            return false;
#endif
        }

        /// <summary>
        /// Mouse position every frame; touch position while a finger is on the screen (hover while dragging).
        /// </summary>
        public static bool TryGetPointerScreenPosition(out Vector2 screenPosition, out int pointerId)
        {
            screenPosition = default;
            pointerId = -1;

#if ENABLE_INPUT_SYSTEM
            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen != null)
            {
                TouchControl primary = touchscreen.primaryTouch;
                if (primary.press.isPressed)
                {
                    screenPosition = primary.position.ReadValue();
                    pointerId = primary.touchId.ReadValue();
                    return true;
                }

                foreach (TouchControl touch in touchscreen.touches)
                {
                    if (!touch.press.isPressed)
                        continue;

                    screenPosition = touch.position.ReadValue();
                    pointerId = touch.touchId.ReadValue();
                    return true;
                }
            }

            if (Mouse.current != null)
            {
                screenPosition = Mouse.current.position.ReadValue();
                pointerId = -1;
                return true;
            }

            return false;
#else
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                screenPosition = touch.position;
                pointerId = touch.fingerId;
                return true;
            }

            screenPosition = Input.mousePosition;
            pointerId = -1;
            return true;
#endif
        }

#if ENABLE_INPUT_SYSTEM
        static bool TryReadTouchPress(out Vector2 screenPosition, out int pointerId)
        {
            screenPosition = default;
            pointerId = -1;

            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen == null)
                return false;

            TouchControl primary = touchscreen.primaryTouch;
            if (primary.press.wasPressedThisFrame)
            {
                screenPosition = primary.position.ReadValue();
                pointerId = primary.touchId.ReadValue();
                return true;
            }

            foreach (TouchControl touch in touchscreen.touches)
            {
                if (!touch.press.wasPressedThisFrame)
                    continue;

                screenPosition = touch.position.ReadValue();
                pointerId = touch.touchId.ReadValue();
                return true;
            }

            return false;
        }
#endif
    }
}
