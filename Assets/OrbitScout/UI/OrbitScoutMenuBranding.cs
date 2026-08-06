using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OrbitScout.UI
{
    /// <summary>
    /// Matches OrbitScout_EditorTest start menu: SOLAR + system quiz + no scoreboard.
    /// </summary>
    public static class OrbitScoutMenuBranding
    {
        const string TitleFontResource = "OrbitScout/Fonts/LuckiestGuy SDF";
        const string TitleFontAsset = "Assets/OrbitScout/UI/Fonts/TMP/LuckiestGuy SDF.asset";
        const string SubtitleFontResource = "OrbitScout/Fonts/GwBantleyRegular SDF";
        const string SubtitleFontAsset = "Assets/OrbitScout/UI/Fonts/TMP/GwBantleyRegular SDF.asset";

        static TMP_FontAsset titleFont;
        static TMP_FontAsset subtitleFont;

        public static void Apply(OrbitScoutHudView view)
        {
            if (view == null || view.menuPanel == null)
                return;

            Transform menu = view.menuPanel.transform;
            HideScoreboard(view, menu);

            StyleSolarTitle(menu);
            StyleSystemQuizSubtitle(menu);
            PlaceSubtitleFlower(menu);

            if (view.playButton != null)
            {
                TMP_Text playLabel = view.playButton.GetComponentInChildren<TMP_Text>(true);
                if (playLabel != null)
                {
                    playLabel.text = "START YOUR JOURNEY";
                    playLabel.color = Color.white;
                    ApplyFont(playLabel, LoadTitleFont());
                }
            }

            if (view.resetJourneyButton != null)
            {
                TMP_Text resetLabel = view.resetJourneyButton.GetComponentInChildren<TMP_Text>(true);
                if (resetLabel != null)
                {
                    resetLabel.text = "RESET JOURNEY";
                    resetLabel.fontStyle = FontStyles.Bold;
                    ApplyFont(resetLabel, LoadTitleFont());
                    resetLabel.fontSize = 35f;
                }
            }

            if (view.levelSelectPanel != null)
            {
                TMP_Text levelTitle = FindNamedText(view.levelSelectPanel.transform, "Title");
                if (levelTitle != null)
                {
                    levelTitle.text = "MISSION LEVEL MAP";
                    ApplyFont(levelTitle, LoadTitleFont());
                    levelTitle.color = new Color(0.21176471f, 0.14509805f, 0.36078432f, 1f);
                }
            }
        }

        static void HideScoreboard(OrbitScoutHudView view, Transform menu)
        {
            view.menuScoresPanel = null;
            view.menuScoresText = null;

            // Prefab keeps MenuScores as a MenuPanel child even when menuScoresPanel ref is null
            Transform scores = menu.Find("MenuScores");
            if (scores != null)
                scores.gameObject.SetActive(false);

            foreach (TMP_Text text in menu.GetComponentsInChildren<TMP_Text>(true))
            {
                if (text == null)
                    continue;
                string value = text.text ?? string.Empty;
                if (value.Contains("Overall:") || value.Contains("Unlocked through") || text.name == "MenuScores")
                    text.gameObject.SetActive(false);
            }
        }

        static void StyleSolarTitle(Transform menu)
        {
            TMP_Text title = FindNamedText(menu, "Title");
            if (title == null)
                return;

            title.gameObject.SetActive(true);
            title.text = "SOLAR";
            title.fontStyle = FontStyles.Bold;
            title.enableAutoSizing = false;
            title.fontSize = 36f;
            title.color = new Color(0.8235294f, 0.7647059f, 0.9647059f, 1f);
            title.alignment = TextAlignmentOptions.Center;
            ApplyFont(title, LoadTitleFont());

            RectTransform rect = title.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.78f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(-7f, -179f);
            rect.sizeDelta = new Vector2(920f, 200f);
            rect.localScale = Vector3.one * 5.4595327f;
            title.transform.SetAsLastSibling();
        }

        static void StyleSystemQuizSubtitle(Transform menu)
        {
            // EditorTest uses a dedicated "SMALLER TEXT" object, not the old Subtitle
            Transform oldSubtitle = menu.Find("Subtitle");
            if (oldSubtitle != null)
                oldSubtitle.gameObject.SetActive(false);

            TMP_Text subtitle = FindNamedText(menu, "SMALLER TEXT");
            if (subtitle == null)
            {
                GameObject go = new GameObject("SMALLER TEXT", typeof(RectTransform), typeof(TextMeshProUGUI));
                go.transform.SetParent(menu, false);
                subtitle = go.GetComponent<TMP_Text>();
            }

            subtitle.gameObject.SetActive(true);
            subtitle.text = "system quiz";
            subtitle.fontStyle = FontStyles.Normal;
            subtitle.enableAutoSizing = false;
            subtitle.fontSize = 56f;
            subtitle.color = new Color(0.60784316f, 0.59607846f, 0.7411765f, 1f);
            subtitle.alignment = TextAlignmentOptions.Center;
            subtitle.raycastTarget = false;
            ApplyFont(subtitle, LoadSubtitleFont());

            RectTransform rect = subtitle.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.78f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(239f, -263f);
            rect.sizeDelta = new Vector2(920f, 200f);
            rect.localScale = Vector3.one * 2.37f;
            subtitle.transform.SetAsLastSibling();
        }

        static void PlaceSubtitleFlower(Transform menu)
        {
            Transform decor = menu.Find(OrbitScoutMenuDecor.RootName);
            if (decor == null)
                return;

            Transform flower = decor.Find("Flower");
            if (flower == null)
                return;

            RectTransform rect = flower.GetComponent<RectTransform>();
            if (rect == null)
                return;

            // Same placement as EditorTest (beside "system quiz")
            rect.anchorMin = rect.anchorMax = new Vector2(0.86f, 0.78f);
            rect.anchoredPosition = new Vector2(-522f, -275f);
            rect.sizeDelta = new Vector2(120f, 120f);
            rect.localScale = Vector3.one * 1.228062f;
        }

        static TMP_Text FindNamedText(Transform root, string name)
        {
            Transform t = root.Find(name);
            if (t != null)
                return t.GetComponent<TMP_Text>();

            foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                if (text != null && text.name == name)
                    return text;
            }

            return null;
        }

        static void ApplyFont(TMP_Text text, TMP_FontAsset font)
        {
            if (text == null || font == null)
                return;
            text.font = font;
        }

        static TMP_FontAsset LoadTitleFont()
        {
            if (titleFont != null)
                return titleFont;
            titleFont = Resources.Load<TMP_FontAsset>(TitleFontResource);
#if UNITY_EDITOR
            if (titleFont == null)
                titleFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TitleFontAsset);
#endif
            return titleFont;
        }

        static TMP_FontAsset LoadSubtitleFont()
        {
            if (subtitleFont != null)
                return subtitleFont;
            subtitleFont = Resources.Load<TMP_FontAsset>(SubtitleFontResource);
#if UNITY_EDITOR
            if (subtitleFont == null)
                subtitleFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SubtitleFontAsset);
#endif
            return subtitleFont;
        }
    }
}
