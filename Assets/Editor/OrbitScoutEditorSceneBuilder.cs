using UnityEditor;

using UnityEditor.SceneManagement;

using UnityEngine;

using OrbitScout.Core;

using OrbitScout.Platform;

using OrbitScout.UI;

using UnityEngine.XR.ARFoundation;



public static class OrbitScoutEditorSceneBuilder

{

    const string ScenePath = "Assets/Scenes/OrbitScout_EditorTest.unity";

    const string ArScenePath = "Assets/Scenes/SampleScene.unity";



    [MenuItem("Orbit Scout/Create Editor Test Scene")]

    public static void CreateEditorTestScene()

    {

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);



        GameObject systems = CreateOrbitScoutRoot(SolarPlayMode.EditorDesktop);



        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))

            AssetDatabase.CreateFolder("Assets", "Scenes");



        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);

        AssetDatabase.SaveAssets();



        EditorUtility.DisplayDialog(

            "Orbit Scout",

            "Editor test scene saved to:\n" + ScenePath +

            "\n\nPress Play → Start Mission → click planets.",

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



        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog(

            "Orbit Scout",

            "AR ready.\n\nBuild to phone → SampleScene → Start Mission → tap floor to place.",

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



        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Orbit Scout", "OrbitScout object updated.", "OK");

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


