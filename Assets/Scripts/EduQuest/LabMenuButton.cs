using UnityEngine;
using UnityEngine.UI;

namespace EduQuest
{
    /// <summary>
    /// Attached to UI buttons. Wires onClick in Start so clicks work in Play Mode.
    /// </summary>
    public class LabMenuButton : MonoBehaviour
    {
        public enum ActionKind
        {
            OpenGermination = 0,
            OpenPendulum = 1,
            OpenFlame = 2,
            ShowMenu = 10,
            Reset = 11,
            Reflect = 12
        }

        [SerializeField] ActionKind m_Action = ActionKind.OpenGermination;

        public void SetAction(ActionKind action) => m_Action = action;

        void Start()
        {
            var button = GetComponent<Button>();
            if (button == null)
            {
                Debug.LogError("LabMenuButton needs a Button on " + name);
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
        }

        void OnClicked()
        {
            var controller = FindAnyObjectByType<LabSceneController>();
            if (controller == null)
            {
                Debug.LogError("No LabSceneController found.");
                return;
            }

            Debug.Log("Clicked: " + m_Action);
            controller.HandleAction(m_Action);
        }
    }
}
