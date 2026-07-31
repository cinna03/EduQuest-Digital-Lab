#if UNITY_EDITOR
using System.IO;
using EduQuest.AR;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EduQuest.EditorTools
{
    /// <summary>
    /// Builds the table-scan germination lab (desktop camera preview):
    /// scan surface → place pot → water → real light → grow.
    /// </summary>
    public static class ARGerminationLabBuilder
    {
        const string ScenePath = "Assets/Scenes/EduQuestLab.unity";
        const string PrefabPath = "Assets/Prefabs/GerminationPot.prefab";

        [MenuItem("EduQuest/Build AR Table Germination Lab", priority = 0)]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.05f, 0.07f);
            camGo.transform.position = new Vector3(0f, 1.35f, -2.2f);
            camGo.transform.rotation = Quaternion.Euler(18f, 0f, 0f);
            camGo.AddComponent<AudioListener>();

            var sun = new GameObject("Sun");
            var dir = sun.AddComponent<Light>();
            dir.type = LightType.Directional;
            dir.intensity = 0.75f;
            sun.transform.rotation = Quaternion.Euler(42f, -25f, 0f);

            EnsureEventSystem();

            // Simulated table (desktop preview of AR plane)
            var room = new GameObject("DesktopARPreview");
            var scanning = GameObject.CreatePrimitive(PrimitiveType.Quad);
            scanning.name = "ScanningOverlay";
            scanning.transform.SetParent(room.transform, false);
            scanning.transform.position = new Vector3(0f, 0.9f, 0.6f);
            scanning.transform.localScale = new Vector3(1.4f, 0.9f, 1f);
            Object.DestroyImmediate(scanning.GetComponent<Collider>());
            SetColor(scanning, new Color(0.2f, 0.75f, 0.95f, 0.25f));

            var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "DetectedTable";
            table.transform.SetParent(room.transform, false);
            table.transform.position = new Vector3(0f, -0.04f, 0.35f);
            table.transform.localScale = new Vector3(1.8f, 0.06f, 1.1f);
            table.layer = 0;
            SetColor(table, new Color(0.35f, 0.55f, 0.7f, 0.55f));
            table.SetActive(false);

            var potPrefab = EnsurePotPrefab();

            var labRoot = new GameObject("ARGerminationLab");
            var controller = labRoot.AddComponent<ARGerminationController>();
            var sensor = labRoot.AddComponent<WorldLightSensor>();
            var placer = labRoot.AddComponent<TablePotPlacer>();
            placer.Configure(cam, potPrefab);

            var canvas = CreateCanvas();

            var previewPanel = Panel(canvas.transform, "CameraPreviewPanel", new Color(0.02f, 0.02f, 0.03f, 0.95f));
            Pin(previewPanel, 0.02f, 0.42f, 0.38f, 0.88f);
            var preview = new GameObject("CameraPreview", typeof(RectTransform));
            preview.transform.SetParent(previewPanel.transform, false);
            Stretch(preview.GetComponent<RectTransform>(), 8f);
            var raw = preview.AddComponent<RawImage>();
            raw.color = new Color(0.15f, 0.15f, 0.18f);
            var previewLabel = Label(previewPanel.transform, "PreviewLabel", "LIVE VIEW · scan table · place pot", 11, TextAnchor.UpperCenter);
            Pin(previewLabel.rectTransform, 0.05f, 0.88f, 0.95f, 0.98f);
            previewLabel.color = new Color(0.7f, 0.9f, 0.95f);
            sensor.SetPreview(raw);

            var top = Panel(canvas.transform, "HUD", new Color(0.04f, 0.06f, 0.09f, 0.92f));
            Pin(top, 0f, 0.9f, 1f, 1f);
            var header = Label(top.transform, "Header", "EduQuest · AR Table Germination", 17, TextAnchor.MiddleLeft);
            Pin(header.rectTransform, 0.02f, 0.15f, 0.55f, 0.85f);
            header.fontStyle = FontStyle.Bold;
            var prompt = Label(top.transform, "Prompt", "Scan table → place pot → water → real light → sprout", 11, TextAnchor.MiddleLeft);
            Pin(prompt.rectTransform, 0.56f, 0.15f, 0.84f, 0.85f);
            var status = Label(top.transform, "Status", "", 1, TextAnchor.MiddleLeft);
            status.color = Color.clear;
            var resetBtn = Btn(top.transform, "ResetButton", "Reset", new Vector2(0.85f, 0.2f), new Vector2(0.92f, 0.8f), new Color(0.25f, 0.28f, 0.34f));
            var reflectBtn = Btn(top.transform, "ReflectButton", "Reflect", new Vector2(0.93f, 0.2f), new Vector2(0.99f, 0.8f), new Color(0.14f, 0.48f, 0.62f));

            var guide = Panel(canvas.transform, "GuidePanel", new Color(0.05f, 0.08f, 0.11f, 0.94f));
            Pin(guide, 0.62f, 0.22f, 0.985f, 0.88f);
            var stepLabel = Label(guide.transform, "StepLabel", "Step 1 / 5", 12, TextAnchor.UpperLeft);
            Pin(stepLabel.rectTransform, 0.07f, 0.9f, 0.93f, 0.98f);
            stepLabel.color = new Color(0.5f, 0.85f, 0.95f);
            var guideTitle = Label(guide.transform, "GuideTitle", "Scan a table", 18, TextAnchor.UpperLeft);
            Pin(guideTitle.rectTransform, 0.07f, 0.76f, 0.93f, 0.9f);
            guideTitle.fontStyle = FontStyle.Bold;
            var guideBody = Label(guide.transform, "GuideBody", "", 12, TextAnchor.UpperLeft);
            Pin(guideBody.rectTransform, 0.07f, 0.34f, 0.93f, 0.76f);
            var reaction = Label(guide.transform, "ReactionText", "Reaction: —", 12, TextAnchor.UpperLeft);
            Pin(reaction.rectTransform, 0.07f, 0.12f, 0.93f, 0.34f);
            reaction.color = new Color(0.95f, 0.85f, 0.45f);
            var hintBtn = Btn(guide.transform, "HintButton", "Repeat hint", new Vector2(0.07f, 0.03f), new Vector2(0.93f, 0.11f), new Color(0.2f, 0.28f, 0.36f));

            var bar = Panel(canvas.transform, "ActionBar", new Color(0.04f, 0.06f, 0.09f, 0.94f));
            Pin(bar, 0f, 0f, 1f, 0.2f);
            var lightFill = FillBar(bar.transform, "LightMeter", 0.02f, 0.55f, 0.4f, 0.88f, new Color(1f, 0.9f, 0.3f));
            var waterFill = FillBar(bar.transform, "WaterMeter", 0.02f, 0.28f, 0.4f, 0.5f, new Color(0.25f, 0.65f, 0.85f));
            var growthFill = FillBar(bar.transform, "GrowthMeter", 0.02f, 0.05f, 0.4f, 0.24f, new Color(0.35f, 0.85f, 0.4f));
            var meterLabel = Label(bar.transform, "MeterLabel", "Light: —", 11, TextAnchor.MiddleLeft);
            Pin(meterLabel.rectTransform, 0.42f, 0.55f, 0.72f, 0.9f);
            var waterBtn = Btn(bar.transform, "WaterButton", "Water pot", new Vector2(0.74f, 0.35f), new Vector2(0.98f, 0.9f), new Color(0.2f, 0.45f, 0.75f));
            var why = Label(bar.transform, "WhyAR", "AR: table is real · light is from your room · water the pot you placed", 11, TextAnchor.MiddleLeft);
            Pin(why.rectTransform, 0.42f, 0.05f, 0.72f, 0.48f);
            why.color = new Color(0.75f, 0.78f, 0.85f);

            controller.Bind(
                placer, sensor, scanning, table,
                stepLabel, guideTitle, guideBody, reaction, meterLabel,
                lightFill, waterFill, growthFill, waterBtn, hintBtn);

            var reflectionGo = new GameObject("ReflectionUI");
            var reflection = reflectionGo.AddComponent<ReflectionUI>();
            var refPanel = Panel(canvas.transform, "ReflectionPanel", new Color(0.05f, 0.09f, 0.12f, 0.96f));
            var refRt = refPanel.GetComponent<RectTransform>();
            refRt.anchorMin = refRt.anchorMax = new Vector2(0.5f, 0.5f);
            refRt.sizeDelta = new Vector2(540f, 320f);
            var refTitle = Label(refPanel.transform, "RefTitle", "Reflection", 20, TextAnchor.UpperCenter);
            Pin(refTitle.rectTransform, 0.05f, 0.82f, 0.95f, 0.96f);
            var refPrompt = Label(refPanel.transform, "RefPrompt", "", 15, TextAnchor.UpperLeft);
            Pin(refPrompt.rectTransform, 0.06f, 0.55f, 0.94f, 0.8f);
            var input = MakeInput(refPanel.transform);
            Pin(input.GetComponent<RectTransform>(), 0.06f, 0.28f, 0.94f, 0.52f);
            var feedback = Label(refPanel.transform, "Feedback", "", 14, TextAnchor.MiddleLeft);
            Pin(feedback.rectTransform, 0.06f, 0.16f, 0.94f, 0.26f);
            var submit = Btn(refPanel.transform, "Submit", "Submit", new Vector2(0.15f, 0.04f), new Vector2(0.45f, 0.14f));
            var close = Btn(refPanel.transform, "Close", "Close", new Vector2(0.55f, 0.04f), new Vector2(0.85f, 0.14f));
            reflection.Configure(refPanel, refTitle, refPrompt, input, feedback, submit, close);
            refPanel.SetActive(false);

            var hub = new GameObject("LabHub").AddComponent<LabHub>();
            hub.Configure(null, header, prompt, status, reflection, new ILabExperiment[] { controller });
            hub.OpenExperiment(0);

            resetBtn.onClick.AddListener(() => controller.ResetExperiment());
            reflectBtn.onClick.AddListener(() => reflection.Show(controller.Title, controller.Prompt));
            new GameObject("LabPerformanceSettings").AddComponent<LabPerformanceSettings>();

            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "AR Table Germination Lab",
                "Desktop tonight:\n1) Play → allow camera\n2) Wait for table scan\n3) Click table → place pot\n4) Water → move to bright light → grow\n\nPhone later:\nFile → Build Settings → Android/iOS\nXR Plug-in Management → ARCore/ARKit\nInstall on device for real plane scan.",
                "OK");
        }

        static GameObject EnsurePotPrefab()
        {
            Directory.CreateDirectory("Assets/Prefabs");
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (existing != null) return existing;

            var root = new GameObject("GerminationPot");
            var potComp = root.AddComponent<GerminationPot>();

            var pot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pot.name = "Pot";
            pot.transform.SetParent(root.transform, false);
            pot.transform.localScale = new Vector3(0.28f, 0.14f, 0.28f);
            pot.transform.localPosition = new Vector3(0f, 0.14f, 0f);
            Object.DestroyImmediate(pot.GetComponent<Collider>());
            SetColor(pot, new Color(0.45f, 0.22f, 0.14f));

            var soil = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            soil.name = "Soil";
            soil.transform.SetParent(root.transform, false);
            soil.transform.localScale = new Vector3(0.24f, 0.05f, 0.24f);
            soil.transform.localPosition = new Vector3(0f, 0.26f, 0f);
            Object.DestroyImmediate(soil.GetComponent<Collider>());
            SetColor(soil, new Color(0.35f, 0.25f, 0.14f));

            var seed = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            seed.name = "Seed";
            seed.transform.SetParent(root.transform, false);
            seed.transform.localScale = Vector3.one * 0.04f;
            seed.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            Object.DestroyImmediate(seed.GetComponent<Collider>());
            SetColor(seed, new Color(0.55f, 0.4f, 0.2f));

            var sprout = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            sprout.name = "Sprout";
            sprout.transform.SetParent(root.transform, false);
            Object.DestroyImmediate(sprout.GetComponent<Collider>());
            SetColor(sprout, new Color(0.25f, 0.7f, 0.3f));
            sprout.SetActive(false);

            var leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leaves.name = "Leaves";
            leaves.transform.SetParent(root.transform, false);
            Object.DestroyImmediate(leaves.GetComponent<Collider>());
            SetColor(leaves, new Color(0.2f, 0.65f, 0.28f));
            leaves.SetActive(false);

            var splashGo = new GameObject("WaterSplash");
            splashGo.transform.SetParent(root.transform, false);
            splashGo.transform.localPosition = new Vector3(0f, 0.32f, 0f);
            var splash = splashGo.AddComponent<ParticleSystem>();
            var sm = splash.main;
            sm.startSize = 0.04f;
            sm.startSpeed = 0.5f;
            sm.startLifetime = 0.6f;
            sm.startColor = new Color(0.4f, 0.7f, 1f, 0.8f);
            splash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var oxygenGo = new GameObject("Oxygen");
            oxygenGo.transform.SetParent(root.transform, false);
            oxygenGo.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            var oxygen = oxygenGo.AddComponent<ParticleSystem>();
            var om = oxygen.main;
            om.startSize = 0.05f;
            om.startSpeed = 0.35f;
            om.startLifetime = 1.2f;
            om.startColor = new Color(0.6f, 1f, 0.75f, 0.8f);
            oxygen.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var glowGo = new GameObject("GrowthGlow");
            glowGo.transform.SetParent(root.transform, false);
            glowGo.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            var glow = glowGo.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.range = 1.5f;
            glow.enabled = false;

            potComp.Configure(
                soil.transform, seed.transform, sprout.transform, leaves.transform,
                soil.GetComponent<Renderer>(), splash, oxygen, glow);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        static Image FillBar(Transform parent, string name, float x0, float y0, float x1, float y1, Color color)
        {
            var bg = new GameObject(name + "Bg", typeof(RectTransform));
            bg.transform.SetParent(parent, false);
            Pin(bg.GetComponent<RectTransform>(), x0, y0, x1, y1);
            bg.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.12f);
            var fillGo = new GameObject(name, typeof(RectTransform));
            fillGo.transform.SetParent(bg.transform, false);
            Stretch(fillGo.GetComponent<RectTransform>());
            var fill = fillGo.AddComponent<Image>();
            fill.color = color;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillAmount = 0.1f;
            return fill;
        }

        static void SetColor(GameObject go, Color color)
        {
            var r = go.GetComponent<Renderer>();
            if (r == null) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", color.a < 0.99f ? 1f : 0f);
            r.sharedMaterial = mat;
        }

        static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        static Canvas CreateCanvas()
        {
            var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            return canvas;
        }

        static GameObject Panel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.AddComponent<Image>().color = color;
            return go;
        }

        static Text Label(Transform parent, string name, string value, int size, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                        ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.text = value;
            text.fontSize = size;
            text.color = Color.white;
            text.alignment = anchor;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        static Button Btn(Transform parent, string name, string label, Vector2 aMin, Vector2 aMax)
            => Btn(parent, name, label, aMin, aMax, new Color(0.14f, 0.5f, 0.72f));

        static Button Btn(Transform parent, string name, string label, Vector2 aMin, Vector2 aMax, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            go.AddComponent<Image>().color = color;
            var btn = go.AddComponent<Button>();
            var text = Label(go.transform, "Label", label, 12, TextAnchor.MiddleCenter);
            text.fontStyle = FontStyle.Bold;
            Stretch(text.rectTransform);
            return btn;
        }

        static InputField MakeInput(Transform parent)
        {
            var go = new GameObject("Input", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.AddComponent<Image>().color = Color.white;
            var input = go.AddComponent<InputField>();
            var text = Label(go.transform, "Text", "", 15, TextAnchor.UpperLeft);
            Stretch(text.rectTransform, 8f);
            text.color = Color.black;
            var ph = Label(go.transform, "Placeholder", "What did the seed need? Why place it on a real table?", 14, TextAnchor.UpperLeft);
            Stretch(ph.rectTransform, 8f);
            ph.color = new Color(0.4f, 0.4f, 0.4f);
            input.textComponent = text;
            input.placeholder = ph;
            input.lineType = InputField.LineType.MultiLineNewline;
            return input;
        }

        static void Stretch(RectTransform rt, float pad = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(pad, pad);
            rt.offsetMax = new Vector2(-pad, -pad);
        }

        static void Pin(GameObject go, float x0, float y0, float x1, float y1)
            => Pin(go.GetComponent<RectTransform>(), x0, y0, x1, y1);

        static void Pin(RectTransform rt, float x0, float y0, float x1, float y1)
        {
            rt.anchorMin = new Vector2(x0, y0);
            rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
#endif
