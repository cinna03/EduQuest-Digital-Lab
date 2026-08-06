using OrbitScout.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OrbitScout.UI
{
    /// <summary>
    /// Orbit Scout visual language: deep space UI + cyan/gold accents.
    /// </summary>
    public static class OrbitScoutUiTheme
    {
        public static readonly Color SpaceBackdrop = new Color(0.03f, 0.05f, 0.11f, 0.94f);
        public static readonly Color MenuBackdrop = new Color(0.03f, 0.05f, 0.11f, 1f);
        public static readonly Color PlayHudBackdrop = new Color(0.02f, 0.04f, 0.08f, 0.42f);
        public static readonly Color PanelSurface = new Color(0.07f, 0.1f, 0.18f, 0.88f);
        public static readonly Color PanelBorder = new Color(0.22f, 0.55f, 0.85f, 0.35f);

        public static readonly Color PrimaryButton = new Color(0.12f, 0.48f, 0.72f, 1f);
        public static readonly Color PrimaryButtonHighlight = new Color(0.2f, 0.62f, 0.9f, 1f);
        public static readonly Color SecondaryButton = new Color(0.14f, 0.16f, 0.24f, 0.95f);
        public static readonly Color DangerButton = new Color(0.45f, 0.16f, 0.2f, 0.95f);

        public static readonly Color TextPrimary = new Color(0.95f, 0.97f, 1f, 1f);
        public static readonly Color TextMuted = new Color(0.65f, 0.75f, 0.88f, 1f);
        public static readonly Color AccentGold = new Color(1f, 0.82f, 0.35f, 1f);
        public static readonly Color AccentCyan = new Color(0.45f, 0.9f, 1f, 1f);

        public const string MenuButtonSpriteResourcePath = "OrbitScout/MenuButton_purple_pill";
        public const string MenuButtonSpriteAssetPath = "Assets/OrbitScout/UI/Visuals/MenuButton_purple_pill.png";
        public const string UiWhiteSpriteResourcePath = "OrbitScout/UIWhite";
        public const string GlassPillShaderName = "OrbitScout/UI/GlassPill";

        static Sprite cachedMenuButtonSprite;
        static Sprite cachedWhiteSprite;
        static Shader cachedGlassShader;

        public static Sprite LoadMenuButtonSprite()
        {
            if (cachedMenuButtonSprite != null)
                return cachedMenuButtonSprite;

            cachedMenuButtonSprite = Resources.Load<Sprite>(MenuButtonSpriteResourcePath);
#if UNITY_EDITOR
            if (cachedMenuButtonSprite == null)
                cachedMenuButtonSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(MenuButtonSpriteAssetPath);
#endif
            return cachedMenuButtonSprite;
        }

        static Sprite LoadWhiteSprite()
        {
            if (cachedWhiteSprite != null)
                return cachedWhiteSprite;

            cachedWhiteSprite = Resources.Load<Sprite>(UiWhiteSpriteResourcePath);
#if UNITY_EDITOR
            if (cachedWhiteSprite == null)
                cachedWhiteSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/OrbitScout/UI/Visuals/UIWhite.png");
#endif
            return cachedWhiteSprite;
        }

        static Shader LoadGlassShader()
        {
            if (cachedGlassShader != null)
                return cachedGlassShader;

            cachedGlassShader = OrbitScoutShaderLibrary.GlassPill ?? Shader.Find(GlassPillShaderName);
            return cachedGlassShader;
        }

        public static void StyleMenuPillButton(Button button, bool primary)
        {
            StyleMenuPillButton(button, primary, null, null);
        }

        public static void StyleMenuPillButton(Button button, bool primary, float? widthOverride, float? heightOverride)
        {
            if (button == null)
                return;

            Image image = button.GetComponent<Image>();
            if (image == null)
                return;

            RectTransform rect = button.GetComponent<RectTransform>();
            float height = heightOverride ?? (primary ? 118f : 100f);
            float width = widthOverride ?? (primary ? 560f : 520f);
            if (rect != null)
                rect.sizeDelta = new Vector2(width, height);

            float aspect = width / Mathf.Max(height, 1f);
            Shader glassShader = LoadGlassShader();
            Sprite white = LoadWhiteSprite();

            if (glassShader != null && white != null)
            {
                image.sprite = white;
                image.type = Image.Type.Simple;
                image.preserveAspect = false;
                Material glassMat = CreateGlassMaterial(glassShader, aspect, primary);
                image.material = glassMat;
                image.color = new Color(0.92f, 0.88f, 1f, 1f);

                OrbitScoutGlassButtonVisual feedback = button.GetComponent<OrbitScoutGlassButtonVisual>();
                if (feedback == null)
                    feedback = button.gameObject.AddComponent<OrbitScoutGlassButtonVisual>();
                feedback.Bind(image, glassMat, primary);

                Shadow bakedShadow = button.GetComponent<Shadow>();
                if (bakedShadow != null)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        Object.DestroyImmediate(bakedShadow);
                    else
#endif
                        Object.Destroy(bakedShadow);
                }
            }
            else
            {
                OrbitScoutGlassButtonVisual feedback = button.GetComponent<OrbitScoutGlassButtonVisual>();
                if (feedback != null)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        Object.DestroyImmediate(feedback);
                    else
#endif
                        Object.Destroy(feedback);
                }

                Sprite sprite = LoadMenuButtonSprite();
                image.material = null;
                if (sprite != null)
                {
                    image.sprite = sprite;
                    image.type = Image.Type.Simple;
                    image.preserveAspect = false;
                    image.color = new Color(1f, 1f, 1f, 0.95f);
                }
                else
                {
                    image.sprite = null;
                    image.color = primary
                        ? new Color(0.72f, 0.55f, 0.95f, 0.75f)
                        : new Color(0.55f, 0.42f, 0.78f, 0.7f);
                }

                Shadow shadow = button.GetComponent<Shadow>();
                if (shadow == null)
                    shadow = button.gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color(0.35f, 0.12f, 0.55f, 0.45f);
                shadow.effectDistance = new Vector2(0f, -10f);
                shadow.useGraphicAlpha = true;
            }

            button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.92f, 0.88f, 1f, 1f);
            colors.highlightedColor = new Color(1f, 0.97f, 1f, 1f);
            colors.pressedColor = primary
                ? new Color(0.72f, 0.55f, 0.95f, 1f)
                : new Color(0.66f, 0.48f, 0.88f, 1f);
            colors.selectedColor = new Color(1f, 0.97f, 1f, 1f);
            colors.disabledColor = new Color(0.55f, 0.55f, 0.62f, 0.4f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.color = Color.white;
                label.fontStyle = primary ? FontStyles.Bold : FontStyles.Normal;
                label.enableAutoSizing = false;
                label.raycastTarget = false;
            }
        }

        public static void StyleCompactGlassButton(Button button, bool primary)
        {
            StyleMenuPillButton(button, primary, 220f, 72f);
        }

        public static void ApplyGlassButtons(OrbitScoutHudView view)
        {
            if (view == null)
                return;

            StyleMenuPillButton(view.playButton, true);
            StyleMenuPillButton(view.resetJourneyButton, false);
            StyleMenuPillButton(view.levelSelectBackButton, false);
            StyleMenuPillButton(view.briefingStartButton, true);
            StyleMenuPillButton(view.briefingBackButton, false);
            StyleMenuPillButton(view.continueNextButton, true);
            StyleMenuPillButton(view.retryLevelButton, true);
            StyleMenuPillButton(view.endLevelSelectButton, false);
            StyleMenuPillButton(view.endMainMenuButton, false);
            StyleCompactGlassButton(view.restartButton, false);
            StyleCompactGlassButton(view.menuButton, false);
            StyleMenuPillButton(view.noneMatchButton, true, 560f, 90f);
        }

        static Material CreateGlassMaterial(Shader shader, float aspect, bool primary)
        {
            Material mat = new Material(shader)
            {
                name = primary ? "OrbitScoutGlassPill_Primary" : "OrbitScoutGlassPill_Secondary",
                hideFlags = HideFlags.HideAndDontSave
            };
            mat.SetFloat("_Aspect", aspect);
            mat.SetColor("_GlassColor", primary
                ? new Color(0.78f, 0.62f, 0.98f, 0.40f)
                : new Color(0.68f, 0.52f, 0.90f, 0.36f));
            mat.SetColor("_GlassCore", primary
                ? new Color(0.95f, 0.90f, 1f, 0.58f)
                : new Color(0.88f, 0.82f, 0.98f, 0.52f));
            mat.SetColor("_RimColor", primary
                ? new Color(0.72f, 0.48f, 0.95f, 0.82f)
                : new Color(0.62f, 0.40f, 0.88f, 0.75f));
            mat.SetColor("_SpecColor", new Color(1f, 0.98f, 1f, 0.9f));
            mat.SetColor("_ShadowColor", new Color(0.35f, 0.12f, 0.55f, 0.38f));
            mat.SetFloat("_RimWidth", 0.12f);
            mat.SetFloat("_SpecStrength", primary ? 1.25f : 1.05f);
            mat.SetFloat("_GlowStrength", primary ? 1.15f : 0.95f);
            return mat;
        }

        public static void StylePlayHudTexts(TMP_Text questionNumber, TMP_Text clue, TMP_Text score, TMP_Text timer, TMP_Text feedback)
        {
            Color lavender = new Color(0.92f, 0.86f, 1f, 1f);
            Color softLavender = new Color(0.82f, 0.72f, 0.98f, 0.95f);
            Color cream = new Color(1f, 0.96f, 1f, 1f);

            if (questionNumber != null)
            {
                questionNumber.gameObject.SetActive(true);
                questionNumber.color = softLavender;
                questionNumber.fontStyle = FontStyles.Bold;
                questionNumber.enableAutoSizing = false;
                questionNumber.alignment = TextAlignmentOptions.Center;
            }

            if (clue != null)
            {
                clue.color = cream;
                clue.fontStyle = FontStyles.Normal;
                clue.enableAutoSizing = false;
                clue.alignment = TextAlignmentOptions.Center;
            }

            if (score != null)
            {
                score.color = lavender;
                score.fontStyle = FontStyles.Bold;
                score.enableAutoSizing = false;
            }

            if (timer != null)
            {
                timer.color = new Color(1f, 0.85f, 0.55f, 1f);
                timer.fontStyle = FontStyles.Bold;
                timer.enableAutoSizing = false;
            }

            if (feedback != null)
            {
                feedback.color = softLavender;
                feedback.enableAutoSizing = false;
                feedback.alignment = TextAlignmentOptions.Center;
            }
        }

        public static void StyleWalkthroughTexts(TMP_Text title, TMP_Text body)
        {
            if (title != null)
            {
                title.color = new Color(0.35f, 0.18f, 0.55f, 1f);
                title.fontStyle = FontStyles.Bold;
                title.alignment = TextAlignmentOptions.Center;
                title.enableAutoSizing = false;
            }

            if (body != null)
            {
                body.color = new Color(0.30f, 0.16f, 0.45f, 1f);
                body.alignment = TextAlignmentOptions.TopLeft;
                body.enableAutoSizing = false;
            }
        }

        public static void StyleButton(Button button, bool primary)
        {
            Image image = button.GetComponent<Image>();
            if (image == null)
                return;

            image.color = primary ? PrimaryButton : SecondaryButton;

            ColorBlock colors = button.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = primary ? PrimaryButtonHighlight : new Color(0.22f, 0.26f, 0.36f, 1f);
            colors.pressedColor = new Color(0.08f, 0.35f, 0.55f, 1f);
            button.colors = colors;
        }

        public static void StyleChipButton(Button button)
        {
            Image image = button.GetComponent<Image>();
            if (image != null)
                image.color = SecondaryButton;
        }

        public static void ApplyPanelBackdrop(GameObject panel, bool card = false, bool playHud = false, bool fullScreenMenu = false)
        {
            if (panel.GetComponent<Image>() == null)
                panel.AddComponent<Image>();

            Image image = panel.GetComponent<Image>();
            if (playHud)
                image.color = PlayHudBackdrop;
            else if (fullScreenMenu)
                image.color = MenuBackdrop;
            else
                image.color = card ? PanelSurface : SpaceBackdrop;

            image.raycastTarget = false;
        }

        public static void StyleTitle(TMP_Text text)
        {
            if (text == null)
                return;
            text.color = AccentGold;
            text.fontStyle = FontStyles.Bold;
        }

        public static void StyleSubtitle(TMP_Text text)
        {
            if (text == null)
                return;
            text.color = TextMuted;
        }

        public static void StyleBody(TMP_Text text)
        {
            if (text == null)
                return;
            text.color = TextPrimary;
        }
    }
}
