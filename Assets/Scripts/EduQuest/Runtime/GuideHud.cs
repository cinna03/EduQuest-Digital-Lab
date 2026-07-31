using UnityEngine;
using UnityEngine.UI;

namespace EduQuest
{
    /// <summary>Camera overlay — guide text only.</summary>
    public class GuideHud : MonoBehaviour
    {
        Text m_Step;
        Text m_Title;
        Text m_Body;
        Text m_Status;

        public static GuideHud Create(Transform canvas)
        {
            var go = new GameObject("GuideHud");
            go.transform.SetParent(canvas, false);
            var rt = go.AddComponent<RectTransform>();
            Stretch(rt);

            var hud = go.AddComponent<GuideHud>();
            hud.m_Step = MakeLabel(go.transform, "Step", 16, new Vector2(0.08f, 0.88f), new Vector2(0.92f, 0.94f),
                new Color(0.7f, 0.95f, 1f));
            hud.m_Title = MakeLabel(go.transform, "Title", 30, new Vector2(0.08f, 0.78f), new Vector2(0.92f, 0.88f),
                Color.white, true);
            hud.m_Body = MakeLabel(go.transform, "Body", 18, new Vector2(0.1f, 0.66f), new Vector2(0.9f, 0.78f),
                new Color(0.95f, 0.97f, 1f));
            hud.m_Status = MakeLabel(go.transform, "Status", 18, new Vector2(0.1f, 0.08f), new Vector2(0.9f, 0.16f),
                new Color(1f, 0.92f, 0.5f), true);
            return hud;
        }

        public void Show(string step, string title, string body, string status)
        {
            if (m_Step) m_Step.text = step;
            if (m_Title) m_Title.text = title;
            if (m_Body) m_Body.text = body;
            if (m_Status) m_Status.text = status;
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
            outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
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
