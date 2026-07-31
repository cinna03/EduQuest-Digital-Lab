#if UNITY_EDITOR
using System.IO;
using EduQuest.AR;
using EduQuest.Experiments;
using EduQuest.Lab;
using EduQuest.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EduQuest.EditorTools
{
    public static class ChemistryLabBuilder
    {
        const string ScenePath = "Assets/Scenes/EduQuestLab.unity";
        const string PrefabPath = "Assets/Prefabs/CrystalBeaker.prefab";

        [MenuItem("EduQuest/Build Photographic Crystal Lab", priority = 0)]
        [MenuItem("EduQuest/Build Chemistry Light & Mix Lab", priority = 1)]
        public static void Build()
        {
            GlassSpriteImporter.Reimport();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.08f, 0.1f);
            camGo.transform.position = new Vector3(0f, 1.4f, -2.35f);
            camGo.transform.rotation = Quaternion.Euler(18f, 0f, 0f);
            camGo.AddComponent<AudioListener>();

            var sun = new GameObject("Sun");
            var dir = sun.AddComponent<Light>();
            dir.type = LightType.Directional;
            dir.intensity = 0.9f;
            dir.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(42f, -25f, 0f);

            var fill = new GameObject("FillLight");
            var fillL = fill.AddComponent<Light>();
            fillL.type = LightType.Directional;
            fillL.intensity = 0.25f;
            fillL.color = new Color(0.7f, 0.85f, 1f);
            fill.transform.rotation = Quaternion.Euler(20f, 140f, 0f);

            EnsureEventSystem();

            var room = new GameObject("DesktopARPreview");
            var hint = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hint.name = "PlacementHint";
            hint.transform.SetParent(room.transform, false);
            hint.transform.position = new Vector3(0f, 0.02f, 0.25f);
            hint.transform.localScale = new Vector3(0.55f, 0.01f, 0.55f);
            Object.DestroyImmediate(hint.GetComponent<Collider>());
            hint.GetComponent<Renderer>().sharedMaterial =
                LabGlassMaterials.MakeGlass(new Color(0.5f, 0.9f, 1f, 0.35f));

            var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "LabTable";
            table.transform.SetParent(room.transform, false);
            table.transform.position = new Vector3(0f, -0.04f, 0.35f);
            table.transform.localScale = new Vector3(2.0f, 0.06f, 1.2f);
            table.GetComponent<Renderer>().sharedMaterial =
                LabGlassMaterials.MakeSolid(new Color(0.18f, 0.16f, 0.15f), 0.25f);

            // Chem-feel glassware row (beaker / Erlenmeyer / cylinder / flask / bottles)
            LabPropFactory.CreateBottle(room.transform, ChemId.SilverNitrate, "AgNO₃", "A · Silver Nitrate",
                new Color(0.85f, 0.9f, 1f, 0.55f), new Vector3(-0.9f, 0f, 0.85f), GlasswareKind.ReagentBottle);
            LabPropFactory.CreateBottle(room.transform, ChemId.SodiumChloride, "NaCl", "B · Sodium Chloride",
                new Color(0.92f, 0.95f, 0.98f, 0.5f), new Vector3(-0.55f, 0f, 0.85f), GlasswareKind.Erlenmeyer);
            LabPropFactory.CreateBottle(room.transform, ChemId.SodiumThiosulfate, "Fixer", "C · Sodium Thiosulfate",
                new Color(0.65f, 0.85f, 0.95f, 0.55f), new Vector3(-0.2f, 0f, 0.85f), GlasswareKind.GraduatedCylinder);
            LabPropFactory.CreateBottle(room.transform, ChemId.DistilledWater, "H₂O", "D · Distilled Water",
                new Color(0.55f, 0.75f, 0.95f, 0.4f), new Vector3(0.15f, 0f, 0.85f), GlasswareKind.GriffinBeaker);
            LabPropFactory.CreateBottle(room.transform, ChemId.SodiumCarbonate, "Na₂CO₃", "E · Sodium Carbonate",
                new Color(0.75f, 0.7f, 0.55f, 0.55f), new Vector3(0.5f, 0f, 0.85f), GlasswareKind.RoundFlask);
            LabPropFactory.CreateBottle(room.transform, ChemId.CopperSulfate, "CuSO₄", "F · Copper Sulfate",
                new Color(0.15f, 0.5f, 0.9f, 0.7f), new Vector3(0.85f, 0f, 0.85f), GlasswareKind.Erlenmeyer);

            // Extra bench measuring cylinder (visual chem feel)
            LabPropFactory.CreateBenchGradCylinder(room.transform, new Vector3(1.15f, 0f, 0.55f));

            var beakerPrefab = EnsureBeakerPrefab();
            var labRoot = new GameObject("PhotographicCrystalLab");
            var lab = labRoot.AddComponent<PhotographicCrystalLab>();
            var sensor = labRoot.AddComponent<WorldLightSensor>();
            var placer = labRoot.AddComponent<TablePotPlacer>();
            placer.Configure(cam, beakerPrefab);
            var taps = labRoot.AddComponent<LabTapSelector>();
            taps.Configure(cam);

            var canvas = CreateCanvas();

            var previewPanel = GlassPanel(canvas.transform, "CameraPreviewPanel");
            Pin(previewPanel, 0.02f, 0.5f, 0.33f, 0.88f);
            var preview = new GameObject("CameraPreview", typeof(RectTransform));
            preview.transform.SetParent(previewPanel.transform, false);
            Stretch(preview.GetComponent<RectTransform>(), 14f);
            var raw = preview.AddComponent<RawImage>();
            raw.color = new Color(0.15f, 0.15f, 0.18f);
            var previewLabel = GlassLabel(previewPanel.transform, "PreviewLabel", "LIVE LIGHT · keep dark while mixing", 12);
            Pin(previewLabel.rectTransform, 0.06f, 0.86f, 0.94f, 0.97f);
            sensor.SetPreview(raw);

            var journalPanel = GlassPanel(canvas.transform, "JournalPanel");
            Pin(journalPanel, 0.02f, 0.22f, 0.33f, 0.49f);
            var journalTitle = GlassLabel(journalPanel.transform, "JournalTitle", "Lab Journal", 14, true);
            Pin(journalTitle.rectTransform, 0.07f, 0.82f, 0.93f, 0.96f);
            var journal = GlassLabel(journalPanel.transform, "JournalBody", "", 11);
            journal.alignment = TextAnchor.UpperLeft;
            Pin(journal.rectTransform, 0.07f, 0.06f, 0.93f, 0.8f);

            var top = GlassPanel(canvas.transform, "HUD");
            Pin(top, 0f, 0.9f, 1f, 1f);
            var header = GlassLabel(top.transform, "Header", "EduQuest · Photographic Crystal", 17, true);
            Pin(header.rectTransform, 0.03f, 0.15f, 0.48f, 0.85f);
            header.alignment = TextAnchor.MiddleLeft;
            var prompt = GlassLabel(top.transform, "Prompt", "Tap bottles → measure → pour · dark first, light last", 12);
            Pin(prompt.rectTransform, 0.48f, 0.15f, 0.82f, 0.85f);
            prompt.alignment = TextAnchor.MiddleLeft;
            var status = GlassLabel(top.transform, "Status", "", 1);
            status.color = Color.clear;
            var resetBtn = GlassBtn(top.transform, "ResetButton", "Waste", new Vector2(0.84f, 0.18f), new Vector2(0.91f, 0.82f));
            var reflectBtn = GlassBtn(top.transform, "ReflectButton", "Reflect", new Vector2(0.92f, 0.18f), new Vector2(0.99f, 0.82f));

            var guide = GlassPanel(canvas.transform, "GuidePanel");
            Pin(guide, 0.68f, 0.22f, 0.985f, 0.88f);
            var stepLabel = GlassLabel(guide.transform, "StepLabel", "Puzzle", 12);
            Pin(stepLabel.rectTransform, 0.08f, 0.9f, 0.92f, 0.98f);
            stepLabel.alignment = TextAnchor.UpperLeft;
            var guideTitle = GlassLabel(guide.transform, "GuideTitle", "Photographic Crystal", 16, true);
            Pin(guideTitle.rectTransform, 0.08f, 0.78f, 0.92f, 0.9f);
            guideTitle.alignment = TextAnchor.UpperLeft;
            var guideBody = GlassLabel(guide.transform, "GuideBody", "", 12);
            guideBody.alignment = TextAnchor.UpperLeft;
            Pin(guideBody.rectTransform, 0.08f, 0.3f, 0.92f, 0.78f);
            var reaction = GlassLabel(guide.transform, "ReactionText", "Reaction: —", 12);
            reaction.alignment = TextAnchor.UpperLeft;
            reaction.color = new Color(1f, 0.92f, 0.55f, 0.95f);
            Pin(reaction.rectTransform, 0.08f, 0.12f, 0.92f, 0.3f);
            var hintBtn = GlassBtn(guide.transform, "HintButton", "Repeat recipe", new Vector2(0.08f, 0.02f), new Vector2(0.92f, 0.1f));

            var bar = GlassPanel(canvas.transform, "ActionBar");
            Pin(bar, 0f, 0f, 1f, 0.2f);

            var lightFill = FillBar(bar.transform, "LightMeter", 0.02f, 0.7f, 0.2f, 0.92f, new Color(1f, 0.9f, 0.35f));
            var lightState = GlassLabel(bar.transform, "LightState", "LIGHT: —", 12);
            Pin(lightState.rectTransform, 0.02f, 0.45f, 0.2f, 0.68f);
            lightState.alignment = TextAnchor.MiddleLeft;
            var meterLabel = GlassLabel(bar.transform, "MeterLabel", "Ag 0 · Cl 0 · Fix 0", 11);
            Pin(meterLabel.rectTransform, 0.02f, 0.22f, 0.2f, 0.44f);
            meterLabel.alignment = TextAnchor.MiddleLeft;
            var scoreText = GlassLabel(bar.transform, "ScoreText", "Score —/100", 11);
            Pin(scoreText.rectTransform, 0.02f, 0.02f, 0.2f, 0.2f);
            scoreText.alignment = TextAnchor.MiddleLeft;
            scoreText.color = new Color(0.75f, 1f, 0.85f);

            string[] names = { "A AgNO₃", "B NaCl", "C Fixer", "D Water", "E Carb.", "F CuSO₄" };
            var chemBtns = new Button[6];
            for (int i = 0; i < 6; i++)
            {
                float x0 = 0.22f + i * 0.09f;
                chemBtns[i] = GlassBtn(bar.transform, "Chem" + i, names[i],
                    new Vector2(x0, 0.55f), new Vector2(x0 + 0.085f, 0.92f));
            }

            var selectedLabel = GlassLabel(bar.transform, "SelectedLabel", "Bottle: —", 12);
            Pin(selectedLabel.rectTransform, 0.22f, 0.28f, 0.42f, 0.52f);
            selectedLabel.alignment = TextAnchor.MiddleLeft;
            var measureLabel = GlassLabel(bar.transform, "MeasureLabel", "Measure: 10 ml", 12);
            Pin(measureLabel.rectTransform, 0.43f, 0.28f, 0.58f, 0.52f);
            measureLabel.alignment = TextAnchor.MiddleLeft;

            var m5 = GlassBtn(bar.transform, "Measure5", "5 ml", new Vector2(0.22f, 0.05f), new Vector2(0.32f, 0.26f));
            var m10 = GlassBtn(bar.transform, "Measure10", "10 ml", new Vector2(0.33f, 0.05f), new Vector2(0.44f, 0.26f));
            var pour = GlassBtn(bar.transform, "PourButton", "Pour", new Vector2(0.46f, 0.05f), new Vector2(0.58f, 0.26f));
            var waste = GlassBtn(bar.transform, "WasteButton", "Waste/Reset", new Vector2(0.6f, 0.05f), new Vector2(0.74f, 0.26f));
            var safety = GlassLabel(bar.transform, "Safety", "SIMULATION ONLY", 11, true);
            Pin(safety.rectTransform, 0.76f, 0.05f, 0.98f, 0.26f);
            safety.color = new Color(1f, 0.65f, 0.5f);

            lab.Bind(
                placer, sensor, hint,
                stepLabel, guideTitle, guideBody, reaction, meterLabel, safety,
                journal, scoreText, measureLabel, selectedLabel, lightState,
                lightFill, chemBtns, m5, m10, pour, waste, hintBtn, taps);

            var reflectionGo = new GameObject("ReflectionUI");
            var reflection = reflectionGo.AddComponent<ReflectionUI>();
            var refPanel = GlassPanel(canvas.transform, "ReflectionPanel");
            var refRt = refPanel.GetComponent<RectTransform>();
            refRt.anchorMin = refRt.anchorMax = new Vector2(0.5f, 0.5f);
            refRt.sizeDelta = new Vector2(560f, 340f);
            var refTitle = GlassLabel(refPanel.transform, "RefTitle", "Reflection", 20, true);
            Pin(refTitle.rectTransform, 0.06f, 0.82f, 0.94f, 0.96f);
            var refPrompt = GlassLabel(refPanel.transform, "RefPrompt", "", 14);
            refPrompt.alignment = TextAnchor.UpperLeft;
            Pin(refPrompt.rectTransform, 0.07f, 0.55f, 0.93f, 0.8f);
            var input = MakeInput(refPanel.transform);
            Pin(input.GetComponent<RectTransform>(), 0.07f, 0.28f, 0.93f, 0.52f);
            var feedback = GlassLabel(refPanel.transform, "Feedback", "", 14);
            feedback.alignment = TextAnchor.MiddleLeft;
            Pin(feedback.rectTransform, 0.07f, 0.16f, 0.93f, 0.26f);
            var submit = GlassBtn(refPanel.transform, "Submit", "Submit", new Vector2(0.15f, 0.04f), new Vector2(0.45f, 0.14f));
            var close = GlassBtn(refPanel.transform, "Close", "Close", new Vector2(0.55f, 0.04f), new Vector2(0.85f, 0.14f));
            reflection.Configure(refPanel, refTitle, refPrompt, input, feedback, submit, close);
            refPanel.SetActive(false);

            var hub = new GameObject("LabHub").AddComponent<LabHub>();
            hub.Configure(null, header, prompt, status, reflection, new ILabExperiment[] { lab });
            hub.OpenExperiment(0);

            resetBtn.onClick.AddListener(() => lab.ResetExperiment());
            reflectBtn.onClick.AddListener(() => reflection.Show(lab.Title, lab.Prompt));
            new GameObject("LabPerformanceSettings").AddComponent<LabPerformanceSettings>();

            Directory.CreateDirectory("Assets/Scenes");
            Directory.CreateDirectory("Assets/Prefabs");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Photographic Crystal · Glass Lab",
                "1) Play → allow camera\n2) Tap table → place Griffin beaker (600 ml look)\n3) Tap chem glassware (Erlenmeyer / cylinder / flask…)\n4) 5/10 ml → Pour\n5) Dark mix → fixer → bright light\n\nGlassware now has a professional chem feel.",
                "OK");
        }

        static GameObject EnsureBeakerPrefab()
        {
            Directory.CreateDirectory("Assets/Prefabs");
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
                AssetDatabase.DeleteAsset(PrefabPath);

            var root = LabPropFactory.CreateCrystalBeakerRoot();
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        static GameObject GlassPanel(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            GlassUi.StylePanel(img, true);
            return go;
        }

        static Text GlassLabel(Transform parent, string name, string value, int size, bool bold = false)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            GlassUi.StyleText(text, size, bold);
            text.text = value;
            text.alignment = TextAnchor.MiddleCenter;
            return text;
        }

        static Button GlassBtn(Transform parent, string name, string label, Vector2 aMin, Vector2 aMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            GlassUi.StylePill(img);
            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.85f, 0.95f, 1f, 1f);
            colors.pressedColor = new Color(0.7f, 0.85f, 0.95f, 1f);
            btn.colors = colors;
            var text = GlassLabel(go.transform, "Label", label, 12, true);
            text.color = new Color(0.08f, 0.12f, 0.18f, 0.95f);
            Stretch(text.rectTransform, 6f);
            return btn;
        }

        static Image FillBar(Transform parent, string name, float x0, float y0, float x1, float y1, Color color)
        {
            var bg = new GameObject(name + "Bg", typeof(RectTransform));
            bg.transform.SetParent(parent, false);
            Pin(bg.GetComponent<RectTransform>(), x0, y0, x1, y1);
            var bgImg = bg.AddComponent<Image>();
            GlassUi.StylePill(bgImg);
            bgImg.color = new Color(1f, 1f, 1f, 0.35f);
            var fillGo = new GameObject(name, typeof(RectTransform));
            fillGo.transform.SetParent(bg.transform, false);
            Stretch(fillGo.GetComponent<RectTransform>(), 4f);
            var fill = fillGo.AddComponent<Image>();
            fill.color = color;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillAmount = 0.1f;
            return fill;
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

        static InputField MakeInput(Transform parent)
        {
            var go = new GameObject("Input", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            GlassUi.StylePill(img);
            img.color = new Color(1f, 1f, 1f, 0.85f);
            var input = go.AddComponent<InputField>();
            var text = GlassLabel(go.transform, "Text", "", 14);
            Stretch(text.rectTransform, 8f);
            text.color = new Color(0.08f, 0.1f, 0.15f);
            text.alignment = TextAnchor.UpperLeft;
            var ph = GlassLabel(go.transform, "Placeholder", "Why form AgCl in the dark before fixer and light?", 13);
            Stretch(ph.rectTransform, 8f);
            ph.color = new Color(0.25f, 0.3f, 0.38f, 0.8f);
            ph.alignment = TextAnchor.UpperLeft;
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
