using UnityEngine;

namespace EduQuest
{
    /// <summary>
    /// 3-level living-room campaign:
    /// L1 combat → find plant → L2 combat → find flowers → light gate → lab mix.
    /// Enemies are cleared during hunt phases. Ground plane stays fixed.
    /// </summary>
    public class CampaignFlow : MonoBehaviour
    {
        [SerializeField] CampaignHud hud;
        [SerializeField] CombatWave combat;
        [SerializeField] Transform kitRoot;
        [SerializeField] Light keyLight;
        [SerializeField] Light fillLight;
        [SerializeField] Camera viewCamera;
        [SerializeField] EditorCrystalExperiment lab;

        readonly GameProgress m_Progress = new();
        CampaignPhase m_Phase = CampaignPhase.Level1Combat;
        Vector3 m_GroundCenter = new Vector3(0f, 0.03f, 0.4f);
        string m_Status = "";

        const string Equation =
            "AgNO3 + NaCl → AgCl (crystal)  then Fixer + controlled LIGHT";

        public GameProgress Progress => m_Progress;
        public CampaignPhase Phase => m_Phase;

        public void Configure(
            CampaignHud campaignHud,
            Transform kit,
            Light key,
            Light fill,
            Camera cam,
            EditorCrystalExperiment experiment)
        {
            hud = campaignHud;
            kitRoot = kit;
            keyLight = key;
            fillLight = fill;
            viewCamera = cam;
            lab = experiment;
        }

        public void Begin()
        {
            if (viewCamera == null) viewCamera = Camera.main;
            if (kitRoot != null)
                m_GroundCenter = kitRoot.position;

            if (combat == null)
                combat = gameObject.GetComponent<CombatWave>() ?? gameObject.AddComponent<CombatWave>();
            combat.Configure(viewCamera);
            combat.WaveCleared -= OnWaveCleared;
            combat.PlayerDefeated -= OnPlayerDefeated;
            combat.WaveCleared += OnWaveCleared;
            combat.PlayerDefeated += OnPlayerDefeated;

            HookHud();
            SetKitVisible(false);
            SetLabEnabled(false);
            EnterPhase(CampaignPhase.Level1Combat);
        }

        void OnDestroy()
        {
            UnhookHud();
            if (combat != null)
            {
                combat.WaveCleared -= OnWaveCleared;
                combat.PlayerDefeated -= OnPlayerDefeated;
            }
        }

        void Update()
        {
            if (m_Phase is CampaignPhase.Level1Combat or CampaignPhase.Level2Combat)
                RefreshCombatHud();
            else if (m_Phase == CampaignPhase.Level3LightGate)
                RefreshLightHud();
        }

        void HookHud()
        {
            if (hud == null) return;
            UnhookHud();
            hud.WinCombatClicked += DebugWinCombat;
            hud.FoundPropClicked += OnFoundProp;
            hud.LightGateClicked += OnLightGateConfirm;
            hud.StartLabClicked += OnEnterLab;
            hud.ResetCampaignClicked += ResetCampaign;
        }

        void UnhookHud()
        {
            if (hud == null) return;
            hud.WinCombatClicked -= DebugWinCombat;
            hud.FoundPropClicked -= OnFoundProp;
            hud.LightGateClicked -= OnLightGateConfirm;
            hud.StartLabClicked -= OnEnterLab;
            hud.ResetCampaignClicked -= ResetCampaign;
        }

        void EnterPhase(CampaignPhase phase)
        {
            m_Phase = phase;
            combat?.ClearWave();

            switch (phase)
            {
                case CampaignPhase.Level1Combat:
                    SetKitVisible(false);
                    SetLabEnabled(false);
                    combat.BeginWave(1, m_GroundCenter);
                    m_Status = "Tap the blue training foes. They do not attack yet.";
                    break;

                case CampaignPhase.Level1HuntPlant:
                    SetKitVisible(false);
                    SetLabEnabled(false);
                    m_Status = "PROPS GUIDE: Find a plant in your living room. No enemies now.";
                    break;

                case CampaignPhase.Level2Combat:
                    SetKitVisible(false);
                    SetLabEnabled(false);
                    combat.BeginWave(2, m_GroundCenter);
                    m_Status = "Harder foes — they attack! Tap them before your HP hits 0.";
                    break;

                case CampaignPhase.Level2HuntFlowers:
                    SetKitVisible(false);
                    SetLabEnabled(false);
                    m_Status = "PROPS GUIDE: Find flowers in your living room. No enemies now.";
                    break;

                case CampaignPhase.Level3LightGate:
                    SetKitVisible(false);
                    SetLabEnabled(false);
                    // Prefer a darkened room for the darkroom fantasy
                    if (keyLight != null) keyLight.intensity = 0.15f;
                    if (fillLight != null) fillLight.intensity = 0.05f;
                    m_Status = "PROPS GUIDE: Make the room darker (use DARK or dim lights).";
                    break;

                case CampaignPhase.Level3LabMix:
                    m_Progress.UnlockedLab = true;
                    SetKitVisible(true);
                    SetLabEnabled(true);
                    m_Status = "Final puzzle: mix A → MIX, B → MIX, wait, C → MIX, then LIGHT.";
                    break;

                case CampaignPhase.CampaignWon:
                    m_Status = "Campaign complete — crystal glowing!";
                    break;
            }

            RefreshPhaseHud();
            Debug.Log($"[EduQuest] Campaign phase → {phase}");
        }

        void OnWaveCleared()
        {
            if (m_Phase == CampaignPhase.Level1Combat)
            {
                m_Progress.EquationRevealed = true;
                hud?.Toast("Wave cleared! Equation unlocked.");
                EnterPhase(CampaignPhase.Level1HuntPlant);
            }
            else if (m_Phase == CampaignPhase.Level2Combat)
            {
                hud?.Toast("Wave cleared! Gather flowers next.");
                EnterPhase(CampaignPhase.Level2HuntFlowers);
            }
        }

        void OnPlayerDefeated()
        {
            m_Status = "You were overwhelmed — wave restarts.";
            hud?.Toast("Defeated! Try again.");
            RefreshPhaseHud();
            // Restart same wave
            if (m_Phase == CampaignPhase.Level2Combat)
                combat.BeginWave(2, m_GroundCenter);
            else if (m_Phase == CampaignPhase.Level1Combat)
                combat.BeginWave(1, m_GroundCenter);
        }

        void DebugWinCombat()
        {
            if (m_Phase is not (CampaignPhase.Level1Combat or CampaignPhase.Level2Combat)) return;
            combat?.ClearWave();
            OnWaveCleared();
        }

        void OnFoundProp()
        {
            if (m_Phase == CampaignPhase.Level1HuntPlant)
            {
                m_Progress.PlantFound = true;
                m_Progress.UnlockedAgNO3 = true;
                hud?.Toast("Plant found! AgNO3 unlocked.");
                EnterPhase(CampaignPhase.Level2Combat);
            }
            else if (m_Phase == CampaignPhase.Level2HuntFlowers)
            {
                m_Progress.FlowersFound = true;
                m_Progress.UnlockedNaCl = true;
                m_Progress.UnlockedFixer = true;
                hud?.Toast("Flowers found! NaCl + Fixer unlocked.");
                EnterPhase(CampaignPhase.Level3LightGate);
            }
        }

        void OnLightGateConfirm()
        {
            if (m_Phase != CampaignPhase.Level3LightGate) return;
            if (!IsRoomDarkEnough())
            {
                m_Status = "Still too bright — dim the room (press DARK / lower lights).";
                RefreshPhaseHud();
                hud?.Toast("Too bright — make it darker.");
                return;
            }

            m_Progress.LightGatePassed = true;
            hud?.Toast("Darkroom ready — enter the lab.");
            EnterPhase(CampaignPhase.Level3LabMix);
        }

        void OnEnterLab()
        {
            if (m_Phase == CampaignPhase.Level3LabMix) return;
            if (m_Phase == CampaignPhase.Level3LightGate && IsRoomDarkEnough())
                OnLightGateConfirm();
        }

        bool IsRoomDarkEnough()
        {
            float key = keyLight != null ? keyLight.intensity : 1f;
            float fill = fillLight != null ? fillLight.intensity : 0.3f;
            return key <= 0.25f && fill <= 0.15f;
        }

        public void NotifyLabSuccess()
        {
            if (m_Phase != CampaignPhase.Level3LabMix) return;
            EnterPhase(CampaignPhase.CampaignWon);
            hud?.Toast("SUCCESS — photographic crystal complete!");
        }

        void ResetCampaign()
        {
            m_Progress.ResetAll();
            SetLabEnabled(false);
            SetKitVisible(false);
            EnterPhase(CampaignPhase.Level1Combat);
            hud?.Toast("Campaign reset.");
        }

        void SetKitVisible(bool on)
        {
            if (kitRoot != null)
                kitRoot.gameObject.SetActive(on);
        }

        void SetLabEnabled(bool on)
        {
            if (lab == null) return;
            lab.enabled = on;
            if (on)
            {
                // Hide campaign chrome a bit — lab has its own ExperimentHud
                if (hud != null) hud.gameObject.SetActive(true);
                lab.Begin();
            }
        }

        void RefreshCombatHud()
        {
            if (hud == null || combat == null) return;
            int level = m_Phase == CampaignPhase.Level2Combat ? 2 : 1;
            string step = $"Level {level} / 3 — Combat";
            string title = level == 1 ? "Training wave" : "Aggressive wave";
            string body = level == 1
                ? "Defeat the foes on the living-room floor."
                : $"Enemies attack! HP {combat.PlayerHp:0}  |  Left {combat.EnemiesAlive}";
            string action = m_Status;
            if (combat.IsActive)
                action = $"{m_Status}  ({combat.EnemiesAlive} left)";

            hud.Show(step, title, body, action, GuideHud.Tone.Warn,
                showWin: true, showFound: false, showLight: false, showLab: false);
        }

        void RefreshLightHud()
        {
            if (hud == null) return;
            bool dark = IsRoomDarkEnough();
            hud.Show(
                "Level 3 / 3 — Light gate",
                "Darkroom check",
                dark
                    ? "Room is dark enough. Confirm to open the mixing lab."
                    : "Dim the lights (DARK button on lab UI, or lower Sun/Fill).",
                m_Status,
                dark ? GuideHud.Tone.Success : GuideHud.Tone.Warn,
                showWin: false, showFound: false, showLight: true, showLab: dark);
        }

        void RefreshPhaseHud()
        {
            if (hud == null) return;

            switch (m_Phase)
            {
                case CampaignPhase.Level1Combat:
                case CampaignPhase.Level2Combat:
                    RefreshCombatHud();
                    break;

                case CampaignPhase.Level1HuntPlant:
                    hud.Show(
                        "Level 1 / 3 — Props guide",
                        "Find a plant",
                        m_Progress.EquationRevealed
                            ? $"Equation: {Equation}\nLook around your living room for a plant. Arena is empty."
                            : "Find a plant in your living room.",
                        m_Status,
                        GuideHud.Tone.Normal,
                        showWin: false, showFound: true, showLight: false, showLab: false);
                    break;

                case CampaignPhase.Level2HuntFlowers:
                    hud.Show(
                        "Level 2 / 3 — Props guide",
                        "Find flowers",
                        "Elements so far: AgNO3 unlocked.\nFind flowers to unlock NaCl + Fixer. No enemies.",
                        m_Status,
                        GuideHud.Tone.Normal,
                        showWin: false, showFound: true, showLight: false, showLab: false);
                    break;

                case CampaignPhase.Level3LightGate:
                    RefreshLightHud();
                    break;

                case CampaignPhase.Level3LabMix:
                    hud.Show(
                        "Level 3 / 3 — Mixing lab",
                        "Final puzzle",
                        "All elements ready. Pour A→MIX, B→MIX, wait, C→MIX, then LIGHT.\nUse the lab HUD below for DARK / LIGHT / RESET.",
                        m_Status,
                        GuideHud.Tone.Success,
                        showWin: false, showFound: false, showLight: false, showLab: false);
                    break;

                case CampaignPhase.CampaignWon:
                    hud.Show(
                        "Campaign complete",
                        "Crystal stabilized!",
                        "You cleared combat, gathered living-room props, set the light, and mixed the plate.",
                        m_Status,
                        GuideHud.Tone.Success,
                        showWin: false, showFound: false, showLight: false, showLab: false);
                    break;
            }
        }
    }
}
