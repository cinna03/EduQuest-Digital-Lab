using System.Collections;
using EduQuest.AR;
using UnityEngine;
using UnityEngine.UI;

namespace EduQuest.Experiments
{
    /// <summary>
    /// Summative AR biology lab: hypothesis → place → water (at table) → real light growth
    /// → real darkness / night → result check. Wrong water+light mixes cause wilt / flood / scorch.
    /// </summary>
    public class GuidedLightLabExperiment : MonoBehaviour, ILabExperiment
    {
        public enum Step
        {
            Hypothesis = 0,
            Place = 1,
            WaterAtTable = 2,
            SeekLight = 3,
            Grow = 4,
            SeekDark = 5,
            NightMode = 6,
            Result = 7,
            Complete = 8
        }

        public enum HypothesisChoice
        {
            None = 0,
            NeedsLightAndWater = 1,
            LightAloneIsEnough = 2,
            DarknessGrowsFaster = 3
        }

        public enum PlantStage
        {
            Seed = 0,
            Sprout = 1,
            Seedling = 2,
            Wilted = 3,
            Flooded = 4,
            Scorched = 5
        }

        [SerializeField] WorldLightSensor lightSensor;
        [SerializeField] Text stepLabel;
        [SerializeField] Text guideTitle;
        [SerializeField] Text guideBody;
        [SerializeField] Text reactionText;
        [SerializeField] Text meterLabel;
        [SerializeField] Text scienceHud;
        [SerializeField] Image meterFill;
        [SerializeField] Image waterFill;
        [SerializeField] Image energyFill;
        [SerializeField] Button placeButton;
        [SerializeField] Button waterButton;
        [SerializeField] Button hintButton;
        [SerializeField] Button hypA;
        [SerializeField] Button hypB;
        [SerializeField] Button hypC;

        [SerializeField] GameObject plantRoot;
        [SerializeField] Transform sprout;
        [SerializeField] Transform leaves;
        [SerializeField] Renderer soil;
        [SerializeField] ParticleSystem oxygenBurst;
        [SerializeField] Light plantGlow;
        [SerializeField] GameObject placementRing;

        Step m_Step = Step.Hypothesis;
        HypothesisChoice m_Hypothesis = HypothesisChoice.None;
        PlantStage m_Stage = PlantStage.Seed;
        bool m_Placed;
        float m_HoldTimer;
        float m_Water;
        float m_Energy;
        float m_Oxygen;
        float m_WarmthTime;
        float m_DayLength;
        string m_Status = "Start with a hypothesis — then test it with your room.";

        const float BrightHold = 2.2f;
        const float DarkHold = 2.2f;
        const float TableDimMax = 0.48f;

        public string Title => "Biology · Light & Life (AR Growth Lab)";
        public string Prompt =>
            "How do real light, darkness, and watering at the table change plant growth — and was your hypothesis right?";
        public string Status => m_Status;
        public GameObject Root => gameObject;

        public void Bind(
            WorldLightSensor sensor,
            Text step,
            Text title,
            Text body,
            Text reaction,
            Text meter,
            Text hud,
            Image lightFill,
            Image waterBar,
            Image energyBar,
            Button place,
            Button water,
            Button hint,
            Button hA,
            Button hB,
            Button hC,
            GameObject plant,
            Transform sproutTf,
            Transform leavesTf,
            Renderer soilRend,
            ParticleSystem oxygen,
            Light glow,
            GameObject ring)
        {
            lightSensor = sensor;
            stepLabel = step;
            guideTitle = title;
            guideBody = body;
            reactionText = reaction;
            meterLabel = meter;
            scienceHud = hud;
            meterFill = lightFill;
            waterFill = waterBar;
            energyFill = energyBar;
            placeButton = place;
            waterButton = water;
            hintButton = hint;
            hypA = hA;
            hypB = hB;
            hypC = hC;
            plantRoot = plant;
            sprout = sproutTf;
            leaves = leavesTf;
            soil = soilRend;
            oxygenBurst = oxygen;
            plantGlow = glow;
            placementRing = ring;

            Wire(placeButton, PlacePlant);
            Wire(waterButton, TryWater);
            Wire(hintButton, RefreshGuide);
            Wire(hypA, () => ChooseHypothesis(HypothesisChoice.NeedsLightAndWater));
            Wire(hypB, () => ChooseHypothesis(HypothesisChoice.LightAloneIsEnough));
            Wire(hypC, () => ChooseHypothesis(HypothesisChoice.DarknessGrowsFaster));
        }

        static void Wire(Button btn, UnityEngine.Events.UnityAction action)
        {
            if (btn == null) return;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
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
            ShowReaction("Starting camera — this lab needs your real environment.");
            yield return lightSensor.StartSensor();
            ShowReaction(lightSensor.IsReady
                ? "Camera live. Light, table view, and darkness are your controls."
                : "Camera blocked — allow webcam. UI alone cannot finish this lab.");
            RefreshGuide();
        }

        public void ResetExperiment()
        {
            m_Step = Step.Hypothesis;
            m_Hypothesis = HypothesisChoice.None;
            m_Stage = PlantStage.Seed;
            m_Placed = false;
            m_HoldTimer = 0f;
            m_Water = 0.08f;
            m_Energy = 0f;
            m_Oxygen = 0f;
            m_WarmthTime = 0f;
            m_DayLength = 0f;
            if (plantRoot) plantRoot.SetActive(false);
            if (placementRing) placementRing.SetActive(true);
            if (plantGlow) plantGlow.enabled = false;
            if (oxygenBurst) oxygenBurst.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ApplyPlantVisuals();
            ShowReaction("Choose a hypothesis first.");
            RefreshGuide();
            RefreshControls();
            UpdateHud();
        }

        void Update()
        {
            if (!isActiveAndEnabled) return;
            UpdateHud();
            RunSimulation(Time.deltaTime);
            RunStepLogic();
            ApplyPlantVisuals();
        }

        void UpdateHud()
        {
            float b = lightSensor != null ? lightSensor.Brightness : 0f;
            if (meterFill != null)
            {
                meterFill.fillAmount = b;
                meterFill.color = lightSensor != null && lightSensor.IsBright ? new Color(1f, 0.9f, 0.3f)
                    : lightSensor != null && lightSensor.IsDark ? new Color(0.25f, 0.35f, 0.7f)
                    : new Color(0.55f, 0.6f, 0.65f);
            }

            if (waterFill != null)
            {
                waterFill.fillAmount = m_Water;
                waterFill.color = m_Water > 0.85f ? new Color(0.3f, 0.45f, 0.9f) : new Color(0.25f, 0.65f, 0.85f);
            }

            if (energyFill != null)
            {
                energyFill.fillAmount = m_Energy;
                energyFill.color = new Color(0.35f, 0.85f, 0.4f);
            }

            if (meterLabel != null)
            {
                string cam = lightSensor != null && lightSensor.IsReady ? lightSensor.Label : "NO CAMERA";
                meterLabel.text = "World light: " + cam + " (" + b.ToString("0%") + ")";
            }

            if (scienceHud != null)
            {
                scienceHud.text =
                    "Soil water " + m_Water.ToString("0%") +
                    "   Energy " + m_Energy.ToString("0%") +
                    "   O2 " + m_Oxygen.ToString("0%") + "\n" +
                    "Warmth time " + m_WarmthTime.ToString("0.0") + "s   Day length " +
                    m_DayLength.ToString("0.0") + "s   Stage: " + m_Stage;
            }
        }

        void RunSimulation(float dt)
        {
            if (!m_Placed || lightSensor == null || !lightSensor.IsReady) return;
            if (m_Step < Step.SeekLight) return;

            bool bright = lightSensor.IsBright;
            bool dark = lightSensor.IsDark;

            m_Water = Mathf.Clamp01(m_Water - dt * (bright ? 0.02f : 0.008f));

            if (bright && m_Water > 0.25f && m_Water < 0.88f)
            {
                m_WarmthTime += dt;
                m_DayLength += dt;
                float rate = 0.08f + 0.06f * m_Water;
                m_Energy = Mathf.Clamp01(m_Energy + dt * rate);
                m_Oxygen = Mathf.Clamp01(m_Oxygen + dt * rate * 0.85f);
                if (oxygenBurst != null && !oxygenBurst.isPlaying) oxygenBurst.Play();
            }
            else if (oxygenBurst != null && oxygenBurst.isPlaying && !bright)
            {
                oxygenBurst.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            if (m_Water > 0.9f && dark && m_Step >= Step.Grow)
            {
                m_Stage = PlantStage.Flooded;
                m_Energy = Mathf.Max(0f, m_Energy - dt * 0.05f);
            }
            else if (bright && m_Water < 0.12f && m_WarmthTime > 3f)
            {
                m_Stage = PlantStage.Scorched;
                m_Energy = Mathf.Max(0f, m_Energy - dt * 0.08f);
            }
            else if (m_Water < 0.15f && m_Energy < 0.2f && m_Step >= Step.Grow)
            {
                m_Stage = PlantStage.Wilted;
            }
            else if (m_Energy > 0.55f)
                m_Stage = PlantStage.Seedling;
            else if (m_Energy > 0.2f)
                m_Stage = PlantStage.Sprout;
            else if (m_Placed)
                m_Stage = PlantStage.Seed;
        }

        void RunStepLogic()
        {
            if (m_Step == Step.Hypothesis || m_Step == Step.Place || m_Step == Step.Result || m_Step == Step.Complete)
                return;

            if (lightSensor == null || !lightSensor.IsReady)
            {
                m_Status = "Waiting for camera — AR light sensing required.";
                return;
            }

            switch (m_Step)
            {
                case Step.WaterAtTable:
                    if (lightSensor.IsBright)
                        ShowReaction("Too bright — you are facing a lamp/window. Turn back to the TABLE to water.");
                    else if (lightSensor.Brightness <= TableDimMax)
                        ShowReaction("Table view OK. Press Water plant (do not flood it).");
                    else
                        ShowReaction("Dim the view toward the table, then water.");
                    break;

                case Step.SeekLight:
                    if (lightSensor.IsBright)
                    {
                        m_HoldTimer += Time.deltaTime;
                        if (plantGlow)
                        {
                            plantGlow.enabled = true;
                            plantGlow.color = new Color(0.55f, 1f, 0.45f);
                            plantGlow.intensity = 0.7f + m_Energy;
                        }

                        if (m_HoldTimer >= BrightHold)
                        {
                            if (m_Water < 0.2f)
                                Advance(Step.Grow, "Light found — but soil is dry. Growth may stall or scorch.");
                            else
                                Advance(Step.Grow, "Bright light + moisture — photosynthesis engaged.");
                        }
                        else
                            ShowReaction("Holding BRIGHT… " + m_HoldTimer.ToString("0.0") + "s (water " + m_Water.ToString("0%") + ")");
                    }
                    else
                    {
                        m_HoldTimer = Mathf.Max(0f, m_HoldTimer - Time.deltaTime);
                        ShowReaction("Find real LIGHT — window or lamp. No fake Add Light button.");
                    }
                    break;

                case Step.Grow:
                    if (m_Stage == PlantStage.Scorched || m_Stage == PlantStage.Flooded)
                    {
                        if (m_Energy >= 0.35f || m_WarmthTime >= 6f)
                            Advance(Step.SeekDark, "Stressed plant — still try DARKNESS to compare day vs night.");
                        else
                            ShowReaction("Plant stressed (" + m_Stage + "). Balance water at table + bright light, or continue to dark.");
                    }
                    else if (m_Energy >= 0.5f && m_Oxygen >= 0.35f)
                    {
                        Advance(Step.SeekDark, "Energy & O2 stored. Now find real DARKNESS for night mode.");
                    }
                    else if (lightSensor.IsBright && m_Water >= 0.2f)
                    {
                        ShowReaction("Growing… energy " + m_Energy.ToString("0%") + " · O2 " + m_Oxygen.ToString("0%"));
                    }
                    else if (lightSensor.IsBright && m_Water < 0.2f)
                    {
                        ShowReaction("Bright but dry — risk of scorch. Return to table view and Water.");
                    }
                    else
                        ShowReaction("Growth paused — need BRIGHT light again (and enough water).");
                    break;

                case Step.SeekDark:
                case Step.NightMode:
                    if (lightSensor.IsDark)
                    {
                        m_HoldTimer += Time.deltaTime;
                        if (plantGlow)
                        {
                            plantGlow.enabled = true;
                            plantGlow.color = new Color(0.25f, 0.35f, 0.8f);
                            plantGlow.intensity = 0.35f;
                        }

                        if (oxygenBurst != null && oxygenBurst.isPlaying)
                            oxygenBurst.Stop(true, ParticleSystemStopBehavior.StopEmitting);

                        if (m_Step == Step.SeekDark && m_HoldTimer >= DarkHold)
                            Advance(Step.NightMode, "Darkness detected — photosynthesis burst stops; night mode.");
                        else if (m_Step == Step.NightMode && m_HoldTimer >= DarkHold + 1f)
                            Advance(Step.Result, "Trial complete — check your hypothesis result.");
                        else
                            ShowReaction("Holding DARK… " + m_HoldTimer.ToString("0.0") + "s");
                    }
                    else
                    {
                        m_HoldTimer = Mathf.Max(0f, m_HoldTimer - Time.deltaTime * 1.4f);
                        ShowReaction("Still too bright. Cover the lens or face a dark corner.");
                    }
                    break;
            }
        }

        void ChooseHypothesis(HypothesisChoice choice)
        {
            m_Hypothesis = choice;
            ShowReaction("Hypothesis locked. Now place the seedling on your table.");
            Advance(Step.Place, null);
        }

        public void PlacePlant()
        {
            if (m_Step != Step.Place)
            {
                ShowReaction("Choose a hypothesis first.");
                return;
            }

            m_Placed = true;
            m_Stage = PlantStage.Seed;
            if (plantRoot) plantRoot.SetActive(true);
            if (placementRing) placementRing.SetActive(false);
            ShowReaction("Seedling placed. Face the TABLE (not the lamp) to water.");
            Advance(Step.WaterAtTable, null);
        }

        public void TryWater()
        {
            if (!m_Placed)
            {
                ShowReaction("Place the seedling first.");
                return;
            }

            if (lightSensor == null || !lightSensor.IsReady)
            {
                ShowReaction("Camera required to confirm you are at the table.");
                return;
            }

            if (lightSensor.IsBright || lightSensor.Brightness > TableDimMax)
            {
                ShowReaction("Blocked — not at table view. Turn away from bright light, then water.");
                return;
            }

            m_Water = Mathf.Clamp01(m_Water + 0.22f);
            ShowReaction(m_Water > 0.88f
                ? "Soil flooded — too much water. In darkness this can rot roots."
                : "Watered. Soil moisture " + m_Water.ToString("0%") + ".");

            if (m_Step == Step.WaterAtTable && m_Water >= 0.28f && m_Water < 0.9f)
                Advance(Step.SeekLight, "Good moisture. Now find REAL bright light for photosynthesis.");
        }

        void Advance(Step step, string reaction)
        {
            if ((int)step < (int)m_Step) return;
            m_Step = step;
            m_HoldTimer = 0f;
            if (!string.IsNullOrEmpty(reaction)) ShowReaction(reaction);
            if (step == Step.Result) ShowResult();
            RefreshGuide();
            RefreshControls();
        }

        void ShowResult()
        {
            bool grewWell = m_Energy >= 0.45f && m_Stage == PlantStage.Seedling;
            string verdict;
            switch (m_Hypothesis)
            {
                case HypothesisChoice.NeedsLightAndWater:
                    verdict = grewWell
                        ? "Supported — light + water produced energy/O2 and a seedling."
                        : "Partly supported — you needed both, but conditions were not balanced enough.";
                    break;
                case HypothesisChoice.LightAloneIsEnough:
                    verdict = "Not supported as stated — healthy growth needed moisture too.";
                    break;
                case HypothesisChoice.DarknessGrowsFaster:
                    verdict = "Not supported — darkness shifted night mode; energy was built in the bright phase.";
                    break;
                default:
                    verdict = "No hypothesis recorded.";
                    break;
            }

            m_Step = Step.Complete;
            m_HoldTimer = 0f;
            ShowReaction(verdict + " Open Reflect.");
            RefreshGuide();
            RefreshControls();
        }

        void ShowReaction(string msg)
        {
            m_Status = msg;
            if (reactionText != null) reactionText.text = "Reaction: " + msg;
        }

        void RefreshGuide()
        {
            if (stepLabel != null)
                stepLabel.text = m_Step == Step.Complete ? "Done" : "Step " + ((int)m_Step + 1) + " / 8";

            string title = "";
            string body = "";
            switch (m_Step)
            {
                case Step.Hypothesis:
                    title = "1 · Hypothesis";
                    body = "Before you touch the plant, predict:\nA) Needs light AND water\nB) Light alone is enough\nC) Darkness grows faster\n\nTap A, B, or C.";
                    break;
                case Step.Place:
                    title = "2 · Place on table";
                    body = "Anchor the seedling in your space.\n\nPress Place seedling.";
                    break;
                case Step.WaterAtTable:
                    title = "3 · Water at the table";
                    body = "Turn the camera toward the TABLE (dimmer than a lamp).\nThen press Water plant.\n\nFlooding is possible — different action, different result.";
                    break;
                case Step.SeekLight:
                    title = "4 · Find real LIGHT";
                    body = "Point at a window/lamp until BRIGHT.\nNo Add Light cheat exists.";
                    break;
                case Step.Grow:
                    title = "5 · Photosynthesis / growth";
                    body = "Hold bright light with decent moisture.\nWatch Energy, O2, stage.\nDry+bright → scorch. Flood+dark → rot risk.";
                    break;
                case Step.SeekDark:
                    title = "6 · Find real DARKNESS";
                    body = "Cover the lens or face a dark corner.\nOpposite environment → opposite reaction.";
                    break;
                case Step.NightMode:
                    title = "7 · Night mode";
                    body = "O2 burst stops; plant shifts night mood.\nHold to finish the trial.";
                    break;
                case Step.Result:
                    title = "8 · Hypothesis check";
                    body = "Compare what you predicted with what your room produced.";
                    break;
                default:
                    title = "Complete";
                    body = "You ran a multi-factor AR growth trial.\n\nPress Reflect: Was your hypothesis supported? Why did the camera matter?";
                    break;
            }

            if (guideTitle != null) guideTitle.text = title;
            if (guideBody != null) guideBody.text = body;
        }

        void RefreshControls()
        {
            Set(hypA, m_Step == Step.Hypothesis);
            Set(hypB, m_Step == Step.Hypothesis);
            Set(hypC, m_Step == Step.Hypothesis);
            Set(placeButton, m_Step == Step.Place);
            Set(waterButton, m_Step == Step.WaterAtTable || m_Step == Step.Grow || m_Step == Step.SeekLight);
        }

        static void Set(Button btn, bool on)
        {
            if (btn == null) return;
            btn.interactable = on;
            var img = btn.GetComponent<Image>();
            if (img != null)
                img.color = on ? new Color(0.14f, 0.55f, 0.72f) : new Color(0.18f, 0.22f, 0.28f);
        }

        void ApplyPlantVisuals()
        {
            float grow = Mathf.Lerp(0.12f, 1f, m_Energy);
            if (sprout != null)
            {
                float h = 0.12f + grow * 0.6f;
                if (m_Stage == PlantStage.Wilted || m_Stage == PlantStage.Scorched) h *= 0.55f;
                if (m_Stage == PlantStage.Flooded) h *= 0.7f;
                sprout.localScale = new Vector3(0.08f, h, 0.08f);
                sprout.localPosition = new Vector3(0f, h * 0.5f + 0.05f, 0f);
                sprout.gameObject.SetActive(m_Placed);
            }

            if (leaves != null)
            {
                float open = m_Energy > 0.2f ? Mathf.Lerp(0.12f, 0.48f, m_Energy) : 0.08f;
                if (m_Step >= Step.NightMode) open *= 0.75f;
                if (m_Stage == PlantStage.Wilted || m_Stage == PlantStage.Scorched) open *= 0.45f;
                leaves.localScale = Vector3.one * open;
                leaves.localPosition = new Vector3(0f, (sprout != null ? sprout.localPosition.y : 0.4f) + 0.18f, 0f);
                leaves.gameObject.SetActive(open > 0.1f);

                var r = leaves.GetComponent<Renderer>();
                if (r != null)
                {
                    Color c = new Color(0.2f, 0.75f, 0.3f);
                    if (m_Step >= Step.NightMode) c = new Color(0.15f, 0.35f, 0.28f);
                    if (m_Stage == PlantStage.Wilted) c = new Color(0.45f, 0.4f, 0.15f);
                    if (m_Stage == PlantStage.Scorched) c = new Color(0.55f, 0.28f, 0.12f);
                    if (m_Stage == PlantStage.Flooded) c = new Color(0.25f, 0.4f, 0.35f);
                    var block = new MaterialPropertyBlock();
                    r.GetPropertyBlock(block);
                    block.SetColor("_BaseColor", c);
                    block.SetColor("_Color", c);
                    r.SetPropertyBlock(block);
                }
            }

            if (soil != null)
            {
                var dry = new Color(0.45f, 0.32f, 0.18f);
                var wet = new Color(0.2f, 0.28f, 0.14f);
                var flood = new Color(0.15f, 0.22f, 0.28f);
                var c = m_Water > 0.88f ? flood : Color.Lerp(dry, wet, m_Water);
                var block = new MaterialPropertyBlock();
                soil.GetPropertyBlock(block);
                block.SetColor("_BaseColor", c);
                block.SetColor("_Color", c);
                soil.SetPropertyBlock(block);
            }
        }
    }
}
