using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OrbitScout.UI
{
    /// <summary>
    /// References to HUD objects you edit in the OrbitScoutHud prefab or scene hierarchy.
    /// </summary>
    public class OrbitScoutHudView : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject menuPanel;
        public GameObject menuScoresPanel;
        public GameObject levelSelectPanel;
        public GameObject briefingPanel;
        public GameObject playPanel;
        public GameObject endPanel;

        [Header("Main menu")]
        public TMP_Text menuScoresText;
        public Button playButton;
        public Button resetJourneyButton;

        [Header("Level select")]
        public Button levelSelectBackButton;
        public TMP_Text levelSelectStatusText;

        [Header("Level briefing")]
        public TMP_Text briefingTitleText;
        public TMP_Text briefingBodyText;
        public Button briefingStartButton;
        public Button briefingBackButton;

        [Header("Play HUD")]
        public TMP_Text questionText;
        public TMP_Text scoreText;
        public TMP_Text timerText;
        public TMP_Text streakText;
        public TMP_Text feedbackText;
        public Button noneMatchButton;
        public Button restartButton;
        public Button menuButton;

        [Header("End screen")]
        public TMP_Text endBodyText;
        public Button continueNextButton;
        public Button retryLevelButton;
        public Button endLevelSelectButton;
        public Button endMainMenuButton;
    }
}
