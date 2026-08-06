using OrbitScout.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OrbitScout.UI
{
    /// <summary>
    /// Win / lose results display on EndPanel (stars, headline, score card, glass CTAs).
    /// </summary>
    public sealed class OrbitScoutResultsPanel : MonoBehaviour
    {
        public const string RootName = "ResultsDisplay";

        [SerializeField] TMP_Text headlineText;
        [SerializeField] TMP_Text levelPillText;
        [SerializeField] TMP_Text scoreLabelText;
        [SerializeField] TMP_Text scoreValueText;
        [SerializeField] TMP_Text detailText;
        [SerializeField] Image[] starImages;
        [SerializeField] Image scoreCard;
        [SerializeField] Image levelPill;

        Sprite goldStar;
        Sprite dimStar;

        public static OrbitScoutResultsPanel EnsureOnEndPanel(GameObject endPanel)
        {
            if (endPanel == null)
                return null;

            OrbitScoutWalkthroughUi.ApplyBackground(endPanel);
            HideLegacyEndLabels(endPanel);

            Transform existing = endPanel.transform.Find(RootName);
            if (existing != null)
            {
                OrbitScoutResultsPanel panel = existing.GetComponent<OrbitScoutResultsPanel>();
                if (panel != null)
                {
                    panel.ApplyLayout();
                    return panel;
                }

                if (Application.isPlaying)
                    Object.Destroy(existing.gameObject);
                else
                    Object.DestroyImmediate(existing.gameObject);
            }

            GameObject root = new GameObject(RootName, typeof(RectTransform), typeof(OrbitScoutResultsPanel));
            root.transform.SetParent(endPanel.transform, false);
            root.transform.SetAsFirstSibling();
            RectTransform rootRect = root.GetComponent<RectTransform>();
            StretchFull(rootRect);

            OrbitScoutResultsPanel results = root.GetComponent<OrbitScoutResultsPanel>();
            results.Build(rootRect);
            return results;
        }

        static void HideLegacyEndLabels(GameObject endPanel)
        {
            if (endPanel == null)
                return;

            foreach (Transform child in endPanel.transform)
            {
                if (child.name == "Title" || child.name == "Body")
                    child.gameObject.SetActive(false);
            }
        }

        void Build(RectTransform root)
        {
            goldStar = Resources.Load<Sprite>("OrbitScout/ResultStar_gold");
            dimStar = Resources.Load<Sprite>("OrbitScout/ResultStar_dim");
#if UNITY_EDITOR
            if (goldStar == null)
                goldStar = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/OrbitScout/UI/Visuals/ResultStar_gold.png");
            if (dimStar == null)
                dimStar = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/OrbitScout/UI/Visuals/ResultStar_dim.png");
#endif

            starImages = new Image[3];
            for (int i = 0; i < 3; i++)
                starImages[i] = CreateImage(root, "Star_" + (i + 1), goldStar, Vector2.zero, Vector2.one);

            levelPill = CreateImage(root, "LevelPill", null, Vector2.zero, Vector2.one);
            levelPill.color = new Color(0.35f, 0.18f, 0.55f, 0.85f);
            levelPillText = CreateLabel(levelPill.transform, "Label", "Mission I", 22f);
            levelPillText.color = Color.white;
            levelPillText.fontStyle = FontStyles.Bold;
            StretchFull(levelPillText.rectTransform);

            headlineText = CreateLabel(root, "Headline", "COMPLETED!", 52f);
            headlineText.fontStyle = FontStyles.Bold;
            headlineText.color = new Color(1f, 0.88f, 0.35f, 1f);

            scoreCard = CreateImage(root, "ScoreCard", null, Vector2.zero, Vector2.one);
            scoreCard.color = new Color(0.22f, 0.12f, 0.38f, 0.82f);
            Outline outline = scoreCard.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.78f, 0.55f, 1f, 0.7f);
            outline.effectDistance = new Vector2(2f, -2f);

            GameObject rewardTab = new GameObject("RewardTab", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            rewardTab.transform.SetParent(scoreCard.transform, false);
            rewardTab.GetComponent<Image>().color = new Color(0.55f, 0.35f, 0.85f, 0.95f);
            TMP_Text rewardLabel = CreateLabel(rewardTab.transform, "Label", "SCORE", 20f);
            rewardLabel.color = new Color(1f, 0.9f, 0.4f, 1f);
            rewardLabel.fontStyle = FontStyles.Bold;
            StretchFull(rewardLabel.rectTransform);
            PlaceAnchored(rewardTab.GetComponent<RectTransform>(), 0.5f, 1f, 0.5f, 0.5f, new Vector2(0f, 18f), new Vector2(180f, 40f));

            scoreValueText = CreateLabel(scoreCard.transform, "ScoreValue", "0", 56f);
            scoreValueText.color = Color.white;
            scoreValueText.fontStyle = FontStyles.Bold;

            scoreLabelText = CreateLabel(scoreCard.transform, "ScoreSub", "Best 0", 22f);
            scoreLabelText.color = new Color(0.85f, 0.75f, 1f, 1f);

            detailText = CreateLabel(root, "Details", "", 24f);
            detailText.color = new Color(0.92f, 0.88f, 1f, 1f);
            detailText.enableWordWrapping = true;
            detailText.overflowMode = TextOverflowModes.Overflow;
            detailText.lineSpacing = 8f;

            ApplyLayout();
        }

        /// <summary>
        /// Clear vertical bands so stars, headline, score, and details never collide.
        /// </summary>
        public void ApplyLayout()
        {
            if (starImages != null && starImages.Length >= 3)
            {
                PlaceAnchored(starImages[0].rectTransform, 0.32f, 0.86f, 0.5f, 0.5f, Vector2.zero, new Vector2(100f, 100f));
                PlaceAnchored(starImages[1].rectTransform, 0.50f, 0.89f, 0.5f, 0.5f, Vector2.zero, new Vector2(130f, 130f));
                PlaceAnchored(starImages[2].rectTransform, 0.68f, 0.86f, 0.5f, 0.5f, Vector2.zero, new Vector2(100f, 100f));
            }

            if (levelPill != null)
                PlaceAnchored(levelPill.rectTransform, 0.5f, 0.76f, 0.5f, 0.5f, Vector2.zero, new Vector2(240f, 48f));

            if (headlineText != null)
                PlaceAnchored(headlineText.rectTransform, 0.5f, 0.68f, 0.5f, 0.5f, Vector2.zero, new Vector2(920f, 70f));

            if (scoreCard != null)
                PlaceAnchored(scoreCard.rectTransform, 0.5f, 0.52f, 0.5f, 0.5f, Vector2.zero, new Vector2(560f, 200f));

            if (scoreValueText != null)
                PlaceAnchored(scoreValueText.rectTransform, 0.5f, 0.58f, 0.5f, 0.5f, Vector2.zero, new Vector2(500f, 72f));

            if (scoreLabelText != null)
                PlaceAnchored(scoreLabelText.rectTransform, 0.5f, 0.26f, 0.5f, 0.5f, Vector2.zero, new Vector2(500f, 40f));

            // Own band under the score card, clear of the CTA buttons
            if (detailText != null)
                PlaceAnchored(detailText.rectTransform, 0.5f, 0.365f, 0.5f, 0.5f, Vector2.zero, new Vector2(880f, 96f));
        }

        public void ShowResult(LevelRunResult result)
        {
            HideLegacyEndLabels(transform.parent != null ? transform.parent.gameObject : null);
            ApplyLayout();

            bool won = result.PassedUnlock;
            int litStars = ComputeStars(result);

            if (headlineText != null)
            {
                headlineText.text = won ? "COMPLETED!" : "TRY AGAIN";
                headlineText.color = won
                    ? new Color(1f, 0.88f, 0.35f, 1f)
                    : new Color(1f, 0.55f, 0.75f, 1f);
            }

            if (levelPillText != null)
                levelPillText.text = "Mission " + OrbitScoutLevelBriefings.RomanNumeral(result.Level);

            if (scoreValueText != null)
                scoreValueText.text = result.Score.ToString();

            if (scoreLabelText != null)
                scoreLabelText.text = "Best " + GameProgress.GetLevelHighScore(result.Level);

            if (detailText != null)
            {
                string summary = string.IsNullOrWhiteSpace(result.Summary) ? string.Empty : result.Summary.Trim();
                string stats = "Correct " + result.CorrectCount + "/" + result.TotalQuestions +
                    "  ·  Overall " + GameProgress.GetOverallScore();
                detailText.text = string.IsNullOrEmpty(summary) ? stats : summary + "\n" + stats;
            }

            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] == null)
                    continue;
                bool lit = i < litStars;
                starImages[i].sprite = lit ? goldStar : dimStar;
                starImages[i].color = lit ? Color.white : new Color(1f, 1f, 1f, 0.75f);
                starImages[i].rectTransform.localScale = lit && i == 1 ? Vector3.one * 1.05f : Vector3.one;
            }
        }

        static int ComputeStars(LevelRunResult result)
        {
            if (!result.PassedUnlock)
                return 0;

            if (result.TotalQuestions <= 0)
                return 3;

            float ratio = result.CorrectCount / (float)result.TotalQuestions;
            if (ratio >= 0.999f)
                return 3;
            if (ratio >= 0.75f)
                return 2;
            return 1;
        }

        static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }

        static void PlaceAnchored(
            RectTransform rect,
            float anchorX,
            float anchorY,
            float pivotX,
            float pivotY,
            Vector2 anchoredPos,
            Vector2 size)
        {
            if (rect == null)
                return;

            rect.anchorMin = rect.anchorMax = new Vector2(anchorX, anchorY);
            rect.pivot = new Vector2(pivotX, pivotY);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;
        }

        static Image CreateImage(Transform parent, string name, Sprite sprite, Vector2 anchor, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            PlaceAnchored(rect, anchor.x, anchor.y, 0.5f, 0.5f, Vector2.zero, size);
            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = sprite != null;
            image.raycastTarget = false;
            image.color = Color.white;
            return image;
        }

        static TMP_Text CreateLabel(Transform parent, string name, string text, float size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            TMP_Text tmp = go.GetComponent<TMP_Text>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            tmp.enableAutoSizing = false;
            return tmp;
        }
    }
}
