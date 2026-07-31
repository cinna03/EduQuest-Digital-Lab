using System.Collections;
using EduQuest.AR;
using UnityEngine;
using UnityEngine.UI;

namespace EduQuest.Experiments
{
    /// <summary>
    /// AR chemistry: place beaker → add A → mix B → real light activates color change + smoke.
    /// Simulation with safety messaging — not a real lab procedure.
    /// </summary>
    public class GuidedChemistryLightLab : MonoBehaviour, ILabExperiment
    {
        public enum Step
        {
            PlaceBeaker = 0,
            AddA = 1,
            MixB = 2,
            ProvideLight = 3,
            Observe = 4,
            Complete = 5
        }

        [SerializeField] TablePotPlacer placer;
        [SerializeField] WorldLightSensor lightSensor;
        [SerializeField] GameObject placementHint;
        [SerializeField] Text stepLabel;
        [SerializeField] Text guideTitle;
        [SerializeField] Text guideBody;
        [SerializeField] Text reactionText;
        [SerializeField] Text meterLabel;
        [SerializeField] Text safetyText;
        [SerializeField] Image lightFill;
        [SerializeField] Image mixFill;
        [SerializeField] Image smokeFill;
        [SerializeField] Button addAButton;
        [SerializeField] Button mixBButton;
        [SerializeField] Button hintButton;

        Step m_Step = Step.PlaceBeaker;
        ReactionBeaker m_Beaker;
        string m_Status = "Place the beaker on the table to begin.";

        public string Title => "Chemistry · Light & Mix Reaction";
        public string Prompt =>
            "Mix two reagents in AR, then use REAL light to activate the colour change and smoke. What did light do?";
        public string Status => m_Status;
        public GameObject Root => gameObject;

        public void Bind(
            TablePotPlacer potPlacer,
            WorldLightSensor sensor,
            GameObject hint,
            Text step,
            Text title,
            Text body,
            Text reaction,
            Text meter,
            Text safety,
            Image light,
            Image mix,
            Image smoke,
            Button addA,
            Button mixB,
            Button hintBtn)
        {
            placer = potPlacer;
            lightSensor = sensor;
            placementHint = hint;
            stepLabel = step;
            guideTitle = title;
            guideBody = body;
            reactionText = reaction;
            meterLabel = meter;
            safetyText = safety;
            lightFill = light;
            mixFill = mix;
            smokeFill = smoke;
            addAButton = addA;
            mixBButton = mixB;
            hintButton = hintBtn;

            if (addAButton != null)
            {
                addAButton.onClick.RemoveAllListeners();
                addAButton.onClick.AddListener(AddReagentA);
            }
            if (mixBButton != null)
            {
                mixBButton.onClick.RemoveAllListeners();
                mixBButton.onClick.AddListener(MixReagentB);
            }
            if (hintButton != null)
            {
                hintButton.onClick.RemoveAllListeners();
                hintButton.onClick.AddListener(RefreshGuide);
            }
            if (safetyText != null)
                safetyText.text = "SIMULATION ONLY — never mix unknown chemicals or recreate this without a teacher, PPE, and a real lab.";
        }

        public void Enter()
        {
            gameObject.SetActive(true);
            ResetExperiment();
            if (lightSensor != null)
                StartCoroutine(BootCamera());
        }

        public void Exit() => gameObject.SetActive(false);

        IEnumerator BootCamera()
        {
            yield return lightSensor.StartSensor();
            ShowReaction(lightSensor.IsReady
                ? "Camera ready — place the beaker, then use real light later."
                : "Allow camera access (needed for AR light + colour activation).");
        }

        public void ResetExperiment()
        {
            m_Step = Step.PlaceBeaker;
            m_Beaker = null;
            if (placer != null)
            {
                placer.ResetPlacement();
                placer.PlacementEnabled = true;
            }
            if (placementHint) placementHint.SetActive(true);
            ShowReaction("Tap/click the table to place your reaction beaker.");
            RefreshGuide();
            RefreshControls();
        }

        void Update()
        {
            if (!isActiveAndEnabled) return;

            if (m_Step == Step.PlaceBeaker && placer != null && placer.HasPot)
            {
                var go = placer.PlacedObject;
                m_Beaker = go != null ? go.GetComponent<ReactionBeaker>() : null;
                if (m_Beaker == null && go != null)
                    m_Beaker = go.GetComponentInChildren<ReactionBeaker>();

                if (m_Beaker != null)
                {
                    if (placementHint) placementHint.SetActive(false);
                    Advance(Step.AddA, "Beaker placed. Add Reagent A.");
                }
            }

            if (m_Beaker != null && m_Step >= Step.ProvideLight)
            {
                float light = lightSensor != null ? lightSensor.Brightness : 0f;
                m_Beaker.TickLight(light, Time.deltaTime);

                if (m_Step == Step.ProvideLight && lightSensor != null && lightSensor.IsBright && m_Beaker.State == ReactionBeaker.MixState.Mixed)
                    Advance(Step.Observe, "Bright light hitting the mix — watch colour + smoke change!");

                if (m_Step == Step.Observe)
                {
                    if (m_Beaker.State == ReactionBeaker.MixState.Overexposed)
                        ShowReaction("Too much harsh light — mixture looks overexposed / burnt tone. Ease off or Reset.");
                    else if (m_Beaker.IsActivated)
                        Advance(Step.Complete, "Light-activated reaction complete — vivid colour + smoke. Open Reflect.");
                    else if (lightSensor != null && !lightSensor.IsBright)
                        ShowReaction("Reaction slowing — move back into bright light.");
                    else
                        ShowReaction($"Activating… light dose {m_Beaker.LightDose:0%} · smoke {m_Beaker.SmokeIntensity:0%}");
                }
            }

            UpdateMeters();
        }

        GameObject FindPlacedBeaker()
        {
            var beakers = FindObjectsByType<ReactionBeaker>(FindObjectsInactive.Exclude);
            return beakers != null && beakers.Length > 0 ? beakers[0].gameObject : null;
        }

        public void AddReagentA()
        {
            if (m_Step < Step.AddA)
            {
                ShowReaction("Place the beaker first.");
                return;
            }
            EnsureBeaker();
            if (m_Beaker == null) return;
            m_Beaker.AddReagentA();
            Advance(Step.MixB, "Reagent A in — pale blue tint + light vapour. Now mix Reagent B.");
        }

        public void MixReagentB()
        {
            if (m_Step < Step.MixB)
            {
                ShowReaction("Add Reagent A first.");
                return;
            }
            EnsureBeaker();
            if (m_Beaker == null) return;
            m_Beaker.MixReagentB();
            Advance(Step.ProvideLight, "Mixed — purple haze + smoke. Now shine REAL bright light on it.");
        }

        void EnsureBeaker()
        {
            if (m_Beaker != null) return;
            var go = FindPlacedBeaker();
            if (go != null) m_Beaker = go.GetComponent<ReactionBeaker>();
        }

        void Advance(Step step, string reaction)
        {
            if ((int)step < (int)m_Step) return;
            m_Step = step;
            if (!string.IsNullOrEmpty(reaction)) ShowReaction(reaction);
            RefreshGuide();
            RefreshControls();
        }

        void ShowReaction(string msg)
        {
            m_Status = msg;
            if (reactionText != null) reactionText.text = "Reaction: " + msg;
        }

        void UpdateMeters()
        {
            float light = lightSensor != null ? lightSensor.Brightness : 0f;
            if (lightFill != null)
            {
                lightFill.fillAmount = light;
                lightFill.color = lightSensor != null && lightSensor.IsBright
                    ? new Color(1f, 0.9f, 0.3f)
                    : new Color(0.55f, 0.6f, 0.65f);
            }

            if (mixFill != null)
                mixFill.fillAmount = m_Beaker != null ? m_Beaker.MixProgress : 0f;

            if (smokeFill != null)
                smokeFill.fillAmount = m_Beaker != null ? m_Beaker.SmokeIntensity : 0f;

            if (meterLabel != null)
            {
                string cam = lightSensor != null && lightSensor.IsReady ? lightSensor.Label : "NO CAMERA";
                string st = m_Beaker != null ? m_Beaker.State.ToString() : "No beaker";
                meterLabel.text = $"Light: {cam} ({light:0%}) · {st}";
            }
        }

        void RefreshGuide()
        {
            if (stepLabel != null)
                stepLabel.text = m_Step == Step.Complete ? "Done" : $"Step {(int)m_Step + 1} / 5";

            string title = "";
            string body = "";
            switch (m_Step)
            {
                case Step.PlaceBeaker:
                    title = "1 · Place beaker";
                    body = "Tap the table to place your AR beaker.\nThis is your reaction vessel in the room.";
                    break;
                case Step.AddA:
                    title = "2 · Add Reagent A";
                    body = "Press Add A.\nWatch a pale colour appear and a little vapour.";
                    break;
                case Step.MixB:
                    title = "3 · Mix Reagent B";
                    body = "Press Mix B.\nColour shifts + smoke increases — mixing started a reaction.";
                    break;
                case Step.ProvideLight:
                    title = "4 · Provide light";
                    body = "Point at a bright window or lamp.\nREAL light activates the photochemical step — colour pops, smoke thickens.\nNo fake Add Light button.";
                    break;
                case Step.Observe:
                    title = "5 · Observe";
                    body = "Keep bright light on the mix.\nNotice colour change + smoke (AR visuals).\nToo harsh for too long → overexpose cue.";
                    break;
                default:
                    title = "Complete";
                    body = "You mixed chemicals and used light to finish the change.\n\nReflect: What did mixing do vs what did light do?";
                    break;
            }

            if (guideTitle != null) guideTitle.text = title;
            if (guideBody != null) guideBody.text = body;
        }

        void RefreshControls()
        {
            if (addAButton != null)
                addAButton.interactable = m_Step == Step.AddA;
            if (mixBButton != null)
                mixBButton.interactable = m_Step == Step.MixB;
        }
    }
}
