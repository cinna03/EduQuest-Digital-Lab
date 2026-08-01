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
        public LiquidVolume Liquid => m_Liquid;

        public void Bind(Renderer liquid, Light glowLight)
        {
            glow = glowLight;
            m_Liquid = GetComponent<LiquidVolume>();
            if (m_Liquid == null)
                m_Liquid = LiquidVolume.Ensure(transform, LabChemicals.ClearMix, 0f, 0.16f, 0.04f);

            if (liquid != null && m_Liquid.Surface != null && liquid.transform != m_Liquid.Surface)
            {
                if (liquid.gameObject.name == "MixLiquid")
                    liquid.enabled = false;
            }
            SetLook(Look.Empty);
        }

        /// <param name="preserveFill">Keep the current fill level (use after a pour animation).</param>
        public void SetLook(Look look, bool preserveFill = false)
        {
            m_Look = look;
            if (m_Liquid == null)
            {
                m_Liquid = GetComponent<LiquidVolume>();
                if (m_Liquid == null)
                    m_Liquid = LiquidVolume.Ensure(transform, LabChemicals.ClearMix, 0f, 0.16f, 0.04f);
            }

            Color c;
            float fill = 0.55f;
            bool glowOn = false;
            Color glowColor = new Color(0.55f, 0.8f, 1f);
            float glowIntensity = 1.4f;

            switch (look)
            {
                case Look.Empty:
                    fill = 0f;
                    c = LabChemicals.ClearMix;
                    break;
                case Look.ClearSolution:
                    // AgNO3(aq) alone — colorless
                    c = LabChemicals.AgNO3;
                    fill = 0.35f;
                    break;
                case Look.WhitePrecipitate:
                    // Fresh AgCl — milky white
                    c = LabChemicals.AgClPrecipitate;
                    fill = 0.55f;
                    break;
                case Look.RawCrystal:
                    c = new Color(0.93f, 0.94f, 0.97f, 1f);
                    fill = 0.6f;
                    break;
                case Look.Stabilized:
                    // Fixer clears the suspension toward pale straw
                    c = Color.Lerp(LabChemicals.AgClPrecipitate, LabChemicals.Fixer, 0.35f);
                    c.a = 0.85f;
                    fill = 0.68f;
                    break;
                case Look.GlowSuccess:
                    c = new Color(0.55f, 0.85f, 1f, 1f);
                    fill = 0.7f;
                    glowOn = true;
                    glowColor = new Color(0.45f, 0.85f, 1f);
                    glowIntensity = 2.6f;
                    break;
                case Look.BurntResidue:
                    c = new Color(0.12f, 0.12f, 0.14f, 1f);
                    fill = 0.35f;
                    break;
                case Look.Contaminated:
                    c = new Color(0.25f, 0.45f, 0.28f, 1f);
                    fill = 0.55f;
                    break;
                default:
                    c = Color.white;
                    break;
            }

            if (preserveFill && look != Look.Empty)
                fill = Mathf.Max(0.08f, m_Liquid.Fill);

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
