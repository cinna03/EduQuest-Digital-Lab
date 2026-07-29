using UnityEngine;
using UnityEngine.UI;

namespace EduQuest.Experiments
{
    /// <summary>
    /// Physics: simple pendulum. Period depends on length (mass is a distractor).
    /// </summary>
    public class PendulumExperiment : MonoBehaviour, ILabExperiment
    {
        [SerializeField] Slider m_Length;
        [SerializeField] Slider m_Mass;
        [SerializeField] Transform m_Pivot;
        [SerializeField] Transform m_Bob;
        [SerializeField] LineRenderer m_String;
        [SerializeField] Text m_Readout;

        const float G = 9.81f;
        float m_Angle = 0.45f;
        float m_Omega;
        string m_Status = "Pull back happens automatically — watch the period.";

        public string Title => "Physics · Pendulum";
        public string Prompt => "Does changing the bob mass change how fast it swings? What about length?";
        public string Status => m_Status;
        public GameObject Root => gameObject;

        public void Bind(Slider length, Slider mass, Transform pivot, Transform bob, LineRenderer cord, Text readout)
        {
            m_Length = length;
            m_Mass = mass;
            m_Pivot = pivot;
            m_Bob = bob;
            m_String = cord;
            m_Readout = readout;
            Wire(m_Length);
            Wire(m_Mass);
        }

        void Wire(Slider s)
        {
            if (s == null) return;
            s.onValueChanged.RemoveListener(OnChanged);
            s.onValueChanged.AddListener(OnChanged);
        }

        void OnChanged(float _)
        {
            // Keep energy-ish angle; reset angular velocity gently when length changes.
            m_Omega *= 0.5f;
            UpdateStatus();
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
            if (m_Length)
            {
                m_Length.minValue = 0.35f;
                m_Length.maxValue = 1.4f;
                m_Length.SetValueWithoutNotify(0.8f);
            }
            if (m_Mass)
            {
                m_Mass.minValue = 0.2f;
                m_Mass.maxValue = 2f;
                m_Mass.SetValueWithoutNotify(1f);
            }
            m_Angle = 0.55f;
            m_Omega = 0f;
            UpdateStatus();
            ApplyVisuals();
        }

        void Update()
        {
            if (!isActiveAndEnabled) return;
            var length = m_Length ? m_Length.value : 0.8f;
            // Small-angle-ish nonlinear pendulum
            var alpha = -(G / length) * Mathf.Sin(m_Angle);
            m_Omega += alpha * Time.deltaTime;
            m_Omega *= 0.999f; // light damping
            m_Angle += m_Omega * Time.deltaTime;
            ApplyVisuals();
            UpdateStatus();
        }

        void ApplyVisuals()
        {
            var length = m_Length ? m_Length.value : 0.8f;
            var mass = m_Mass ? m_Mass.value : 1f;
            if (m_Pivot == null || m_Bob == null) return;

            var bobPos = m_Pivot.position + new Vector3(Mathf.Sin(m_Angle), -Mathf.Cos(m_Angle), 0f) * length;
            m_Bob.position = bobPos;
            var scale = Mathf.Lerp(0.12f, 0.28f, Mathf.InverseLerp(0.2f, 2f, mass));
            m_Bob.localScale = Vector3.one * scale;

            if (m_String != null)
            {
                m_String.positionCount = 2;
                m_String.SetPosition(0, m_Pivot.position);
                m_String.SetPosition(1, bobPos);
            }
        }

        void UpdateStatus()
        {
            var length = m_Length ? m_Length.value : 0.8f;
            var mass = m_Mass ? m_Mass.value : 1f;
            var period = 2f * Mathf.PI * Mathf.Sqrt(length / G);
            m_Status = $"Approx. period ≈ {period:0.00}s for length {length:0.00}m · mass {mass:0.00}kg (mass should barely affect period).";
            if (m_Readout != null)
                m_Readout.text = m_Status;
        }
    }
}
