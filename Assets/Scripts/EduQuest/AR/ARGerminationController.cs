using System.Collections;
using EduQuest;
using UnityEngine;
using UnityEngine.UI;

namespace EduQuest.AR
{
    /// <summary>
    /// Table AR germination lab:
    /// scan table → tap to place pot → water → provide light → sprout → reflect.
    /// </summary>
    public class ARGerminationController : MonoBehaviour, ILabExperiment
    {
        public enum Step
        {
            ScanTable = 0,
            PlacePot = 1,
            Water = 2,
            ProvideLight = 3,
            Growing = 4,
            Complete = 5
        }

        [SerializeField] TablePotPlacer placer;
        [SerializeField] WorldLightSensor lightSensor;
        [SerializeField] GameObject scanningVisual;
        [SerializeField] GameObject tablePlane;
        [SerializeField] Text stepLabel;
        [SerializeField] Text guideTitle;
        [SerializeField] Text guideBody;
        [SerializeField] Text reactionText;
        [SerializeField] Text meterLabel;
        [SerializeField] Image lightFill;
        [SerializeField] Image waterFill;
        [SerializeField] Image growthFill;
        [SerializeField] Button waterButton;
        [SerializeField] Button hintButton;

        Step m_Step = Step.ScanTable;
        float m_ScanTimer;
        string m_Status = "Scan a flat table to begin.";

        const float ScanSeconds = 2.2f;
        const float GrowTarget = 0.5f;

        public string Title => "Biology · AR Table Germination";
        public string Prompt =>
            "Place a pot on a real table, water it, and give it real light so the seed can germinate.";
        public string Status => m_Status;
        public GameObject Root => gameObject;

        public void Bind(
            TablePotPlacer potPlacer,
            WorldLightSensor sensor,
            GameObject scanning,
            GameObject table,
            Text step,
            Text title,
            Text body,
            Text reaction,
            Text meter,
            Image light,
            Image water,
            Image growth,
            Button waterBtn,
            Button hint)
        {
            placer = potPlacer;
            lightSensor = sensor;
            scanningVisual = scanning;
            tablePlane = table;
            stepLabel = step;
            guideTitle = title;
            guideBody = body;
            reactionText = reaction;
            meterLabel = meter;
            lightFill = light;
            waterFill = water;
            growthFill = growth;
            waterButton = waterBtn;
            hintButton = hint;

            if (waterButton != null)
            {
                waterButton.onClick.RemoveAllListeners();
                waterButton.onClick.AddListener(WaterPot);
            }

            if (hintButton != null)
            {
                hintButton.onClick.RemoveAllListeners();
                hintButton.onClick.AddListener(RefreshGuide);
            }
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
                ? "Camera ready — look at a table surface."
                : "Allow camera access (needed for light sensing + AR preview).");
        }

        public void ResetExperiment()
        {
            m_Step = Step.ScanTable;
            m_ScanTimer = 0f;
            if (placer != null)
            {
                placer.ResetPlacement();
                placer.PlacementEnabled = false;
            }
            if (scanningVisual) scanningVisual.SetActive(true);
            if (tablePlane) tablePlane.SetActive(false);
            ShowReaction("Move around and look at a flat table — scanning surfaces…");
            RefreshGuide();
            RefreshControls();
        }

        void Update()
        {
            if (!isActiveAndEnabled) return;
            UpdateMeters();

            switch (m_Step)
            {
                case Step.ScanTable:
                    m_ScanTimer += Time.deltaTime;
                    float scanProgress = Mathf.Clamp01(m_ScanTimer / ScanSeconds);
                    ShowReaction($"Scanning for a table… {scanProgress:0%}");
                    if (m_ScanTimer >= ScanSeconds)
                    {
                        if (scanningVisual) scanningVisual.SetActive(false);
                        if (tablePlane) tablePlane.SetActive(true);
                        if (placer != null) placer.PlacementEnabled = true;
                        Advance(Step.PlacePot, "Table found. Tap/click the table to place the pot with soil and seed.");
                    }
                    break;

                case Step.PlacePot:
                    if (placer != null && placer.HasPot)
                        Advance(Step.Water, "Pot placed. Water the soil so germination can start.");
                    break;

                case Step.Water:
                case Step.ProvideLight:
                case Step.Growing:
                    var pot = GetPot();
                    if (pot == null) break;

                    float light = lightSensor != null ? lightSensor.Brightness : 0f;
                    pot.Tick(light, Time.deltaTime);

                    if (m_Step == Step.Water && pot.Water >= 0.28f)
                        Advance(Step.ProvideLight, "Soil is moist. Now give the plant REAL light — face a window or lamp.");

                    if (m_Step == Step.ProvideLight && lightSensor != null && lightSensor.IsBright && pot.Water >= 0.2f)
                        Advance(Step.Growing, "Bright light detected — photosynthesis/germination under way.");

                    if (m_Step == Step.Growing)
                    {
                        if (pot.CurrentStage == GerminationPot.Stage.Scorched)
                            ShowReaction("Too bright and dry — water again or ease off harsh light.");
                        else if (pot.Growth >= GrowTarget)
                            Advance(Step.Complete, "Seedling established — germination success. Open Reflect.");
                        else if (lightSensor != null && lightSensor.IsBright && pot.Water >= 0.2f)
                            ShowReaction($"Growing… {pot.Growth:0%} · keep light + moisture ({pot.CurrentStage})");
                        else if (lightSensor != null && !lightSensor.IsBright)
                            ShowReaction("Growth slowed — move back into bright light.");
                        else
                            ShowReaction("Soil drying — water the pot again.");
                    }
                    break;
            }
        }

        public void WaterPot()
        {
            if (m_Step < Step.Water)
            {
                ShowReaction("Place the pot on the table first.");
                return;
            }

            var pot = GetPot();
            if (pot == null)
            {
                ShowReaction("No pot placed yet — tap the table.");
                return;
            }

            pot.AddWater(0.3f);
            ShowReaction(pot.Water > 0.9f
                ? "Careful — soil is getting waterlogged."
                : $"Watered. Moisture {pot.Water:0%}.");
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
            var pot = GetPot();

            if (lightFill != null)
            {
                lightFill.fillAmount = light;
                lightFill.color = lightSensor != null && lightSensor.IsBright
                    ? new Color(1f, 0.9f, 0.3f)
                    : new Color(0.55f, 0.6f, 0.65f);
            }

            if (waterFill != null)
                waterFill.fillAmount = pot != null ? pot.Water : 0f;

            if (growthFill != null)
                growthFill.fillAmount = pot != null ? pot.Growth : 0f;

            if (meterLabel != null)
            {
                string cam = lightSensor != null && lightSensor.IsReady ? lightSensor.Label : "NO CAMERA";
                string stage = pot != null ? pot.CurrentStage.ToString() : "No pot";
                meterLabel.text = $"Light: {cam} ({light:0%}) · {stage}";
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
                case Step.ScanTable:
                    title = "1 · Scan a table";
                    body = "Point the camera at a flat table/desk.\nThe app looks for a surface to place your pot.\n\n(On phone builds this uses AR plane detection.)";
                    break;
                case Step.PlacePot:
                    title = "2 · Place the pot";
                    body = "Tap the highlighted table to place the pot with soil and seed.\nYou only place once.";
                    break;
                case Step.Water:
                    title = "3 · Water the soil";
                    body = "Seeds need moisture to germinate.\nPress Water pot.\n\nToo little = no sprout. Too much = stress.";
                    break;
                case Step.ProvideLight:
                    title = "4 · Provide light";
                    body = "Move so the plant gets REAL bright light (window/lamp).\nThere is no fake Add Light cheat.";
                    break;
                case Step.Growing:
                    title = "5 · Germination";
                    body = "Keep moisture + light.\nWatch the seed sprout into a seedling.";
                    break;
                default:
                    title = "Complete";
                    body = "Your seed germinated on the table.\n\nPress Reflect: What did the seed need, and why did placing it in your room matter?";
                    break;
            }

            if (guideTitle != null) guideTitle.text = title;
            if (guideBody != null) guideBody.text = body;
        }

        void RefreshControls()
        {
            if (waterButton != null)
                waterButton.interactable = m_Step >= Step.Water && m_Step != Step.Complete;
        }

        GerminationPot GetPot()
        {
            if (placer == null || placer.PlacedObject == null) return null;
            return placer.PlacedObject.GetComponent<GerminationPot>();
        }
    }
}
