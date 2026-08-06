using UnityEngine;
using UnityEngine.UI;

namespace OrbitScout.UI
{
    /// <summary>
    /// Edit mode: World Space canvas (visible in Scene view).
    /// Play mode: root Screen Space Overlay so the HUD always draws in Game view.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [DisallowMultipleComponent]
    public class OrbitScoutHudCanvasDriver : MonoBehaviour
    {
        public const float EditReferenceWidth = 1080f;
        public const float EditReferenceHeight = 1920f;
        public const float EditWorldScale = 0.002f;

        void Awake() => ApplyPlayModePresentation();

        void Start() => ApplyPlayModePresentation();

        public void ApplyEditModePresentation()
        {
            Canvas canvas = GetComponent<Canvas>();
            RectTransform rect = (RectTransform)transform;
            if (canvas == null || rect == null)
                return;

            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = null;
            canvas.planeDistance = 1f;

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(EditReferenceWidth, EditReferenceHeight);
            rect.localScale = Vector3.one * EditWorldScale;
            rect.localRotation = Quaternion.identity;
            rect.localPosition = Vector3.zero;
        }

        public void ApplyPlayModePresentation()
        {
            if (!Application.isPlaying)
                return;

            // Overlay canvases must not live under a 3D transform (XR Origin / OrbitScout)
            if (transform.parent != null)
                transform.SetParent(null, false);

            Canvas canvas = GetComponent<Canvas>();
            RectTransform rect = (RectTransform)transform;
            if (canvas == null || rect == null)
                return;

            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            rect.localPosition = Vector3.zero;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.sizeDelta = Vector2.zero;

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.worldCamera = null;
            canvas.sortingOrder = 5000;
            canvas.enabled = true;
            // TMP needs these channels or labels disappear
            canvas.additionalShaderChannels =
                AdditionalCanvasShaderChannels.TexCoord1
                | AdditionalCanvasShaderChannels.Normal
                | AdditionalCanvasShaderChannels.Tangent;

            CanvasScaler scaler = GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(EditReferenceWidth, EditReferenceHeight);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }

            gameObject.SetActive(true);
        }

        public void BindWorldCamera() => ApplyPlayModePresentation();
    }
}
