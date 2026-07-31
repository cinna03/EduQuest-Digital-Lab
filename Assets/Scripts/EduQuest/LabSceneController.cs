using EduQuest.Experiments;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace EduQuest
{
    public class LabSceneController : MonoBehaviour
    {
        LabHub m_Hub;
        GameObject m_GerminationControls;
        GameObject m_PendulumControls;
        GameObject m_FlameControls;
        GerminationExperiment m_Germination;
        PendulumExperiment m_Pendulum;
        BlueFlameExperiment m_Flame;
        ReflectionUI m_Reflection;
        GameObject m_MenuPanel;
        Text m_Header;
        Text m_Prompt;
        Text m_Status;

        void Awake()
        {
            EnsureEventSystem();
            AutoFind();
            RebindExperiments();
            RebindReflection();

            if (m_Hub != null)
            {
                m_Hub.Configure(
                    m_MenuPanel,
                    m_Header,
                    m_Prompt,
                    m_Status,
                    m_Reflection,
                    new ILabExperiment[] { m_Germination, m_Pendulum, m_Flame });
            }

            // Backup wiring in case LabMenuButton is missing
            BindButton("GerminationButton", () => HandleAction(LabMenuButton.ActionKind.OpenGermination));
            BindButton("PendulumButton", () => HandleAction(LabMenuButton.ActionKind.OpenPendulum));
            BindButton("FlameButton", () => HandleAction(LabMenuButton.ActionKind.OpenFlame));
            BindButton("MenuButton", () => HandleAction(LabMenuButton.ActionKind.ShowMenu));
            BindButton("ResetButton", () => HandleAction(LabMenuButton.ActionKind.Reset));
            BindButton("ReflectButton", () => HandleAction(LabMenuButton.ActionKind.Reflect));

            ShowMenu();
            Debug.Log("EduQuest ready. Hub=" + (m_Hub != null) + " GermBtn=" + (GameObject.Find("GerminationButton") != null));
        }

        public void HandleAction(LabMenuButton.ActionKind action)
        {
            switch (action)
            {
                case LabMenuButton.ActionKind.OpenGermination:
                    Open(0);
                    break;
                case LabMenuButton.ActionKind.OpenPendulum:
                    Open(1);
                    break;
                case LabMenuButton.ActionKind.OpenFlame:
                    Open(2);
                    break;
                case LabMenuButton.ActionKind.ShowMenu:
                    ShowMenu();
                    break;
                case LabMenuButton.ActionKind.Reset:
                    m_Hub?.ResetActive();
                    break;
                case LabMenuButton.ActionKind.Reflect:
                    m_Hub?.OpenReflection();
                    break;
            }
        }

        void AutoFind()
        {
            m_Hub = FindAnyObjectByType<LabHub>();
            m_Germination = FindAnyObjectByType<GerminationExperiment>(FindObjectsInactive.Include);
            m_Pendulum = FindAnyObjectByType<PendulumExperiment>(FindObjectsInactive.Include);
            m_Flame = FindAnyObjectByType<BlueFlameExperiment>(FindObjectsInactive.Include);
            m_Reflection = FindAnyObjectByType<ReflectionUI>(FindObjectsInactive.Include);
            m_MenuPanel = GameObject.Find("MenuPanel");
            m_GerminationControls = GameObject.Find("GerminationControls");
            m_PendulumControls = GameObject.Find("PendulumControls");
            m_FlameControls = GameObject.Find("FlameControls");

            // GameObject.Find skips inactive — controls start inactive; search including inactive
            m_GerminationControls ??= FindInactiveByName("GerminationControls");
            m_PendulumControls ??= FindInactiveByName("PendulumControls");
            m_FlameControls ??= FindInactiveByName("FlameControls");

            var headerGo = GameObject.Find("Header");
            m_Header = headerGo ? headerGo.GetComponent<Text>() : null;
            var promptGo = GameObject.Find("Prompt");
            m_Prompt = promptGo ? promptGo.GetComponent<Text>() : null;
            var statusGo = GameObject.Find("Status");
            m_Status = statusGo ? statusGo.GetComponent<Text>() : null;
        }

        static GameObject FindInactiveByName(string name)
        {
            foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
            {
                if (t.name == name && t.gameObject.scene.IsValid())
                    return t.gameObject;
            }
            return null;
        }

        void RebindExperiments()
        {
            if (m_Germination != null && m_GerminationControls != null)
            {
                m_Germination.Bind(
                    FindSlider(m_GerminationControls.transform, "Water"),
                    FindSlider(m_GerminationControls.transform, "Warmth"),
                    FindSlider(m_GerminationControls.transform, "Days"),
                    m_Germination.transform.Find("Seed"),
                    m_Germination.transform.Find("Sprout"),
                    m_Germination.transform.Find("Leaves"),
                    m_Germination.transform.Find("Soil")?.GetComponent<Renderer>(),
                    m_Germination.transform.Find("GrowLight")?.GetComponent<Light>());
            }

            if (m_Pendulum != null && m_PendulumControls != null)
            {
                m_Pendulum.Bind(
                    FindSlider(m_PendulumControls.transform, "Length"),
                    FindSlider(m_PendulumControls.transform, "Mass"),
                    m_Pendulum.transform.Find("Pivot"),
                    m_Pendulum.transform.Find("Bob"),
                    m_Pendulum.transform.Find("Cord")?.GetComponent<LineRenderer>(),
                    m_PendulumControls.transform.Find("Readout")?.GetComponent<Text>());
            }

            if (m_Flame != null && m_FlameControls != null)
            {
                m_Flame.Bind(
                    FindSlider(m_FlameControls.transform, "Acid amount (HCl)"),
                    m_FlameControls.transform.Find("IgniteButton")?.GetComponent<Button>(),
                    m_Flame.transform.Find("Bubbles")?.GetComponent<ParticleSystem>(),
                    m_Flame.transform.Find("Flame")?.GetComponent<ParticleSystem>(),
                    m_Flame.transform.Find("Liquid")?.GetComponent<Renderer>(),
                    m_Flame.transform.Find("FlameLight")?.GetComponent<Light>(),
                    m_FlameControls.transform.Find("Safety")?.GetComponent<Text>());
            }
        }

        void RebindReflection()
        {
            if (m_Reflection == null) return;
            var panel = FindInactiveByName("ReflectionPanel") ?? GameObject.Find("ReflectionPanel");
            if (panel == null) return;
            m_Reflection.Configure(
                panel,
                panel.transform.Find("RefTitle")?.GetComponent<Text>(),
                panel.transform.Find("RefPrompt")?.GetComponent<Text>(),
                panel.transform.Find("Input")?.GetComponent<InputField>(),
                panel.transform.Find("Feedback")?.GetComponent<Text>(),
                panel.transform.Find("Submit")?.GetComponent<Button>(),
                panel.transform.Find("Close")?.GetComponent<Button>());
        }

        static Slider FindSlider(Transform root, string rowName)
        {
            var row = root.Find(rowName + "Row");
            return row == null ? null : row.GetComponentInChildren<Slider>(true);
        }

        void Open(int index)
        {
            Debug.Log("Opening experiment " + index);
            m_Hub?.OpenExperiment(index);
            SetControls(index);
        }

        void ShowMenu()
        {
            m_Hub?.ShowMenu();
            SetControls(-1);
        }

        void SetControls(int index)
        {
            if (m_GerminationControls) m_GerminationControls.SetActive(index == 0);
            if (m_PendulumControls) m_PendulumControls.SetActive(index == 1);
            if (m_FlameControls) m_FlameControls.SetActive(index == 2);
        }

        static void BindButton(string name, UnityEngine.Events.UnityAction action)
        {
            var go = GameObject.Find(name);
            if (go == null) return;
            var button = go.GetComponent<Button>();
            if (button == null) return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        static void EnsureEventSystem()
        {
            var es = FindAnyObjectByType<EventSystem>();
            if (es == null)
            {
                var go = new GameObject("EventSystem");
                es = go.AddComponent<EventSystem>();
            }

            // Prefer modules that work with mouse in the Game view.
            if (es.GetComponent<StandaloneInputModule>() == null)
                es.gameObject.AddComponent<StandaloneInputModule>();

            var inputSys = es.GetComponent<InputSystemUIInputModule>();
            if (inputSys == null)
                inputSys = es.gameObject.AddComponent<InputSystemUIInputModule>();

            // Avoid both modules fighting — disable Input System module if Standalone is present
            // when project allows legacy input; otherwise keep Input System module enabled.
#if ENABLE_LEGACY_INPUT_MANAGER
            inputSys.enabled = false;
            var standalone = es.GetComponent<StandaloneInputModule>();
            if (standalone != null) standalone.enabled = true;
#else
            inputSys.enabled = true;
            var standalone = es.GetComponent<StandaloneInputModule>();
            if (standalone != null) standalone.enabled = false;
#endif
        }
    }
}
