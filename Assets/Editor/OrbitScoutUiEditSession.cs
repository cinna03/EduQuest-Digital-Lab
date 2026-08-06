using OrbitScout.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class OrbitScoutUiEditSession
{
    public enum UiPanel
    {
        HudRoot,
        MainMenu,
        LevelSelect,
        PlayHud,
        EndScreen
    }

    static OrbitScoutHudView ActiveView =>
        Object.FindAnyObjectByType<OrbitScoutHudView>(FindObjectsInactive.Include);

    [MenuItem("Orbit Scout/UI Editing/Prepare Scene For UI Hierarchy", false, 0)]
    public static void MenuPrepareScene()
    {
        OrbitScoutUiEditSceneOrganizer.PrepareSceneForUiHierarchyEditing(frameMenu: true);
        EditorSceneManager.SaveOpenScenes();

        string path = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
        if (path == OrbitScoutGameSceneSync.SampleScenePath
            || path == OrbitScoutGameSceneSync.EditorTestScenePath)
            OrbitScoutGameSceneSync.SyncOtherGameSceneFrom(path);

        EditorUtility.DisplayDialog(
            "Orbit Scout",
            "Hierarchy ready:\n\n" +
            "• " + OrbitScoutUiEditSceneOrganizer.UiRootName + " → OrbitScoutHud\n" +
            "• EventSystem added if missing\n" +
            "• Use UI Editing menu to jump to each panel",
            "OK");
    }

    [MenuItem("Orbit Scout/UI Editing/Select HUD Root", false, 20)]
    public static void MenuSelectHud() => FocusPanel(ActiveView, UiPanel.HudRoot);

    [MenuItem("Orbit Scout/UI Editing/Edit Main Menu", false, 21)]
    public static void MenuEditMainMenu() => FocusPanel(ActiveView, UiPanel.MainMenu);

    [MenuItem("Orbit Scout/UI Editing/Edit Level Select", false, 22)]
    public static void MenuEditLevelSelect() => FocusPanel(ActiveView, UiPanel.LevelSelect);

    [MenuItem("Orbit Scout/UI Editing/Edit Play HUD", false, 23)]
    public static void MenuEditPlayHud() => FocusPanel(ActiveView, UiPanel.PlayHud);

    [MenuItem("Orbit Scout/UI Editing/Edit End Screen", false, 24)]
    public static void MenuEditEndScreen() => FocusPanel(ActiveView, UiPanel.EndScreen);

    [MenuItem("Orbit Scout/UI Editing/Show All Panels", false, 40)]
    public static void MenuShowAll()
    {
        OrbitScoutHudView view = ActiveView;
        if (view != null)
            ShowAllPanels(view);
    }

    [MenuItem("Orbit Scout/UI Editing/Frame HUD In Scene View", false, 41)]
    public static void MenuFrameHud()
    {
        OrbitScoutHudView view = ActiveView;
        if (view != null)
            OrbitScoutHudEditModeLayout.FrameHudForEditing(view);
    }

    public static void FocusPanel(OrbitScoutHudView view, UiPanel panel)
    {
        if (view == null)
        {
            EditorUtility.DisplayDialog("Orbit Scout", "No OrbitScoutHud in this scene.", "OK");
            return;
        }

        OrbitScoutUiEditSceneOrganizer.OrganizeHudInHierarchy(view);
        view.GetComponent<OrbitScoutHudCanvasDriver>()?.ApplyEditModePresentation();

        SetPanelVisibility(view, panel);
        GameObject target = ResolveTarget(view, panel);
        if (target == null)
            return;

        Selection.activeGameObject = target;
        EditorGUIUtility.PingObject(target);
        OrbitScoutHudEditModeLayout.FrameHudForEditing(view);

        EditorSceneManager.MarkSceneDirty(view.gameObject.scene);
    }

    public static void ShowAllPanels(OrbitScoutHudView view)
    {
        if (view == null)
            return;

        SetActive(view.menuPanel, true);
        SetActive(view.menuScoresPanel, true);
        SetActive(view.levelSelectPanel, true);
        SetActive(view.playPanel, true);
        SetActive(view.endPanel, true);
        EditorUtility.SetDirty(view.gameObject);
    }

    static void SetPanelVisibility(OrbitScoutHudView view, UiPanel panel)
    {
        bool menu = panel == UiPanel.MainMenu || panel == UiPanel.HudRoot;
        bool level = panel == UiPanel.LevelSelect;
        bool play = panel == UiPanel.PlayHud;
        bool end = panel == UiPanel.EndScreen;
        bool all = panel == UiPanel.HudRoot;

        SetActive(view.menuPanel, all || menu);
        SetActive(view.menuScoresPanel, all || menu);
        SetActive(view.levelSelectPanel, all || level);
        SetActive(view.playPanel, all || play);
        SetActive(view.endPanel, all || end);
    }

    static GameObject ResolveTarget(OrbitScoutHudView view, UiPanel panel)
    {
        switch (panel)
        {
            case UiPanel.HudRoot: return view.gameObject;
            case UiPanel.MainMenu: return view.menuPanel != null ? view.menuPanel : view.gameObject;
            case UiPanel.LevelSelect: return view.levelSelectPanel != null ? view.levelSelectPanel : view.gameObject;
            case UiPanel.PlayHud: return view.playPanel != null ? view.playPanel : view.gameObject;
            case UiPanel.EndScreen: return view.endPanel != null ? view.endPanel : view.gameObject;
            default: return view.gameObject;
        }
    }

    static void SetActive(GameObject go, bool active)
    {
        if (go != null && go.activeSelf != active)
            go.SetActive(active);
    }
}
