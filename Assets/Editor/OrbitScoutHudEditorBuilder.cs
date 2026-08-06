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
        if (!EnsureHudInScene(replaceExisting: true, selectHud: true))
            return;

        EditorUtility.DisplayDialog(
            "Orbit Scout",
            "HUD is in the Hierarchy under OrbitScout → OrbitScoutHud.\n\n" +
            "Edit like any UI: select panels, change Image Source Image, fonts, layout.\n" +
            "Save the scene when done.\n\n" +
            "Prefab: " + PrefabPath,
            "OK");
    }

    /// <summary>
    /// Ensures OrbitScoutHud exists as a saved prefab instance in the scene (normal Unity editing).
    /// </summary>
    public static bool EnsureHudInScene(bool replaceExisting, bool selectHud)
    {
        EnsureFolders();

        GameObject orbitScout = GameObject.Find("OrbitScout");
        if (orbitScout == null)
        {
            EditorUtility.DisplayDialog(
                "Orbit Scout",
                "No OrbitScout object in this scene.\nRun Create Editor Test Scene or Setup AR first.",
                "OK");
            return false;
        }

        OrbitScoutHudView existing = orbitScout.GetComponentInChildren<OrbitScoutHudView>(true);
        if (existing != null)
        {
            if (!replaceExisting)
            {
                OrbitScoutUiEditSceneOrganizer.OrganizeHudInHierarchy(existing);
                UpgradeHudCanvasForSceneEditing(existing.gameObject);
                OrbitScoutEditorSceneBuilder.EnsureMainCameraForUiEditing();
                existing.GetComponent<OrbitScoutHudCanvasDriver>()?.ApplyEditModePresentation();
                return true;
            }

            if (!EditorUtility.DisplayDialog(
                    "Orbit Scout",
                    "Replace existing OrbitScoutHud in the scene?",
                    "Replace",
                    "Cancel"))
                return false;

            Object.DestroyImmediate(existing.gameObject);
        }

        GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefabAsset == null)
        {
            GameObject prefabRoot = BuildHudHierarchy();
            OrbitScoutMenuBackgroundEditor.BakeSpritesOnView(prefabRoot.GetComponent<OrbitScoutHudView>());
            prefabAsset = SaveAsPrefab(prefabRoot, PrefabPath);
            AssetDatabase.CopyAsset(PrefabPath, ResourcesPrefabPath);
            Object.DestroyImmediate(prefabRoot);
            AssetDatabase.SaveAssets();
        }
        else
        {
            UpgradeHudPrefabAsset();
            prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
        instance.name = "OrbitScoutHud";
        OrbitScoutUiEditSceneOrganizer.OrganizeHudInHierarchy(instance.GetComponent<OrbitScoutHudView>());

        UpgradeHudCanvasForSceneEditing(instance);
        OrbitScoutMenuBackgroundEditor.BakeSpritesOnView(instance.GetComponent<OrbitScoutHudView>());
        instance.GetComponent<OrbitScoutHudCanvasDriver>()?.ApplyEditModePresentation();

        MissionHud hud = orbitScout.GetComponent<MissionHud>();
        if (hud != null)
        {
            SerializedObject so = new SerializedObject(hud);
            so.FindProperty("hudView").objectReferenceValue = instance.GetComponent<OrbitScoutHudView>();
            so.FindProperty("hudPrefab").objectReferenceValue = prefabAsset;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        if (selectHud)
            Selection.activeGameObject = instance;

        return true;
    }

    [MenuItem("Orbit Scout/Prepare Scene For UI Editing")]
    public static void PrepareSceneForUiEditing()
    {
        OrbitScoutUiEditSceneOrganizer.PrepareSceneForUiHierarchyEditing(frameMenu: true);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        string activePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
        if (activePath == OrbitScoutGameSceneSync.SampleScenePath
            || activePath == OrbitScoutGameSceneSync.EditorTestScenePath)
            OrbitScoutGameSceneSync.SyncOtherGameSceneFrom(activePath);

        EditorUtility.DisplayDialog(
            "Orbit Scout",
            "UI editing layout applied:\n\n" +
            "• " + OrbitScoutUiEditSceneOrganizer.UiRootName + " in Hierarchy → OrbitScoutHud\n" +
            "• Orbit Scout → UI Editing to jump to each screen\n\n" +
            "Select MenuPanel → Image to change the background.",
            "OK");
    }

    internal static void UpgradeHudCanvasForSceneEditing(GameObject hudRoot)
    {
        if (hudRoot == null)
            return;

        OrbitScoutHudCanvasDriver driver = hudRoot.GetComponent<OrbitScoutHudCanvasDriver>();
        if (driver == null)
            driver = hudRoot.AddComponent<OrbitScoutHudCanvasDriver>();

        driver.ApplyEditModePresentation();
        EditorUtility.SetDirty(hudRoot);
    }

    internal static void UpgradeHudPrefabAsset()
    {
        GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefabRoot == null)
            return;

        string path = AssetDatabase.GetAssetPath(prefabRoot);
        GameObject contents = PrefabUtility.LoadPrefabContents(path);
        if (contents.GetComponent<OrbitScoutHudCanvasDriver>() == null)
            contents.AddComponent<OrbitScoutHudCanvasDriver>();
        OrbitScoutMenuBackgroundEditor.BakeSpritesOnView(contents.GetComponent<OrbitScoutHudView>());
        PrefabUtility.SaveAsPrefabAsset(contents, path);
        PrefabUtility.UnloadPrefabContents(contents);
        AssetDatabase.CopyAsset(PrefabPath, ResourcesPrefabPath);
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
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.planeDistance = 100f;
        canvas.sortingOrder = 200;

        root.AddComponent<OrbitScoutHudCanvasDriver>();

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

        view.briefingPanel = CreatePanel("BriefingPanel", root.transform, fullScreenMenu: true);
        view.briefingTitleText = CreateLabel(view.briefingPanel.transform, "BriefingTitle", "Mission Briefing", 32, new Vector2(0.5f, 0.82f), out _);
        view.briefingBodyText = CreateLabel(view.briefingPanel.transform, "BriefingBody", "", 22, new Vector2(0.5f, 0.52f), out _);
        view.briefingBodyText.alignment = TextAlignmentOptions.TopLeft;
        view.briefingBodyText.rectTransform.sizeDelta = new Vector2(400f, 340f);
        view.briefingStartButton = CreateButton(view.briefingPanel.transform, "BriefingStart", "Start Mission", new Vector2(0.5f, 0.14f), primary: true);
        view.briefingBackButton = CreateButton(view.briefingPanel.transform, "BriefingBack", "Back", new Vector2(0.5f, 0.05f), primary: false);
        OrbitScoutWalkthroughUi.EnsureOnBriefingPanel(view.briefingPanel, view.briefingTitleText, view.briefingBodyText);

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
        view.feedbackText = CreateLabel(view.playPanel.transform, "Feedback", "", 24, new Vector2(0.5f, 0.16f), out _);
        OrbitScoutUiTheme.StylePlayHudTexts(view.streakText, view.questionText, view.scoreText, view.timerText, view.feedbackText);
        view.noneMatchButton = CreateButton(view.playPanel.transform, "NoneMatchButton", "No planet matches", new Vector2(0.5f, 0.06f), primary: true);
        view.restartButton = CreateButton(view.playPanel.transform, "RestartButton", "Restart", new Vector2(0.18f, 0.96f), primary: false);
        view.menuButton = CreateButton(view.playPanel.transform, "MenuButton", "Menu", new Vector2(0.82f, 0.96f), primary: false);
        view.noneMatchButton.gameObject.SetActive(false);

        view.endPanel = CreatePanel("EndPanel", root.transform, fullScreenMenu: true);
        view.endBodyText = CreateLabel(view.endPanel.transform, "Body", "", 26, new Vector2(0.5f, 0.52f), out _);
        view.endBodyText.gameObject.SetActive(false);
        view.continueNextButton = CreateButton(view.endPanel.transform, "ContinueNextButton", "Continue to Next Level", new Vector2(0.5f, 0.22f), primary: true);
        view.retryLevelButton = CreateButton(view.endPanel.transform, "RetryButton", "Retry Level", new Vector2(0.5f, 0.14f), primary: true);
        view.endLevelSelectButton = CreateButton(view.endPanel.transform, "LevelSelectButton", "Level Select", new Vector2(0.5f, 0.08f), primary: false);
        view.endMainMenuButton = CreateButton(view.endPanel.transform, "MainMenuButton", "Main Menu", new Vector2(0.5f, 0.03f), primary: false);
        OrbitScoutResultsPanel.EnsureOnEndPanel(view.endPanel);
        OrbitScoutUiTheme.ApplyGlassButtons(view);

        OrbitScoutMenuBackgroundEditor.BakeSpritesOnView(view);

        view.levelSelectPanel.SetActive(false);
        view.briefingPanel.SetActive(false);
        view.playPanel.SetActive(false);
        view.endPanel.SetActive(false);
        view.continueNextButton.gameObject.SetActive(false);

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
