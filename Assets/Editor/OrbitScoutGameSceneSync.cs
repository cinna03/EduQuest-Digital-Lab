using System.Collections.Generic;
using OrbitScout.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Keeps OrbitScout HUD + menu background identical in SampleScene (AR) and OrbitScout_EditorTest (desktop).
/// </summary>
public static class OrbitScoutGameSceneSync
{
    public const string EditorTestScenePath = "Assets/Scenes/OrbitScout_EditorTest.unity";
    public const string SampleScenePath = "Assets/Scenes/SampleScene.unity";

    public static IReadOnlyList<string> AllGameScenePaths { get; } = new[]
    {
        SampleScenePath,
        EditorTestScenePath
    };

    [MenuItem("Orbit Scout/Sync HUD And Menu Background (Both Game Scenes)")]
    public static void SyncHudAndMenuBackgroundInAllGameScenes()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        string restorePath = SceneManager.GetActiveScene().path;
        var results = new List<string>();
        int ok = 0;

        OrbitScoutHudEditorBuilder.UpgradeHudPrefabAsset();

        foreach (string scenePath in AllGameScenePaths)
        {
            if (ApplyHudAndMenuBackgroundInScene(scenePath, saveScene: true, out string message))
            {
                ok++;
                results.Add("✓ " + scenePath);
            }
            else
                results.Add("✗ " + scenePath + ": " + message);
        }

        if (!string.IsNullOrEmpty(restorePath))
            EditorSceneManager.OpenScene(restorePath, OpenSceneMode.Single);

        EditorUtility.DisplayDialog(
            "Orbit Scout",
            "HUD + menu background synced (" + ok + "/" + AllGameScenePaths.Count + " scenes):\n\n" +
            string.Join("\n", results),
            "OK");
    }

    /// <summary>
    /// Call after updating SampleScene so Editor Test stays matched (and vice versa).
    /// </summary>
    public static void SyncOtherGameSceneFrom(string sourceScenePath)
    {
        foreach (string scenePath in AllGameScenePaths)
        {
            if (scenePath == sourceScenePath)
                continue;

            ApplyHudAndMenuBackgroundInScene(scenePath, saveScene: true, out _);
        }
    }

    public static bool ApplyHudAndMenuBackgroundInScene(string scenePath, bool saveScene, out string error)
    {
        error = null;

        if (!System.IO.File.Exists(scenePath))
        {
            error = "file not found";
            return false;
        }

        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        if (GameObject.Find("OrbitScout") == null)
        {
            error = "no OrbitScout (run Create Editor Test Scene or Setup AR)";
            return false;
        }

        OrbitScoutEditorSceneBuilder.EnsureMainCameraForUiEditing();
        OrbitScoutHudEditorBuilder.EnsureHudInScene(replaceExisting: false, selectHud: false);
        BakeHudMenuBackgroundInOpenScene();

        if (saveScene)
            EditorSceneManager.SaveScene(scene);

        return true;
    }

    internal static void BakeHudMenuBackgroundInOpenScene()
    {
        OrbitScoutHudView[] views = Object.FindObjectsByType<OrbitScoutHudView>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (OrbitScoutHudView view in views)
        {
            OrbitScoutMenuBackgroundEditor.BakeSpritesOnView(view);
            OrbitScoutUiEditSceneOrganizer.OrganizeHudInHierarchy(view);

            OrbitScoutHudCanvasDriver driver = view.GetComponent<OrbitScoutHudCanvasDriver>();
            if (driver == null)
                driver = view.gameObject.AddComponent<OrbitScoutHudCanvasDriver>();

            driver.ApplyEditModePresentation();
            EditorUtility.SetDirty(view.gameObject);
            PrefabUtility.RecordPrefabInstancePropertyModifications(view.gameObject);
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }
}
