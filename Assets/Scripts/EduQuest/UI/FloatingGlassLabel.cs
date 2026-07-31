using UnityEngine;
using UnityEngine.UI;

namespace EduQuest.UI
{
    /// <summary>World-space glass pill that billboards toward the camera.</summary>
    public class FloatingGlassLabel : MonoBehaviour
    {
        [SerializeField] Text label;
        [SerializeField] Image glass;
        [SerializeField] Canvas canvas;
        [SerializeField] Vector3 worldOffset = new Vector3(0f, 0.42f, 0f);
        [SerializeField] Transform follow;

        Camera m_Cam;
        bool m_Selected;

        public void Configure(Transform followTarget, string text, Vector3 offset)
        {
            follow = followTarget;
            worldOffset = offset;
            EnsureUi();
            SetText(text);
            SetSelected(false);
        }

        public void SetText(string text)
        {
            EnsureUi();
            if (label != null) label.text = text;
        }

        public void SetSelected(bool selected)
        {
            m_Selected = selected;
            if (glass == null) return;
            glass.color = selected
                ? new Color(0.75f, 0.95f, 1f, 1f)
                : new Color(1f, 1f, 1f, 0.92f);
            if (label != null)
                label.color = selected
                    ? new Color(0.05f, 0.12f, 0.2f, 1f)
                    : new Color(0.92f, 0.96f, 1f, 0.95f);
        }

        void EnsureUi()
        {
            if (canvas != null) return;

            var canvasGo = new GameObject("GlassLabelCanvas", typeof(RectTransform));
            canvasGo.transform.SetParent(transform, false);
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 20f;
            canvasGo.AddComponent<GraphicRaycaster>();

            var rt = canvasGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(220f, 56f);
            rt.localScale = Vector3.one * 0.0032f;

            var imgGo = new GameObject("Glass", typeof(RectTransform));
            imgGo.transform.SetParent(canvasGo.transform, false);
            glass = imgGo.AddComponent<Image>();
            GlassUi.StylePill(glass);
            Stretch(imgGo.GetComponent<RectTransform>());

            var textGo = new GameObject("Text", typeof(RectTransform));
            textGo.transform.SetParent(imgGo.transform, false);
            label = textGo.AddComponent<Text>();
            GlassUi.StyleText(label, 22, true);
            Stretch(textGo.GetComponent<RectTransform>(), 10f);
        }

        void LateUpdate()
        {
            if (follow != null)
                transform.position = follow.position + worldOffset;

            if (m_Cam == null) m_Cam = Camera.main;
            if (m_Cam != null)
                transform.rotation = Quaternion.LookRotation(transform.position - m_Cam.transform.position);

            // Soft bob
            if (follow != null && !m_Selected)
            {
                float bob = Mathf.Sin(Time.time * 2.2f) * 0.012f;
                transform.position = follow.position + worldOffset + Vector3.up * bob;
            }
        }

        static void Stretch(RectTransform rt, float pad = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(pad, pad);
            rt.offsetMax = new Vector2(-pad, -pad);
        }
    }
}
