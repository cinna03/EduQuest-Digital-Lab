using OrbitScout.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class OrbitScoutLevelMapEditor
{
    [MenuItem("Orbit Scout/Rebuild Level Map In Hierarchy")]
    public static void RebuildLevelMapInHierarchy()
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
        foreach (OrbitScoutHudView view in views)
        {
            if (view.levelSelectPanel == null)
                continue;

            Transform old = view.levelSelectPanel.transform.Find(OrbitScoutLevelMapController.MapRootName);
            if (old != null)
                Undo.DestroyObjectImmediate(old.gameObject);

            OrbitScoutLevelMapController map = OrbitScoutLevelMapController.EnsureOnPanel(view.levelSelectPanel, replaceExisting: false);
            if (map != null)
            {
                Undo.RegisterCreatedObjectUndo(map.gameObject, "Rebuild Level Map");
                selected = map.gameObject;
                EditorUtility.SetDirty(view.levelSelectPanel);
                PrefabUtility.RecordPrefabInstancePropertyModifications(view.gameObject);
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        if (selected != null)
            Selection.activeGameObject = selected;

        EditorUtility.DisplayDialog(
            "Orbit Scout",
            "Level map is in the Hierarchy:\n\n" +
            "OrbitScoutHud → LevelSelectPanel → LevelMap\n" +
            "  • Path_1…Path_3 (connection lines)\n" +
            "  • Node_I … Node_IV (tap / hover targets)\n" +
            "  • HoverPopup (name + number)\n\n" +
            "Move the Node_* objects to reshape the path.\n" +
            "Save the scene when you’re happy.",
            "OK");
    }
}
