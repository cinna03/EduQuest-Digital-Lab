using UnityEngine;
using UnityEngine.UI;

namespace EduQuest.Experiments
{
    /// <summary>
    /// Chemistry sim: Al + HCl → H2 bubbles, then ignited pale blue "dancing" flame.
    /// Educational simulation only — includes safety messaging.
    /// </summary>
    public class BlueFlameExperiment : MonoBehaviour, ILabExperiment
    {
        [SerializeField] Slider m_AcidAmount;
        [SerializeField] Button m_IgniteButton;
        [SerializeField] ParticleSystem m_Bubbles;
        [SerializeField] ParticleSystem m_Flame;
        [SerializeField] Renderer m_Liquid;
        [SerializeField] Light m_FlameLight;
        [SerializeField] Text m_Safety;

        bool m_Ignited;
        float m_Dance;
        string m_Status = "Add acid to react with aluminium and release hydrogen.";

        public string Title => "Chemistry · Dancing Blue Flame";
        public string Prompt => "Aluminium + hydrochloric acid releases hydrogen. What happens when that gas is ignited?";
        public string Status => m_Status;
        public GameObject Root => gameObject;

        public void Bind(Slider acid, Button ignite, ParticleSystem bubbles, ParticleSystem flame, Renderer liquid, Light flameLight, Text safety)
        {
            m_AcidAmount = acid;
            m_IgniteButton = ignite;
            m_Bubbles = bubbles;
            m_Flame = flame;
            m_Liquid = liquid;
            m_FlameLight = flameLight;
            m_Safety = safety;

            if (m_AcidAmount != null)
            {
                m_AcidAmount.minValue = 0f;
                m_AcidAmount.maxValue = 1f;
                m_AcidAmount.onValueChanged.RemoveListener(OnAcid);
                m_AcidAmount.onValueChanged.AddListener(OnAcid);
            }

            if (m_IgniteButton != null)
            {
                m_IgniteButton.onClick.RemoveAllListeners();
                m_IgniteButton.onClick.AddListener(Ignite);
            }

            if (m_Safety != null)
                m_Safety.text = "Simulation only. Real HCl + Al needs a teacher, PPE, and proper ventilation — never try unsupervised.";
        }

        void OnAcid(float _)
        {
            if (m_Ignited) Extinguish();
            EvaluateGas();
        }

        public void Enter()
        {
            gameObject.SetActive(true);
            ResetExperiment();
        }

        public void Exit()
        {
            Extinguish();
            gameObject.SetActive(false);
        }

        public void ResetExperiment()
        {
            Extinguish();
            if (m_AcidAmount) m_AcidAmount.SetValueWithoutNotify(0.05f);
            EvaluateGas();
        }

        void EvaluateGas()
        {
            var acid = m_AcidAmount ? m_AcidAmount.value : 0f;
            if (m_Bubbles != null)
            {
                var emission = m_Bubbles.emission;
                emission.rateOverTime = Mathf.Lerp(0f, 60f, acid);
                if (acid > 0.08f)
                {
                    if (!m_Bubbles.isPlaying) m_Bubbles.Play();
                }
                else
                {
                    m_Bubbles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }

            if (m_Liquid != null)
            {
                var block = new MaterialPropertyBlock();
                m_Liquid.GetPropertyBlock(block);
                var clear = new Color(0.75f, 0.85f, 0.95f, 0.45f);
                var reacted = new Color(0.55f, 0.75f, 0.9f, 0.65f);
                var c = Color.Lerp(clear, reacted, acid);
                block.SetColor("_BaseColor", c);
                block.SetColor("_Color", c);
                m_Liquid.SetPropertyBlock(block);
            }

            if (acid < 0.15f)
                m_Status = "Low reaction — add more acid to generate hydrogen bubbles.";
            else if (!m_Ignited)
                m_Status = "Hydrogen gas forming (bubbles). Press Ignite when ready.";
            else
                m_Status = "Hydrogen ignited — pale blue flame dancing with the gas flow.";
        }

        public void Ignite()
        {
            var acid = m_AcidAmount ? m_AcidAmount.value : 0f;
            if (acid < 0.2f)
            {
                m_Status = "Not enough hydrogen yet — increase acid first.";
                return;
            }

            m_Ignited = true;
            if (m_Flame != null && !m_Flame.isPlaying) m_Flame.Play();
            if (m_FlameLight != null) m_FlameLight.enabled = true;
            m_Status = "Hydrogen ignited — pale blue flame dancing with the gas flow.";
        }

        void Extinguish()
        {
            m_Ignited = false;
            if (m_Flame != null) m_Flame.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (m_FlameLight != null)
            {
                m_FlameLight.enabled = false;
                m_FlameLight.intensity = 0f;
            }
        }

        void Update()
        {
            if (!m_Ignited || m_FlameLight == null) return;
            m_Dance += Time.deltaTime * 8f;
            var acid = m_AcidAmount ? m_AcidAmount.value : 0.5f;
            var flicker = 1f + Mathf.Sin(m_Dance) * 0.25f + Mathf.Sin(m_Dance * 2.7f) * 0.15f;
            m_FlameLight.intensity = Mathf.Lerp(0.8f, 2.2f, acid) * flicker;
            m_FlameLight.color = new Color(0.35f, 0.55f, 1f);

            if (m_Flame != null)
            {
                var main = m_Flame.main;
                main.startColor = new Color(0.4f, 0.65f, 1f, 0.85f);
                var emission = m_Flame.emission;
                emission.rateOverTime = 25f * flicker * acid;
            }
        }
    }
}
