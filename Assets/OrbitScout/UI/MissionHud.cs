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
        const string ResourcesHudPath = "OrbitScout/OrbitScoutHud";

        [Header("Editable UI (Hierarchy / Prefab)")]
        [Tooltip("Assign OrbitScoutHud from the scene, or run Orbit Scout → Create Editable HUD In Scene.")]
        [SerializeField] OrbitScoutHudView hudView;

        [SerializeField] GameObject hudPrefab;

        GameObject menuPanel;
        GameObject menuScoresPanel;
        GameObject levelSelectPanel;
        GameObject briefingPanel;
        GameObject playPanel;
        GameObject endPanel;

        TMP_Text menuScoresText;
        TMP_Text briefingTitleText;
        TMP_Text briefingBodyText;
        TMP_Text questionText;
        TMP_Text scoreText;
        TMP_Text timerText;
        TMP_Text feedbackText;
        TMP_Text streakText;
        TMP_Text endBodyText;
        TMP_Text levelSelectStatusText;

        Button noneMatchButton;
        Button continueNextButton;
        Button briefingStartButton;
        Button briefingBackButton;

        LevelId pendingLevel;
        LevelId? continueTargetLevel;
        OrbitScoutResultsPanel resultsPanel;

        void Awake()
        {
            GameProgress.UseNormalProgression();
            EnsureHudView();
            EnsureBriefingAndContinueUi();
            WireButtonListeners();
            ShowMainMenu();
        }

        void Start()
        {
            WireSession();
            SunTapReject.OnSunTapMessage += HandleSunTap;
            SyncArCameraToActivePanel();
        }

        void EnsureHudView()
        {
            if (hudView == null)
                hudView = GetComponentInChildren<OrbitScoutHudView>(true);

            if (hudView == null)
                hudView = FindAnyObjectByType<OrbitScoutHudView>(FindObjectsInactive.Include);

            if (hudView != null)
            {
                CacheReferencesFromView();
                ApplyStartMenuPillButtons();
                return;
            }

            GameObject prefab = hudPrefab;
            if (prefab == null)
                prefab = Resources.Load<GameObject>(ResourcesHudPath);

            if (prefab != null)
            {
                GameObject instance = Instantiate(prefab, transform);
                instance.name = "OrbitScoutHud";
                hudView = instance.GetComponent<OrbitScoutHudView>();
                CacheReferencesFromView();
                ApplyStartMenuPillButtons();
                return;
            }

            Debug.LogError(
                "Orbit Scout: no OrbitScoutHud in this scene. " +
                "Add one under OrbitScout (Orbit Scout → Create Editable HUD In Scene) and save the scene.");
            enabled = false;
        }

        void CacheReferencesFromView()
        {
            menuPanel = hudView.menuPanel;
            menuScoresPanel = hudView.menuScoresPanel;
            levelSelectPanel = hudView.levelSelectPanel;
            briefingPanel = hudView.briefingPanel;
            playPanel = hudView.playPanel;
            endPanel = hudView.endPanel;
            menuScoresText = hudView.menuScoresText;
            briefingTitleText = hudView.briefingTitleText;
            briefingBodyText = hudView.briefingBodyText;
            briefingStartButton = hudView.briefingStartButton;
            briefingBackButton = hudView.briefingBackButton;
            questionText = hudView.questionText;
            scoreText = hudView.scoreText;
            timerText = hudView.timerText;
            feedbackText = hudView.feedbackText;
            streakText = hudView.streakText;
            endBodyText = hudView.endBodyText;
            continueNextButton = hudView.continueNextButton;
            levelSelectStatusText = hudView.levelSelectStatusText;
            noneMatchButton = hudView.noneMatchButton;
        }

        void ApplyStartMenuPillButtons()
        {
            if (hudView == null)
                return;

            OrbitScoutMenuDecor.EnsureOnMenuPanel(hudView.menuPanel);
            OrbitScoutLevelMapController.EnsureOnPanel(hudView.levelSelectPanel);
            OrbitScoutWalkthroughUi.EnsureOnBriefingPanel(hudView.briefingPanel, hudView.briefingTitleText, hudView.briefingBodyText);
            OrbitScoutUiTheme.StyleWalkthroughTexts(hudView.briefingTitleText, hudView.briefingBodyText);
            OrbitScoutUiTheme.StyleMenuChromeTexts(hudView.menuPanel, hudView.levelSelectPanel);
            resultsPanel = OrbitScoutResultsPanel.EnsureOnEndPanel(hudView.endPanel);
            OrbitScoutUiTheme.ApplyGlassButtons(hudView);
            OrbitScoutUiTheme.StylePlayHudTexts(
                hudView.streakText,
                hudView.questionText,
                hudView.scoreText,
                hudView.timerText,
                hudView.feedbackText);
            LayoutEndScreenButtons();
        }

        void LayoutEndScreenButtons()
        {
            if (hudView == null)
                return;

            PlaceButton(hudView.continueNextButton, 0.22f);
            PlaceButton(hudView.retryLevelButton, 0.14f);
            PlaceButton(hudView.endLevelSelectButton, 0.08f);
            PlaceButton(hudView.endMainMenuButton, 0.03f);
        }

        static void PlaceButton(Button button, float anchorY)
        {
            if (button == null)
                return;
            RectTransform rect = button.GetComponent<RectTransform>();
            if (rect == null)
                return;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, anchorY);
            rect.anchoredPosition = Vector2.zero;
        }

        void EnsureBriefingAndContinueUi()
        {
            if (hudView == null)
                return;

            Transform canvas = hudView.transform;

            if (briefingPanel == null)
            {
                briefingPanel = CreateRuntimePanel(canvas, "BriefingPanel");

                briefingTitleText = CreateRuntimeLabel(briefingPanel.transform, "BriefingTitle", "Mission Briefing", 32, new Vector2(0.5f, 0.82f), 420f, 90f);
                briefingBodyText = CreateRuntimeLabel(briefingPanel.transform, "BriefingBody", "", 22, new Vector2(0.5f, 0.52f), 400f, 340f);
                briefingBodyText.alignment = TextAlignmentOptions.TopLeft;

                briefingStartButton = CreateRuntimeButton(briefingPanel.transform, "BriefingStart", "Start Mission", new Vector2(0.5f, 0.14f), true);
                briefingBackButton = CreateRuntimeButton(briefingPanel.transform, "BriefingBack", "Back", new Vector2(0.5f, 0.05f), false);

                hudView.briefingPanel = briefingPanel;
                hudView.briefingTitleText = briefingTitleText;
                hudView.briefingBodyText = briefingBodyText;
                hudView.briefingStartButton = briefingStartButton;
                hudView.briefingBackButton = briefingBackButton;
                OrbitScoutWalkthroughUi.EnsureOnBriefingPanel(briefingPanel, briefingTitleText, briefingBodyText);
                briefingPanel.SetActive(false);
            }
            else
            {
                OrbitScoutWalkthroughUi.EnsureOnBriefingPanel(briefingPanel, briefingTitleText, briefingBodyText);
            }

            if (endPanel != null)
                resultsPanel = OrbitScoutResultsPanel.EnsureOnEndPanel(endPanel);

            if (continueNextButton == null && endPanel != null)
            {
                continueNextButton = CreateRuntimeButton(endPanel.transform, "ContinueNextButton", "Continue to Next Level", new Vector2(0.5f, 0.22f), true);
                hudView.continueNextButton = continueNextButton;
                continueNextButton.gameObject.SetActive(false);
            }

            OrbitScoutUiTheme.ApplyGlassButtons(hudView);
            LayoutEndScreenButtons();
        }

        static GameObject CreateRuntimePanel(Transform parent, string name)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return panel;
        }

        static TMP_Text CreateRuntimeLabel(Transform parent, string name, string text, float size, Vector2 anchor, float width, float height)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(width, height);
            TMP_Text tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            return tmp;
        }

        static Button CreateRuntimeButton(Transform parent, string name, string label, Vector2 anchor, bool primary)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = new Vector2(560f, 72f);
            Button button = go.GetComponent<Button>();
            OrbitScoutUiTheme.StyleMenuPillButton(button, primary);
            CreateRuntimeLabel(go.transform, "Label", label, 26, new Vector2(0.5f, 0.5f), 520f, 60f);
            return button;
        }

        void WireButtonListeners()
        {
            if (hudView == null)
                return;

            Wire(hudView.playButton, ShowLevelSelect);
            Wire(hudView.resetJourneyButton, OnResetJourney);
            Wire(hudView.levelSelectBackButton, ShowMainMenu);
            Wire(hudView.noneMatchButton, OnNoneMatchClicked);
            Wire(hudView.restartButton, RestartCurrentLevel);
            Wire(hudView.menuButton, QuitPlayToMainMenu);
            Wire(hudView.retryLevelButton, RetryLastLevel);
            Wire(hudView.endLevelSelectButton, ShowLevelSelect);
            Wire(hudView.endMainMenuButton, ShowMainMenu);
            Wire(briefingStartButton, BeginMissionAfterBriefing);
            Wire(briefingBackButton, ShowLevelSelect);
            Wire(continueNextButton, ContinueToNextLevel);

            if (levelSelectPanel != null)
            {
                foreach (OrbitScoutLevelCardButton card in levelSelectPanel.GetComponentsInChildren<OrbitScoutLevelCardButton>(true))
                {
                    Button button = card.GetComponent<Button>();
                    if (button == null)
                        continue;

                    LevelId level = card.level;
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => ShowLevelBriefing(level));
                }
            }
        }

        static void Wire(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
                return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        void OnDestroy()
        {
            SunTapReject.OnSunTapMessage -= HandleSunTap;
            UnwireSession();
        }

        void SyncArCameraToActivePanel()
        {
            if (playPanel != null && playPanel.activeSelf)
                OrbitScoutArCameraPresentation.ApplyLevelPresentation();
            else
                OrbitScoutArCameraPresentation.ApplyMenuPresentation();
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

        void HideAllPanels()
        {
            if (menuPanel != null)
                menuPanel.SetActive(false);
            if (menuScoresPanel != null)
                menuScoresPanel.SetActive(false);
            if (levelSelectPanel != null)
                levelSelectPanel.SetActive(false);
            if (briefingPanel != null)
                briefingPanel.SetActive(false);
            if (playPanel != null)
                playPanel.SetActive(false);
            if (endPanel != null)
                endPanel.SetActive(false);
        }

        void RefreshScoreboard()
        {
            if (menuScoresText == null)
                return;

            menuScoresText.text =
                "Overall: " + GameProgress.GetOverallScore() + "\n" +
                "L1 best: " + GameProgress.GetLevelHighScore(LevelId.Level1) + "  " +
                "L2: " + GameProgress.GetLevelHighScore(LevelId.Level2) + "\n" +
                "L3: " + GameProgress.GetLevelHighScore(LevelId.Level3) + "  " +
                "L4: " + GameProgress.GetLevelHighScore(LevelId.Level4) + "\n" +
                "Unlocked through Level " + GameProgress.GetUnlockedLevel();
        }

        void ShowMainMenu()
        {
            if (menuPanel == null)
                return;

            HideAllPanels();
            menuPanel.SetActive(true);
            if (menuScoresPanel != null)
                menuScoresPanel.SetActive(true);
            OrbitScoutArCameraPresentation.ApplyMenuPresentation();
            RefreshScoreboard();
        }

        void ShowLevelSelect()
        {
            GameProgress.UseNormalProgression();
            WireSession();
            HideAllPanels();
            levelSelectPanel.SetActive(true);
            levelSelectPanel.transform.SetAsLastSibling();
            OrbitScoutArCameraPresentation.ApplyMenuPresentation();
            RefreshLevelCardLocks();
            if (levelSelectStatusText != null)
                levelSelectStatusText.text = "Follow the path · clear a mission to unlock the next";
            RefreshScoreboard();
        }

        void ShowLevelBriefing(LevelId level)
        {
            if (!GameProgress.IsLevelUnlocked(level))
            {
                string need = level == LevelId.Level1
                    ? "Mission I"
                    : "Mission " + OrbitScoutLevelBriefings.RomanNumeral((LevelId)((int)level - 1));
                SetLevelSelectStatus("Locked — pass " + need + " first to open this mission.");
                return;
            }

            pendingLevel = level;
            WireSession();
            HideAllPanels();
            if (briefingPanel == null)
            {
                BeginMissionAfterBriefing();
                return;
            }

            briefingPanel.SetActive(true);
            briefingPanel.transform.SetAsLastSibling();
            if (briefingTitleText != null)
                briefingTitleText.text = OrbitScoutLevelBriefings.Title(level);
            if (briefingBodyText != null)
                briefingBodyText.text = OrbitScoutLevelBriefings.Body(level);
            OrbitScoutArCameraPresentation.ApplyMenuPresentation();
        }

        void BeginMissionAfterBriefing()
        {
            LevelId level = pendingLevel;
            WireSession();

            MissionController session = MissionController.Instance;
            if (session == null)
            {
                SetLevelSelectStatus("Missing MissionController on OrbitScout object.");
                ShowLevelSelect();
                return;
            }

            SolarBootstrap bootstrap = SolarBootstrap.Instance ?? FindAnyObjectByType<SolarBootstrap>();
            if (bootstrap == null)
            {
                SetLevelSelectStatus("Missing SolarBootstrap on OrbitScout object.");
                ShowLevelSelect();
                return;
            }

            if (bootstrap.playMode == SolarPlayMode.AugmentedReality)
            {
                ArSessionBridge bridge = FindAnyObjectByType<ArSessionBridge>();
                if (bridge == null)
                {
                    SetLevelSelectStatus("AR bridge missing — open SampleScene and run Orbit Scout → Setup AR.");
                    ShowLevelSelect();
                    return;
                }

                bootstrap.SetPendingLevel(level);
                HideAllPanels();
                playPanel.SetActive(true);
                playPanel.transform.SetAsLastSibling();
                OrbitScoutArCameraPresentation.ApplyLevelPresentation();
                if (questionText != null)
                    questionText.text = "Scan a surface…";
                if (feedbackText != null)
                    feedbackText.text = string.Empty;
                if (streakText != null)
                {
                    streakText.text = string.Empty;
                    streakText.gameObject.SetActive(false);
                }
                if (scoreText != null)
                    scoreText.text = "Score  0";
                if (timerText != null)
                    timerText.text = string.Empty;
                bridge.BeginPlacement();
                return;
            }

            bootstrap.StartLevelSession(level);
        }

        void RefreshLevelCardLocks()
        {
            if (levelSelectPanel == null)
                return;

            OrbitScoutLevelMapController map = levelSelectPanel.GetComponentInChildren<OrbitScoutLevelMapController>(true);
            if (map != null)
            {
                map.RefreshLocks();
                return;
            }

            foreach (OrbitScoutLevelCardButton card in levelSelectPanel.GetComponentsInChildren<OrbitScoutLevelCardButton>(true))
            {
                if (card is OrbitScoutLevelMapNode)
                    continue;

                Button button = card.GetComponent<Button>();
                if (button == null)
                    continue;

                bool unlocked = GameProgress.IsLevelUnlocked(card.level);
                button.interactable = unlocked;
                button.enabled = true;

                Image image = card.GetComponent<Image>();
                if (image != null)
                {
                    Color c = OrbitScoutUiTheme.PanelSurface;
                    c.a = unlocked ? 0.88f : 0.32f;
                    image.color = c;
                    image.raycastTarget = unlocked;
                }
            }
        }

        void SetLevelSelectStatus(string message)
        {
            if (levelSelectStatusText != null)
                levelSelectStatusText.text = message;
        }

        void RetryLastLevel() => ShowLevelBriefing(pendingLevel);

        void ContinueToNextLevel()
        {
            if (continueTargetLevel == null)
            {
                ShowLevelSelect();
                return;
            }

            ShowLevelBriefing(continueTargetLevel.Value);
        }

        void RestartCurrentLevel()
        {
            WireSession();
            SolarBootstrap bootstrap = SolarBootstrap.Instance ?? FindAnyObjectByType<SolarBootstrap>();
            if (bootstrap == null)
                return;

            bootstrap.EndPlaySession();
            ShowLevelBriefing(pendingLevel);
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
            RefreshScoreboard();
            ShowMainMenu();
        }

        void OnNoneMatchClicked() => MissionController.Instance?.SubmitNoMatchingPlanet();

        void HandleLevelStarted(LevelId level)
        {
            pendingLevel = level;
            HideAllPanels();
            playPanel.SetActive(true);
            playPanel.transform.SetAsLastSibling();
            OrbitScoutArCameraPresentation.ApplyLevelPresentation();
            if (noneMatchButton != null)
                noneMatchButton.gameObject.SetActive(false);
            if (streakText != null)
            {
                streakText.text = string.Empty;
                streakText.gameObject.SetActive(false);
            }
            if (feedbackText != null)
                feedbackText.text = string.Empty;
            if (scoreText != null)
                scoreText.text = "Score  0";
            OrbitScoutUiTheme.StylePlayHudTexts(streakText, questionText, scoreText, timerText, feedbackText);
        }

        void HandleQuestionChanged(string header, string body)
        {
            // Clue / characteristics only — no question-count UI
            if (streakText != null)
            {
                streakText.text = string.Empty;
                streakText.gameObject.SetActive(false);
            }
            if (questionText != null)
                questionText.text = body;
        }

        void HandleFeedback(string message)
        {
            if (feedbackText == null)
                return;

            feedbackText.text = string.IsNullOrWhiteSpace(message) ? string.Empty : message;
        }

        void HandleScoreChanged(int score)
        {
            if (scoreText != null)
                scoreText.text = "Score  " + score;
        }

        void HandleTimerTick(float time)
        {
            MissionController session = MissionController.Instance;
            if (session == null || timerText == null)
                return;

            if (session.ActiveLevel == LevelId.Level3)
                timerText.text = Mathf.CeilToInt(time) + "s";
            else if (session.ActiveLevel == LevelId.Level4 && session.Level4Phase != Level4Phase.None)
                timerText.text = Mathf.CeilToInt(time) + "s";
            else
                timerText.text = string.Empty;
        }

        void HandleLevel4PhaseChanged(Level4Phase phase)
        {
            if (noneMatchButton != null)
                noneMatchButton.gameObject.SetActive(false);
        }

        void HandleLevelEnded(LevelRunResult result)
        {
            HideAllPanels();
            endPanel.SetActive(true);
            if (noneMatchButton != null)
                noneMatchButton.gameObject.SetActive(false);
            OrbitScoutArCameraPresentation.ApplyMenuPresentation();

            LevelId? next = OrbitScoutLevelBriefings.NextLevel(result.Level);
            bool canContinue = result.PassedUnlock && next != null && GameProgress.IsLevelUnlocked(next.Value);
            continueTargetLevel = canContinue ? next : null;

            if (resultsPanel == null && endPanel != null)
                resultsPanel = OrbitScoutResultsPanel.EnsureOnEndPanel(endPanel);
            resultsPanel?.ShowResult(result);

            if (continueNextButton != null)
            {
                continueNextButton.gameObject.SetActive(canContinue);
                if (canContinue)
                {
                    TMP_Text label = continueNextButton.GetComponentInChildren<TMP_Text>();
                    if (label != null)
                        label.text = "Continue to Mission " + OrbitScoutLevelBriefings.RomanNumeral(next.Value);
                }
            }

            if (hudView != null && hudView.retryLevelButton != null)
            {
                hudView.retryLevelButton.gameObject.SetActive(true);
                OrbitScoutUiTheme.StyleMenuPillButton(hudView.retryLevelButton, !canContinue);
                TMP_Text retryLabel = hudView.retryLevelButton.GetComponentInChildren<TMP_Text>();
                if (retryLabel != null)
                    retryLabel.text = result.PassedUnlock ? "Replay Mission" : "Try Again";
            }

            LayoutEndScreenButtons();

            // Keep legacy body text in sync for accessibility / debugging (hidden visually)
            if (endBodyText != null)
            {
                endBodyText.gameObject.SetActive(false);
                endBodyText.text =
                    result.Summary + "\n\n" +
                    "Score: " + result.Score + " (best " + GameProgress.GetLevelHighScore(result.Level) + ")\n" +
                    "Correct: " + result.CorrectCount + "/" + result.TotalQuestions + "\n" +
                    "Overall: " + GameProgress.GetOverallScore();
            }
        }

        void HandleSunTap(string message)
        {
            if (feedbackText != null)
                feedbackText.text = message;
        }
    }
}
