using UnityEngine;

namespace EduQuest.AR
{
    /// <summary>Pot with soil + seed that germinates from water + light.</summary>
    public class GerminationPot : MonoBehaviour
    {
        public enum Stage { DrySeed, Watered, Sprouting, Seedling, Wilted, Scorched }

        [SerializeField] Transform soil;
        [SerializeField] Transform seed;
        [SerializeField] Transform sprout;
        [SerializeField] Transform leaves;
        [SerializeField] Renderer soilRenderer;
        [SerializeField] ParticleSystem waterSplash;
        [SerializeField] ParticleSystem oxygenBurst;
        [SerializeField] Light growthGlow;

        float m_Water;
        float m_LightExposure;
        float m_Growth;
        Stage m_Stage = Stage.DrySeed;

        public float Water => m_Water;
        public float LightExposure => m_LightExposure;
        public float Growth => m_Growth;
        public Stage CurrentStage => m_Stage;
        public bool HasSprouted => m_Growth >= 0.35f;

        public void Configure(
            Transform soilTf, Transform seedTf, Transform sproutTf, Transform leavesTf,
            Renderer soilRend, ParticleSystem splash, ParticleSystem oxygen, Light glow)
        {
            soil = soilTf;
            seed = seedTf;
            sprout = sproutTf;
            leaves = leavesTf;
            soilRenderer = soilRend;
            waterSplash = splash;
            oxygenBurst = oxygen;
            growthGlow = glow;
            ApplyVisuals();
        }

        public void AddWater(float amount = 0.28f)
        {
            m_Water = Mathf.Clamp01(m_Water + amount);
            if (waterSplash != null)
            {
                waterSplash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                waterSplash.Play();
            }
            Tick(0f, 0f);
        }

        /// <summary>light01 from AR/world sensor; dt in seconds.</summary>
        public void Tick(float light01, float dt)
        {
            bool bright = light01 >= 0.55f;
            bool dark = light01 <= 0.22f;

            m_Water = Mathf.Clamp01(m_Water - dt * (bright ? 0.018f : 0.006f));

            if (bright && m_Water > 0.25f && m_Water < 0.9f)
            {
                m_LightExposure += dt;
                m_Growth = Mathf.Clamp01(m_Growth + dt * (0.07f + 0.05f * m_Water));
                if (oxygenBurst != null && !oxygenBurst.isPlaying) oxygenBurst.Play();
                if (growthGlow != null)
                {
                    growthGlow.enabled = true;
                    growthGlow.intensity = 0.5f + m_Growth;
                    growthGlow.color = new Color(0.55f, 1f, 0.45f);
                }
            }
            else
            {
                if (oxygenBurst != null && oxygenBurst.isPlaying)
                    oxygenBurst.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                if (growthGlow != null && dark)
                {
                    growthGlow.enabled = true;
                    growthGlow.color = new Color(0.25f, 0.35f, 0.75f);
                    growthGlow.intensity = 0.3f;
                }
            }

            if (bright && m_Water < 0.12f && m_LightExposure > 4f)
                m_Stage = Stage.Scorched;
            else if (m_Water > 0.92f && dark)
                m_Stage = Stage.Wilted; // waterlogged stress cue
            else if (m_Growth >= 0.55f)
                m_Stage = Stage.Seedling;
            else if (m_Growth >= 0.2f)
                m_Stage = Stage.Sprouting;
            else if (m_Water >= 0.25f)
                m_Stage = Stage.Watered;
            else
                m_Stage = Stage.DrySeed;

            ApplyVisuals();
        }

        void ApplyVisuals()
        {
            if (seed != null)
                seed.gameObject.SetActive(m_Growth < 0.85f);

            if (sprout != null)
            {
                float h = Mathf.Lerp(0.02f, 0.55f, m_Growth);
                if (m_Stage == Stage.Scorched || m_Stage == Stage.Wilted) h *= 0.5f;
                sprout.localScale = new Vector3(0.06f, h, 0.06f);
                sprout.localPosition = new Vector3(0f, 0.12f + h * 0.5f, 0f);
                sprout.gameObject.SetActive(m_Growth > 0.08f);
            }

            if (leaves != null)
            {
                float open = m_Growth > 0.35f ? Mathf.Lerp(0.08f, 0.35f, m_Growth) : 0f;
                if (m_Stage == Stage.Scorched || m_Stage == Stage.Wilted) open *= 0.4f;
                leaves.localScale = Vector3.one * open;
                leaves.localPosition = new Vector3(0f, sprout != null ? sprout.localPosition.y + 0.2f : 0.5f, 0f);
                leaves.gameObject.SetActive(open > 0.05f);
                SetColor(leaves.gameObject,
                    m_Stage == Stage.Scorched ? new Color(0.55f, 0.28f, 0.12f)
                    : m_Stage == Stage.Wilted ? new Color(0.45f, 0.4f, 0.15f)
                    : new Color(0.22f, 0.7f, 0.3f));
            }

            if (soilRenderer != null)
            {
                var dry = new Color(0.45f, 0.32f, 0.18f);
                var wet = new Color(0.22f, 0.28f, 0.14f);
                var c = Color.Lerp(dry, wet, m_Water);
                var block = new MaterialPropertyBlock();
                soilRenderer.GetPropertyBlock(block);
                block.SetColor("_BaseColor", c);
                block.SetColor("_Color", c);
                soilRenderer.SetPropertyBlock(block);
            }
        }

        static void SetColor(GameObject go, Color color)
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
