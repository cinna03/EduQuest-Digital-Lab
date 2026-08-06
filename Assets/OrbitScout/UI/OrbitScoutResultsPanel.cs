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

            Transform existing = endPanel.transform.Find(RootName);
            if (existing != null)
            {
                OrbitScoutResultsPanel panel = existing.GetComponent<OrbitScoutResultsPanel>();
                if (panel != null)
                    return panel;
                Object.DestroyImmediate(existing.gameObject);
            }

            // Hide legacy flat title/body if present (we replace the look)
            foreach (Transform child in endPanel.transform)
            {
                if (child.name == "Title" || child.name == "Body")
                    child.gameObject.SetActive(false);
            }

            GameObject root = new GameObject(RootName, typeof(RectTransform), typeof(OrbitScoutResultsPanel));
            root.transform.SetParent(endPanel.transform, false);
            root.transform.SetAsFirstSibling();
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            OrbitScoutResultsPanel results = root.GetComponent<OrbitScoutResultsPanel>();
            results.Build(rootRect);
            return results;
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
            Vector2[] starAnchors =
            {
                new Vector2(0.32f, 0.78f),
                new Vector2(0.50f, 0.82f),
                new Vector2(0.68f, 0.78f),
            };
            Vector2[] starSizes =
            {
                new Vector2(100f, 100f),
                new Vector2(130f, 130f),
                new Vector2(100f, 100f),
            };
            for (int i = 0; i < 3; i++)
            {
                starImages[i] = CreateImage(root, "Star_" + (i + 1), goldStar, starAnchors[i], starSizes[i]);
            }

            levelPill = CreateImage(root, "LevelPill", null, new Vector2(0.5f, 0.70f), new Vector2(220f, 48f));
            levelPill.color = new Color(0.35f, 0.18f, 0.55f, 0.85f);
            levelPillText = CreateLabel(levelPill.transform, "Label", "Mission I", 22f, Vector2.zero, new Vector2(200f, 40f));
            levelPillText.color = Color.white;
            levelPillText.fontStyle = FontStyles.Bold;

            headlineText = CreateLabel(root, "Headline", "COMPLETED!", 56f, new Vector2(0.5f, 0.62f), new Vector2(900f, 90f));
            headlineText.fontStyle = FontStyles.Bold;
            headlineText.color = new Color(1f, 0.88f, 0.35f, 1f);
            OrbitScoutUiTheme.ApplyFont(headlineText, title: true);

            scoreCard = CreateImage(root, "ScoreCard", null, new Vector2(0.5f, 0.46f), new Vector2(520f, 200f));
            scoreCard.color = new Color(0.22f, 0.12f, 0.38f, 0.82f);
            Outline outline = scoreCard.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.78f, 0.55f, 1f, 0.7f);
            outline.effectDistance = new Vector2(2f, -2f);

            GameObject rewardTab = new GameObject("RewardTab", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            rewardTab.transform.SetParent(scoreCard.transform, false);
            RectTransform tabRect = rewardTab.GetComponent<RectTransform>();
            tabRect.anchorMin = tabRect.anchorMax = new Vector2(0.5f, 1f);
            tabRect.pivot = new Vector2(0.5f, 0.5f);
            tabRect.anchoredPosition = new Vector2(0f, 18f);
            tabRect.sizeDelta = new Vector2(180f, 40f);
            rewardTab.GetComponent<Image>().color = new Color(0.55f, 0.35f, 0.85f, 0.95f);
            TMP_Text rewardLabel = CreateLabel(rewardTab.transform, "Label", "SCORE", 20f, Vector2.zero, new Vector2(160f, 36f));
            rewardLabel.color = new Color(1f, 0.9f, 0.4f, 1f);
            rewardLabel.fontStyle = FontStyles.Bold;

            scoreValueText = CreateLabel(scoreCard.transform, "ScoreValue", "0", 52f, new Vector2(0f, 18f), new Vector2(480f, 70f));
            scoreValueText.color = Color.white;
            scoreValueText.fontStyle = FontStyles.Bold;
            OrbitScoutUiTheme.ApplyFont(scoreValueText, title: true);

            scoreLabelText = CreateLabel(scoreCard.transform, "ScoreSub", "Best 0", 22f, new Vector2(0f, -40f), new Vector2(480f, 40f));
            scoreLabelText.color = new Color(0.85f, 0.75f, 1f, 1f);
            OrbitScoutUiTheme.ApplyFont(scoreLabelText, title: false);

            detailText = CreateLabel(root, "Details", "", 22f, new Vector2(0.5f, 0.30f), new Vector2(860f, 100f));
            detailText.color = new Color(0.92f, 0.88f, 1f, 1f);
            OrbitScoutUiTheme.ApplyFont(detailText, title: false);
        }

        public void ShowResult(LevelRunResult result)
        {
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
                detailText.text = result.Summary + "\n" +
                    "Correct " + result.CorrectCount + "/" + result.TotalQuestions +
                    "  ·  Overall " + GameProgress.GetOverallScore();
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

        static Image CreateImage(Transform parent, string name, Sprite sprite, Vector2 anchor, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = sprite != null;
            image.raycastTarget = false;
            image.color = Color.white;
            return image;
        }

        static Image CreateImage(RectTransform parent, string name, Sprite sprite, Vector2 anchor, Vector2 size)
            => CreateImage((Transform)parent, name, sprite, anchor, size);

        static TMP_Text CreateLabel(Transform parent, string name, string text, float size, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = sizeDelta;
            TMP_Text tmp = go.GetComponent<TMP_Text>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            return tmp;
        }
    }
}
