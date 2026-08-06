using OrbitScout.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class OrbitScoutMenuDecorEditor
{
    const string PrefabPath = "Assets/OrbitScout/UI/Prefabs/OrbitScoutHud.prefab";
    const string ResourcesPrefabPath = "Assets/Resources/OrbitScout/OrbitScoutHud.prefab";

    [MenuItem("Orbit Scout/Add Menu Decorations To Hierarchy")]
    public static void AddMenuDecorationsToHierarchy()
    {
        OrbitScoutHudView[] views = Object.FindObjectsByType<OrbitScoutHudView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (views.Length == 0)
        {
            EditorUtility.DisplayDialog(
                "Orbit Scout",
                "No OrbitScoutHud in this scene.\nRun Orbit Scout → Create Editable HUD In Scene first.",
                "OK");
            return;
        }

        GameObject selected = null;
        int count = 0;
        foreach (OrbitScoutHudView view in views)
        {
            if (view.menuPanel == null)
                continue;

            // Replace so re-running the menu always gives a clean, editable set
            bool hadExisting = view.menuPanel.transform.Find(OrbitScoutMenuDecor.RootName) != null;
            if (hadExisting)
            {
                Transform old = view.menuPanel.transform.Find(OrbitScoutMenuDecor.RootName);
                Undo.DestroyObjectImmediate(old.gameObject);
            }

            GameObject root = OrbitScoutMenuDecor.EnsureOnMenuPanel(view.menuPanel, replaceExisting: false);
            if (root == null)
                continue;

            Undo.RegisterCreatedObjectUndo(root, "Add Menu Decorations");
            EditorUtility.SetDirty(view.menuPanel);
            EditorUtility.SetDirty(view.gameObject);
            PrefabUtility.RecordPrefabInstancePropertyModifications(view.gameObject);
            selected = root;
            count++;
        }

        if (count == 0)
        {
            EditorUtility.DisplayDialog(
                "Orbit Scout",
                "Could not add decorations.\nCheck that MenuPanel exists on OrbitScoutHudView.",
                "OK");
            return;
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        if (selected != null)
            Selection.activeGameObject = selected;

        EditorUtility.DisplayDialog(
            "Orbit Scout",
            "Menu decorations are in the Hierarchy:\n\n" +
            "OrbitScoutHud → MenuPanel → MenuDecor\n" +
            "  • Orb, Sparkle, Sparkle_2, Flower, Stars\n\n" +
            "Select any child to move, resize, recolor, or disable it.\n" +
            "Save the scene when you’re happy.\n\n" +
            "Tip: also run “Bake Menu Decorations Into HUD Prefab” so builds keep your layout.",
            "OK");
    }

    [MenuItem("Orbit Scout/Bake Menu Decorations Into HUD Prefab")]
    public static void BakeMenuDecorationsIntoPrefab()
    {
        // Prefer scene instance edits if present
        OrbitScoutHudView sceneView = Object.FindFirstObjectByType<OrbitScoutHudView>(FindObjectsInactive.Include);
        if (sceneView != null && sceneView.menuPanel != null)
        {
            Transform sceneDecor = sceneView.menuPanel.transform.Find(OrbitScoutMenuDecor.RootName);
            if (sceneDecor == null)
                OrbitScoutMenuDecor.EnsureOnMenuPanel(sceneView.menuPanel, replaceExisting: false);
        }

        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            OrbitScoutHudView view = prefabRoot.GetComponent<OrbitScoutHudView>();
            if (view == null || view.menuPanel == null)
            {
                EditorUtility.DisplayDialog("Orbit Scout", "HUD prefab has no MenuPanel.", "OK");
                return;
            }

            // If scene has MenuDecor, copy that hierarchy into the prefab so manual edits stick
            if (sceneView != null && sceneView.menuPanel != null)
            {
                Transform sceneDecor = sceneView.menuPanel.transform.Find(OrbitScoutMenuDecor.RootName);
                Transform prefabDecor = view.menuPanel.transform.Find(OrbitScoutMenuDecor.RootName);
                if (prefabDecor != null)
                    Object.DestroyImmediate(prefabDecor.gameObject);

                if (sceneDecor != null)
                {
                    GameObject copy = Object.Instantiate(sceneDecor.gameObject, view.menuPanel.transform);
                    copy.name = OrbitScoutMenuDecor.RootName;
                    copy.transform.SetAsFirstSibling();
                }
                else
                {
                    OrbitScoutMenuDecor.EnsureOnMenuPanel(view.menuPanel, replaceExisting: true);
                }
            }
            else
            {
                OrbitScoutMenuDecor.EnsureOnMenuPanel(view.menuPanel, replaceExisting: true);
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        AssetDatabase.CopyAsset(PrefabPath, ResourcesPrefabPath);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog(
            "Orbit Scout",
            "MenuDecor baked into:\n" + PrefabPath + "\n\nResources copy updated too.",
            "OK");
    }
}
