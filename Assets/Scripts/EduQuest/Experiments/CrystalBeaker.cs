using UnityEngine;

namespace EduQuest.Experiments
{
    /// <summary>Visuals for the Photographic Crystal puzzle (AgCl-inspired AR simulation).</summary>
    public class CrystalBeaker : MonoBehaviour
    {
        public enum Look
        {
            Empty,
            ClearSolution,
            WhitePrecipitate,
            RawCrystal,
            StableGlow,
            BurntResidue,
            IncompleteFlakes,
            WeakCloudy,
            Contaminated,
            Dissolved,
            UnstableGrey,
            YellowUnstable,
            NoCrystal
        }

        [SerializeField] Renderer liquid;
        [SerializeField] Transform liquidTransform;
        [SerializeField] Transform crystal;
        [SerializeField] ParticleSystem smoke;
        [SerializeField] ParticleSystem sparkle;
        [SerializeField] Light glow;

        Look m_Look = Look.Empty;
        Color m_Color = new Color(0.9f, 0.92f, 0.95f, 0.2f);

        public Look CurrentLook => m_Look;

        public void Configure(
            Renderer liquidRend, Transform liquidTf, Transform crystalTf,
            ParticleSystem smokePs, ParticleSystem sparklePs, Light glowLight)
        {
            liquid = liquidRend;
            liquidTransform = liquidTf;
            crystal = crystalTf;
            smoke = smokePs;
            sparkle = sparklePs;
            glow = glowLight;
            SetLook(Look.Empty);
        }

        public void SetLook(Look look)
        {
            m_Look = look;
            bool showCrystal = look == Look.RawCrystal || look == Look.StableGlow
                               || look == Look.BurntResidue || look == Look.UnstableGrey;
            bool showLiquid = look != Look.Empty;

            if (crystal != null)
            {
                crystal.gameObject.SetActive(showCrystal);
                float scale = look == Look.StableGlow ? 0.22f : look == Look.RawCrystal ? 0.16f : 0.14f;
                crystal.localScale = Vector3.one * scale;
                SetRendererColor(crystal.gameObject,
                    look == Look.StableGlow ? new Color(0.65f, 0.85f, 1f)
                    : look == Look.BurntResidue || look == Look.UnstableGrey ? new Color(0.15f, 0.15f, 0.18f)
                    : new Color(0.92f, 0.92f, 0.95f));
            }

            if (liquidTransform != null)
                liquidTransform.gameObject.SetActive(showLiquid);

            switch (look)
            {
                case Look.Empty:
                    m_Color = new Color(0.9f, 0.92f, 0.95f, 0.15f);
                    SetSmoke(0f, Color.clear);
                    SetGlow(false, Color.white, 0f);
                    StopSparkle();
                    break;
                case Look.ClearSolution:
                    m_Color = new Color(0.85f, 0.9f, 1f, 0.4f);
                    SetSmoke(0.05f, new Color(0.8f, 0.85f, 0.95f, 0.25f));
                    SetGlow(false, Color.white, 0f);
                    StopSparkle();
                    SetLiquidHeight(0.2f);
                    break;
                case Look.WhitePrecipitate:
                    m_Color = new Color(0.95f, 0.95f, 0.97f, 0.75f);
                    SetSmoke(0.35f, new Color(0.9f, 0.9f, 0.95f, 0.45f));
                    SetGlow(false, Color.white, 0f);
                    StopSparkle();
                    SetLiquidHeight(0.28f);
                    break;
                case Look.RawCrystal:
                    m_Color = new Color(0.9f, 0.9f, 0.93f, 0.55f);
                    SetSmoke(0.15f, new Color(0.85f, 0.85f, 0.9f, 0.3f));
                    SetGlow(true, new Color(0.8f, 0.85f, 0.95f), 0.25f);
                    StopSparkle();
                    SetLiquidHeight(0.22f);
                    break;
                case Look.StableGlow:
                    m_Color = new Color(0.55f, 0.75f, 0.95f, 0.55f);
                    SetSmoke(0.2f, new Color(0.6f, 0.85f, 1f, 0.4f));
                    SetGlow(true, new Color(0.55f, 0.85f, 1f), 1.4f);
                    PlaySparkle();
                    SetLiquidHeight(0.2f);
                    break;
                case Look.BurntResidue:
                    m_Color = new Color(0.2f, 0.2f, 0.22f, 0.85f);
                    SetSmoke(0.7f, new Color(0.35f, 0.3f, 0.28f, 0.55f));
                    SetGlow(true, new Color(0.4f, 0.25f, 0.15f), 0.5f);
                    StopSparkle();
                    SetLiquidHeight(0.2f);
                    break;
                case Look.IncompleteFlakes:
                    m_Color = new Color(0.85f, 0.88f, 0.92f, 0.4f);
                    SetSmoke(0.2f, new Color(0.8f, 0.82f, 0.88f, 0.3f));
                    SetGlow(false, Color.white, 0f);
                    StopSparkle();
                    SetLiquidHeight(0.18f);
                    break;
                case Look.WeakCloudy:
                    m_Color = new Color(0.8f, 0.82f, 0.86f, 0.45f);
                    SetSmoke(0.25f, new Color(0.75f, 0.78f, 0.82f, 0.35f));
                    SetGlow(false, Color.white, 0f);
                    StopSparkle();
                    SetLiquidHeight(0.22f);
                    break;
                case Look.Contaminated:
                    m_Color = new Color(0.2f, 0.55f, 0.45f, 0.7f);
                    SetSmoke(0.65f, new Color(0.3f, 0.6f, 0.35f, 0.5f));
                    SetGlow(true, new Color(0.2f, 0.7f, 0.4f), 0.6f);
                    StopSparkle();
                    SetLiquidHeight(0.26f);
                    break;
                case Look.Dissolved:
                    m_Color = new Color(0.75f, 0.85f, 0.9f, 0.35f);
                    SetSmoke(0.1f, new Color(0.7f, 0.8f, 0.9f, 0.25f));
                    SetGlow(false, Color.white, 0f);
                    StopSparkle();
                    SetLiquidHeight(0.24f);
                    if (crystal != null) crystal.gameObject.SetActive(false);
                    break;
                case Look.UnstableGrey:
                    m_Color = new Color(0.45f, 0.45f, 0.48f, 0.75f);
                    SetSmoke(0.4f, new Color(0.4f, 0.4f, 0.42f, 0.45f));
                    SetGlow(true, new Color(0.5f, 0.5f, 0.55f), 0.4f);
                    StopSparkle();
                    SetLiquidHeight(0.2f);
                    break;
                case Look.YellowUnstable:
                    m_Color = new Color(0.95f, 0.9f, 0.55f, 0.65f);
                    SetSmoke(0.35f, new Color(0.9f, 0.8f, 0.4f, 0.4f));
                    SetGlow(false, Color.white, 0f);
                    StopSparkle();
                    SetLiquidHeight(0.26f);
                    break;
                case Look.NoCrystal:
                    m_Color = new Color(0.85f, 0.9f, 0.95f, 0.35f);
                    SetSmoke(0.08f, new Color(0.8f, 0.85f, 0.9f, 0.2f));
                    SetGlow(false, Color.white, 0f);
                    StopSparkle();
                    SetLiquidHeight(0.22f);
                    if (crystal != null) crystal.gameObject.SetActive(false);
                    break;
            }

            ApplyLiquidColor();
        }

        void SetLiquidHeight(float h)
        {
            if (liquidTransform == null) return;
            liquidTransform.localScale = new Vector3(0.22f, h, 0.22f);
            liquidTransform.localPosition = new Vector3(0f, 0.12f + h * 0.5f, 0f);
        }

        void ApplyLiquidColor()
        {
            if (liquid == null) return;
            var block = new MaterialPropertyBlock();
            liquid.GetPropertyBlock(block);
            block.SetColor("_BaseColor", m_Color);
            block.SetColor("_Color", m_Color);
            liquid.SetPropertyBlock(block);
        }

        void SetSmoke(float intensity, Color color)
        {
            if (smoke == null) return;
            var emission = smoke.emission;
            emission.rateOverTime = intensity * 50f;
            var main = smoke.main;
            main.startColor = color;
            if (intensity > 0.05f && !smoke.isPlaying) smoke.Play();
            if (intensity <= 0.05f && smoke.isPlaying)
                smoke.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        void SetGlow(bool on, Color color, float intensity)
        {
            if (glow == null) return;
            glow.enabled = on;
            glow.color = color;
            glow.intensity = intensity;
        }

        void PlaySparkle()
        {
            if (sparkle == null) return;
            if (!sparkle.isPlaying) sparkle.Play();
        }

        void StopSparkle()
        {
            if (sparkle == null) return;
            sparkle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        static void SetRendererColor(GameObject go, Color color)
        {
            var r = go.GetComponent<Renderer>();
            if (r == null) return;
            var block = new MaterialPropertyBlock();
            r.GetPropertyBlock(block);
            block.SetColor("_BaseColor", color);
            block.SetColor("_Color", color);
            r.SetPropertyBlock(block);
        }
    }
}
