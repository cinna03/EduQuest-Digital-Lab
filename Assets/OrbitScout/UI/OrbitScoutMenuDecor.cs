using UnityEngine;
using UnityEngine.UI;

namespace OrbitScout.UI
{
    /// <summary>
    /// Places looping sticker decorations on the start menu for playful 2D flair.
    /// Created objects live under MenuPanel → MenuDecor so you can edit them in the Hierarchy.
    /// </summary>
    public static class OrbitScoutMenuDecor
    {
        public const string RootName = "MenuDecor";
        const string FlowerPath = "OrbitScout/Decor/Flower";
        const string SparklePath = "OrbitScout/Decor/Sparkle";
        const string OrbPath = "OrbitScout/Decor/Sticker_Orb";
        const string StarsPath = "OrbitScout/Decor/Sticker_Stars";

        /// <summary>
        /// Ensures MenuDecor exists under the menu panel. Returns the root object.
        /// </summary>
        public static GameObject EnsureOnMenuPanel(GameObject menuPanel, bool replaceExisting = false)
        {
            if (menuPanel == null)
                return null;

            Transform existing = menuPanel.transform.Find(RootName);
            if (existing != null)
            {
                if (!replaceExisting)
                {
                    existing.SetAsFirstSibling();
                    return existing.gameObject;
                }

                Object.DestroyImmediate(existing.gameObject);
            }

            GameObject root = new GameObject(RootName, typeof(RectTransform));
            root.transform.SetParent(menuPanel.transform, false);
            root.transform.SetAsFirstSibling();

            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            CreateDecor(root.transform, "Orb", LoadSingle(OrbPath), new Vector2(0.72f, 0.86f),
                new Vector2(220f, 220f), new Color(1f, 1f, 1f, 0.85f), fps: 1f, bob: 6f, pulse: 0.04f);

            CreateDecor(root.transform, "Sparkle", LoadSequence(SparklePath), new Vector2(0.18f, 0.84f),
                new Vector2(160f, 160f), Color.white, fps: 18f, bob: 4f, pulse: 0.03f);

            CreateDecor(root.transform, "Sparkle_2", LoadSequence(SparklePath), new Vector2(0.88f, 0.40f),
                new Vector2(130f, 130f), new Color(1f, 1f, 1f, 0.9f), fps: 18f, bob: 5f, pulse: 0.035f);

            CreateDecor(root.transform, "Flower", LoadSequence(FlowerPath), new Vector2(0.86f, 0.78f),
                new Vector2(120f, 120f), Color.white, fps: 2f, bob: 8f, pulse: 0.05f);

            CreateDecor(root.transform, "Stars", LoadSingle(StarsPath), new Vector2(0.14f, 0.42f),
                new Vector2(140f, 130f), new Color(1f, 1f, 1f, 0.95f), fps: 1f, bob: 7f, pulse: 0.045f);

            return root;
        }

        static Sprite[] LoadSequence(string resourcesFolder)
        {
            Sprite[] loaded = Resources.LoadAll<Sprite>(resourcesFolder);
            if (loaded == null || loaded.Length == 0)
                return System.Array.Empty<Sprite>();

            System.Array.Sort(loaded, (a, b) => string.CompareOrdinal(a.name, b.name));
            return loaded;
        }

        static Sprite[] LoadSingle(string resourcesPath)
        {
            Sprite sprite = Resources.Load<Sprite>(resourcesPath);
            return sprite != null ? new[] { sprite } : System.Array.Empty<Sprite>();
        }

        static void CreateDecor(
            Transform parent,
            string name,
            Sprite[] frames,
            Vector2 anchor,
            Vector2 size,
            Color color,
            float fps,
            float bob,
            float pulse)
        {
            if (frames == null || frames.Length == 0 || frames[0] == null)
                return;

            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(OrbitScoutUiSpriteAnimation));
            go.transform.SetParent(parent, false);

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;

            Image image = go.GetComponent<Image>();
            image.sprite = frames[0];
            image.color = color;
            image.raycastTarget = false;
            image.preserveAspect = true;

            OrbitScoutUiSpriteAnimation anim = go.GetComponent<OrbitScoutUiSpriteAnimation>();
            anim.Configure(frames, fps, bob, pulse);
        }
    }
}
