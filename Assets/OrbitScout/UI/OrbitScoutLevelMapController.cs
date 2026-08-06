using OrbitScout.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OrbitScout.UI
{
    /// <summary>
    /// Builds / refreshes the zig-zag mission map on LevelSelectPanel.
    /// Hierarchy: LevelSelectPanel → LevelMap → Path_*, Node_*, HoverPopup
    /// </summary>
    public sealed class OrbitScoutLevelMapController : MonoBehaviour
    {
        public const string MapRootName = "LevelMap";
        public const string StarfieldResourcePath = "OrbitScout/LevelSelect_starfield";
        public const string StarfieldAssetPath = "Assets/OrbitScout/UI/Visuals/LevelSelect_starfield.png";

        static readonly Vector2[] NodeAnchors =
        {
            new Vector2(0.50f, 0.24f), // I
            new Vector2(0.28f, 0.40f), // II
            new Vector2(0.72f, 0.54f), // III
            new Vector2(0.42f, 0.70f), // IV
        };

        static readonly LevelId[] Levels =
        {
            LevelId.Level1,
            LevelId.Level2,
            LevelId.Level3,
            LevelId.Level4,
        };

        [SerializeField] RectTransform mapRoot;
        [SerializeField] RectTransform hoverPopup;
        [SerializeField] TMP_Text hoverTitle;
        [SerializeField] TMP_Text hoverNumeral;
        [SerializeField] OrbitScoutLevelMapNode[] nodes;
        [SerializeField] Image[] pathSegments;

        OrbitScoutLevelMapNode hovered;
        float hideAt = -1f;

        public static OrbitScoutLevelMapController EnsureOnPanel(GameObject levelSelectPanel, bool replaceExisting = false)
        {
            if (levelSelectPanel == null)
                return null;

            ApplyStarfieldBackground(levelSelectPanel);

            // Hide legacy list cards so only the map is used
            foreach (Transform child in levelSelectPanel.transform)
            {
                if (child.name.StartsWith("LevelCard_"))
                    child.gameObject.SetActive(false);
            }

            Transform existing = levelSelectPanel.transform.Find(MapRootName);
            if (existing != null)
            {
                bool needsStarUpgrade = existing.Find("Node_I/Star") == null;
                if (!replaceExisting && !needsStarUpgrade)
                {
                    OrbitScoutLevelMapController ctrl = existing.GetComponent<OrbitScoutLevelMapController>();
                    if (ctrl != null)
                    {
                        ApplyStarfieldBackground(levelSelectPanel);
                        ctrl.RefreshLocks();
                        return ctrl;
                    }
                }

                // Immediate destroy so a fresh star map can be created this frame
                Object.DestroyImmediate(existing.gameObject);
            }

            GameObject root = new GameObject(MapRootName, typeof(RectTransform), typeof(OrbitScoutLevelMapController));
            root.transform.SetParent(levelSelectPanel.transform, false);
            // Above background, below title/back if those are later siblings — put after bg, before title ideally
            root.transform.SetSiblingIndex(1);

            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            OrbitScoutLevelMapController map = root.GetComponent<OrbitScoutLevelMapController>();
            map.Build(rootRect);
            map.RefreshLocks();
            return map;
        }

        public static void ApplyStarfieldBackground(GameObject panel)
        {
            if (panel == null)
                return;

            Sprite sprite = Resources.Load<Sprite>(StarfieldResourcePath);
#if UNITY_EDITOR
            if (sprite == null)
                sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(StarfieldAssetPath);
#endif
            Image image = panel.GetComponent<Image>();
            if (image == null)
                image = panel.AddComponent<Image>();

            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = Image.Type.Simple;
                image.preserveAspect = false;
            }

            image.color = Color.white;
            image.raycastTarget = false;
        }

        void Build(RectTransform root)
        {
            mapRoot = root;
            Sprite star = Resources.Load<Sprite>("OrbitScout/LevelNode_star");
            Sprite lockSprite = Resources.Load<Sprite>("OrbitScout/LevelNode_lock");
            Sprite pathDot = Resources.Load<Sprite>("OrbitScout/LevelPath_dot");
#if UNITY_EDITOR
            if (star == null)
                star = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/OrbitScout/UI/Visuals/LevelNode_star.png");
            if (lockSprite == null)
                lockSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/OrbitScout/UI/Visuals/LevelNode_lock.png");
            if (pathDot == null)
                pathDot = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/OrbitScout/UI/Visuals/LevelPath_dot.png");
#endif

            nodes = new OrbitScoutLevelMapNode[Levels.Length];
            pathSegments = new Image[Levels.Length - 1];

            // Path behind nodes
            for (int i = 0; i < Levels.Length - 1; i++)
            {
                GameObject segGo = new GameObject("Path_" + (i + 1), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                segGo.transform.SetParent(root, false);
                Image seg = segGo.GetComponent<Image>();
                seg.sprite = pathDot;
                seg.type = Image.Type.Sliced;
                seg.color = new Color(0.72f, 0.48f, 0.98f, 0.85f);
                seg.raycastTarget = false;
                pathSegments[i] = seg;
            }

            for (int i = 0; i < Levels.Length; i++)
            {
                nodes[i] = CreateNode(root, Levels[i], NodeAnchors[i], star, lockSprite);
            }

            CreateHoverPopup(root);
            Canvas.ForceUpdateCanvases();
            LayoutPathSegments();
        }

        OrbitScoutLevelMapNode CreateNode(
            RectTransform parent,
            LevelId level,
            Vector2 anchor,
            Sprite star,
            Sprite lockSprite)
        {
            GameObject go = new GameObject(
                "Node_" + OrbitScoutLevelBriefings.RomanNumeral(level),
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button),
                typeof(OrbitScoutLevelMapNode));
            go.transform.SetParent(parent, false);

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(130f, 130f);
            rect.anchoredPosition = Vector2.zero;

            Image hit = go.GetComponent<Image>();
            hit.sprite = star;
            hit.color = new Color(1f, 1f, 1f, 0.01f);
            hit.raycastTarget = true;
            hit.preserveAspect = true;

            Button button = go.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.95f, 1f, 1f);
            colors.pressedColor = new Color(0.85f, 0.75f, 1f, 1f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            button.targetGraphic = hit;
            button.transition = Selectable.Transition.ColorTint;

            // Soft glow star behind main star
            Image glow = CreateChildImage(go.transform, "Glow", star, new Vector2(150f, 150f));
            glow.color = new Color(0.85f, 0.70f, 1f, 0.35f);

            Image fill = CreateChildImage(go.transform, "Star", star, new Vector2(118f, 118f));
            Image lockImg = CreateChildImage(go.transform, "Lock", lockSprite, new Vector2(40f, 40f));
            lockImg.rectTransform.anchoredPosition = new Vector2(0f, -8f);

            TMP_Text numeral = CreateChildLabel(go.transform, "Numeral", OrbitScoutLevelBriefings.RomanNumeral(level), 28f);
            numeral.fontStyle = FontStyles.Bold;
            numeral.rectTransform.anchoredPosition = new Vector2(0f, -52f);
            OrbitScoutUiTheme.ApplyFont(numeral, title: true);

            OrbitScoutLevelMapNode node = go.GetComponent<OrbitScoutLevelMapNode>();
            node.Bind(this, level, fill, glow, lockImg, numeral);
            return node;
        }

        static Image CreateChildImage(Transform parent, string name, Sprite sprite, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.color = Color.white;
            return image;
        }

        static TMP_Text CreateChildLabel(Transform parent, string name, string text, float size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(100f, 80f);
            TMP_Text tmp = go.GetComponent<TMP_Text>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            tmp.color = Color.white;
            return tmp;
        }

        void CreateHoverPopup(RectTransform parent)
        {
            GameObject popup = new GameObject("HoverPopup", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            popup.transform.SetParent(parent, false);
            hoverPopup = popup.GetComponent<RectTransform>();
            hoverPopup.anchorMin = hoverPopup.anchorMax = new Vector2(0.5f, 0.5f);
            hoverPopup.sizeDelta = new Vector2(340f, 86f);
            hoverPopup.gameObject.SetActive(false);

            Image bg = popup.GetComponent<Image>();
            bg.color = new Color(0.18f, 0.10f, 0.32f, 0.92f);
            bg.raycastTarget = false;

            Outline outline = popup.AddComponent<Outline>();
            outline.effectColor = new Color(0.78f, 0.55f, 1f, 0.9f);
            outline.effectDistance = new Vector2(2f, -2f);

            hoverNumeral = CreateChildLabel(popup.transform, "Numeral", "I", 22f);
            hoverNumeral.rectTransform.anchoredPosition = new Vector2(0f, 18f);
            hoverNumeral.color = new Color(0.85f, 0.70f, 1f, 1f);
            hoverNumeral.fontStyle = FontStyles.Bold;
            OrbitScoutUiTheme.ApplyFont(hoverNumeral, title: true);

            hoverTitle = CreateChildLabel(popup.transform, "Title", "First Orbit", 26f);
            hoverTitle.rectTransform.anchoredPosition = new Vector2(0f, -12f);
            hoverTitle.rectTransform.sizeDelta = new Vector2(320f, 40f);
            hoverTitle.color = Color.white;
            hoverTitle.fontStyle = FontStyles.Bold;
            OrbitScoutUiTheme.ApplyFont(hoverTitle, title: true);
        }

        void LateUpdate()
        {
            if (hideAt > 0f && Time.unscaledTime >= hideAt && hovered == null)
            {
                hideAt = -1f;
                if (hoverPopup != null)
                    hoverPopup.gameObject.SetActive(false);
            }
        }

        public void ShowHover(OrbitScoutLevelMapNode node)
        {
            if (node == null || hoverPopup == null)
                return;

            hovered = node;
            hideAt = -1f;

            hoverPopup.gameObject.SetActive(true);
            if (hoverNumeral != null)
                hoverNumeral.text = "Mission " + OrbitScoutLevelBriefings.RomanNumeral(node.level);
            if (hoverTitle != null)
            {
                hoverTitle.text = node.IsUnlocked
                    ? OrbitScoutLevelBriefings.ShortTitle(node.level)
                    : "Locked · pass Mission " + PreviousRoman(node.level);
            }

            RectTransform nodeRect = node.GetComponent<RectTransform>();
            Vector2 local = WorldCenterToLocal(mapRoot, nodeRect);
            hoverPopup.anchoredPosition = local + new Vector2(0f, 92f);
            hoverPopup.SetAsLastSibling();
        }

        public void HideHover(OrbitScoutLevelMapNode node)
        {
            if (hovered != node)
                return;
            hovered = null;
            hideAt = Time.unscaledTime + 0.35f;
        }

        static string PreviousRoman(LevelId level)
        {
            switch (level)
            {
                case LevelId.Level2: return "I";
                case LevelId.Level3: return "II";
                case LevelId.Level4: return "III";
                default: return "I";
            }
        }

        public void RefreshLocks()
        {
            if (nodes == null)
                return;

            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] == null)
                    continue;

                bool unlocked = GameProgress.IsLevelUnlocked(nodes[i].level);
                nodes[i].SetUnlocked(unlocked);

                if (i > 0 && pathSegments != null && i - 1 < pathSegments.Length && pathSegments[i - 1] != null)
                {
                    pathSegments[i - 1].color = unlocked
                        ? new Color(0.78f, 0.55f, 1f, 0.95f)
                        : new Color(0.35f, 0.28f, 0.45f, 0.45f);
                }
            }

            LayoutPathSegments();
        }

        void LayoutPathSegments()
        {
            if (mapRoot == null || nodes == null || pathSegments == null)
                return;

            Canvas.ForceUpdateCanvases();

            for (int i = 0; i < pathSegments.Length; i++)
            {
                if (pathSegments[i] == null || nodes[i] == null || nodes[i + 1] == null)
                    continue;

                RectTransform aRect = nodes[i].GetComponent<RectTransform>();
                RectTransform bRect = nodes[i + 1].GetComponent<RectTransform>();
                Vector2 a = WorldCenterToLocal(mapRoot, aRect);
                Vector2 b = WorldCenterToLocal(mapRoot, bRect);
                Vector2 delta = b - a;
                float dist = delta.magnitude;
                float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

                RectTransform seg = pathSegments[i].rectTransform;
                seg.anchorMin = seg.anchorMax = new Vector2(0.5f, 0.5f);
                seg.pivot = new Vector2(0.5f, 0.5f);
                seg.sizeDelta = new Vector2(Mathf.Max(40f, dist - 70f), 18f);
                seg.anchoredPosition = (a + b) * 0.5f;
                seg.localRotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        static Vector2 WorldCenterToLocal(RectTransform parent, RectTransform child)
        {
            Vector3 world = child.TransformPoint(child.rect.center);
            Vector3 local = parent.InverseTransformPoint(world);
            return new Vector2(local.x, local.y);
        }

        static Vector2 AnchorToLocal(RectTransform parent, Vector2 anchor)
        {
            Rect r = parent.rect;
            return new Vector2((anchor.x - 0.5f) * r.width, (anchor.y - 0.5f) * r.height);
        }

        void OnRectTransformDimensionsChange()
        {
            LayoutPathSegments();
        }
    }
}
