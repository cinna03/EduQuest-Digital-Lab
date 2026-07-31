using UnityEngine;
using UnityEngine.UI;

namespace EduQuest.UI
{
    /// <summary>Applies the transparent glass vibe to uGUI Images/Text.</summary>
    public static class GlassUi
    {
        static Sprite s_Label;
        static Sprite s_Panel;

        public static Sprite LabelSprite => s_Label != null ? s_Label : (s_Label = LoadOrMake("UI/Glass/glass_label"));
        public static Sprite PanelSprite => s_Panel != null ? s_Panel : (s_Panel = LoadOrMake("UI/Glass/glass_panel"));

        public static void StylePanel(Image image, bool usePanelSprite = true)
        {
            if (image == null) return;
            var sprite = usePanelSprite ? PanelSprite : LabelSprite;
            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = Image.Type.Simple;
                image.preserveAspect = false;
            }
            image.color = new Color(1f, 1f, 1f, 0.9f);
            image.raycastTarget = true;
        }

        public static void StylePill(Image image)
        {
            if (image == null) return;
            if (LabelSprite != null)
            {
                image.sprite = LabelSprite;
                image.type = Image.Type.Simple;
                image.preserveAspect = false;
            }
            image.color = new Color(1f, 1f, 1f, 0.95f);
        }

        public static void StyleText(Text text, int size, bool emphasis = false)
        {
            if (text == null) return;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                        ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = size;
            text.fontStyle = emphasis ? FontStyle.Bold : FontStyle.Normal;
            text.color = new Color(0.92f, 0.95f, 1f, 0.95f);
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
        }

        static Sprite LoadOrMake(string resourcesPath)
        {
            var sprite = Resources.Load<Sprite>(resourcesPath);
            if (sprite != null) return sprite;

            var tex = Resources.Load<Texture2D>(resourcesPath);
            if (tex == null) return null;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
