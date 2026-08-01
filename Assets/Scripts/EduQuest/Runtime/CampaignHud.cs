using System;
using UnityEngine;
using UnityEngine.UI;

namespace EduQuest
{
    /// <summary>UI for the 3-level campaign (combat → props guide → light → lab).</summary>
    public class CampaignHud : MonoBehaviour
    {
        public event Action WinCombatClicked;
        public event Action FoundPropClicked;
        public event Action LightGateClicked;
        public event Action StartLabClicked;
        public event Action ResetCampaignClicked;

        Text m_Step;
        Text m_Title;
        Text m_Body;
        Text m_Action;
        Text m_Toast;
        Image m_Banner;
        GameObject m_WinBtn;
        GameObject m_FoundBtn;
        GameObject m_LightBtn;
        GameObject m_LabBtn;
        float m_ToastTimer;

        public static CampaignHud Create(Transform canvas)
        {
            var go = new GameObject("CampaignHud", typeof(RectTransform));
            go.transform.SetParent(canvas, false);
            Stretch(go.GetComponent<RectTransform>());

            var hud = go.AddComponent<CampaignHud>();

            var top = Panel(go.transform, "Top", new Vector2(0.06f, 0.78f), new Vector2(0.94f, 0.97f),
                new Color(0.04f, 0.06f, 0.1f, 0.82f));
            hud.m_Step = Label(top.transform, "Step", 15, new Vector2(0.04f, 0.72f), new Vector2(0.96f, 0.95f),
                new Color(0.65f, 0.9f, 1f), FontStyle.Normal);
            hud.m_Title = Label(top.transform, "Title", 24, new Vector2(0.04f, 0.35f), new Vector2(0.96f, 0.75f),
                Color.white, FontStyle.Bold);
            hud.m_Body = Label(top.transform, "Body", 15, new Vector2(0.04f, 0.05f), new Vector2(0.96f, 0.38f),
                new Color(0.9f, 0.94f, 1f), FontStyle.Normal);

            var mid = Panel(go.transform, "Banner", new Vector2(0.1f, 0.68f), new Vector2(0.9f, 0.76f),
                new Color(0.1f, 0.12f, 0.16f, 0.85f));
            hud.m_Banner = mid.GetComponent<Image>();
            hud.m_Action = Label(mid.transform, "Action", 17, new Vector2(0.04f, 0.1f), new Vector2(0.96f, 0.9f),
                new Color(1f, 0.92f, 0.45f), FontStyle.Bold);

            hud.m_Toast = Label(go.transform, "Toast", 20, new Vector2(0.12f, 0.55f), new Vector2(0.88f, 0.62f),
                new Color(0.55f, 1f, 0.7f), FontStyle.Bold);
            hud.m_Toast.text = "";

            hud.m_WinBtn = MakeBtn(go.transform, "WIN WAVE", () => hud.WinCombatClicked?.Invoke(),
                new Vector2(0.08f, 0.03f), new Vector2(0.36f, 0.12f), new Color(0.15f, 0.35f, 0.2f));
            hud.m_FoundBtn = MakeBtn(go.transform, "SOLVED RIDDLE", () => hud.FoundPropClicked?.Invoke(),
                new Vector2(0.38f, 0.03f), new Vector2(0.66f, 0.12f), new Color(0.2f, 0.3f, 0.45f));
            hud.m_LightBtn = MakeBtn(go.transform, "SOLVED RIDDLE", () => hud.LightGateClicked?.Invoke(),
                new Vector2(0.38f, 0.03f), new Vector2(0.66f, 0.12f), new Color(0.45f, 0.35f, 0.1f));
            hud.m_LabBtn = MakeBtn(go.transform, "CONFIRM", () => hud.StartLabClicked?.Invoke(),
                new Vector2(0.68f, 0.03f), new Vector2(0.92f, 0.12f), new Color(0.25f, 0.2f, 0.45f));

            MakeBtn(go.transform, "RESET RUN", () => hud.ResetCampaignClicked?.Invoke(),
                new Vector2(0.08f, 0.13f), new Vector2(0.36f, 0.2f), new Color(0.35f, 0.12f, 0.12f));

            return hud;
        }

        public void Show(string step, string title, string body, string action, HudTone tone,
            bool showWin, bool showFound, bool showLight, bool showLab)
        {
            if (m_Step) m_Step.text = step;
            if (m_Title) m_Title.text = title;
            if (m_Body) m_Body.text = body;
            if (m_Action) m_Action.text = action;

            if (m_WinBtn) m_WinBtn.SetActive(showWin);
            if (m_FoundBtn) m_FoundBtn.SetActive(showFound);
            if (m_LightBtn) m_LightBtn.SetActive(showLight);
            if (m_LabBtn) m_LabBtn.SetActive(showLab);

            Color banner = new Color(0.1f, 0.12f, 0.16f, 0.88f);
            Color actionCol = new Color(1f, 0.92f, 0.45f);
            switch (tone)
            {
                case HudTone.Success:
                    banner = new Color(0.05f, 0.35f, 0.15f, 0.92f);
                    actionCol = new Color(0.55f, 1f, 0.7f);
                    break;
                case HudTone.Fail:
                    banner = new Color(0.4f, 0.08f, 0.08f, 0.92f);
                    actionCol = new Color(1f, 0.55f, 0.45f);
                    break;
                case HudTone.Warn:
                    banner = new Color(0.3f, 0.22f, 0.05f, 0.9f);
                    actionCol = new Color(1f, 0.9f, 0.4f);
                    break;
            }
            if (m_Banner) m_Banner.color = banner;
            if (m_Action) m_Action.color = actionCol;
        }

        public void Toast(string message)
        {
            if (m_Toast == null) return;
            m_Toast.text = message;
            m_ToastTimer = 2.5f;
        }

        void Update()
        {
            if (m_ToastTimer <= 0f) return;
            m_ToastTimer -= Time.deltaTime;
            if (m_ToastTimer <= 0f && m_Toast != null)
                m_Toast.text = "";
        }

        static GameObject MakeBtn(Transform parent, string text, Action onClick, Vector2 aMin, Vector2 aMax, Color color)
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
            t.fontSize = 18;
            t.fontStyle = FontStyle.Bold;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            t.raycastTarget = false;
            return go;
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
