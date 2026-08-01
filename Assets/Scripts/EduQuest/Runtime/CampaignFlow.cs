using UnityEngine;

namespace EduQuest
{
    /// <summary>
    /// Timed science evaluation campaign:
    /// combat → science riddle → real-world find (sky / flowers / light).
    /// Finish all 3 levels before time runs out. Faster = higher score.
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
        bool m_TimerRunning;

        // Riddles never name the object — kids must infer it
        const string RiddleSky =
            "I am the blue blanket above us by day; white ships of vapor drift across me. Point your camera at me.";
        const string RiddleFlowers =
            "Before I bloom I sleep in soil; scientists call my waking germination. Find my colorful stage nearby.";
        const string RiddleLight =
            "In physics I am energy that travels as waves (and particles) and lets you see. Show me a source of me.";

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

            m_Progress.ResetAll();
            m_Progress.TimeLimitSeconds = 180f; // 3 minutes for the full evaluation
            m_TimerRunning = true;
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
            if (m_TimerRunning &&
                m_Phase is not (CampaignPhase.CampaignWon or CampaignPhase.CampaignFailed))
            {
                m_Progress.ElapsedSeconds += Time.deltaTime;
                if (m_Progress.RemainingSeconds <= 0f)
                {
                    FailByTimeout();
                    return;
                }
            }

            if (m_Phase is CampaignPhase.Level1Combat or CampaignPhase.Level2Combat)
                RefreshCombatHud();
            else if (m_Phase is CampaignPhase.Level1RiddleSky
                     or CampaignPhase.Level2RiddleFlowers
                     or CampaignPhase.Level3RiddleLight)
                RefreshRiddleHud();
        }

        void HookHud()
        {
            if (hud == null) return;
            UnhookHud();
            hud.WinCombatClicked += DebugWinCombat;
            hud.FoundPropClicked += OnSolvedRiddle;
            hud.LightGateClicked += OnSolvedRiddle; // Level 3 uses same confirm
            hud.StartLabClicked += OnSolvedRiddle;
            hud.ResetCampaignClicked += ResetCampaign;
        }

        void UnhookHud()
        {
            if (hud == null) return;
            hud.WinCombatClicked -= DebugWinCombat;
            hud.FoundPropClicked -= OnSolvedRiddle;
            hud.LightGateClicked -= OnSolvedRiddle;
            hud.StartLabClicked -= OnSolvedRiddle;
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
                    m_Status = "Clear the wave to earn your first science riddle.";
                    break;

                case CampaignPhase.Level1RiddleSky:
                    SetKitVisible(false);
                    SetLabEnabled(false);
                    m_Status = "Solve the riddle — what should you scan? (No enemies.)";
                    break;

                case CampaignPhase.Level2Combat:
                    SetKitVisible(false);
                    SetLabEnabled(false);
                    combat.BeginWave(2, m_GroundCenter);
                    m_Status = "Harder foes attack! Survive to earn the next riddle.";
                    break;

                case CampaignPhase.Level2RiddleFlowers:
                    SetKitVisible(false);
                    SetLabEnabled(false);
                    m_Status = "Solve the riddle — what should you scan? (No enemies.)";
                    break;

                case CampaignPhase.Level3RiddleLight:
                    SetKitVisible(false);
                    SetLabEnabled(false);
                    m_Status = "Final riddle — prove you know what light is.";
                    break;

                case CampaignPhase.CampaignWon:
                    m_TimerRunning = false;
                    m_Progress.ComputeScore();
                    m_Status =
                        $"Finished in {FormatTime(m_Progress.ElapsedSeconds)} · " +
                        $"Score {m_Progress.FinalScore} · Stars {m_Progress.Stars}/3";
                    break;

                case CampaignPhase.CampaignFailed:
                    m_TimerRunning = false;
                    combat?.ClearWave();
                    m_Status = "Time is up — evaluation failed. RESET to try a faster run.";
                    break;
            }

            RefreshPhaseHud();
            Debug.Log($"[EduQuest] Campaign phase → {phase}");
        }

        void OnWaveCleared()
        {
            if (m_Phase == CampaignPhase.Level1Combat)
            {
                hud?.Toast("Wave cleared! Riddle unlocked.");
                EnterPhase(CampaignPhase.Level1RiddleSky);
            }
            else if (m_Phase == CampaignPhase.Level2Combat)
            {
                hud?.Toast("Wave cleared! Next riddle unlocked.");
                EnterPhase(CampaignPhase.Level2RiddleFlowers);
            }
        }

        void OnPlayerDefeated()
        {
            m_Status = "Overwhelmed — wave restarts (timer still running!).";
            hud?.Toast("Defeated! Hurry — clock is ticking.");
            RefreshPhaseHud();
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

        /// <summary>Editor stub for AR find — later replaced by real sky/flower/light detection.</summary>
        void OnSolvedRiddle()
        {
            if (m_Phase == CampaignPhase.Level1RiddleSky)
            {
                m_Progress.SkyFound = true;
                hud?.Toast("Correct — sky / clouds!");
                EnterPhase(CampaignPhase.Level2Combat);
            }
            else if (m_Phase == CampaignPhase.Level2RiddleFlowers)
            {
                m_Progress.FlowersFound = true;
                hud?.Toast("Correct — flowers!");
                EnterPhase(CampaignPhase.Level3RiddleLight);
            }
            else if (m_Phase == CampaignPhase.Level3RiddleLight)
            {
                m_Progress.LightFound = true;
                hud?.Toast("Correct — light!");
                EnterPhase(CampaignPhase.CampaignWon);
                hud?.Toast($"YOU WIN · Score {m_Progress.FinalScore}");
            }
        }

        void FailByTimeout()
        {
            if (m_Phase is CampaignPhase.CampaignFailed or CampaignPhase.CampaignWon) return;
            EnterPhase(CampaignPhase.CampaignFailed);
            hud?.Toast("TIME UP — try again for a higher score.");
        }

        public void NotifyLabSuccess()
        {
            // Lab no longer required to win the timed evaluation.
        }

        void ResetCampaign()
        {
            m_Progress.ResetAll();
            m_Progress.TimeLimitSeconds = 180f;
            m_TimerRunning = true;
            SetLabEnabled(false);
            SetKitVisible(false);
            EnterPhase(CampaignPhase.Level1Combat);
            hud?.Toast("New run — beat the clock!");
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
            if (on) lab.Begin();
        }

        string TimerLine()
        {
            var rem = m_Progress.RemainingSeconds;
            var urgency = rem <= 30f ? "!" : "";
            return $"Time {FormatTime(rem)}{urgency}  ·  Elapsed {FormatTime(m_Progress.ElapsedSeconds)}";
        }

        static string FormatTime(float seconds)
        {
            seconds = Mathf.Max(0f, seconds);
            int m = Mathf.FloorToInt(seconds / 60f);
            int s = Mathf.FloorToInt(seconds % 60f);
            return $"{m:0}:{s:00}";
        }

        void RefreshCombatHud()
        {
            if (hud == null || combat == null) return;
            int level = m_Phase == CampaignPhase.Level2Combat ? 2 : 1;
            string title = level == 1 ? "Level 1 — Training wave" : "Level 2 — Aggressive wave";
            string body = level == 1
                ? $"{TimerLine()}\nDefeat frogs/rats to unlock a sky riddle."
                : $"{TimerLine()}\nHP {combat.PlayerHp:0} · Left {combat.EnemiesAlive}\nWin to unlock a germination riddle.";
            string action = combat.IsActive ? $"{m_Status} ({combat.EnemiesAlive} left)" : m_Status;

            hud.Show($"Combat · {TimerLine()}", title, body, action,
                remTone(),
                showWin: true, showFound: false, showLight: false, showLab: false);
        }

        void RefreshRiddleHud()
        {
            if (hud == null) return;

            string step, title, riddle;
            bool showLightBtn = false;

            switch (m_Phase)
            {
                case CampaignPhase.Level1RiddleSky:
                    step = "Level 1 / 3 — Science riddle";
                    title = "What am I looking for?";
                    riddle = RiddleSky;
                    break;
                case CampaignPhase.Level2RiddleFlowers:
                    step = "Level 2 / 3 — Science riddle";
                    title = "What am I looking for?";
                    riddle = RiddleFlowers;
                    break;
                default:
                    step = "Level 3 / 3 — Science riddle";
                    title = "Final evaluation";
                    riddle = RiddleLight;
                    showLightBtn = true;
                    break;
            }

            hud.Show(
                $"{step} · {TimerLine()}",
                title,
                $"{riddle}\n\n{TimerLine()}\n(No enemies — figure it out, then confirm.)",
                m_Status,
                remTone(),
                showWin: false,
                showFound: !showLightBtn,
                showLight: showLightBtn,
                showLab: false);
        }

        GuideHud.Tone remTone()
        {
            if (m_Progress.RemainingSeconds <= 30f) return GuideHud.Tone.Fail;
            if (m_Progress.RemainingSeconds <= 60f) return GuideHud.Tone.Warn;
            return GuideHud.Tone.Normal;
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

                case CampaignPhase.Level1RiddleSky:
                case CampaignPhase.Level2RiddleFlowers:
                case CampaignPhase.Level3RiddleLight:
                    RefreshRiddleHud();
                    break;

                case CampaignPhase.CampaignWon:
                    hud.Show(
                        "Evaluation complete",
                        "YOU WIN",
                        $"All 3 science finds solved before time ran out.\n" +
                        $"Time used: {FormatTime(m_Progress.ElapsedSeconds)} / {FormatTime(m_Progress.TimeLimitSeconds)}\n" +
                        $"Score: {m_Progress.FinalScore}   Stars: {m_Progress.Stars}/3\n" +
                        "Faster runs earn more points. RESET to beat your score.",
                        m_Status,
                        GuideHud.Tone.Success,
                        showWin: false, showFound: false, showLight: false, showLab: false);
                    break;

                case CampaignPhase.CampaignFailed:
                    hud.Show(
                        "Evaluation failed",
                        "TIME UP",
                        "You did not finish all 3 levels in time.\nRESET for another attempt — speed + correct riddles raise your score.",
                        m_Status,
                        GuideHud.Tone.Fail,
                        showWin: false, showFound: false, showLight: false, showLab: false);
                    break;
            }
        }
    }
}
