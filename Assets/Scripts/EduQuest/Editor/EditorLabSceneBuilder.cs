#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EduQuest.EditorTools
{
    /// <summary>Desktop workspace for the timed riddle combat campaign.</summary>
    public static class EditorLabSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/EduQuestLab_EditorTest.unity";
        const string LitMatPath = "Assets/Resources/EduQuest/Materials/LabLit.mat";

        [MenuItem("EduQuest/Editor Test/Build Editor Test Scene", priority = 10)]
        public static void Build()
        {
            BakeLitMaterial();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.14f, 0.16f, 0.18f);
            camGo.transform.position = new Vector3(0f, 1.45f, -1.65f);
            camGo.transform.rotation = Quaternion.Euler(30f, 0f, 0f);
            camGo.AddComponent<AudioListener>();

            var sun = new GameObject("Sun");
            var dir = sun.AddComponent<Light>();
            dir.type = LightType.Directional;
            dir.intensity = 1.05f;
            dir.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(42f, -25f, 0f);

            var fill = new GameObject("Fill");
            var fillL = fill.AddComponent<Light>();
            fillL.type = LightType.Directional;
            fillL.intensity = 0.35f;
            fillL.color = new Color(0.75f, 0.85f, 1f);
            fill.transform.rotation = Quaternion.Euler(20f, 140f, 0f);

            EnsureEventSystem();

            var room = new GameObject("EditorRoom");

            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(room.transform, false);
            floor.transform.position = new Vector3(0f, -0.08f, 0.4f);
            floor.transform.localScale = new Vector3(5f, 0.02f, 5f);
            floor.GetComponent<Renderer>().sharedMaterial =
                LabMaterials.Solid(new Color(0.5f, 0.5f, 0.52f), 0.1f);

            var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "ArenaTable";
            table.transform.SetParent(room.transform, false);
            table.transform.position = new Vector3(0f, -0.02f, 0.4f);
            table.transform.localScale = new Vector3(1.8f, 0.08f, 1.1f);
            table.GetComponent<Renderer>().sharedMaterial =
                LabMaterials.Solid(new Color(0.4f, 0.28f, 0.18f), 0.3f);

            var arena = new GameObject("ArenaCenter");
            arena.transform.SetParent(room.transform, false);
            arena.transform.position = new Vector3(0f, 0.03f, 0.4f);

            var canvasGo = new GameObject("Canvas", typeof(RectTransform));
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            canvasGo.AddComponent<GraphicRaycaster>();

            var appGo = new GameObject("EditorLabApp");
            var app = appGo.AddComponent<EditorLabApp>();
            app.Configure(arena.transform, dir, fillL, cam);

            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Campaign Editor Ready",
                "Created:\n" + ScenePath +
                "\n\nPress Play.\nCombat → science riddle → SOLVED RIDDLE\nBeat the 3:00 timer for a higher score.",
                "OK");
        }

        [MenuItem("EduQuest/Editor Test/Open & Play Ready", priority = 11)]
        public static void Open()
        {
            if (!File.Exists(ScenePath))
                Build();
            else
                EditorSceneManager.OpenScene(ScenePath);

            EditorUtility.DisplayDialog(
                "Timed Riddle Campaign",
                "Scene: EduQuestLab_EditorTest\n\nPress Play ▶\nWin waves → solve riddles → beat the clock.",
                "OK");
        }

        static void BakeLitMaterial()
        {
            Directory.CreateDirectory("Assets/Resources/EduQuest/Materials");
            var src = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/MobileARTemplateAssets/Materials/ObjectMaterial.mat");
            if (src == null)
            {
                src = AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/Samples/XR Interaction Toolkit/3.3.0/Starter Assets/DemoSceneAssets/Materials/Lit White.mat");
            }

            var mat = AssetDatabase.LoadAssetAtPath<Material>(LitMatPath);
            if (mat == null)
            {
                mat = src != null
                    ? new Material(src)
                    : new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, LitMatPath);
            }
            else if (src != null)
            {
                mat.shader = src.shader;
            }

            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", new Color(0.85f, 0.9f, 0.95f, 1f));
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
        }

        static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
    }
}
#endif
