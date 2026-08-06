using System.IO;
using OrbitScout.Core;
using OrbitScout.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class OrbitScoutHudEditorBuilder
{
    const string PrefabPath = "Assets/OrbitScout/UI/Prefabs/OrbitScoutHud.prefab";
    const string ResourcesPrefabPath = "Assets/Resources/OrbitScout/OrbitScoutHud.prefab";

    [MenuItem("Orbit Scout/Create Editable HUD In Scene")]
    public static void CreateEditableHudInScene()
    {
        EnsureFolders();
        GameObject prefabRoot = BuildHudHierarchy();
        OrbitScoutHudView view = prefabRoot.GetComponent<OrbitScoutHudView>();

        GameObject prefabAsset = SaveAsPrefab(prefabRoot, PrefabPath);
        AssetDatabase.CopyAsset(PrefabPath, ResourcesPrefabPath);
        AssetDatabase.SaveAssets();

        Object.DestroyImmediate(prefabRoot);

        GameObject orbitScout = GameObject.Find("OrbitScout");
        if (orbitScout == null)
        {
            EditorUtility.DisplayDialog(
                "Orbit Scout",
                "No OrbitScout object in this scene.\n\nOpen OrbitScout_EditorTest or run Setup AR first.",
                "OK");
            return;
        }

        OrbitScoutHudView existing = orbitScout.GetComponentInChildren<OrbitScoutHudView>(true);
        if (existing != null)
        {
            if (!EditorUtility.DisplayDialog(
                    "Orbit Scout",
                    "OrbitScoutHud already exists under OrbitScout.\nReplace it with a fresh prefab instance?",
                    "Replace",
                    "Cancel"))
                return;

            Object.DestroyImmediate(existing.gameObject);
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset, orbitScout.transform);
        instance.name = "OrbitScoutHud";

        MissionHud hud = orbitScout.GetComponent<MissionHud>();
        if (hud != null)
        {
            SerializedObject so = new SerializedObject(hud);
            so.FindProperty("hudView").objectReferenceValue = instance.GetComponent<OrbitScoutHudView>();
            so.FindProperty("hudPrefab").objectReferenceValue = prefabAsset;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = instance;

        EditorUtility.DisplayDialog(
            "Orbit Scout",
            "Editable HUD added under OrbitScout.\n\n" +
            "Expand OrbitScout → OrbitScoutHud in the Hierarchy.\n" +
            "Edit fonts, colors, and layout on MenuPanel and children.\n\n" +
            "Prefab saved to:\n" + PrefabPath,
            "OK");
    }

    [MenuItem("Orbit Scout/Open HUD Prefab For Editing")]
    public static void OpenHudPrefab()
    {
        EnsureFolders();
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (asset == null)
        {
            CreateEditableHudInScene();
            asset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        }

        if (asset != null)
            AssetDatabase.OpenAsset(asset);
    }

    static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/OrbitScout/UI/Prefabs"))
        {
            if (!AssetDatabase.IsValidFolder("Assets/OrbitScout/UI"))
                AssetDatabase.CreateFolder("Assets/OrbitScout", "UI");
            AssetDatabase.CreateFolder("Assets/OrbitScout/UI", "Prefabs");
        }

        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/OrbitScout"))
            AssetDatabase.CreateFolder("Assets/Resources", "OrbitScout");
    }

    static GameObject SaveAsPrefab(GameObject source, string path)
    {
        string dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(source, path);
        AssetDatabase.SaveAssets();
        return prefab;
    }

    static GameObject BuildHudHierarchy()
    {
        GameObject root = new GameObject("OrbitScoutHud", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(OrbitScoutHudView));
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        OrbitScoutHudView view = root.GetComponent<OrbitScoutHudView>();

        view.menuPanel = CreatePanel("MenuPanel", root.transform, fullScreenMenu: true);
        CreateLabel(view.menuPanel.transform, "Title", "Orbit Scout", 56, new Vector2(0.5f, 0.78f), out TMP_Text title);
        OrbitScoutUiTheme.StyleTitle(title);
        CreateLabel(view.menuPanel.transform, "Subtitle", "Educational AR · Solar System Quiz", 22, new Vector2(0.5f, 0.70f), out TMP_Text subtitle);
        OrbitScoutUiTheme.StyleSubtitle(subtitle);
        view.playButton = CreateButton(view.menuPanel.transform, "PlayButton", "Play", new Vector2(0.5f, 0.46f), primary: true);
        view.resetJourneyButton = CreateButton(view.menuPanel.transform, "ResetJourneyButton", "Reset Journey", new Vector2(0.5f, 0.36f), primary: false);
        OrbitScoutUiTheme.StyleMenuPillButton(view.playButton, primary: true);
        OrbitScoutUiTheme.StyleMenuPillButton(view.resetJourneyButton, primary: false);
        OrbitScoutMenuDecor.EnsureOnMenuPanel(view.menuPanel);

        view.menuScoresPanel = CreateMenuScoresPanel(root.transform);
        view.menuScoresText = CreateLabel(view.menuScoresPanel.transform, "MenuScores", "", 24, new Vector2(0.5f, 0.5f), out _);
        OrbitScoutUiTheme.StyleBody(view.menuScoresText);
        view.menuScoresText.alignment = TextAlignmentOptions.Center;

        view.levelSelectPanel = CreatePanel("LevelSelectPanel", root.transform, fullScreenMenu: true);
        OrbitScoutMenuBackground.ApplyLevelSelectBackground(view.levelSelectPanel);
        CreateLabel(view.levelSelectPanel.transform, "Title", "Choose Mission", 46, new Vector2(0.5f, 0.88f), out TMP_Text levelTitle);
        OrbitScoutUiTheme.StyleTitle(levelTitle);
        OrbitScoutLevelMapController.EnsureOnPanel(view.levelSelectPanel, replaceExisting: true);
        view.levelSelectBackButton = CreateButton(view.levelSelectPanel.transform, "BackButton", "Back", new Vector2(0.5f, 0.10f), primary: false);
        view.levelSelectStatusText = CreateLabel(view.levelSelectPanel.transform, "Status", "", 22, new Vector2(0.5f, 0.04f), out _);
        OrbitScoutUiTheme.StyleSubtitle(view.levelSelectStatusText);

        view.playPanel = CreatePanel("PlayPanel", root.transform, playHud: true);
        CreateHudCard(view.playPanel.transform, "QuestionCard", new Vector2(0.5f, 0.82f), new Vector2(920f, 280f));
        view.questionText = CreateLabel(view.playPanel.transform, "Question", "", 28, new Vector2(0.5f, 0.82f), out _);
        OrbitScoutUiTheme.StyleBody(view.questionText);
        view.scoreText = CreateLabel(view.playPanel.transform, "Score", "Score 0", 26, new Vector2(0.5f, 0.94f), out _);
        view.scoreText.color = OrbitScoutUiTheme.AccentCyan;
        view.timerText = CreateLabel(view.playPanel.transform, "Timer", "", 26, new Vector2(0.5f, 0.90f), out _);
        view.timerText.color = OrbitScoutUiTheme.AccentGold;
        view.streakText = CreateLabel(view.playPanel.transform, "Streak", "", 28, new Vector2(0.5f, 0.72f), out _);
        view.streakText.fontStyle = FontStyles.Bold;
        view.streakText.color = OrbitScoutUiTheme.AccentGold;
        CreateHudCard(view.playPanel.transform, "FeedbackCard", new Vector2(0.5f, 0.16f), new Vector2(920f, 140f));
        view.feedbackText = CreateLabel(view.playPanel.transform, "Feedback", "Tap a planet.", 24, new Vector2(0.5f, 0.16f), out _);
        OrbitScoutUiTheme.StyleBody(view.feedbackText);
        view.noneMatchButton = CreateButton(view.playPanel.transform, "NoneMatchButton", "No planet matches", new Vector2(0.5f, 0.06f), primary: true);
        view.restartButton = CreateButton(view.playPanel.transform, "RestartButton", "Restart", new Vector2(0.18f, 0.96f), primary: false);
        view.menuButton = CreateButton(view.playPanel.transform, "MenuButton", "Menu", new Vector2(0.82f, 0.96f), primary: false);
        view.noneMatchButton.gameObject.SetActive(false);

        view.endPanel = CreatePanel("EndPanel", root.transform, fullScreenMenu: true);
        view.endBodyText = CreateLabel(view.endPanel.transform, "Body", "", 26, new Vector2(0.5f, 0.50f), out _);
        view.endBodyText.gameObject.SetActive(false);
        view.retryLevelButton = CreateButton(view.endPanel.transform, "RetryButton", "Retry Level", new Vector2(0.5f, 0.14f), primary: true);
        view.endLevelSelectButton = CreateButton(view.endPanel.transform, "LevelSelectButton", "Level Select", new Vector2(0.5f, 0.08f), primary: false);
        view.endMainMenuButton = CreateButton(view.endPanel.transform, "MainMenuButton", "Main Menu", new Vector2(0.5f, 0.03f), primary: false);
        OrbitScoutResultsPanel.EnsureOnEndPanel(view.endPanel);
        OrbitScoutUiTheme.ApplyGlassButtons(view);

        view.levelSelectPanel.SetActive(false);
        view.playPanel.SetActive(false);
        view.endPanel.SetActive(false);

        return root;
    }

    static GameObject CreateMenuScoresPanel(Transform parent)
    {
        GameObject panel = new GameObject("MenuScoresPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.06f, 0.48f);
        rect.anchorMax = new Vector2(0.94f, 0.66f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = panel.GetComponent<Image>();
        image.color = OrbitScoutUiTheme.PanelSurface;
        image.raycastTarget = false;

        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = OrbitScoutUiTheme.PanelBorder;
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        return panel;
    }

    static GameObject CreatePanel(string name, Transform parent, bool fullScreenMenu = false, bool playHud = false)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        OrbitScoutUiTheme.ApplyPanelBackdrop(panel, fullScreenMenu: fullScreenMenu, playHud: playHud);
        return panel;
    }

    static void CreateHudCard(Transform parent, string name, Vector2 anchor, Vector2 size)
    {
        GameObject card = new GameObject(name, typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform rect = card.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.sizeDelta = size;
        Image image = card.GetComponent<Image>();
        image.color = new Color(0.07f, 0.1f, 0.18f, 0.78f);
        image.raycastTarget = false;
    }

    static TMP_Text CreateLabel(Transform parent, string name, string text, float size, Vector2 anchor, out TMP_Text tmp)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(920f, 200f);

        tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;
        return tmp;
    }

    static Button CreateButton(Transform parent, string name, string label, Vector2 anchor, bool primary)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.sizeDelta = new Vector2(anchor.y > 0.9f ? 200f : 520f, 72f);

        Button button = go.GetComponent<Button>();
        OrbitScoutUiTheme.StyleButton(button, primary);
        CreateLabel(go.transform, "Label", label, 26, new Vector2(0.5f, 0.5f), out TMP_Text labelTmp);
        labelTmp.color = OrbitScoutUiTheme.TextPrimary;
        labelTmp.fontStyle = primary ? FontStyles.Bold : FontStyles.Normal;
        return button;
    }

    static void CreateLevelCard(Transform parent, string name, LevelId level, string numeral, string title, string desc, float anchorY)
    {
        GameObject card = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(OrbitScoutLevelCardButton));
        card.transform.SetParent(parent, false);
        RectTransform rect = card.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, anchorY);
        rect.anchorMax = new Vector2(0.5f, anchorY);
        rect.sizeDelta = new Vector2(880f, 96f);

        card.GetComponent<OrbitScoutLevelCardButton>().level = level;
        Image bg = card.GetComponent<Image>();
        bg.color = OrbitScoutUiTheme.PanelSurface;
        Outline outline = card.AddComponent<Outline>();
        outline.effectColor = OrbitScoutUiTheme.PanelBorder;
        outline.effectDistance = new Vector2(1.5f, -1.5f);
        OrbitScoutUiTheme.StyleButton(card.GetComponent<Button>(), false);

        CreateLabel(card.transform, "Numeral", numeral, 34, new Vector2(0.12f, 0.5f), out TMP_Text num);
        num.color = OrbitScoutUiTheme.AccentCyan;
        num.fontStyle = FontStyles.Bold;
        CreateLabel(card.transform, "Title", title, 28, new Vector2(0.52f, 0.62f), out TMP_Text titleText);
        titleText.color = OrbitScoutUiTheme.TextPrimary;
        titleText.alignment = TextAlignmentOptions.Left;
        titleText.rectTransform.sizeDelta = new Vector2(520f, 80f);
        CreateLabel(card.transform, "Description", desc, 22, new Vector2(0.52f, 0.38f), out TMP_Text descText);
        descText.color = OrbitScoutUiTheme.TextMuted;
        descText.alignment = TextAlignmentOptions.Left;
        descText.rectTransform.sizeDelta = new Vector2(520f, 60f);
    }
}
