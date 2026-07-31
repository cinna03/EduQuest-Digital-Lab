#if UNITY_EDITOR
using System.IO;
using EduQuest.AR;
using EduQuest.Experiments;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EduQuest.EditorTools
{
    /// <summary>
    /// AR-native Light & Life lab: real camera brightness drives photosynthesis / night mode.
    /// </summary>
    public static class LightLabBuilder
    {
        const string ScenePath = "Assets/Scenes/EduQuestLab.unity";

        [MenuItem("EduQuest/Build Light Lab Scene", priority = 0)]
        [MenuItem("EduQuest/Build Light Lab Scene (AR)", priority = 1)]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.07f, 0.1f);
            camGo.transform.position = new Vector3(0f, 1.4f, -2.6f);
            camGo.transform.rotation = Quaternion.Euler(15f, 0f, 0f);
            camGo.AddComponent<AudioListener>();

            var sun = new GameObject("Sun");
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.85f;
            sun.transform.rotation = Quaternion.Euler(40f, -20f, 0f);

            var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "LabTable";
            table.transform.position = new Vector3(0f, -0.05f, 0.2f);
            table.transform.localScale = new Vector3(2.2f, 0.08f, 1.4f);
            SetColor(table, new Color(0.22f, 0.2f, 0.18f));

            EnsureEventSystem();

            var station = BuildPlantStation();
            var lab = station.GetComponent<GuidedLightLabExperiment>();
            var sensor = station.AddComponent<WorldLightSensor>();

            var canvas = CreateCanvas();

            // Camera preview (AR viewfinder) — left side
            var previewPanel = Panel(canvas.transform, "CameraPreviewPanel", new Color(0.02f, 0.02f, 0.03f, 0.95f));
            Pin(previewPanel, 0.02f, 0.42f, 0.38f, 0.88f);
            var preview = new GameObject("CameraPreview", typeof(RectTransform));
            preview.transform.SetParent(previewPanel.transform, false);
            Stretch(preview.GetComponent<RectTransform>(), 8f);
            var raw = preview.AddComponent<RawImage>();
            raw.color = new Color(0.15f, 0.15f, 0.18f);
            var previewLabel = Label(previewPanel.transform, "PreviewLabel", "LIVE CAMERA (world light sensor)", 11, TextAnchor.UpperCenter);
            Pin(previewLabel.rectTransform, 0.05f, 0.88f, 0.95f, 0.98f);
            previewLabel.color = new Color(0.7f, 0.85f, 0.95f);

            sensor.SetPreview(raw);

            // Top
            var top = Panel(canvas.transform, "HUD", new Color(0.04f, 0.06f, 0.09f, 0.92f));
            Pin(top, 0f, 0.9f, 1f, 1f);
            var header = Label(top.transform, "Header", "EduQuest · Light & Life (AR)", 18, TextAnchor.MiddleLeft);
            Pin(header.rectTransform, 0.02f, 0.15f, 0.55f, 0.85f);
            header.fontStyle = FontStyle.Bold;
            var prompt = Label(top.transform, "Prompt", "Your room’s light controls the plant — not a fake slider.", 12, TextAnchor.MiddleLeft);
            Pin(prompt.rectTransform, 0.56f, 0.15f, 0.84f, 0.85f);
            var status = Label(top.transform, "Status", "", 1, TextAnchor.MiddleLeft);
            Pin(status.rectTransform, 0f, 0f, 0.01f, 0.01f);
            status.color = Color.clear;
            var resetBtn = Btn(top.transform, "ResetButton", "Reset", new Vector2(0.85f, 0.2f), new Vector2(0.92f, 0.8f), new Color(0.25f, 0.28f, 0.34f));
            var reflectBtn = Btn(top.transform, "ReflectButton", "Reflect", new Vector2(0.93f, 0.2f), new Vector2(0.99f, 0.8f), new Color(0.14f, 0.48f, 0.62f));

            // Guide
            var guide = Panel(canvas.transform, "GuidePanel", new Color(0.05f, 0.08f, 0.11f, 0.94f));
            Pin(guide, 0.62f, 0.22f, 0.985f, 0.88f);
            var stepLabel = Label(guide.transform, "StepLabel", "Step 1 / 5", 12, TextAnchor.UpperLeft);
            Pin(stepLabel.rectTransform, 0.07f, 0.9f, 0.93f, 0.98f);
            stepLabel.color = new Color(0.5f, 0.85f, 0.95f);
            var guideTitle = Label(guide.transform, "GuideTitle", "Place", 18, TextAnchor.UpperLeft);
            Pin(guideTitle.rectTransform, 0.07f, 0.76f, 0.93f, 0.9f);
            guideTitle.fontStyle = FontStyle.Bold;
            var guideBody = Label(guide.transform, "GuideBody", "", 13, TextAnchor.UpperLeft);
            Pin(guideBody.rectTransform, 0.07f, 0.34f, 0.93f, 0.76f);
            var reaction = Label(guide.transform, "ReactionText", "Reaction: —", 12, TextAnchor.UpperLeft);
            Pin(reaction.rectTransform, 0.07f, 0.12f, 0.93f, 0.34f);
            reaction.color = new Color(0.95f, 0.85f, 0.45f);
            var hintBtn = Btn(guide.transform, "HintButton", "Repeat hint", new Vector2(0.07f, 0.03f), new Vector2(0.93f, 0.11f), new Color(0.2f, 0.28f, 0.36f));

            // Meter + actions
            var bar = Panel(canvas.transform, "ActionBar", new Color(0.04f, 0.06f, 0.09f, 0.94f));
            Pin(bar, 0f, 0f, 1f, 0.2f);

            var meterBg = new GameObject("MeterBg", typeof(RectTransform));
            meterBg.transform.SetParent(bar.transform, false);
            Pin(meterBg.GetComponent<RectTransform>(), 0.02f, 0.55f, 0.55f, 0.88f);
            meterBg.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.12f);
            var meterFillGo = new GameObject("MeterFill", typeof(RectTransform));
            meterFillGo.transform.SetParent(meterBg.transform, false);
            Stretch(meterFillGo.GetComponent<RectTransform>());
            var meterFill = meterFillGo.AddComponent<Image>();
            meterFill.color = new Color(1f, 0.9f, 0.3f);
            meterFill.type = Image.Type.Filled;
            meterFill.fillMethod = Image.FillMethod.Horizontal;
            meterFill.fillAmount = 0.2f;

            var meterLabel = Label(bar.transform, "MeterLabel", "World light: —", 13, TextAnchor.MiddleLeft);
            Pin(meterLabel.rectTransform, 0.02f, 0.15f, 0.55f, 0.5f);

            var placeBtn = Btn(bar.transform, "PlaceButton", "Place seedling", new Vector2(0.58f, 0.5f), new Vector2(0.78f, 0.88f), new Color(0.2f, 0.55f, 0.35f));
            var confirmBtn = Btn(bar.transform, "ConfirmButton", "I'm stuck? Hint", new Vector2(0.8f, 0.5f), new Vector2(0.98f, 0.88f), new Color(0.25f, 0.35f, 0.45f));
            var why = Label(bar.transform, "WhyAR", "No “Add light” button on purpose — move your camera in the real world.", 11, TextAnchor.MiddleLeft);
            Pin(why.rectTransform, 0.58f, 0.08f, 0.98f, 0.42f);
            why.color = new Color(0.75f, 0.78f, 0.85f);

            lab.Bind(
                sensor,
                stepLabel,
                guideTitle,
                guideBody,
                reaction,
                meterLabel,
                meterFill,
                placeBtn,
                confirmBtn,
                hintBtn,
                station.transform.Find("Plant").gameObject,
                station.transform.Find("Plant/Sprout"),
                station.transform.Find("Plant/Leaves"),
                station.transform.Find("Plant/Soil").GetComponent<Renderer>(),
                station.transform.Find("Plant/Oxygen").GetComponent<ParticleSystem>(),
                station.transform.Find("Plant/PlantGlow").GetComponent<Light>(),
                station.transform.Find("PlacementRing").gameObject);

            // Reflection
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
            hub.Configure(null, header, prompt, status, reflection, new ILabExperiment[] { lab });
            // Don't auto-open via hub Enter twice — OpenExperiment calls Enter
            hub.OpenExperiment(0);

            resetBtn.onClick.AddListener(() => lab.ResetExperiment());
            reflectBtn.onClick.AddListener(() => reflection.Show(lab.Title, lab.Prompt));
            new GameObject("LabPerformanceSettings").AddComponent<LabPerformanceSettings>();

            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Light & Life (AR)",
                "Scene ready.\n\nPlay → allow camera → Place seedling →\npoint at a BRIGHT light, then cover for DARK.\n\nThe room is the controller.",
                "OK");
        }

        static GameObject BuildPlantStation()
        {
            var root = new GameObject("LightLabStation");
            root.transform.position = Vector3.zero;
            root.AddComponent<GuidedLightLabExperiment>();

            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "PlacementRing";
            ring.transform.SetParent(root.transform, false);
            ring.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            ring.transform.localScale = new Vector3(0.9f, 0.02f, 0.9f);
            Object.DestroyImmediate(ring.GetComponent<Collider>());
            SetColor(ring, new Color(0.2f, 0.7f, 0.9f, 0.5f));

            var plant = new GameObject("Plant");
            plant.transform.SetParent(root.transform, false);
            plant.SetActive(false);

            var soil = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            soil.name = "Soil";
            soil.transform.SetParent(plant.transform, false);
            soil.transform.localScale = new Vector3(0.9f, 0.18f, 0.9f);
            Object.DestroyImmediate(soil.GetComponent<Collider>());
            SetColor(soil, new Color(0.35f, 0.25f, 0.14f));

            var sprout = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            sprout.name = "Sprout";
            sprout.transform.SetParent(plant.transform, false);
            Object.DestroyImmediate(sprout.GetComponent<Collider>());
            SetColor(sprout, new Color(0.25f, 0.7f, 0.3f));

            var leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leaves.name = "Leaves";
            leaves.transform.SetParent(plant.transform, false);
            Object.DestroyImmediate(leaves.GetComponent<Collider>());
            SetColor(leaves, new Color(0.2f, 0.65f, 0.28f));

            var oxygenGo = new GameObject("Oxygen");
            oxygenGo.transform.SetParent(plant.transform, false);
            oxygenGo.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            var ps = oxygenGo.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startSize = 0.06f;
            main.startSpeed = 0.4f;
            main.startLifetime = 1.5f;
            main.startColor = new Color(0.6f, 1f, 0.75f, 0.8f);
            var emission = ps.emission;
            emission.rateOverTime = 12f;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var glowGo = new GameObject("PlantGlow");
            glowGo.transform.SetParent(plant.transform, false);
            glowGo.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            var glow = glowGo.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.range = 2.5f;
            glow.enabled = false;

            return root;
        }

        static void SetColor(GameObject go, Color color)
        {
            var r = go.GetComponent<Renderer>();
            if (r == null) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
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
            var text = Label(go.transform, "Label", label, 13, TextAnchor.MiddleCenter);
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
            var ph = Label(go.transform, "Placeholder", "Why couldn’t a normal UI button replace the camera here?", 14, TextAnchor.UpperLeft);
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
