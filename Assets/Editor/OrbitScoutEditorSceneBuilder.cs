using UnityEditor;

using UnityEditor.SceneManagement;

using UnityEngine;

using OrbitScout.Core;

using OrbitScout.Platform;

using OrbitScout.UI;

using UnityEngine.XR.ARFoundation;



public static class OrbitScoutEditorSceneBuilder

{

    const string ScenePath = OrbitScoutGameSceneSync.EditorTestScenePath;

    const string ArScenePath = OrbitScoutGameSceneSync.SampleScenePath;



    [MenuItem("Orbit Scout/Create Editor Test Scene")]

    public static void CreateEditorTestScene()

    {

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);



        GameObject systems = CreateOrbitScoutRoot(SolarPlayMode.EditorDesktop);

        EnsureMainCameraForUiEditing();
        OrbitScoutHudEditorBuilder.UpgradeHudPrefabAsset();
        OrbitScoutHudEditorBuilder.EnsureHudInScene(replaceExisting: false, selectHud: false);
        OrbitScoutUiEditSceneOrganizer.PrepareSceneForUiHierarchyEditing(frameMenu: false);

        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))

            AssetDatabase.CreateFolder("Assets", "Scenes");



        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);

        AssetDatabase.SaveAssets();

        OrbitScoutGameSceneSync.ApplyHudAndMenuBackgroundInScene(ArScenePath, saveScene: true, out _);

        if (!string.IsNullOrEmpty(ScenePath))
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        EditorUtility.DisplayDialog(

            "Orbit Scout",

            "Editor test scene saved to:\n" + ScenePath +

            "\n\nUse Hierarchy: UI (Edit Here) → OrbitScoutHud.\n" +
            "Orbit Scout → UI Editing for panel shortcuts.\n" +
            "SampleScene was updated to match.",

            "OK");

    }



    [MenuItem("Orbit Scout/Setup AR In Active Scene")]

    public static void SetupArInActiveScene()

    {

        ARRaycastManager raycast = Object.FindAnyObjectByType<ARRaycastManager>();

        if (raycast == null)

        {

            EditorUtility.DisplayDialog(

                "Orbit Scout",

                "No ARRaycastManager in this scene.\nOpen Assets/Scenes/SampleScene.unity first.",

                "OK");

            return;

        }



        GameObject systems = GameObject.Find("OrbitScout");

        if (systems == null)

            systems = CreateOrbitScoutRoot(SolarPlayMode.AugmentedReality);

        else

            ConfigureOrbitScout(systems, SolarPlayMode.AugmentedReality);



        ArSessionBridge bridge = raycast.GetComponent<ArSessionBridge>();

        if (bridge == null)

            bridge = raycast.gameObject.AddComponent<ArSessionBridge>();



        OrbitScoutArTemplateSuppress.Apply();

        EnsureMainCameraForUiEditing();
        OrbitScoutHudEditorBuilder.UpgradeHudPrefabAsset();
        OrbitScoutHudEditorBuilder.EnsureHudInScene(replaceExisting: false, selectHud: false);

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        OrbitScoutGameSceneSync.SyncOtherGameSceneFrom(ArScenePath);
        EditorSceneManager.OpenScene(ArScenePath, OpenSceneMode.Single);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog(

            "Orbit Scout",

            "AR ready.\n\nSampleScene saved. OrbitScout_EditorTest was updated to match.\n" +
            "Build to phone → SampleScene → Start Mission → tap floor to place.",

            "OK");

    }



    [MenuItem("Orbit Scout/Fix OrbitScout Object In Active Scene")]

    public static void FixOrbitScoutInActiveScene()

    {

        GameObject systems = GameObject.Find("OrbitScout");

        if (systems == null)

        {

            EditorUtility.DisplayDialog("Orbit Scout", "No GameObject named OrbitScout in this scene.", "OK");

            return;

        }



        SolarPlayMode mode = Object.FindAnyObjectByType<ARRaycastManager>() != null

            ? SolarPlayMode.AugmentedReality

            : SolarPlayMode.EditorDesktop;



        ConfigureOrbitScout(systems, mode);

        EnsureMainCameraForUiEditing();
        OrbitScoutHudEditorBuilder.UpgradeHudPrefabAsset();
        OrbitScoutHudEditorBuilder.EnsureHudInScene(replaceExisting: false, selectHud: false);

        string activePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
        EditorSceneManager.SaveOpenScenes();
        if (activePath == OrbitScoutGameSceneSync.SampleScenePath
            || activePath == OrbitScoutGameSceneSync.EditorTestScenePath)
            OrbitScoutGameSceneSync.SyncOtherGameSceneFrom(activePath);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Orbit Scout", "OrbitScout object updated.\nThe other game scene was synced to match.", "OK");

    }



    public static void EnsureMainCameraForUiEditing()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            cam = Object.FindAnyObjectByType<Camera>();
            if (cam != null && cam.gameObject.tag != "MainCamera")
                cam.gameObject.tag = "MainCamera";
        }

        if (cam == null)
        {
            GameObject camObject = new GameObject("Main Camera");
            camObject.tag = "MainCamera";
            cam = camObject.AddComponent<Camera>();
            camObject.AddComponent<AudioListener>();
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.transform.rotation = Quaternion.identity;
        }

        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.02f, 0.03f, 0.06f, 1f);
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 1000f;
    }

    static GameObject CreateOrbitScoutRoot(SolarPlayMode mode)

    {

        GameObject systems = new GameObject("OrbitScout");

        ConfigureOrbitScout(systems, mode);

        systems.AddComponent<OrbitScoutSceneEntry>();

        return systems;

    }



    static void ConfigureOrbitScout(GameObject systems, SolarPlayMode mode)

    {

        if (systems.GetComponent<MissionController>() == null)

            systems.AddComponent<MissionController>();



        SolarBootstrap bootstrap = systems.GetComponent<SolarBootstrap>();

        if (bootstrap == null)

            bootstrap = systems.AddComponent<SolarBootstrap>();

        bootstrap.playMode = mode;



        if (systems.GetComponent<MissionHud>() == null)

            systems.AddComponent<MissionHud>();



        if (systems.GetComponent<OrbitScoutSceneEntry>() == null)

            systems.AddComponent<OrbitScoutSceneEntry>();

    }

}


