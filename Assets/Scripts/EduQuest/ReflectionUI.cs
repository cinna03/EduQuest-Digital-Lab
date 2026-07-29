using UnityEngine;
using UnityEngine.UI;

namespace EduQuest
{
    /// <summary>
    /// Shared reflection panel so learning is explicit after each experiment.
    /// </summary>
    public class ReflectionUI : MonoBehaviour
    {
        [SerializeField] GameObject m_Panel;
        [SerializeField] Text m_Title;
        [SerializeField] Text m_Prompt;
        [SerializeField] InputField m_Input;
        [SerializeField] Text m_Feedback;
        [SerializeField] Button m_Submit;
        [SerializeField] Button m_Close;

        public void Configure(GameObject panel, Text title, Text prompt, InputField input, Text feedback, Button submit, Button close)
        {
            m_Panel = panel;
            m_Title = title;
            m_Prompt = prompt;
            m_Input = input;
            m_Feedback = feedback;
            m_Submit = submit;
            m_Close = close;
            if (m_Submit != null)
            {
                m_Submit.onClick.RemoveAllListeners();
                m_Submit.onClick.AddListener(Submit);
            }
            if (m_Close != null)
            {
                m_Close.onClick.RemoveAllListeners();
                m_Close.onClick.AddListener(Hide);
            }
            Hide();
        }

        public void Show(string experimentTitle, string discoveryPrompt)
        {
            if (m_Panel != null) m_Panel.SetActive(true);
            if (m_Title != null) m_Title.text = experimentTitle + " — Reflection";
            if (m_Prompt != null) m_Prompt.text = discoveryPrompt + "\n\nWhat did you discover?";
            if (m_Input != null) m_Input.text = string.Empty;
            if (m_Feedback != null) m_Feedback.text = string.Empty;
        }

        public void Hide()
        {
            if (m_Panel != null) m_Panel.SetActive(false);
        }

        void Submit()
        {
            if (m_Input != null && string.IsNullOrWhiteSpace(m_Input.text))
            {
                if (m_Feedback != null)
                    m_Feedback.text = "Write one short sentence about what changed.";
                return;
            }

            if (m_Feedback != null)
                m_Feedback.text = "Great — try another experiment or reset this one.";
        }
    }
}
