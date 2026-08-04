using OrbitScout.Core;
using OrbitScout.Tapping;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OrbitScout.View
{
    /// <summary>
    /// Screen-space planet name while the pointer or finger is over that planet.
    /// </summary>
    public class PlanetNameHoverLabel : MonoBehaviour
    {
        public Camera viewCamera;
        public float maxRange = 40f;
        public bool hoverEnabled = true;

        Canvas canvas;
        RectTransform pillRect;
        TMP_Text label;

        void Awake()
        {
            EnsureUi();
            Hide();
        }

        void LateUpdate()
        {
            if (!hoverEnabled || label == null)
                return;

            MissionController mission = MissionController.Instance;
            if (mission == null || !mission.IsPlaying)
            {
                Hide();
                return;
            }

            if (!PointerPressReader.TryGetPointerScreenPosition(out Vector2 screenPosition, out int pointerId))
            {
                Hide();
                return;
            }

            if (PlanetPointerHelpers.IsOverUiButton(screenPosition, pointerId))
            {
                Hide();
                return;
            }

            Camera cam = viewCamera != null ? viewCamera : Camera.main;
            if (!PlanetPointerHelpers.TryPickPlanet(cam, screenPosition, maxRange, out PlanetBody planet))
            {
                Hide();
                return;
            }

            Show(planet, cam);
        }

        void Show(PlanetBody planet, Camera cam)
        {
            if (cam == null)
            {
                Hide();
                return;
            }

            Vector3 world = PlanetPointerHelpers.GetLabelWorldPosition(planet);
            Vector3 screen = cam.WorldToScreenPoint(world);
            if (screen.z <= 0f)
            {
                Hide();
                return;
            }

            label.text = planet.planetId.ToString();
            pillRect.position = screen;

            canvas.gameObject.SetActive(true);
        }

        void Hide()
        {
            if (canvas != null)
                canvas.gameObject.SetActive(false);
        }

        void EnsureUi()
        {
            if (canvas != null)
                return;

            GameObject canvasObject = new GameObject("PlanetHoverCanvas");
            canvasObject.transform.SetParent(transform, false);

            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 300;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);

            GameObject pill = new GameObject("HoverPill", typeof(RectTransform), typeof(Image));
            pill.transform.SetParent(canvasObject.transform, false);
            pillRect = pill.GetComponent<RectTransform>();
            pillRect.sizeDelta = new Vector2(280f, 72f);

            Image pillImage = pill.GetComponent<Image>();
            pillImage.color = new Color(0.06f, 0.1f, 0.18f, 0.92f);
            pillImage.raycastTarget = false;

            GameObject textObject = new GameObject("Label", typeof(RectTransform));
            textObject.transform.SetParent(pill.transform, false);

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            label = textObject.AddComponent<TextMeshProUGUI>();
            if (TMP_Settings.defaultFontAsset != null)
                label.font = TMP_Settings.defaultFontAsset;

            label.fontSize = 36f;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.color = OrbitScout.UI.OrbitScoutUiTheme.AccentCyan;
            label.raycastTarget = false;

            canvasObject.SetActive(false);
        }

        public static void EnsureOnMainCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
                return;

            if (cam.GetComponent<PlanetNameHoverLabel>() == null)
                cam.gameObject.AddComponent<PlanetNameHoverLabel>();
        }
    }
}
