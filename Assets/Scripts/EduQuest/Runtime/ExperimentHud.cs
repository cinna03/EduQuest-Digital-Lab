using System;
using UnityEngine;
using UnityEngine.UI;

namespace EduQuest
{
    /// <summary>
    /// Status UI for the editor experiment (no pour buttons — interaction is on glassware).
    /// </summary>
    public class ExperimentHud : MonoBehaviour
    {
        public event Action<bool> DarkRequested;
        public event Action ResetRequested;

        Text m_Step;
        Text m_Title;
        Text m_Body;
        Text m_Action;
        Text m_Beaker;
        Text m_Selected;
        Image m_Banner;

        public static ExperimentHud Create(Transform canvas)
        {
            var go = new GameObject("ExperimentHud", typeof(RectTransform));
            go.transform.SetParent(canvas, false);
            Stretch(go.GetComponent<RectTransform>());

            var hud = go.AddComponent<ExperimentHud>();

            // Compact top card — leave the table readable
            var topBg = Panel(go.transform, "TopPanel",
                new Vector2(0.08f, 0.82f), new Vector2(0.92f, 0.97f),
                new Color(0.04f, 0.06f, 0.09f, 0.72f));

            hud.m_Step = Label(topBg.transform, "Step", 15,
                new Vector2(0.04f, 0.68f), new Vector2(0.96f, 0.95f),
                new Color(0.65f, 0.9f, 1f), FontStyle.Normal);
            hud.m_Title = Label(topBg.transform, "Title", 24,
                new Vector2(0.04f, 0.28f), new Vector2(0.96f, 0.72f),
                Color.white, FontStyle.Bold);
            hud.m_Body = Label(topBg.transform, "Body", 14,
                new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.32f),
                new Color(0.9f, 0.94f, 1f), FontStyle.Normal);

            var mid = Panel(go.transform, "ActionBanner",
                new Vector2(0.12f, 0.72f), new Vector2(0.88f, 0.80f),
                new Color(0.08f, 0.1f, 0.14f, 0.75f));
            hud.m_Banner = mid.GetComponent<Image>();
            hud.m_Action = Label(mid.transform, "Action", 17,
                new Vector2(0.04f, 0.1f), new Vector2(0.96f, 0.9f),
                new Color(1f, 0.92f, 0.45f), FontStyle.Bold);

            hud.m_Selected = Label(go.transform, "Selected", 15,
                new Vector2(0.12f, 0.66f), new Vector2(0.88f, 0.71f),
                new Color(1f, 0.85f, 0.45f), FontStyle.Bold);

            hud.m_Beaker = Label(go.transform, "BeakerState", 15,
                new Vector2(0.12f, 0.61f), new Vector2(0.88f, 0.66f),
                new Color(0.8f, 0.95f, 0.85f), FontStyle.Normal);

            float cy0 = 0.03f, cy1 = 0.12f;
            hud.MakeActionButton(go.transform, "DARK (D)", () => hud.DarkRequested?.Invoke(true),
                new Vector2(0.08f, cy0), new Vector2(0.34f, cy1), new Color(0.15f, 0.15f, 0.22f));
            hud.MakeActionButton(go.transform, "LIGHT (L)", () => hud.DarkRequested?.Invoke(false),
                new Vector2(0.36f, cy0), new Vector2(0.62f, cy1), new Color(0.45f, 0.4f, 0.15f));
            hud.MakeActionButton(go.transform, "RESET (R)", () => hud.ResetRequested?.Invoke(),
                new Vector2(0.64f, cy0), new Vector2(0.90f, cy1), new Color(0.35f, 0.15f, 0.15f));

            return hud;
        }

        public void Show(
            string step, string title, string body, string action,
            string selected, string beakerState, GuideHud.Tone tone)
        {
            if (m_Step) m_Step.text = step;
            if (m_Title) m_Title.text = title;
            if (m_Body) m_Body.text = body;
            if (m_Action) m_Action.text = action;
            if (m_Selected) m_Selected.text = "Selected: " + selected;
            if (m_Beaker) m_Beaker.text = "MIX beaker: " + beakerState;

            Color banner = new Color(0.1f, 0.12f, 0.16f, 0.88f);
            Color actionCol = new Color(1f, 0.92f, 0.45f);
            Color titleCol = Color.white;
            switch (tone)
            {
                case GuideHud.Tone.Success:
                    banner = new Color(0.05f, 0.35f, 0.15f, 0.92f);
                    actionCol = new Color(0.55f, 1f, 0.7f);
                    titleCol = new Color(0.5f, 1f, 0.65f);
                    break;
                case GuideHud.Tone.Fail:
                    banner = new Color(0.4f, 0.08f, 0.08f, 0.92f);
                    actionCol = new Color(1f, 0.55f, 0.45f);
                    titleCol = new Color(1f, 0.4f, 0.4f);
                    break;
                case GuideHud.Tone.Warn:
                    banner = new Color(0.3f, 0.22f, 0.05f, 0.9f);
                    actionCol = new Color(1f, 0.9f, 0.4f);
                    titleCol = new Color(1f, 0.85f, 0.35f);
                    break;
            }
            if (m_Banner) m_Banner.color = banner;
            if (m_Action) m_Action.color = actionCol;
            if (m_Title) m_Title.color = titleCol;
        }

        void MakeActionButton(Transform parent, string text, Action onClick, Vector2 aMin, Vector2 aMax, Color color)
        {
            var go = new GameObject(text, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.offsetMin = new Vector2(4, 4);
            rt.offsetMax = new Vector2(-4, -4);
            var img = go.AddComponent<Image>();
            img.color = color;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => onClick?.Invoke());

            var labelGo = new GameObject("Text", typeof(RectTransform));
            labelGo.transform.SetParent(go.transform, false);
            Stretch(labelGo.GetComponent<RectTransform>());
            var t = labelGo.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                     ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.text = text;
            t.fontSize = 20;
            t.fontStyle = FontStyle.Bold;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            t.raycastTarget = false;
        }

        static GameObject Panel(Transform parent, string name, Vector2 aMin, Vector2 aMax, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return go;
        }

        static Text Label(Transform parent, string name, int size, Vector2 aMin, Vector2 aMax, Color color, FontStyle style)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var t = go.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                     ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            var o = go.AddComponent<Outline>();
            o.effectColor = new Color(0f, 0f, 0f, 0.85f);
            o.effectDistance = new Vector2(1.5f, -1.5f);
            return t;
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
