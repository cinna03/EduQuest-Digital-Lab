using EduQuest.Experiments;
using EduQuest.UI;
using UnityEngine;

namespace EduQuest.Lab
{
    /// <summary>Tappable 3D reagent bottle with liquid + hovering glass label.</summary>
    [RequireComponent(typeof(Collider))]
    public class ChemicalBottle : MonoBehaviour
    {
        [SerializeField] ChemId chemId;
        [SerializeField] string displayName;
        [SerializeField] FloatingGlassLabel floatingLabel;
        [SerializeField] Renderer liquidRenderer;
        [SerializeField] Renderer bodyRenderer;

        bool m_Selected;
        Color m_LiquidColor;

        public ChemId ChemId => chemId;
        public string DisplayName => displayName;
        public bool IsSelected => m_Selected;

        public void Setup(ChemId id, string name, Color liquidColor, FloatingGlassLabel label, Renderer liquid, Renderer body)
        {
            chemId = id;
            displayName = name;
            m_LiquidColor = liquidColor;
            floatingLabel = label;
            liquidRenderer = liquid;
            bodyRenderer = body;
            ApplyLiquid();
            if (floatingLabel != null)
                floatingLabel.SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            m_Selected = selected;
            if (floatingLabel != null) floatingLabel.SetSelected(selected);
            if (bodyRenderer != null)
            {
                var c = selected
                    ? new Color(0.85f, 0.95f, 1f, 0.55f)
                    : new Color(0.75f, 0.85f, 0.92f, 0.35f);
                SetColor(bodyRenderer, c);
            }
        }

        void ApplyLiquid()
        {
            if (liquidRenderer != null)
                SetColor(liquidRenderer, m_LiquidColor);
        }

        static void SetColor(Renderer r, Color color)
        {
            var block = new MaterialPropertyBlock();
            r.GetPropertyBlock(block);
            block.SetColor("_BaseColor", color);
            block.SetColor("_Color", color);
            r.SetPropertyBlock(block);
        }
    }
}
