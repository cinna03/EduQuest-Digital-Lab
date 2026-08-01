using UnityEngine;
using UnityEngine.UI;

namespace EduQuest
{
    /// <summary>Camera overlay — guide text only, with clear success/fail coloring.</summary>
    public class GuideHud : MonoBehaviour
    {
        public enum Tone { Normal, Success, Fail, Warn }

        Text m_Step;
        Text m_Title;
        Text m_Body;
        Text m_Status;
        Image m_Banner;

        public static GuideHud Create(Transform canvas)
        {
            var go = new GameObject("GuideHud");
            go.transform.SetParent(canvas, false);
            var rt = go.AddComponent<RectTransform>();
            Stretch(rt);

            var hud = go.AddComponent<GuideHud>();

            // Bottom result banner (hidden until success/fail)
            var bannerGo = new GameObject("Banner", typeof(RectTransform));
            bannerGo.transform.SetParent(go.transform, false);
            var brt = bannerGo.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.08f, 0.18f);
            brt.anchorMax = new Vector2(0.92f, 0.32f);
            brt.offsetMin = Vector2.zero;
            brt.offsetMax = Vector2.zero;
            hud.m_Banner = bannerGo.AddComponent<Image>();
            hud.m_Banner.color = new Color(0f, 0f, 0f, 0f);
            hud.m_Banner.raycastTarget = false;

            hud.m_Step = MakeLabel(go.transform, "Step", 16, new Vector2(0.08f, 0.88f), new Vector2(0.92f, 0.94f),
                new Color(0.7f, 0.95f, 1f));
            hud.m_Title = MakeLabel(go.transform, "Title", 32, new Vector2(0.08f, 0.78f), new Vector2(0.92f, 0.88f),
                Color.white, true);
            hud.m_Body = MakeLabel(go.transform, "Body", 18, new Vector2(0.1f, 0.62f), new Vector2(0.9f, 0.78f),
                new Color(0.95f, 0.97f, 1f));
            hud.m_Status = MakeLabel(go.transform, "Status", 22, new Vector2(0.1f, 0.2f), new Vector2(0.9f, 0.3f),
                new Color(1f, 0.92f, 0.5f), true);
            return hud;
        }

        public void Show(string step, string title, string body, string status, Tone tone = Tone.Normal)
        {
            if (m_Step) m_Step.text = step;
            if (m_Title) m_Title.text = title;
            if (m_Body) m_Body.text = body;
            if (m_Status) m_Status.text = status;

            switch (tone)
            {
                case Tone.Success:
                    if (m_Title) m_Title.color = new Color(0.45f, 1f, 0.65f);
                    if (m_Status) m_Status.color = new Color(0.55f, 1f, 0.7f);
                    if (m_Banner) m_Banner.color = new Color(0.05f, 0.35f, 0.15f, 0.75f);
                    break;
                case Tone.Fail:
                    if (m_Title) m_Title.color = new Color(1f, 0.4f, 0.4f);
                    if (m_Status) m_Status.color = new Color(1f, 0.55f, 0.45f);
                    if (m_Banner) m_Banner.color = new Color(0.35f, 0.05f, 0.05f, 0.8f);
                    break;
                case Tone.Warn:
                    if (m_Title) m_Title.color = new Color(1f, 0.85f, 0.35f);
                    if (m_Status) m_Status.color = new Color(1f, 0.9f, 0.4f);
                    if (m_Banner) m_Banner.color = new Color(0.25f, 0.18f, 0.05f, 0.7f);
                    break;
                default:
                    if (m_Title) m_Title.color = Color.white;
                    if (m_Status) m_Status.color = new Color(1f, 0.92f, 0.5f);
                    if (m_Banner) m_Banner.color = new Color(0f, 0f, 0f, 0f);
                    break;
            }
        }

        static Text MakeLabel(Transform parent, string name, int size, Vector2 aMin, Vector2 aMax, Color color, bool bold = false)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                        ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = size;
            text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;

            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(2f, -2f);
            return text;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
