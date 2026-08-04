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
            text.color = AccentGold;
            text.fontStyle = FontStyles.Bold;
        }

        public static void StyleSubtitle(TMP_Text text)
        {
            text.color = TextMuted;
        }

        public static void StyleBody(TMP_Text text)
        {
            text.color = TextPrimary;
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
    }
}
