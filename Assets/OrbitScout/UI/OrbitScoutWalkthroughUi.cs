using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OrbitScout.UI
{
    /// <summary>
    /// Styles briefing / walkthrough panels: starfield background + sticky-note text holder.
    /// </summary>
    public static class OrbitScoutWalkthroughUi
    {
        public const string NoteRootName = "InstructionNote";
        public const string BgResourcePath = "OrbitScout/Walkthrough_starfield";
        public const string BgAssetPath = "Assets/OrbitScout/UI/Visuals/Walkthrough_starfield.png";
        public const string NoteResourcePath = "OrbitScout/StickyNote";
        public const string NoteAssetPath = "Assets/OrbitScout/UI/Visuals/StickyNote.png";

        public static void EnsureOnBriefingPanel(GameObject briefingPanel, TMP_Text title, TMP_Text body)
        {
            if (briefingPanel == null)
                return;

            ApplyBackground(briefingPanel);

            Transform existing = briefingPanel.transform.Find(NoteRootName);
            if (existing == null)
                existing = CreateNoteRoot(briefingPanel.transform).transform;

            // Place title + body onto the note so instructions sit on the sticky paper
            if (title != null)
            {
                title.transform.SetParent(existing, false);
                RectTransform tr = title.rectTransform;
                tr.anchorMin = tr.anchorMax = new Vector2(0.5f, 0.78f);
                tr.anchoredPosition = Vector2.zero;
                tr.sizeDelta = new Vector2(420f, 90f);
                title.color = new Color(0.35f, 0.18f, 0.55f, 1f);
                title.fontStyle = FontStyles.Bold;
                title.alignment = TextAlignmentOptions.Center;
                title.enableAutoSizing = false;
                if (title.fontSize < 28f)
                    title.fontSize = 32f;
            }

            if (body != null)
            {
                body.transform.SetParent(existing, false);
                RectTransform br = body.rectTransform;
                br.anchorMin = br.anchorMax = new Vector2(0.5f, 0.42f);
                br.anchoredPosition = Vector2.zero;
                br.sizeDelta = new Vector2(400f, 340f);
                if (body.fontSize > 26f || body.fontSize < 18f)
                    body.fontSize = 22f;
            }

            OrbitScoutUiTheme.StyleWalkthroughTexts(title, body);

            existing.SetAsFirstSibling();
            // Keep note above bg; buttons should stay as later siblings
            existing.SetSiblingIndex(1);
        }

        public static void ApplyBackground(GameObject panel)
        {
            if (panel == null)
                return;

            Sprite sprite = Resources.Load<Sprite>(BgResourcePath);
#if UNITY_EDITOR
            if (sprite == null)
                sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(BgAssetPath);
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

        static GameObject CreateNoteRoot(Transform parent)
        {
            Sprite noteSprite = Resources.Load<Sprite>(NoteResourcePath);
#if UNITY_EDITOR
            if (noteSprite == null)
                noteSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(NoteAssetPath);
#endif
            GameObject note = new GameObject(NoteRootName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            note.transform.SetParent(parent, false);
            RectTransform rect = note.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.55f);
            rect.sizeDelta = new Vector2(620f, 660f);
            rect.anchoredPosition = Vector2.zero;
            rect.localEulerAngles = new Vector3(0f, 0f, -3f);

            Image image = note.GetComponent<Image>();
            image.sprite = noteSprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.color = Color.white;
            return note;
        }
    }
}
