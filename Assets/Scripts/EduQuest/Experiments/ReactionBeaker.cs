using UnityEngine;

namespace EduQuest.Experiments
{
    /// <summary>Beaker whose liquid color + smoke respond to mix state and real light.</summary>
    public class ReactionBeaker : MonoBehaviour
    {
        public enum MixState { Empty, ReagentA, Mixed, LightActivated, Overexposed }

        [SerializeField] Renderer liquid;
        [SerializeField] Renderer glass;
        [SerializeField] ParticleSystem smoke;
        [SerializeField] ParticleSystem sparks;
        [SerializeField] Light reactionGlow;
        [SerializeField] Transform liquidTransform;

        MixState m_State = MixState.Empty;
        float m_MixProgress;
        float m_LightDose;
        float m_SmokeIntensity;
        Color m_TargetColor = new Color(0.85f, 0.9f, 0.95f, 0.35f);
        Color m_CurrentColor = new Color(0.85f, 0.9f, 0.95f, 0.25f);

        public MixState State => m_State;
        public float MixProgress => m_MixProgress;
        public float LightDose => m_LightDose;
        public float SmokeIntensity => m_SmokeIntensity;
        public bool IsActivated => m_State == MixState.LightActivated || m_LightDose >= 1f;

        static readonly Color Clearish = new Color(0.85f, 0.9f, 0.95f, 0.25f);
        static readonly Color ReagentAColor = new Color(0.75f, 0.85f, 1f, 0.45f);
        static readonly Color MixedDim = new Color(0.55f, 0.45f, 0.7f, 0.55f);
        static readonly Color Activated = new Color(0.15f, 0.85f, 0.55f, 0.75f);
        static readonly Color Overexposed = new Color(0.95f, 0.35f, 0.15f, 0.8f);

        public void Configure(
            Renderer liquidRend, Renderer glassRend,
            ParticleSystem smokePs, ParticleSystem sparkPs,
            Light glow, Transform liquidTf)
        {
            liquid = liquidRend;
            glass = glassRend;
            smoke = smokePs;
            sparks = sparkPs;
            reactionGlow = glow;
            liquidTransform = liquidTf;
            ApplyVisuals(0f);
        }

        public void ResetBeaker()
        {
            m_State = MixState.Empty;
            m_MixProgress = 0f;
            m_LightDose = 0f;
            m_SmokeIntensity = 0f;
            m_TargetColor = Clearish;
            m_CurrentColor = Clearish;
            if (smoke != null) smoke.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (sparks != null) sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (reactionGlow != null) reactionGlow.enabled = false;
            ApplyVisuals(0f);
        }

        public void AddReagentA()
        {
            if (m_State != MixState.Empty) return;
            m_State = MixState.ReagentA;
            m_MixProgress = 0.35f;
            m_TargetColor = ReagentAColor;
            SetSmoke(0.15f);
            ApplyVisuals(0f);
        }

        public void MixReagentB()
        {
            if (m_State != MixState.ReagentA && m_State != MixState.Mixed) return;
            m_State = MixState.Mixed;
            m_MixProgress = 0.7f;
            m_TargetColor = MixedDim;
            SetSmoke(0.45f);
            if (sparks != null)
            {
                sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                sparks.Play();
            }
            ApplyVisuals(0f);
        }

        /// <summary>light01 from world/AR sensor. Bright light activates the mixture (photochemical sim).</summary>
        public void TickLight(float light01, float dt)
        {
            if (m_State != MixState.Mixed && m_State != MixState.LightActivated && m_State != MixState.Overexposed)
            {
                ApplyVisuals(dt);
                return;
            }

            bool bright = light01 >= 0.55f;
            if (bright && m_State == MixState.Mixed)
            {
                m_LightDose = Mathf.Clamp01(m_LightDose + dt * 0.35f);
                m_SmokeIntensity = Mathf.Lerp(m_SmokeIntensity, 0.55f + m_LightDose * 0.45f, dt * 3f);
                m_TargetColor = Color.Lerp(MixedDim, Activated, m_LightDose);
                if (m_LightDose >= 1f)
                    m_State = MixState.LightActivated;
            }
            else if (bright && m_State == MixState.LightActivated)
            {
                // Extra harsh light after activation → overexpose cue
                m_LightDose = Mathf.Clamp(m_LightDose + dt * 0.15f, 0f, 1.6f);
                if (m_LightDose > 1.35f)
                {
                    m_State = MixState.Overexposed;
                    m_TargetColor = Overexposed;
                    m_SmokeIntensity = 0.95f;
                }
            }
            else if (!bright)
            {
                m_SmokeIntensity = Mathf.Lerp(m_SmokeIntensity, m_State == MixState.Mixed ? 0.35f : 0.5f, dt * 1.5f);
            }

            SetSmoke(m_SmokeIntensity);
            ApplyVisuals(dt);
        }

        void SetSmoke(float intensity)
        {
            m_SmokeIntensity = Mathf.Clamp01(intensity);
            if (smoke == null) return;

            var emission = smoke.emission;
            emission.rateOverTime = 8f + m_SmokeIntensity * 55f;
            var main = smoke.main;
            main.startColor = m_State == MixState.Overexposed
                ? new Color(0.7f, 0.35f, 0.2f, 0.55f)
                : m_State == MixState.LightActivated
                    ? new Color(0.45f, 0.95f, 0.7f, 0.5f)
                    : new Color(0.75f, 0.75f, 0.8f, 0.4f);

            if (m_SmokeIntensity > 0.05f && !smoke.isPlaying) smoke.Play();
            if (m_SmokeIntensity <= 0.05f && smoke.isPlaying)
                smoke.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        void ApplyVisuals(float dt)
        {
            if (liquid != null)
            {
                m_CurrentColor = dt <= 0f
                    ? m_TargetColor
                    : Color.Lerp(m_CurrentColor, m_TargetColor, Mathf.Clamp01(dt * 4f));
                var block = new MaterialPropertyBlock();
                liquid.GetPropertyBlock(block);
                block.SetColor("_BaseColor", m_CurrentColor);
                block.SetColor("_Color", m_CurrentColor);
                liquid.SetPropertyBlock(block);
            }

            if (liquidTransform != null)
            {
                float h = m_State == MixState.Empty ? 0.08f : 0.18f + m_MixProgress * 0.12f;
                liquidTransform.localScale = new Vector3(0.22f, h, 0.22f);
                liquidTransform.localPosition = new Vector3(0f, 0.12f + h * 0.5f, 0f);
                liquidTransform.gameObject.SetActive(m_State != MixState.Empty || m_MixProgress > 0f);
            }

            if (reactionGlow != null)
            {
                bool on = m_State == MixState.LightActivated || m_State == MixState.Overexposed || m_LightDose > 0.3f;
                reactionGlow.enabled = on;
                if (on)
                {
                    reactionGlow.color = m_State == MixState.Overexposed
                        ? new Color(1f, 0.4f, 0.2f)
                        : new Color(0.3f, 1f, 0.65f);
                    reactionGlow.intensity = 0.4f + m_LightDose * 1.2f;
                }
            }
        }
    }
}
