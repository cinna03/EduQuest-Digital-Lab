using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace OrbitScout.Platform
{
    /// <summary>
    /// On device AR: live camera only while playing (placement + quiz). Menus use solid space UI.
    /// </summary>
    public static class OrbitScoutArCameraPresentation
    {
        static readonly Color MenuCameraColor = new Color(0.03f, 0.05f, 0.11f, 1f);
        static bool liveFeedEnabled = true;

        public static void SetLiveFeedEnabled(bool enabled)
        {
            SolarBootstrap bootstrap = SolarBootstrap.Instance;
            if (bootstrap == null || bootstrap.playMode != SolarPlayMode.AugmentedReality)
                return;

            liveFeedEnabled = enabled;
            Camera cam = Camera.main;
            if (cam == null)
            {
                // XR Origin camera may not be tagged MainCamera yet
                foreach (Camera c in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
                {
                    if (c != null && c.enabled)
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
                background.enabled = enabled;

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = enabled ? Color.black : MenuCameraColor;
        }

        public static void ApplyMenuPresentation()
        {
            SetLiveFeedEnabled(false);
        }

        public static void ApplyLevelPresentation()
        {
            SetLiveFeedEnabled(true);
        }
    }
}
