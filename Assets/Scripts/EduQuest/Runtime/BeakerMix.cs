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

        [SerializeField] Light glow;
        LiquidVolume m_Liquid;
        Look m_Look = Look.Empty;

        public Look Current => m_Look;

        public void Bind(Renderer liquid, Light glowLight)
        {
            glow = glowLight;
            m_Liquid = LiquidVolume.Ensure(transform, new Color(0.75f, 0.88f, 0.95f), 0f, 0.13f, 0.09f);
            // Keep legacy renderer reference in sync with LiquidVolume mesh
            if (liquid != null && m_Liquid.Surface != null && liquid.transform != m_Liquid.Surface)
            {
                // hide duplicate mix liquid if any
                if (liquid.gameObject.name == "MixLiquid" && liquid.transform != m_Liquid.Surface)
                    liquid.enabled = false;
            }
            SetLook(Look.Empty);
        }

        public void SetLook(Look look)
        {
            m_Look = look;
            if (m_Liquid == null)
                m_Liquid = GetComponent<LiquidVolume>() ?? LiquidVolume.Ensure(transform, Color.white, 0f, 0.13f, 0.09f);

            Color c;
            float fill = 0.55f;
            bool glowOn = false;
            Color glowColor = new Color(0.55f, 0.8f, 1f);
            float glowIntensity = 1.4f;

            switch (look)
            {
                case Look.Empty:
                    fill = 0f;
                    c = new Color(0.75f, 0.88f, 0.95f);
                    break;
                case Look.ClearSolution:
                    c = new Color(0.75f, 0.88f, 0.95f);
                    fill = 0.4f;
                    break;
                case Look.WhitePrecipitate:
                    c = new Color(0.96f, 0.96f, 0.98f);
                    fill = 0.62f;
                    break;
                case Look.RawCrystal:
                    c = new Color(0.92f, 0.93f, 0.98f);
                    fill = 0.68f;
                    break;
                case Look.Stabilized:
                    c = new Color(0.86f, 0.9f, 0.96f);
                    fill = 0.7f;
                    break;
                case Look.GlowSuccess:
                    c = new Color(0.55f, 0.85f, 1f);
                    fill = 0.72f;
                    glowOn = true;
                    glowColor = new Color(0.45f, 0.85f, 1f);
                    glowIntensity = 2.6f;
                    break;
                case Look.BurntResidue:
                    c = new Color(0.12f, 0.12f, 0.14f);
                    fill = 0.35f;
                    break;
                case Look.Contaminated:
                    c = new Color(0.3f, 0.5f, 0.22f);
                    fill = 0.6f;
                    break;
                default:
                    c = Color.white;
                    break;
            }

            m_Liquid.SetLiquid(c, fill, instant: true);

            if (glow != null)
            {
                glow.enabled = glowOn;
                glow.color = glowColor;
                glow.intensity = glowIntensity;
                glow.range = 1.1f;
            }
        }
    }
}
