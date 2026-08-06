using UnityEngine;

namespace OrbitScout.UI
{
    /// <summary>
    /// Edit mode: World Space canvas (visible in Scene view). Play mode: Screen Space Camera HUD.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [DisallowMultipleComponent]
    public class OrbitScoutHudCanvasDriver : MonoBehaviour
    {
        public const float EditReferenceWidth = 1080f;
        public const float EditReferenceHeight = 1920f;
        public const float EditWorldScale = 0.002f;

        [SerializeField] float planeDistance = 100f;

        void Awake() => ApplyPlayModePresentation();

        void OnDestroy()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
        }

        public void ApplyEditModePresentation()
        {
            Canvas canvas = GetComponent<Canvas>();
            RectTransform rect = (RectTransform)transform;
            if (canvas == null || rect == null)
                return;

            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = null;
            canvas.planeDistance = planeDistance;

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
            Canvas canvas = GetComponent<Canvas>();
            RectTransform rect = (RectTransform)transform;
            if (canvas == null || rect == null)
                return;

            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            rect.localPosition = Vector3.zero;

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.planeDistance = planeDistance;

            Camera cam = Camera.main;
            if (cam != null)
                canvas.worldCamera = cam;
        }

        public void BindWorldCamera() => ApplyPlayModePresentation();
    }
}
