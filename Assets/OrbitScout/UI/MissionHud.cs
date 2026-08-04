using OrbitScout.Core;
using OrbitScout.Platform;
using OrbitScout.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OrbitScout.UI
{
    public class MissionHud : MonoBehaviour
    {
        Canvas canvas;
        GameObject menuPanel;
        GameObject levelSelectPanel;
        GameObject playPanel;
        GameObject endPanel;

        TMP_Text menuScoresText;
        TMP_Text questionText;
        TMP_Text scoreText;
        TMP_Text timerText;
        TMP_Text feedbackText;
        TMP_Text streakText;
        TMP_Text endBodyText;
        TMP_Text levelSelectStatusText;

        Button noneMatchButton;
        LevelId pendingLevel;

        void Awake()
        {
            GameProgress.UnlockAllLevelsForTesting();
            BuildUi();
            ShowMainMenu();
        }

        void Start()
        {
            WireSession();
            SunTapReject.OnSunTapMessage += HandleSunTap;
            SyncArCameraToActivePanel();
        }

        void SyncArCameraToActivePanel()
        {
            if (playPanel != null && playPanel.activeSelf)
                OrbitScoutArCameraPresentation.ApplyLevelPresentation();
            else
                OrbitScoutArCameraPresentation.ApplyMenuPresentation();
        }

        void OnDestroy()
        {
            SunTapReject.OnSunTapMessage -= HandleSunTap;
            UnwireSession();
        }

        void WireSession()
        {
            MissionController session = MissionController.Instance;
            if (session == null)
                return;

            UnwireSession();
            session.OnLevelStarted += HandleLevelStarted;
            session.OnLevelEnded += HandleLevelEnded;
            session.OnQuestionChanged += HandleQuestionChanged;
            session.OnFeedback += HandleFeedback;
            session.OnScoreChanged += HandleScoreChanged;
            session.OnTimerTick += HandleTimerTick;
            session.OnLevel4PhaseChanged += HandleLevel4PhaseChanged;
        }

        void UnwireSession()
        {
            MissionController session = MissionController.Instance;
            if (session == null)
                return;

            session.OnLevelStarted -= HandleLevelStarted;
            session.OnLevelEnded -= HandleLevelEnded;
            session.OnQuestionChanged -= HandleQuestionChanged;
            session.OnFeedback -= HandleFeedback;
            session.OnScoreChanged -= HandleScoreChanged;
            session.OnTimerTick -= HandleTimerTick;
            session.OnLevel4PhaseChanged -= HandleLevel4PhaseChanged;
        }

        void BuildUi()
        {
            OrbitScoutUiInputSetup.EnsureEventSystem();

            GameObject canvasObject = new GameObject("OrbitScoutHud");
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080f, 1920f);
            canvasObject.AddComponent<GraphicRaycaster>();

            menuPanel = CreatePanel("MenuPanel", canvas.transform);
            OrbitScoutUiTheme.ApplyPanelBackdrop(menuPanel, fullScreenMenu: true);
            TMP_Text title = CreateLabel(menuPanel.transform, "Orbit Scout", 56, new Vector2(0.5f, 0.78f), out _);
            OrbitScoutUiTheme.StyleTitle(title);
            TMP_Text subtitle = CreateLabel(menuPanel.transform, "Educational AR · Solar System Quiz", 22, new Vector2(0.5f, 0.70f), out _);
            OrbitScoutUiTheme.StyleSubtitle(subtitle);
            menuScoresText = CreateLabel(menuPanel.transform, "", 24, new Vector2(0.5f, 0.58f), out _);
            OrbitScoutUiTheme.StyleBody(menuScoresText);
            CreateStyledButton(menuPanel.transform, "Play", new Vector2(0.5f, 0.46f), ShowLevelSelect, true);
            CreateStyledButton(menuPanel.transform, "Reset Journey", new Vector2(0.5f, 0.36f), OnResetJourney, false);

            playPanel = CreatePanel("PlayPanel", canvas.transform);
            OrbitScoutUiTheme.ApplyPanelBackdrop(playPanel, playHud: true);
            CreateHudCard(playPanel.transform, new Vector2(0.5f, 0.82f), new Vector2(920f, 280f));
            questionText = CreateLabel(playPanel.transform, "", 28, new Vector2(0.5f, 0.82f), out _);
            OrbitScoutUiTheme.StyleBody(questionText);
            scoreText = CreateLabel(playPanel.transform, "Score 0", 26, new Vector2(0.5f, 0.94f), out _);
            scoreText.color = OrbitScoutUiTheme.AccentCyan;
            timerText = CreateLabel(playPanel.transform, "", 26, new Vector2(0.5f, 0.90f), out _);
            timerText.color = OrbitScoutUiTheme.AccentGold;
            streakText = CreateLabel(playPanel.transform, "", 28, new Vector2(0.5f, 0.72f), out _);
            streakText.fontStyle = FontStyles.Bold;
            streakText.color = OrbitScoutUiTheme.AccentGold;
            CreateHudCard(playPanel.transform, new Vector2(0.5f, 0.16f), new Vector2(920f, 140f));
            feedbackText = CreateLabel(playPanel.transform, "Tap a planet.", 24, new Vector2(0.5f, 0.16f), out _);
            OrbitScoutUiTheme.StyleBody(feedbackText);

            noneMatchButton = CreateStyledButtonReturn(playPanel.transform, "No planet matches", new Vector2(0.5f, 0.06f), OnNoneMatchClicked, true);
            noneMatchButton.gameObject.SetActive(false);

            CreateStyledButtonReturn(playPanel.transform, "Restart", new Vector2(0.18f, 0.96f), RestartCurrentLevel, false);
            CreateStyledButtonReturn(playPanel.transform, "Menu", new Vector2(0.82f, 0.96f), QuitPlayToMainMenu, false);

            endPanel = CreatePanel("EndPanel", canvas.transform);
            OrbitScoutUiTheme.ApplyPanelBackdrop(endPanel, fullScreenMenu: true);
            TMP_Text endTitle = CreateLabel(endPanel.transform, "Mission Debrief", 48, new Vector2(0.5f, 0.70f), out _);
            OrbitScoutUiTheme.StyleTitle(endTitle);
            endBodyText = CreateLabel(endPanel.transform, "", 26, new Vector2(0.5f, 0.50f), out _);
            OrbitScoutUiTheme.StyleBody(endBodyText);
            CreateStyledButton(endPanel.transform, "Retry Level", new Vector2(0.5f, 0.32f), RetryLastLevel, true);
            CreateStyledButton(endPanel.transform, "Level Select", new Vector2(0.5f, 0.22f), ShowLevelSelect, false);
            CreateStyledButton(endPanel.transform, "Main Menu", new Vector2(0.5f, 0.12f), ShowMainMenu, false);

            levelSelectPanel = CreatePanel("LevelSelectPanel", canvas.transform);
            OrbitScoutUiTheme.ApplyPanelBackdrop(levelSelectPanel, fullScreenMenu: true);
            TMP_Text levelTitle = CreateLabel(levelSelectPanel.transform, "Choose Mission", 46, new Vector2(0.5f, 0.84f), out _);
            OrbitScoutUiTheme.StyleTitle(levelTitle);
            CreateLevelCard(levelSelectPanel.transform, "I", "First Orbit", "8 planets · learn the basics", 0.68f, LevelId.Level1);
            CreateLevelCard(levelSelectPanel.transform, "II", "Save the Planets", "Restore color before they break", 0.56f, LevelId.Level2);
            CreateLevelCard(levelSelectPanel.transform, "III", "Shared Traits", "Multi-select · timed challenge", 0.44f, LevelId.Level3);
            CreateLevelCard(levelSelectPanel.transform, "IV", "Gauntlet", "Blind timed final exam", 0.32f, LevelId.Level4);
            CreateStyledButton(levelSelectPanel.transform, "Back", new Vector2(0.5f, 0.18f), ShowMainMenu, false);
            levelSelectStatusText = CreateLabel(levelSelectPanel.transform, "", 22, new Vector2(0.5f, 0.08f), out _);
            OrbitScoutUiTheme.StyleSubtitle(levelSelectStatusText);
        }

        void CreateLevelCard(Transform parent, string numeral, string title, string desc, float anchorY, LevelId level)
        {
            GameObject card = new GameObject("LevelCard_" + (int)level, typeof(RectTransform), typeof(Image), typeof(Button));
            card.transform.SetParent(parent, false);
            RectTransform rect = card.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, anchorY);
            rect.anchorMax = new Vector2(0.5f, anchorY);
            rect.sizeDelta = new Vector2(880f, 96f);

            Image bg = card.GetComponent<Image>();
            bg.color = OrbitScoutUiTheme.PanelSurface;
            Outline outline = card.AddComponent<Outline>();
            outline.effectColor = OrbitScoutUiTheme.PanelBorder;
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            Button button = card.GetComponent<Button>();
            button.onClick.AddListener(() => TryStartLevel(level));
            OrbitScoutUiTheme.StyleButton(button, false);

            TMP_Text num = CreateLabel(card.transform, numeral, 34, new Vector2(0.12f, 0.5f), out _);
            num.color = OrbitScoutUiTheme.AccentCyan;
            num.fontStyle = FontStyles.Bold;
            TMP_Text titleText = CreateLabel(card.transform, title, 28, new Vector2(0.52f, 0.62f), out _);
            titleText.color = OrbitScoutUiTheme.TextPrimary;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.rectTransform.sizeDelta = new Vector2(520f, 80f);
            TMP_Text descText = CreateLabel(card.transform, desc, 22, new Vector2(0.52f, 0.38f), out _);
            descText.color = OrbitScoutUiTheme.TextMuted;
            descText.alignment = TextAlignmentOptions.Left;
            descText.rectTransform.sizeDelta = new Vector2(520f, 60f);
        }

        static void CreateHudCard(Transform parent, Vector2 anchor, Vector2 size)
        {
            GameObject card = new GameObject("HudCard", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(parent, false);
            RectTransform rect = card.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = size;
            Image image = card.GetComponent<Image>();
            image.color = new Color(0.07f, 0.1f, 0.18f, 0.78f);
            image.raycastTarget = false;
        }

        static void CreateStyledButton(Transform parent, string label, Vector2 anchor, UnityEngine.Events.UnityAction onClick, bool primary)
        {
            CreateStyledButtonReturn(parent, label, anchor, onClick, primary);
        }

        static Button CreateStyledButtonReturn(
            Transform parent,
            string label,
            Vector2 anchor,
            UnityEngine.Events.UnityAction onClick,
            bool primary)
        {
            Button button = CreateButtonReturn(parent, label, anchor, onClick);
            OrbitScoutUiTheme.StyleButton(button, primary);
            TMP_Text labelTmp = button.GetComponentInChildren<TextMeshProUGUI>();
            if (labelTmp != null)
            {
                labelTmp.color = OrbitScoutUiTheme.TextPrimary;
                labelTmp.fontStyle = primary ? FontStyles.Bold : FontStyles.Normal;
            }
            return button;
        }

        void RefreshScoreboard()
        {
            menuScoresText.text =
                "Overall: " + GameProgress.GetOverallScore() + "\n" +
                "L1 best: " + GameProgress.GetLevelHighScore(LevelId.Level1) + "  " +
                "L2: " + GameProgress.GetLevelHighScore(LevelId.Level2) + "\n" +
                "L3: " + GameProgress.GetLevelHighScore(LevelId.Level3) + "  " +
                "L4: " + GameProgress.GetLevelHighScore(LevelId.Level4) + "\n" +
                (GameProgress.BypassLevelLocks
                    ? "Testing: all levels unlocked"
                    : "Unlocked through Level " + GameProgress.GetUnlockedLevel());
        }

        void ShowMainMenu()
        {
            menuPanel.SetActive(true);
            levelSelectPanel.SetActive(false);
            playPanel.SetActive(false);
            endPanel.SetActive(false);
            OrbitScoutArCameraPresentation.ApplyMenuPresentation();
            RefreshScoreboard();
        }

        void ShowLevelSelect()
        {
            GameProgress.UnlockAllLevelsForTesting();
            WireSession();
            menuPanel.SetActive(false);
            levelSelectPanel.SetActive(true);
            playPanel.SetActive(false);
            endPanel.SetActive(false);
            levelSelectPanel.transform.SetAsLastSibling();
            OrbitScoutArCameraPresentation.ApplyMenuPresentation();
            if (levelSelectStatusText != null)
                levelSelectStatusText.text = GameProgress.BypassLevelLocks
                    ? "Test mode: pick any level"
                    : "Unlocked through level " + GameProgress.GetUnlockedLevel();
            RefreshScoreboard();
        }

        void SetLevelSelectStatus(string message)
        {
            if (levelSelectStatusText != null)
                levelSelectStatusText.text = message;
        }

        void TryStartLevel(LevelId level)
        {
            if (!GameProgress.IsLevelUnlocked(level))
            {
                SetLevelSelectStatus("Beat the previous level to unlock this one.");
                return;
            }

            SetLevelSelectStatus("Starting level " + (int)level + "…");
            pendingLevel = level;
            WireSession();

            MissionController session = MissionController.Instance;
            if (session == null)
            {
                SetLevelSelectStatus("Missing MissionController on OrbitScout object.");
                return;
            }

            SolarBootstrap bootstrap = SolarBootstrap.Instance ?? FindAnyObjectByType<SolarBootstrap>();
            if (bootstrap == null)
            {
                SetLevelSelectStatus("Missing SolarBootstrap on OrbitScout object.");
                return;
            }

            if (bootstrap.playMode == SolarPlayMode.AugmentedReality)
            {
                ArSessionBridge bridge = FindAnyObjectByType<ArSessionBridge>();
                if (bridge == null)
                {
                    SetLevelSelectStatus("AR bridge missing — open SampleScene and run Orbit Scout → Setup AR.");
                    return;
                }

                bootstrap.SetPendingLevel(level);
                levelSelectPanel.SetActive(false);
                playPanel.SetActive(true);
                playPanel.transform.SetAsLastSibling();
                OrbitScoutArCameraPresentation.ApplyLevelPresentation();
                questionText.text = "Scan a surface…";
                feedbackText.text = "Tap to place the solar system.";
                bridge.BeginPlacement();
                return;
            }

            bootstrap.StartLevelSession(level);
        }

        void RetryLastLevel()
        {
            TryStartLevel(pendingLevel);
        }

        void RestartCurrentLevel()
        {
            WireSession();
            SolarBootstrap bootstrap = SolarBootstrap.Instance ?? FindAnyObjectByType<SolarBootstrap>();
            if (bootstrap == null)
                return;

            bootstrap.EndPlaySession();
            if (bootstrap.playMode == SolarPlayMode.AugmentedReality)
                bootstrap.RestartCurrentLevelInPlace();
            else
                bootstrap.StartLevelSession(pendingLevel);
        }

        void QuitPlayToMainMenu()
        {
            SolarBootstrap bootstrap = SolarBootstrap.Instance ?? FindAnyObjectByType<SolarBootstrap>();
            bootstrap?.EndPlaySession();
            ShowMainMenu();
        }

        void OnResetJourney()
        {
            GameProgress.ResetJourney();
            ShowMainMenu();
        }

        void OnNoneMatchClicked()
        {
            MissionController.Instance?.SubmitNoMatchingPlanet();
        }

        void HandleLevelStarted(LevelId level)
        {
            pendingLevel = level;
            menuPanel.SetActive(false);
            levelSelectPanel.SetActive(false);
            playPanel.SetActive(true);
            endPanel.SetActive(false);
            playPanel.transform.SetAsLastSibling();
            OrbitScoutArCameraPresentation.ApplyLevelPresentation();
            noneMatchButton.gameObject.SetActive(level == LevelId.Level4);
            streakText.text = LevelRulesLine(level);
            feedbackText.text = MissionBanter.PickMissionStart();
        }

        static string LevelRulesLine(LevelId level)
        {
            switch (level)
            {
                case LevelId.Level1: return "8 questions · no timer · need 5/8 to unlock L2";
                case LevelId.Level2: return "24 facts · restore planets · save 3 to unlock L3";
                case LevelId.Level3: return "10 questions · 10 min · need 7/10 · one wrong fails Q";
                case LevelId.Level4: return "5 questions · 10s read + 10s answer · need 5/5";
                default: return string.Empty;
            }
        }

        void HandleQuestionChanged(string header, string body)
        {
            questionText.text = header + "\n" + body;
        }

        void HandleFeedback(string message)
        {
            feedbackText.text = message;
        }

        void HandleScoreChanged(int score)
        {
            scoreText.text = "Score " + score;
        }

        void HandleTimerTick(float time)
        {
            MissionController session = MissionController.Instance;
            if (session == null)
                return;

            if (session.ActiveLevel == LevelId.Level3)
                timerText.text = "Time " + Mathf.CeilToInt(time) + "s";
            else if (session.ActiveLevel == LevelId.Level4 && session.Level4Phase != Level4Phase.None)
                timerText.text = session.Level4Phase + " " + Mathf.CeilToInt(time) + "s";
            else
                timerText.text = string.Empty;
        }

        void HandleLevel4PhaseChanged(Level4Phase phase)
        {
            noneMatchButton.gameObject.SetActive(phase == Level4Phase.Answering);
        }

        void HandleLevelEnded(LevelRunResult result)
        {
            playPanel.SetActive(false);
            endPanel.SetActive(true);
            noneMatchButton.gameObject.SetActive(false);
            OrbitScoutArCameraPresentation.ApplyMenuPresentation();

            endBodyText.text =
                result.Summary + "\n\n" +
                "Score: " + result.Score + " (best " + GameProgress.GetLevelHighScore(result.Level) + ")\n" +
                "Correct: " + result.CorrectCount + "/" + result.TotalQuestions + "\n" +
                "Overall: " + GameProgress.GetOverallScore();
        }

        void HandleSunTap(string message)
        {
            feedbackText.text = message;
        }

        static GameObject CreatePanel(string name, Transform parent)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return panel;
        }

        static TMP_Text CreateLabel(Transform parent, string text, float size, Vector2 anchor, out GameObject go)
        {
            go = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(920f, 200f);

            TMP_Text tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;
            return tmp;
        }

        static void CreateButton(Transform parent, string label, Vector2 anchor, UnityEngine.Events.UnityAction onClick)
        {
            CreateButtonReturn(parent, label, anchor, onClick);
        }

        static Button CreateButtonReturn(Transform parent, string label, Vector2 anchor, UnityEngine.Events.UnityAction onClick)
        {
            GameObject go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = new Vector2(anchor.y > 0.9f ? 200f : 520f, 72f);

            Image image = go.GetComponent<Image>();
            image.color = new Color(0.15f, 0.55f, 0.75f, 1f);

            Button button = go.GetComponent<Button>();
            button.onClick.AddListener(onClick);

            TMP_Text labelText = CreateLabel(go.transform, label, 26, new Vector2(0.5f, 0.5f), out _);
            labelText.raycastTarget = false;
            return button;
        }
    }
}
