using UnityEngine;

namespace EduQuest.AR
{
    /// <summary>
    /// Phone (Android/iOS): AR Session + XR Origin active, desktop preview hidden.
    /// Editor/Standalone: desktop table preview + Main Camera.
    /// </summary>
    public class ARPlatformBootstrap : MonoBehaviour
    {
        [SerializeField] GameObject desktopPreviewRoot;
        [SerializeField] GameObject arSession;
        [SerializeField] GameObject xrOrigin;
        [SerializeField] Camera desktopCamera;
        [SerializeField] Camera arCamera;
        [SerializeField] TablePotPlacer placer;
        [SerializeField] bool forceDesktopInEditor = true;

        public bool IsPhoneAr { get; private set; }

        public void Configure(
            GameObject desktop,
            GameObject session,
            GameObject origin,
            Camera deskCam,
            Camera phoneCam,
            TablePotPlacer potPlacer)
        {
            desktopPreviewRoot = desktop;
            arSession = session;
            xrOrigin = origin;
            desktopCamera = deskCam;
            arCamera = phoneCam;
            placer = potPlacer;
            Apply();
        }

        void Awake() => Apply();

        public void Apply()
        {
#if UNITY_ANDROID || UNITY_IOS
            IsPhoneAr = true;
#else
            IsPhoneAr = false;
#endif
#if UNITY_EDITOR
            if (forceDesktopInEditor)
                IsPhoneAr = false;
#endif
            if (desktopPreviewRoot != null)
                desktopPreviewRoot.SetActive(!IsPhoneAr);
            if (arSession != null)
                arSession.SetActive(IsPhoneAr);
            if (xrOrigin != null)
                xrOrigin.SetActive(IsPhoneAr);

            if (desktopCamera != null)
            {
                desktopCamera.enabled = !IsPhoneAr;
                desktopCamera.gameObject.tag = IsPhoneAr ? "Untagged" : "MainCamera";
            }

            if (arCamera != null)
            {
                arCamera.enabled = IsPhoneAr;
                arCamera.gameObject.tag = IsPhoneAr ? "MainCamera" : "Untagged";
            }

            if (placer != null)
            {
                placer.UsePhysicsPlacement = !IsPhoneAr;
                placer.SetCamera(IsPhoneAr && arCamera != null ? arCamera : desktopCamera);
            }
        }
    }
}
