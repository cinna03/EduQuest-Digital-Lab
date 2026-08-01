using UnityEngine;

namespace EduQuest
{
    /// <summary>Visual state of the reaction beaker contents (editor experiment).</summary>
    public class BeakerMix : MonoBehaviour
    {
        public enum Look
        {
            Empty,
            ClearSolution,
            WhitePrecipitate,
            RawCrystal,
            Stabilized,
            GlowSuccess,
            BurntResidue,
            Contaminated
        }

        [SerializeField] Renderer liquidRenderer;
        [SerializeField] Light glow;

        Look m_Look = Look.Empty;

        public Look Current => m_Look;

        public void Bind(Renderer liquid, Light glowLight)
        {
            liquidRenderer = liquid;
            glow = glowLight;
            SetLook(Look.Empty);
        }

        public void SetLook(Look look)
        {
            m_Look = look;
            if (liquidRenderer == null) return;

            Color c;
            float smoothness = 0.7f;
            bool showLiquid = true;
            bool glowOn = false;
            Color glowColor = new Color(0.55f, 0.8f, 1f);
            float glowIntensity = 1.4f;

            switch (look)
            {
                case Look.Empty:
                    showLiquid = false;
                    c = Color.clear;
                    break;
                case Look.ClearSolution:
                    c = new Color(0.75f, 0.88f, 0.95f, 1f);
                    break;
                case Look.WhitePrecipitate:
                    c = new Color(0.95f, 0.95f, 0.97f, 1f);
                    smoothness = 0.35f;
                    break;
                case Look.RawCrystal:
                    c = new Color(0.92f, 0.93f, 0.98f, 1f);
                    smoothness = 0.55f;
                    break;
                case Look.Stabilized:
                    c = new Color(0.88f, 0.9f, 0.95f, 1f);
                    break;
                case Look.GlowSuccess:
                    c = new Color(0.65f, 0.85f, 1f, 1f);
                    glowOn = true;
                    glowColor = new Color(0.55f, 0.85f, 1f);
                    glowIntensity = 2.2f;
                    break;
                case Look.BurntResidue:
                    c = new Color(0.12f, 0.12f, 0.14f, 1f);
                    smoothness = 0.2f;
                    break;
                case Look.Contaminated:
                    c = new Color(0.35f, 0.55f, 0.25f, 1f);
                    break;
                default:
                    c = Color.white;
                    break;
            }

            liquidRenderer.enabled = showLiquid;
            liquidRenderer.sharedMaterial = LabMaterials.Solid(c, smoothness);

            if (glow != null)
            {
                glow.enabled = glowOn;
                glow.color = glowColor;
                glow.intensity = glowIntensity;
                glow.range = 0.8f;
            }
        }
    }
}
