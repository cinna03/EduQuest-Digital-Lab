using UnityEngine;
using UnityEngine.UI;

namespace EduQuest.Experiments
{
    /// <summary>
    /// Primary science: germination — water + warmth grow a seed into a sprout.
    /// </summary>
    public class GerminationExperiment : MonoBehaviour, ILabExperiment
    {
        [SerializeField] Slider m_Water;
        [SerializeField] Slider m_Warmth;
        [SerializeField] Slider m_Days;
        [SerializeField] Transform m_Seed;
        [SerializeField] Transform m_Sprout;
        [SerializeField] Transform m_Leaves;
        [SerializeField] Renderer m_Soil;
        [SerializeField] Light m_GrowLight;

        float m_Progress;
        string m_Status = "Add water and warmth, then advance days.";

        public string Title => "Science · Germination";
        public string Prompt => "What does a seed need before it germinates and sprouts?";
        public string Status => m_Status;
        public GameObject Root => gameObject;

        public void Bind(Slider water, Slider warmth, Slider days, Transform seed, Transform sprout, Transform leaves, Renderer soil, Light growLight)
        {
            m_Water = water;
            m_Warmth = warmth;
            m_Days = days;
            m_Seed = seed;
            m_Sprout = sprout;
            m_Leaves = leaves;
            m_Soil = soil;
            m_GrowLight = growLight;
            Wire(m_Water);
            Wire(m_Warmth);
            Wire(m_Days);
        }

        void Wire(Slider s)
        {
            if (s == null) return;
            s.minValue = 0f;
            s.maxValue = 1f;
            s.onValueChanged.RemoveListener(OnChanged);
            s.onValueChanged.AddListener(OnChanged);
        }

        void OnChanged(float _)
        {
            Evaluate();
        }

        public void Enter()
        {
            gameObject.SetActive(true);
            ResetExperiment();
        }

        public void Exit()
        {
            gameObject.SetActive(false);
        }

        public void ResetExperiment()
        {
            if (m_Water) m_Water.SetValueWithoutNotify(0.2f);
            if (m_Warmth) m_Warmth.SetValueWithoutNotify(0.25f);
            if (m_Days) m_Days.SetValueWithoutNotify(0f);
            Evaluate();
        }

        void Evaluate()
        {
            var water = m_Water ? m_Water.value : 0f;
            var warmth = m_Warmth ? m_Warmth.value : 0f;
            var days = m_Days ? m_Days.value : 0f;

            var conditions = Mathf.Min(water, warmth);
            var canGrow = conditions > 0.35f;
            m_Progress = canGrow ? days * Mathf.Lerp(0.4f, 1f, conditions) : days * 0.05f;

            if (m_Seed)
            {
                m_Seed.localScale = Vector3.one * Mathf.Lerp(0.18f, 0.08f, m_Progress);
                m_Seed.gameObject.SetActive(m_Progress < 0.85f);
            }

            if (m_Sprout)
            {
                var h = Mathf.Lerp(0.02f, 0.7f, Mathf.Clamp01((m_Progress - 0.15f) / 0.7f));
                m_Sprout.localScale = new Vector3(0.06f, h, 0.06f);
                m_Sprout.localPosition = new Vector3(0f, h * 0.5f + 0.05f, 0f);
                m_Sprout.gameObject.SetActive(m_Progress > 0.15f);
            }

            if (m_Leaves)
            {
                var open = Mathf.Clamp01((m_Progress - 0.55f) / 0.45f);
                m_Leaves.localScale = Vector3.one * Mathf.Lerp(0.05f, 0.35f, open);
                m_Leaves.gameObject.SetActive(open > 0.05f);
            }

            if (m_Soil)
            {
                var block = new MaterialPropertyBlock();
                m_Soil.GetPropertyBlock(block);
                var dry = new Color(0.45f, 0.32f, 0.18f);
                var wet = new Color(0.22f, 0.28f, 0.14f);
                var c = Color.Lerp(dry, wet, water);
                block.SetColor("_BaseColor", c);
                block.SetColor("_Color", c);
                m_Soil.SetPropertyBlock(block);
            }

            if (m_GrowLight)
                m_GrowLight.intensity = Mathf.Lerp(0.4f, 1.4f, warmth);

            if (!canGrow && days > 0.2f)
                m_Status = "Little change — seeds need enough water and warmth to germinate.";
            else if (m_Progress < 0.2f)
                m_Status = "Seed resting in soil. Increase water, warmth, and days.";
            else if (m_Progress < 0.55f)
                m_Status = "Germination started — radicle/sprout emerging.";
            else
                m_Status = "Seedling established — leaves opening.";
        }
    }
}
