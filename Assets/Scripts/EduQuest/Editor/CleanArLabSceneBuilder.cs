#if UNITY_EDITOR
using System.IO;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace EduQuest.EditorTools
{
    public static class CleanArLabSceneBuilder
    {
        const string ScenePath = "Assets/Scenes/EduQuestLab.unity";
        const string PlanePath = "Assets/Prefabs/ARPlane.prefab";
        const string LitMatPath = "Assets/Resources/EduQuest/Materials/LabLit.mat";

        [MenuItem("EduQuest/Build Clean AR Lab (Glassware Kit)", priority = 0)]
        public static void Build()
        {
            BakeLitMaterial();
            LabGlassPrefabBaker.BakeQuiet();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ---- Desktop camera / lights ----
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var deskCam = camGo.AddComponent<Camera>();
            deskCam.clearFlags = CameraClearFlags.SolidColor;
            deskCam.backgroundColor = new Color(0.12f, 0.14f, 0.16f);
            camGo.transform.position = new Vector3(0f, 1.5f, -1.7f);
            camGo.transform.rotation = Quaternion.Euler(28f, 0f, 0f);
            camGo.AddComponent<AudioListener>();

            var sun = new GameObject("Sun");
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(45f, -30f, 0f);

            EnsureEventSystem();

            // ---- Desktop table preview ----
            var desktop = new GameObject("DesktopPreview");
            var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "Table";
            table.transform.SetParent(desktop.transform, false);
            table.transform.position = new Vector3(0f, -0.03f, 0.35f);
            table.transform.localScale = new Vector3(1.6f, 0.06f, 1.0f);
            table.GetComponent<Renderer>().sharedMaterial =
                LabMaterials.Solid(new Color(0.35f, 0.25f, 0.18f), 0.25f);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(desktop.transform, false);
            floor.transform.position = new Vector3(0f, -0.08f, 0.5f);
            floor.transform.localScale = new Vector3(4f, 0.02f, 4f);
            floor.GetComponent<Renderer>().sharedMaterial =
                LabMaterials.Solid(new Color(0.55f, 0.55f, 0.55f), 0.15f);

            // ---- Phone AR stack ----
            var planePrefab = EnsurePlanePrefab();

            var arSession = new GameObject("AR Session");
            arSession.AddComponent<ARSession>();
            arSession.AddComponent<ARInputManager>();

            var xrOrigin = new GameObject("XR Origin");
            var origin = xrOrigin.AddComponent<XROrigin>();
            xrOrigin.AddComponent<ARRaycastManager>();
            var planeMgr = xrOrigin.AddComponent<ARPlaneManager>();
            planeMgr.planePrefab = planePrefab;
            planeMgr.requestedDetectionMode = PlaneDetectionMode.Horizontal;
            var placer = xrOrigin.AddComponent<ArTablePlacer>();
            placer.Configure(planeMgr);

            var camOffset = new GameObject("Camera Offset");
            camOffset.transform.SetParent(xrOrigin.transform, false);
            var arCamGo = new GameObject("AR Camera");
            arCamGo.transform.SetParent(camOffset.transform, false);
            var arCam = arCamGo.AddComponent<Camera>();
            arCam.enabled = false;
            arCam.nearClipPlane = 0.1f;
            arCamGo.AddComponent<ARCameraManager>();
            arCamGo.AddComponent<ARCameraBackground>();
            origin.CameraFloorOffsetObject = camOffset;
            origin.Camera = arCam;

            // ---- UI ----
            var canvasGo = new GameObject("Canvas", typeof(RectTransform));
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            canvasGo.AddComponent<GraphicRaycaster>();
            var hud = GuideHud.Create(canvasGo.transform);

            // ---- App ----
            var appGo = new GameObject("LabApp");
            var app = appGo.AddComponent<LabApp>();
            app.Configure(hud, placer, desktop, arSession, xrOrigin, deskCam, arCam, appGo.transform);

            Directory.CreateDirectory("Assets/Scenes");
            Directory.CreateDirectory("Assets/Prefabs");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Clean AR Lab",
                "Scene: Assets/Scenes/EduQuestLab.unity\n\nEDITOR:\nPress Play → lab appears on the table (~1.5s).\n\nPHONE:\nBuild And Run → scan table → lab appears.\n\nGuide text only.",
                "OK");
        }

        [MenuItem("EduQuest/Open Editor Lab Test", priority = 1)]
        public static void OpenTest()
        {
            // Dedicated desktop scene (no AR stack)
            EditorLabSceneBuilder.Open();
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

        static GameObject EnsurePlanePrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PlanePath) != null)
                AssetDatabase.DeleteAsset(PlanePath);

            var go = new GameObject("ARPlane");
            go.AddComponent<ARPlane>();
            go.AddComponent<ARPlaneMeshVisualizer>();
            go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = LabMaterials.Solid(new Color(0.3f, 0.85f, 1f), 0.2f);
            go.AddComponent<MeshCollider>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, PlanePath);
            Object.DestroyImmediate(go);
            return prefab;
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
