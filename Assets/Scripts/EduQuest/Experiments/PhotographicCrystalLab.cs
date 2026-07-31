using System.Collections;
using System.Text;
using EduQuest.AR;
using EduQuest.Lab;
using UnityEngine;
using UnityEngine.UI;

namespace EduQuest.Experiments
{
    public enum ChemId
    {
        None = 0,
        SilverNitrate = 1,      // A — needed 10 ml
        SodiumChloride = 2,     // B — needed 10 ml
        SodiumThiosulfate = 3,  // C — needed 5 ml
        DistilledWater = 4,     // D — trap
        SodiumCarbonate = 5,    // E — trap
        CopperSulfate = 6       // F — trap
    }

    /// <summary>
    /// Photographic Crystal Puzzle — AgCl-inspired AR simulation.
    /// Darkness → measure/mix → wait → fix → light ON for stable glow.
    /// SIMULATION ONLY.
    /// </summary>
    public class PhotographicCrystalLab : MonoBehaviour, ILabExperiment
    {
        const float CorrectAg = 10f;
        const float CorrectCl = 10f;
        const float CorrectFix = 5f;
        const float MeasureTol = 0.5f;
        const float ClWindowSec = 10f;
        const float SettleSec = 5f;
        const float FixLateSec = 12f;

        [SerializeField] TablePotPlacer placer;
        [SerializeField] WorldLightSensor lightSensor;
        [SerializeField] GameObject placementHint;
        [SerializeField] Text stepLabel;
        [SerializeField] Text guideTitle;
        [SerializeField] Text guideBody;
        [SerializeField] Text reactionText;
        [SerializeField] Text meterLabel;
        [SerializeField] Text safetyText;
        [SerializeField] Text journalText;
        [SerializeField] Text scoreText;
        [SerializeField] Text measureLabel;
        [SerializeField] Text selectedLabel;
        [SerializeField] Text lightStateLabel;
        [SerializeField] Image lightFill;
        [SerializeField] Button[] chemButtons;
        [SerializeField] Button measure5Btn;
        [SerializeField] Button measure10Btn;
        [SerializeField] Button pourBtn;
        [SerializeField] Button wasteBtn;
        [SerializeField] Button hintButton;
        [SerializeField] LabTapSelector tapSelector;
        [SerializeField] ArChemBench arBench;
        [SerializeField] ARPlatformBootstrap platform;
        [SerializeField] ARFoundationPlaceBridge placeBridge;

        CrystalBeaker m_Beaker;
        bool m_SpawnedArBench;
        ChemId m_Selected = ChemId.None;
        float m_MeasureMl = 10f;

        float m_AgMl;
        float m_ClMl;
        float m_FixMl;
        bool m_Ended;
        bool m_Success;
        bool m_PrecipitateFormed;
        bool m_Settled;
        bool m_Stabilized;

        float m_AgAddedAt = -999f;
        float m_PrecipAt = -999f;
        float m_SettleTimer;
        string m_Outcome = "—";
        readonly StringBuilder m_Journal = new StringBuilder();

        int m_ScoreChem;
        int m_ScoreMeasure;
        int m_ScoreLight;
        int m_ScoreTiming;

        string m_Status = "Place the beaker, then work in darkness.";

        public string Title => "Chemistry · Photographic Crystal Puzzle";
        public string Prompt =>
            "In darkness: 10 ml AgNO₃ + 10 ml NaCl → wait → 5 ml fixer → then real light ON for a stable crystal.";
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
            Text journal,
            Text score,
            Text measure,
            Text selected,
            Text lightState,
            Image light,
            Button[] chems,
            Button m5,
            Button m10,
            Button pour,
            Button waste,
            Button hintBtn,
            LabTapSelector taps = null,
            ArChemBench bench = null,
            ARPlatformBootstrap bootstrap = null,
            ARFoundationPlaceBridge bridge = null)
        {
            placer = potPlacer;
            lightSensor = sensor;
            placementHint = hint;
            arBench = bench;
            platform = bootstrap;
            placeBridge = bridge;
            stepLabel = step;
            guideTitle = title;
            guideBody = body;
            reactionText = reaction;
            meterLabel = meter;
            safetyText = safety;
            journalText = journal;
            scoreText = score;
            measureLabel = measure;
            selectedLabel = selected;
            lightStateLabel = lightState;
            lightFill = light;
            chemButtons = chems;
            measure5Btn = m5;
            measure10Btn = m10;
            pourBtn = pour;
            wasteBtn = waste;
            hintButton = hintBtn;
            tapSelector = taps;

            if (tapSelector != null)
            {
                tapSelector.BottleTapped -= OnBottleTapped;
                tapSelector.BottleTapped += OnBottleTapped;
                tapSelector.BeakerTapped -= OnBeakerTapped;
                tapSelector.BeakerTapped += OnBeakerTapped;
            }

            WireChem(0, ChemId.SilverNitrate);
            WireChem(1, ChemId.SodiumChloride);
            WireChem(2, ChemId.SodiumThiosulfate);
            WireChem(3, ChemId.DistilledWater);
            WireChem(4, ChemId.SodiumCarbonate);
            WireChem(5, ChemId.CopperSulfate);

            if (measure5Btn != null)
            {
                measure5Btn.onClick.RemoveAllListeners();
                measure5Btn.onClick.AddListener(() => SetMeasure(5f));
            }
            if (measure10Btn != null)
            {
                measure10Btn.onClick.RemoveAllListeners();
                measure10Btn.onClick.AddListener(() => SetMeasure(10f));
            }
            if (pourBtn != null)
            {
                pourBtn.onClick.RemoveAllListeners();
                pourBtn.onClick.AddListener(Pour);
            }
            if (wasteBtn != null)
            {
                wasteBtn.onClick.RemoveAllListeners();
                wasteBtn.onClick.AddListener(ResetExperiment);
            }
            if (hintButton != null)
            {
                hintButton.onClick.RemoveAllListeners();
                hintButton.onClick.AddListener(RefreshGuide);
            }

            if (safetyText != null)
                safetyText.text =
                    "SIMULATION ONLY — AgNO₃ is hazardous in real life (corrosive / oxidizer). Never try unsupervised.";
        }

        void WireChem(int index, ChemId id)
        {
            if (chemButtons == null || index >= chemButtons.Length || chemButtons[index] == null) return;
            var captured = id;
            chemButtons[index].onClick.RemoveAllListeners();
            chemButtons[index].onClick.AddListener(() => SelectChem(captured));
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
                ? "Camera ready — keep the room DARK while mixing."
                : "Allow camera access (light/dark is part of the puzzle).");
        }

        public void ResetExperiment()
        {
            m_Ended = false;
            m_Success = false;
            m_Selected = ChemId.None;
            m_MeasureMl = 10f;
            m_AgMl = m_ClMl = m_FixMl = 0f;
            m_PrecipitateFormed = false;
            m_Settled = false;
            m_Stabilized = false;
            m_AgAddedAt = m_PrecipAt = -999f;
            m_SettleTimer = 0f;
            m_Outcome = "Batch reset — waste cleared.";
            m_ScoreChem = m_ScoreMeasure = m_ScoreLight = m_ScoreTiming = 0;
            m_Journal.Clear();
            Log("New batch. Work in darkness first.");

            if (placer != null)
            {
                placer.ResetPlacement();
                placer.PlacementEnabled = true;
            }
            tapSelector?.ClearSelection();
            arBench?.Clear();
            placeBridge?.ResetPlanes();
            m_SpawnedArBench = false;
            m_Beaker = null;
            if (placer != null)
                placer.PlacementEnabled = true;
            if (placementHint) placementHint.SetActive(!IsPhoneAr());

            ShowReaction(IsPhoneAr()
                ? "Scan a table — when planes appear, tap the surface to place your beaker."
                : "Tap the table to place your beaker.");
            RefreshGuide();
            RefreshHud();
        }

        bool IsPhoneAr() => platform != null && platform.IsPhoneAr;

        void OnBottleTapped(ChemicalBottle bottle)
        {
            if (bottle == null) return;
            SelectChem(bottle.ChemId);
            ShowReaction("Selected " + bottle.DisplayName + ". Set ml, then Pour into the beaker.");
        }

        void OnBeakerTapped()
        {
            if (m_Beaker == null)
            {
                ShowReaction("Place the reaction beaker on the table first.");
                return;
            }
            if (m_Selected == ChemId.None)
            {
                ShowReaction("Tap a reagent bottle first (glass label above it), then Pour.");
                return;
            }
            ShowReaction("Beaker ready. Use Pour to add " + ChemName(m_Selected) + ".");
        }

        void Update()
        {
            if (!isActiveAndEnabled) return;

            if (m_Beaker == null && placer != null && placer.HasPot)
            {
                var go = placer.PlacedObject;
                m_Beaker = go != null ? go.GetComponent<CrystalBeaker>() : null;
                if (m_Beaker == null && go != null)
                    m_Beaker = go.GetComponentInChildren<CrystalBeaker>();
                if (m_Beaker != null)
                {
                    if (placementHint) placementHint.SetActive(false);
                    if (IsPhoneAr() && arBench != null && !m_SpawnedArBench)
                    {
                        arBench.SpawnAround(m_Beaker.transform);
                        m_SpawnedArBench = true;
                        Log("AR bench spawned around beaker — tap glassware to select.");
                    }
                    ShowReaction(IsPhoneAr()
                        ? "Beaker anchored on table. Tap reagents around it — keep room dark while mixing."
                        : "Beaker placed. Tap a bottle, set ml, then Pour — keep light OFF.");
                    RefreshGuide();
                }
            }

            if (!m_Ended && m_PrecipitateFormed && !m_Settled && !m_Stabilized)
            {
                m_SettleTimer += Time.deltaTime;
                if (m_SettleTimer >= SettleSec)
                {
                    m_Settled = true;
                    if (m_Beaker != null) m_Beaker.SetLook(CrystalBeaker.Look.RawCrystal);
                    Log("Precipitate settled → raw crystal ready for fixer.");
                    ShowReaction("Raw crystal formed. Add 5 ml Sodium Thiosulfate (still in darkness).");
                }
            }

            // Light ON too early while unstabilized precipitate/crystal exists
            if (!m_Ended && IsLightOn() && m_PrecipitateFormed && !m_Stabilized)
            {
                Fail("Burnt Silver Residue",
                    "Light exposure occurred before stabilization.",
                    CrystalBeaker.Look.BurntResidue,
                    chem: 10, measure: ScoreMeasurePartial(), light: 0, timing: 0);
            }

            // Stabilizer too late window: if settled long and never fixed, then light hits
            if (!m_Ended && m_Settled && !m_Stabilized && m_FixMl <= 0f
                && Time.time - m_PrecipAt > FixLateSec && IsLightOn())
            {
                Fail("Overexposed Crystal",
                    "Stabilizer added too late / light hit the raw crystal.",
                    CrystalBeaker.Look.BurntResidue,
                    chem: 20, measure: ScoreMeasurePartial(), light: 0, timing: 0);
            }

            RefreshHud();
        }

        void SelectChem(ChemId id)
        {
            if (m_Ended) { ShowReaction("Batch finished — Waste/Reset to try again."); return; }
            m_Selected = id;
            RefreshHud();
            ShowReaction("Selected " + ChemName(id) + ". Set measure, then Pour.");
        }

        void SetMeasure(float ml)
        {
            m_MeasureMl = ml;
            RefreshHud();
        }

        public void Pour()
        {
            if (m_Ended)
            {
                ShowReaction("Batch finished — Waste/Reset.");
                return;
            }
            if (m_Beaker == null)
            {
                ShowReaction("Place the beaker first.");
                return;
            }
            if (m_Selected == ChemId.None)
            {
                ShowReaction("Select a chemical bottle first.");
                return;
            }

            Log($"Poured {m_MeasureMl:0} ml {ChemName(m_Selected)} (light {(IsLightOn() ? "ON" : "OFF")}).");

            switch (m_Selected)
            {
                case ChemId.DistilledWater:
                case ChemId.SodiumCarbonate:
                case ChemId.CopperSulfate:
                    Fail("Contaminated Solution",
                        "Wrong chemical ruined the batch.",
                        CrystalBeaker.Look.Contaminated,
                        chem: 0, measure: 0, light: IsLightOn() ? 0 : 10, timing: 5);
                    return;

                case ChemId.SodiumThiosulfate:
                    PourFixer();
                    return;

                case ChemId.SilverNitrate:
                    PourSilver();
                    return;

                case ChemId.SodiumChloride:
                    PourChloride();
                    return;
            }
        }

        void PourSilver()
        {
            if (m_FixMl > 0f && m_AgMl <= 0f)
            {
                Fail("No Crystal Formation",
                    "Fixer first interrupted crystal-building.",
                    CrystalBeaker.Look.NoCrystal,
                    chem: 5, measure: 0, light: DarkBonus(), timing: 0);
                return;
            }

            m_AgMl += m_MeasureMl;
            m_AgAddedAt = Time.time;
            m_Beaker.SetLook(CrystalBeaker.Look.ClearSolution);

            if (IsLightOn())
                ShowReaction("Warning: light is ON — Ag compounds prefer darkness. Cover the camera / dim the room.");
            else
                ShowReaction($"Silver Nitrate in ({m_AgMl:0} ml). Quickly add 10 ml Sodium Chloride (≤{ClWindowSec:0}s).");

            TryFormPrecipitate();
        }

        void PourChloride()
        {
            if (m_AgMl <= 0f)
            {
                ShowReaction("Add Silver Nitrate first (in darkness).");
                return;
            }

            if (m_FixMl > 0f && !m_PrecipitateFormed)
            {
                Fail("No Crystal Formation",
                    "Fixer was added before the precipitate could form.",
                    CrystalBeaker.Look.NoCrystal,
                    chem: 10, measure: ScoreMeasurePartial(), light: DarkBonus(), timing: 0);
                return;
            }

            float delay = Time.time - m_AgAddedAt;
            m_ClMl += m_MeasureMl;

            if (delay > ClWindowSec)
            {
                Fail("Degraded Batch",
                    "Sodium Chloride added too late — silver nitrate 'degraded' in gameplay.",
                    CrystalBeaker.Look.WeakCloudy,
                    chem: 15, measure: ScoreMeasurePartial(), light: DarkBonus(), timing: 0);
                return;
            }

            TryFormPrecipitate();
        }

        void TryFormPrecipitate()
        {
            if (m_PrecipitateFormed || m_AgMl <= 0f || m_ClMl <= 0f) return;

            if (IsLightOn())
            {
                // Forms then immediately burns
                m_PrecipitateFormed = true;
                m_PrecipAt = Time.time;
                Fail("Burnt Silver Residue",
                    "Light was ON while forming AgCl — darkened before stabilization.",
                    CrystalBeaker.Look.BurntResidue,
                    chem: ScoreChemPartial(), measure: ScoreMeasurePartial(), light: 0, timing: 5);
                return;
            }

            bool agOk = Mathf.Abs(m_AgMl - CorrectAg) <= MeasureTol;
            bool clOk = Mathf.Abs(m_ClMl - CorrectCl) <= MeasureTol;

            if (m_AgMl > CorrectAg + 2f && m_ClMl >= CorrectCl - MeasureTol)
            {
                m_PrecipitateFormed = true;
                m_PrecipAt = Time.time;
                m_Beaker.SetLook(CrystalBeaker.Look.YellowUnstable);
                ShowReaction("Too much Silver Nitrate — unstable silver-rich mix. You can still try to fix…");
                m_SettleTimer = 0f;
                return;
            }

            if (m_ClMl > CorrectCl + 2f)
            {
                m_PrecipitateFormed = true;
                m_PrecipAt = Time.time;
                m_Beaker.SetLook(CrystalBeaker.Look.WeakCloudy);
                ShowReaction("Too much NaCl — weak cloudy mixture. Settling…");
                m_SettleTimer = 0f;
                return;
            }

            if (m_ClMl < CorrectCl - 2f || m_AgMl < CorrectAg - 2f)
            {
                m_PrecipitateFormed = true;
                m_PrecipAt = Time.time;
                m_Beaker.SetLook(CrystalBeaker.Look.IncompleteFlakes);
                ShowReaction("Incomplete precipitate — small flakes. Check 10:10 ml ratio.");
                // Still allow settle/fix path for learning
                m_SettleTimer = 0f;
                return;
            }

            m_PrecipitateFormed = true;
            m_PrecipAt = Time.time;
            m_SettleTimer = 0f;
            m_Beaker.SetLook(CrystalBeaker.Look.WhitePrecipitate);
            ShowReaction(agOk && clOk
                ? "Pure white AgCl precipitate! Wait ~5s (still dark), then add 5 ml fixer."
                : "White precipitate forming. Wait ~5s, then stabilize.");
        }

        void PourFixer()
        {
            if (!m_PrecipitateFormed)
            {
                m_FixMl += m_MeasureMl;
                m_Beaker.SetLook(CrystalBeaker.Look.NoCrystal);
                ShowReaction("Fixer too early — no proper crystal stage. Waste/Reset or continue carefully.");
                if (m_AgMl > 0f || m_ClMl > 0f)
                {
                    Fail("No Crystal Formation",
                        "Stabilizer interrupted crystal-building.",
                        CrystalBeaker.Look.NoCrystal,
                        chem: 10, measure: 0, light: DarkBonus(), timing: 0);
                }
                return;
            }

            if (!m_Settled)
            {
                ShowReaction("Wait for the precipitate to settle into a raw crystal (~5s).");
                return;
            }

            m_FixMl += m_MeasureMl;

            if (m_FixMl > CorrectFix + 2f)
            {
                Fail("Dissolved Crystal",
                    "Too much Sodium Thiosulfate dissolved the crystal.",
                    CrystalBeaker.Look.Dissolved,
                    chem: 25, measure: 10, light: DarkBonus(), timing: 10);
                return;
            }

            m_Stabilized = m_FixMl >= CorrectFix - MeasureTol;
            if (m_Stabilized)
            {
                m_Beaker.SetLook(CrystalBeaker.Look.RawCrystal);
                ShowReaction("Stabilized! Now turn Light ON (bright window/lamp) to activate the crystal.");
                Log("Fixer OK — ready for light activation.");
            }
            else
            {
                ShowReaction($"Fixer low ({m_FixMl:0} ml). Need ~5 ml or crystal may grey under light.");
            }
        }

        // Called continuously when stabilized — bright light finishes success / low fixer greys
        void LateUpdate()
        {
            if (m_Ended || !m_Stabilized || m_Beaker == null) return;
            if (!IsLightOn()) return;

            bool measurePerfect =
                Mathf.Abs(m_AgMl - CorrectAg) <= MeasureTol
                && Mathf.Abs(m_ClMl - CorrectCl) <= MeasureTol
                && Mathf.Abs(m_FixMl - CorrectFix) <= MeasureTol;

            bool measureOk =
                Mathf.Abs(m_AgMl - CorrectAg) <= 2f
                && Mathf.Abs(m_ClMl - CorrectCl) <= 2f
                && m_FixMl >= CorrectFix - 1f && m_FixMl <= CorrectFix + 2f;

            if (m_FixMl < CorrectFix - 1.5f)
            {
                Fail("Unstable Crystal",
                    "Too little fixer — crystal turned grey under light.",
                    CrystalBeaker.Look.UnstableGrey,
                    chem: 25, measure: 10, light: 10, timing: 15);
                return;
            }

            if (m_AgMl > CorrectAg + 2f)
            {
                Fail("Unstable Silver-Rich Crystal",
                    "Excess AgNO₃ darkened fast under light.",
                    CrystalBeaker.Look.BurntResidue,
                    chem: 25, measure: 5, light: 15, timing: 15);
                return;
            }

            if (!measureOk)
            {
                // Partial success path — incomplete but not full glow
                Fail("Incomplete Photographic Crystal",
                    "Nearly there — tighten the 10:10:5 ml recipe.",
                    CrystalBeaker.Look.IncompleteFlakes,
                    chem: 25, measure: 15, light: 15, timing: 15);
                return;
            }

            m_Success = true;
            m_Ended = true;
            m_Beaker.SetLook(CrystalBeaker.Look.StableGlow);
            m_ScoreChem = 30;
            m_ScoreMeasure = measurePerfect ? 30 : 22;
            m_ScoreLight = 20;
            m_ScoreTiming = 20;
            m_Outcome = "Stable Photographic Crystal";
            Log("SUCCESS — Stable Photographic Crystal unlocked.");
            ShowReaction("Crystal glows silver-blue! Open Reflect. Score " + TotalScore() + "/100.");
            RefreshGuide();
        }

        void Fail(string outcome, string why, CrystalBeaker.Look look, int chem, int measure, int light, int timing)
        {
            if (m_Ended) return;
            m_Ended = true;
            m_Success = false;
            m_Outcome = outcome;
            m_ScoreChem = chem;
            m_ScoreMeasure = measure;
            m_ScoreLight = light;
            m_ScoreTiming = timing;
            if (m_Beaker != null) m_Beaker.SetLook(look);
            Log("FAIL — " + outcome + ": " + why);
            ShowReaction(outcome + " — " + why + " Score " + TotalScore() + "/100. Waste/Reset.");
            RefreshGuide();
        }

        int ScoreMeasurePartial()
        {
            int s = 0;
            if (Mathf.Abs(m_AgMl - CorrectAg) <= 2f) s += 10;
            if (Mathf.Abs(m_ClMl - CorrectCl) <= 2f) s += 10;
            return s;
        }

        int ScoreChemPartial()
        {
            int s = 0;
            if (m_AgMl > 0f) s += 10;
            if (m_ClMl > 0f) s += 10;
            return s;
        }

        int DarkBonus() => IsLightOn() ? 0 : 10;

        int TotalScore() => m_ScoreChem + m_ScoreMeasure + m_ScoreLight + m_ScoreTiming;

        bool IsLightOn()
        {
            if (lightSensor == null || !lightSensor.IsReady) return false;
            return lightSensor.IsBright;
        }

        void ShowReaction(string msg)
        {
            m_Status = msg;
            if (reactionText != null) reactionText.text = "Reaction: " + msg;
        }

        void Log(string line)
        {
            m_Journal.AppendLine("• " + line);
            if (journalText != null) journalText.text = m_Journal.ToString();
        }

        void RefreshHud()
        {
            float light = lightSensor != null ? lightSensor.Brightness : 0f;
            if (lightFill != null)
            {
                lightFill.fillAmount = light;
                lightFill.color = IsLightOn() ? new Color(1f, 0.9f, 0.3f) : new Color(0.25f, 0.3f, 0.55f);
            }

            if (lightStateLabel != null)
                lightStateLabel.text = IsLightOn() ? "LIGHT: ON (bright)" : "LIGHT: OFF (dark/dim)";

            if (measureLabel != null)
                measureLabel.text = $"Measure: {m_MeasureMl:0} ml";

            if (selectedLabel != null)
                selectedLabel.text = "Bottle: " + (m_Selected == ChemId.None ? "—" : ChemName(m_Selected));

            if (meterLabel != null)
            {
                string cam = lightSensor != null && lightSensor.IsReady ? lightSensor.Label : "NO CAMERA";
                meterLabel.text = $"Ag {m_AgMl:0} · Cl {m_ClMl:0} · Fix {m_FixMl:0} ml · {cam}";
            }

            if (scoreText != null)
            {
                scoreText.text = m_Ended
                    ? $"Score {TotalScore()}/100 · {m_Outcome}\nChem {m_ScoreChem} · Measure {m_ScoreMeasure} · Light {m_ScoreLight} · Timing {m_ScoreTiming}"
                    : "Score —/100 (finish a batch)";
            }

            bool canPour = !m_Ended && m_Beaker != null;
            if (pourBtn != null) pourBtn.interactable = canPour;
            if (measure5Btn != null) measure5Btn.interactable = !m_Ended;
            if (measure10Btn != null) measure10Btn.interactable = !m_Ended;
            if (chemButtons != null)
            {
                foreach (var b in chemButtons)
                    if (b != null) b.interactable = !m_Ended && m_Beaker != null;
            }
        }

        void RefreshGuide()
        {
            if (stepLabel != null)
                stepLabel.text = m_Ended ? (m_Success ? "Success" : "Failed") : "Puzzle";

            if (guideTitle != null)
                guideTitle.text = m_Ended
                    ? (m_Success ? "Stable Photographic Crystal" : m_Outcome)
                    : "Photographic Crystal";

            if (guideBody == null) return;

            if (m_Ended)
            {
                guideBody.text = m_Success
                    ? "AgNO₃ + NaCl formed light-sensitive AgCl in the dark.\nFixer stabilized it; light activated a silver-blue glow.\n\nPress Reflect."
                    : "Read the journal. Waste/Reset and try the correct path:\nDark → 10 ml AgNO₃ → 10 ml NaCl → wait 5s → 5 ml fixer → Light ON.";
                return;
            }

            guideBody.text = IsPhoneAr()
                ? "PHONE AR:\nScan table → tap plane to place beaker.\nReagents spawn around it — tap to select.\n\n1) LIGHT OFF\n2) 10 ml A\n3) 10 ml B\n4) Wait ~5s\n5) 5 ml C\n6) LIGHT ON → glow\n\nSIMULATION ONLY."
                : "Tap glass bottles (hover labels) to select.\nMeasure 5/10 ml → Pour into beaker.\n\n1) LIGHT OFF\n2) 10 ml A\n3) 10 ml B\n4) Wait ~5s\n5) 5 ml C\n6) LIGHT ON → glow";
        }

        static string ChemName(ChemId id) => id switch
        {
            ChemId.SilverNitrate => "A · Silver Nitrate",
            ChemId.SodiumChloride => "B · Sodium Chloride",
            ChemId.SodiumThiosulfate => "C · Sodium Thiosulfate",
            ChemId.DistilledWater => "D · Distilled Water",
            ChemId.SodiumCarbonate => "E · Sodium Carbonate",
            ChemId.CopperSulfate => "F · Copper Sulfate",
            _ => "—"
        };
    }
}
