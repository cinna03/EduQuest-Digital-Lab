using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OrbitScout.UI
{
    /// <summary>
    /// Restores clear hover / press feedback on procedural glass pills
    /// (tint + glow / specular intensity).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class OrbitScoutGlassButtonVisual : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        ISelectHandler,
        IDeselectHandler
    {
        static readonly int GlowId = Shader.PropertyToID("_GlowStrength");
        static readonly int SpecId = Shader.PropertyToID("_SpecStrength");
        static readonly int GlassColorId = Shader.PropertyToID("_GlassColor");
        static readonly int GlassCoreId = Shader.PropertyToID("_GlassCore");
        static readonly int RimColorId = Shader.PropertyToID("_RimColor");

        Image image;
        Material glassMat;
        bool primary;
        bool hovered;
        bool pressed;
        bool selected;

        float baseGlow = 1.1f;
        float baseSpec = 1.15f;
        Color baseGlass;
        Color baseCore;
        Color baseRim;

        public void Bind(Image targetImage, Material material, bool isPrimary)
        {
            image = targetImage;
            glassMat = material;
            primary = isPrimary;

            if (glassMat != null)
            {
                baseGlow = glassMat.GetFloat(GlowId);
                baseSpec = glassMat.GetFloat(SpecId);
                baseGlass = glassMat.GetColor(GlassColorId);
                baseCore = glassMat.GetColor(GlassCoreId);
                baseRim = glassMat.GetColor(RimColorId);
            }

            ApplyVisualState();
        }

        void OnDisable()
        {
            hovered = false;
            pressed = false;
            selected = false;
            ApplyVisualState();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovered = true;
            ApplyVisualState();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovered = false;
            pressed = false;
            ApplyVisualState();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            pressed = true;
            ApplyVisualState();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            pressed = false;
            ApplyVisualState();
        }

        public void OnSelect(BaseEventData eventData)
        {
            selected = true;
            ApplyVisualState();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            selected = false;
            ApplyVisualState();
        }

        void ApplyVisualState()
        {
            if (image == null)
                image = GetComponent<Image>();

            Color tint;
            float glowMul;
            float specMul;
            float darken;

            if (pressed)
            {
                tint = primary
                    ? new Color(0.72f, 0.55f, 0.95f, 1f)
                    : new Color(0.66f, 0.48f, 0.88f, 1f);
                glowMul = 0.72f;
                specMul = 0.55f;
                darken = 0.82f;
            }
            else if (hovered || selected)
            {
                tint = primary
                    ? new Color(1f, 0.97f, 1f, 1f)
                    : new Color(0.97f, 0.94f, 1f, 1f);
                glowMul = 1.4f;
                specMul = 1.35f;
                darken = 1.08f;
            }
            else
            {
                tint = new Color(0.92f, 0.88f, 1f, 1f);
                glowMul = 1f;
                specMul = 1f;
                darken = 1f;
            }

            // When ColorTint is active, Button owns Image.color — only boost glass shader params
            Button button = GetComponent<Button>();
            bool colorTint = button != null && button.transition == Selectable.Transition.ColorTint;
            if (image != null && !colorTint)
                image.color = tint;

            if (glassMat == null)
                return;

            glassMat.SetFloat(GlowId, baseGlow * glowMul);
            glassMat.SetFloat(SpecId, baseSpec * specMul);
            glassMat.SetColor(GlassColorId, ScaleColor(baseGlass, darken));
            glassMat.SetColor(GlassCoreId, ScaleColor(baseCore, darken));
            glassMat.SetColor(RimColorId, ScaleColor(baseRim, pressed ? 0.9f : (hovered || selected) ? 1.06f : 1f));
        }

        static Color ScaleColor(Color c, float rgbMul)
        {
            return new Color(
                Mathf.Clamp01(c.r * rgbMul),
                Mathf.Clamp01(c.g * rgbMul),
                Mathf.Clamp01(c.b * rgbMul),
                c.a);
        }
    }
}
