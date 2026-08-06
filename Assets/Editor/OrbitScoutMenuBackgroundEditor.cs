using OrbitScout.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class OrbitScoutMenuBackgroundEditor
{
    [MenuItem("Orbit Scout/Assign Menu Panel Background (Scene View)")]
    public static void BakeBackgroundIntoHud()
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(OrbitScoutMenuBackground.MenuPanelBackgroundAssetPath);
        if (sprite == null)
        {
            EditorUtility.DisplayDialog(
                "Orbit Scout",
                "Could not load sprite at:\n" + OrbitScoutMenuBackground.MenuPanelBackgroundAssetPath +
                "\n\nSelect the PNG → Texture Type: Sprite (2D and UI) → Filter Mode: Point → Apply.",
                "OK");
            return;
        }

        OrbitScoutHudView[] views = Object.FindObjectsByType<OrbitScoutHudView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (views.Length == 0)
        {
            EditorUtility.DisplayDialog(
                "Orbit Scout",
                "No OrbitScoutHud in this scene.\nRun Orbit Scout → Create Editable HUD In Scene first.",
                "OK");
            return;
        }

        foreach (OrbitScoutHudView view in views)
        {
            BakeSpritesOnView(view);
            EditorUtility.SetDirty(view.gameObject);
            PrefabUtility.RecordPrefabInstancePropertyModifications(view.gameObject);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        if (EditorSceneManager.GetActiveScene().isDirty)
            EditorSceneManager.SaveOpenScenes();

        string activePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
        if (activePath == OrbitScoutGameSceneSync.SampleScenePath
            || activePath == OrbitScoutGameSceneSync.EditorTestScenePath)
        {
            OrbitScoutGameSceneSync.SyncOtherGameSceneFrom(activePath);
        }

        EditorUtility.DisplayDialog(
            "Orbit Scout",
            "Menu panel background assigned on MenuPanel → Image.\n" +
            "Active scene saved; the other game scene (Sample / Editor Test) was updated to match.",
            "OK");
    }

    static bool ApplySprite(GameObject panel, Sprite sprite)
    {
        if (panel == null)
            return false;

        Image image = panel.GetComponent<Image>();
        if (image == null)
            image = panel.AddComponent<Image>();

        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.preserveAspect = false;
        image.color = Color.white;
        image.raycastTarget = false;
        return true;
    }

    internal static void BakeSpritesOnView(OrbitScoutHudView view)
    {
        if (view == null)
            return;

        Sprite menuSprite = AssetDatabase.LoadAssetAtPath<Sprite>(OrbitScoutMenuBackground.MenuPanelBackgroundAssetPath);
        if (menuSprite != null)
            ApplySprite(view.menuPanel, menuSprite);

        Sprite planetSprite = AssetDatabase.LoadAssetAtPath<Sprite>(OrbitScoutMenuBackground.PlanetBackgroundAssetPath);
        Sprite levelSelectSprite = AssetDatabase.LoadAssetAtPath<Sprite>(OrbitScoutMenuBackground.LevelSelectBackgroundAssetPath);
        if (planetSprite != null)
        {
            ApplySprite(view.endPanel, planetSprite);
            ApplySprite(view.briefingPanel, planetSprite);
            ApplySprite(view.menuPanel, planetSprite);
        }
        else if (menuSprite != null)
            ApplySprite(view.menuPanel, menuSprite);

        if (levelSelectSprite != null)
            ApplySprite(view.levelSelectPanel, levelSelectSprite);
        else if (planetSprite != null)
            ApplySprite(view.levelSelectPanel, planetSprite);

        Sprite walkthroughSprite = AssetDatabase.LoadAssetAtPath<Sprite>(OrbitScoutMenuBackground.WalkthroughBackgroundAssetPath);
        if (walkthroughSprite != null)
        {
            ApplySprite(view.briefingPanel, walkthroughSprite);
            ApplySprite(view.endPanel, walkthroughSprite);
        }
        else if (planetSprite != null)
        {
            ApplySprite(view.briefingPanel, planetSprite);
            ApplySprite(view.endPanel, planetSprite);
        }
    }
}
