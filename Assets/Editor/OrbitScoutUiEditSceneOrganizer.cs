using OrbitScout.Platform;
using OrbitScout.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class OrbitScoutUiEditSceneOrganizer
{
    public const string UiRootName = "UI (Edit Here)";

    public static GameObject EnsureUiRoot()
    {
        GameObject root = GameObject.Find(UiRootName);
        if (root != null)
            return root;

        root = new GameObject(UiRootName);
        root.transform.SetSiblingIndex(0);

        OrbitScoutUiEditAnchor anchor = root.GetComponent<OrbitScoutUiEditAnchor>();
        if (anchor == null)
            anchor = root.AddComponent<OrbitScoutUiEditAnchor>();

        return root;
    }

    public static void OrganizeHudInHierarchy(OrbitScoutHudView view)
    {
        if (view == null)
            return;

        GameObject uiRoot = EnsureUiRoot();
        OrbitScoutUiEditAnchor anchor = uiRoot.GetComponent<OrbitScoutUiEditAnchor>();
        if (anchor == null)
            anchor = uiRoot.AddComponent<OrbitScoutUiEditAnchor>();

        view.transform.SetParent(uiRoot.transform, true);
        view.gameObject.name = "OrbitScoutHud";
        anchor.SetHudView(view);
        EditorUtility.SetDirty(uiRoot);
        EditorUtility.SetDirty(view.gameObject);
    }

    public static void PrepareSceneForUiHierarchyEditing(bool frameMenu)
    {
        OrbitScoutEditorSceneBuilder.EnsureMainCameraForUiEditing();
        OrbitScoutUiInputSetup.EnsureEventSystem();
        OrbitScoutHudEditorBuilder.UpgradeHudPrefabAsset();
        OrbitScoutHudEditorBuilder.EnsureHudInScene(replaceExisting: false, selectHud: false);

        OrbitScoutHudView view = Object.FindAnyObjectByType<OrbitScoutHudView>(FindObjectsInactive.Include);
        if (view == null)
            return;

        OrganizeHudInHierarchy(view);
        view.GetComponent<OrbitScoutHudCanvasDriver>()?.ApplyEditModePresentation();
        OrbitScoutMenuBackgroundEditor.BakeSpritesOnView(view);
        OrbitScoutUiEditSession.ShowAllPanels(view);

        if (frameMenu)
            OrbitScoutUiEditSession.FocusPanel(view, OrbitScoutUiEditSession.UiPanel.MainMenu);

        EditorSceneManager.MarkSceneDirty(view.gameObject.scene);
    }
}
