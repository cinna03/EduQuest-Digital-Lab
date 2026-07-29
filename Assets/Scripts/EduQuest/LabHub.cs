using UnityEngine;
using UnityEngine.UI;

namespace EduQuest
{
    /// <summary>
    /// Hub that switches between Germination, Pendulum, and Blue Flame labs.
    /// </summary>
    public class LabHub : MonoBehaviour
    {
        [SerializeField] GameObject m_MenuPanel;
        [SerializeField] Text m_Header;
        [SerializeField] Text m_Prompt;
        [SerializeField] Text m_Status;
        [SerializeField] ReflectionUI m_Reflection;

        ILabExperiment[] m_Experiments;
        ILabExperiment m_Active;

        public void Configure(GameObject menuPanel, Text header, Text prompt, Text status, ReflectionUI reflection, ILabExperiment[] experiments)
        {
            m_MenuPanel = menuPanel;
            m_Header = header;
            m_Prompt = prompt;
            m_Status = status;
            m_Reflection = reflection;
            m_Experiments = experiments;
            ShowMenu();
        }

        public void ShowMenu()
        {
            if (m_Active != null)
            {
                m_Active.Exit();
                m_Active = null;
            }

            if (m_MenuPanel != null) m_MenuPanel.SetActive(true);
            if (m_Header != null) m_Header.text = "EduQuest Digital Lab";
            if (m_Prompt != null) m_Prompt.text = "Choose an experiment. Change variables. Watch what happens. Reflect.";
            if (m_Status != null) m_Status.text = "3 stations: Germination · Pendulum · Dancing Blue Flame";
            m_Reflection?.Hide();
        }

        public void OpenExperiment(int index)
        {
            if (m_Experiments == null || index < 0 || index >= m_Experiments.Length)
                return;

            if (m_Active != null) m_Active.Exit();
            m_Active = m_Experiments[index];
            if (m_MenuPanel != null) m_MenuPanel.SetActive(false);
            m_Active.Enter();
            RefreshHud();
        }

        public void ResetActive()
        {
            m_Active?.ResetExperiment();
            RefreshHud();
        }

        public void OpenReflection()
        {
            if (m_Active == null) return;
            m_Reflection?.Show(m_Active.Title, m_Active.Prompt);
        }

        public void RefreshHud()
        {
            if (m_Active == null) return;
            if (m_Header != null) m_Header.text = m_Active.Title;
            if (m_Prompt != null) m_Prompt.text = m_Active.Prompt;
            if (m_Status != null) m_Status.text = m_Active.Status;
        }

        void Update()
        {
            if (m_Active != null && m_Status != null)
                m_Status.text = m_Active.Status;
        }
    }
}
