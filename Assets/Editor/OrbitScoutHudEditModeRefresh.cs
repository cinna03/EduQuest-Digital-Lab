using OrbitScout.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
static class OrbitScoutHudEditModeRefresh
{
    static OrbitScoutHudEditModeRefresh()
    {
        EditorApplication.hierarchyChanged += RefreshAllHudCanvases;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        EditorApplication.delayCall += RefreshAllHudCanvases;
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
            EditorApplication.delayCall += RefreshAllHudCanvases;
    }

    static void RefreshAllHudCanvases()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        OrbitScoutHudCanvasDriver[] drivers = Object.FindObjectsByType<OrbitScoutHudCanvasDriver>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (OrbitScoutHudCanvasDriver driver in drivers)
            driver.ApplyEditModePresentation();
    }
}

public static class OrbitScoutHudEditModeLayout
{
    public static void FrameHudForEditing(OrbitScoutHudView view)
    {
        if (view == null)
            return;

        OrbitScoutHudCanvasDriver driver = view.GetComponent<OrbitScoutHudCanvasDriver>();
        if (driver != null)
            driver.ApplyEditModePresentation();

        Camera cam = Camera.main;
        if (cam == null)
            return;

        cam.transform.position = new Vector3(0f, 0f, -4f);
        cam.transform.rotation = Quaternion.identity;
        cam.orthographic = false;
        cam.fieldOfView = 60f;

        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            sceneView.in2DMode = false;
            sceneView.AlignViewToObject(cam.transform);
            sceneView.Frame(new Bounds(Vector3.zero, new Vector3(3f, 5f, 0.1f)), false);
        }

        Selection.activeGameObject = view.menuPanel != null ? view.menuPanel : view.gameObject;
    }

    public static void MoveHudToSceneRoot(GameObject hudInstance)
    {
        if (hudInstance == null || hudInstance.transform.parent == null)
            return;

        hudInstance.transform.SetParent(null, true);
        EditorSceneManager.MarkSceneDirty(hudInstance.scene);
    }
}
