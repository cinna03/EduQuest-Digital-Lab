#if UNITY_EDITOR
using System.IO;
using EduQuest.Experiments;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EduQuest.EditorTools
{
    public static class LabSceneBuilder
    {
        const string ScenePath = "Assets/Scenes/EduQuestLab.unity";

        [MenuItem("EduQuest/Build Digital Lab Scene", priority = 0)]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cam = new GameObject("Main Camera");
            cam.tag = "MainCamera";
            var camera = cam.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.16f, 0.2f);
            cam.transform.position = new Vector3(0f, 1.6f, -4.2f);
            cam.transform.rotation = Quaternion.Euler(12f, 0f, 0f);
            cam.AddComponent<AudioListener>();

            var sun = new GameObject("Sun");
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            sun.transform.rotation = Quaternion.Euler(40f, -25f, 0f);

            EnsureEventSystem();
            var canvas = CreateCanvas();

            // Stations
            var germRoot = BuildGerminationStation();
            var pendRoot = BuildPendulumStation();
            var flameRoot = BuildFlameStation();
            germRoot.SetActive(false);
            pendRoot.SetActive(false);
            flameRoot.SetActive(false);

            var germ = germRoot.GetComponent<GerminationExperiment>();
            var pend = pendRoot.GetComponent<PendulumExperiment>();
            var flame = flameRoot.GetComponent<BlueFlameExperiment>();

            // HUD
            var hud = CreatePanel(canvas.transform, "HUD", new Color(0.05f, 0.08f, 0.1f, 0.82f));
            var hudRt = hud.GetComponent<RectTransform>();
            hudRt.anchorMin = new Vector2(0f, 0.72f);
            hudRt.anchorMax = Vector2.one;
            hudRt.offsetMin = Vector2.zero;
            hudRt.offsetMax = Vector2.zero;

            var header = CreateText(hud.transform, "Header", "EduQuest Digital Lab", 26, TextAnchor.UpperLeft);
            Pin(header.rectTransform, 0.02f, 0.55f, 0.7f, 0.95f);
            var prompt = CreateText(hud.transform, "Prompt", "Choose an experiment.", 16, TextAnchor.UpperLeft);
            Pin(prompt.rectTransform, 0.02f, 0.15f, 0.75f, 0.55f);
            var status = CreateText(hud.transform, "Status", "", 14, TextAnchor.LowerLeft);
            Pin(status.rectTransform, 0.02f, 0.02f, 0.75f, 0.18f);

            var backBtn = CreateButton(hud.transform, "MenuButton", "Labs", new Vector2(0.82f, 0.55f), new Vector2(0.96f, 0.9f));
            var resetBtn = CreateButton(hud.transform, "ResetButton", "Reset", new Vector2(0.82f, 0.2f), new Vector2(0.96f, 0.5f));
            var reflectBtn = CreateButton(hud.transform, "ReflectButton", "Reflect", new Vector2(0.66f, 0.2f), new Vector2(0.8f, 0.5f));

            // Menu
            var menu = CreatePanel(canvas.transform, "MenuPanel", new Color(0.06f, 0.1f, 0.14f, 0.94f));
            Stretch(menu.GetComponent<RectTransform>(), 80f, 70f, 80f, 90f);
            var menuTitle = CreateText(menu.transform, "MenuTitle", "EduQuest Digital Lab\nSafe virtual experiments for youth science", 22, TextAnchor.UpperCenter);
            Pin(menuTitle.rectTransform, 0.05f, 0.72f, 0.95f, 0.95f);

            var b1 = CreateButton(menu.transform, "GerminationButton", "1 · Science: Germination", new Vector2(0.2f, 0.48f), new Vector2(0.8f, 0.62f));
            var b2 = CreateButton(menu.transform, "PendulumButton", "2 · Physics: Pendulum", new Vector2(0.2f, 0.32f), new Vector2(0.8f, 0.46f));
            var b3 = CreateButton(menu.transform, "FlameButton", "3 · Chemistry: Dancing Blue Flame", new Vector2(0.2f, 0.16f), new Vector2(0.8f, 0.3f));

            // Reflection
            var reflectionGo = new GameObject("ReflectionUI");
            var reflection = reflectionGo.AddComponent<ReflectionUI>();
            var refPanel = CreatePanel(canvas.transform, "ReflectionPanel", new Color(0.05f, 0.09f, 0.12f, 0.96f));
            var refRt = refPanel.GetComponent<RectTransform>();
            refRt.anchorMin = new Vector2(0.5f, 0.5f);
            refRt.anchorMax = new Vector2(0.5f, 0.5f);
            refRt.sizeDelta = new Vector2(540f, 340f);
            var refTitle = CreateText(refPanel.transform, "RefTitle", "Reflection", 20, TextAnchor.UpperCenter);
            Pin(refTitle.rectTransform, 0.05f, 0.82f, 0.95f, 0.96f);
            var refPrompt = CreateText(refPanel.transform, "RefPrompt", "", 15, TextAnchor.UpperLeft);
            Pin(refPrompt.rectTransform, 0.06f, 0.55f, 0.94f, 0.8f);
            var input = CreateInput(refPanel.transform);
            Pin(input.GetComponent<RectTransform>(), 0.06f, 0.28f, 0.94f, 0.52f);
            var feedback = CreateText(refPanel.transform, "Feedback", "", 14, TextAnchor.MiddleLeft);
            Pin(feedback.rectTransform, 0.06f, 0.16f, 0.94f, 0.26f);
            var submit = CreateButton(refPanel.transform, "Submit", "Submit", new Vector2(0.15f, 0.04f), new Vector2(0.45f, 0.14f));
            var close = CreateButton(refPanel.transform, "Close", "Close", new Vector2(0.55f, 0.04f), new Vector2(0.85f, 0.14f));
            reflection.Configure(refPanel, refTitle, refPrompt, input, feedback, submit, close);

            // Controls per experiment (bottom strip)
            var controls = CreatePanel(canvas.transform, "Controls", new Color(0.05f, 0.08f, 0.1f, 0.88f));
            var cRt = controls.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0f, 0f);
            cRt.anchorMax = new Vector2(1f, 0.22f);
            cRt.offsetMin = Vector2.zero;
            cRt.offsetMax = Vector2.zero;

            var germControls = new GameObject("GerminationControls");
            germControls.transform.SetParent(controls.transform, false);
            Stretch(germControls.AddComponent<RectTransform>());
            var water = CreateLabeledSlider(germControls.transform, "Water", 0.72f);
            var warmth = CreateLabeledSlider(germControls.transform, "Warmth", 0.42f);
            var days = CreateLabeledSlider(germControls.transform, "Days", 0.12f);
            germ.Bind(water, warmth, days,
                germRoot.transform.Find("Seed"),
                germRoot.transform.Find("Sprout"),
                germRoot.transform.Find("Leaves"),
                germRoot.transform.Find("Soil").GetComponent<Renderer>(),
                germRoot.transform.Find("GrowLight").GetComponent<Light>());
            germControls.SetActive(false);

            var pendControls = new GameObject("PendulumControls");
            pendControls.transform.SetParent(controls.transform, false);
            Stretch(pendControls.AddComponent<RectTransform>());
            var length = CreateLabeledSlider(pendControls.transform, "Length", 0.55f);
            var mass = CreateLabeledSlider(pendControls.transform, "Mass", 0.2f);
            var readout = CreateText(pendControls.transform, "Readout", "", 13, TextAnchor.UpperLeft);
            Pin(readout.rectTransform, 0.52f, 0.15f, 0.98f, 0.85f);
            pend.Bind(length, mass,
                pendRoot.transform.Find("Pivot"),
                pendRoot.transform.Find("Bob"),
                pendRoot.transform.Find("Cord").GetComponent<LineRenderer>(),
                readout);
            pendControls.SetActive(false);

            var flameControls = new GameObject("FlameControls");
            flameControls.transform.SetParent(controls.transform, false);
            Stretch(flameControls.AddComponent<RectTransform>());
            var acid = CreateLabeledSlider(flameControls.transform, "Acid amount (HCl)", 0.55f);
            var ignite = CreateButton(flameControls.transform, "IgniteButton", "Ignite H₂", new Vector2(0.55f, 0.25f), new Vector2(0.85f, 0.75f));
            var safety = CreateText(flameControls.transform, "Safety", "", 12, TextAnchor.LowerLeft);
            Pin(safety.rectTransform, 0.02f, 0.02f, 0.98f, 0.22f);
            flame.Bind(acid, ignite,
                flameRoot.transform.Find("Bubbles").GetComponent<ParticleSystem>(),
                flameRoot.transform.Find("Flame").GetComponent<ParticleSystem>(),
                flameRoot.transform.Find("Liquid").GetComponent<Renderer>(),
                flameRoot.transform.Find("FlameLight").GetComponent<Light>(),
                safety);
            flameControls.SetActive(false);

            var hubGo = new GameObject("LabHub");
            var hub = hubGo.AddComponent<LabHub>();
            hub.Configure(menu, header, prompt, status, reflection, new ILabExperiment[] { germ, pend, flame });

            void ShowControls(int i)
            {
                germControls.SetActive(i == 0);
                pendControls.SetActive(i == 1);
                flameControls.SetActive(i == 2);
            }

            b1.onClick.AddListener(() => { hub.OpenExperiment(0); ShowControls(0); });
            b2.onClick.AddListener(() => { hub.OpenExperiment(1); ShowControls(1); });
            b3.onClick.AddListener(() => { hub.OpenExperiment(2); ShowControls(2); });
            backBtn.onClick.AddListener(() => { hub.ShowMenu(); ShowControls(-1); germControls.SetActive(false); pendControls.SetActive(false); flameControls.SetActive(false); });
            resetBtn.onClick.AddListener(() => hub.ResetActive());
            reflectBtn.onClick.AddListener(() => hub.OpenReflection());

            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddBuildSettings(ScenePath);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("EduQuest", "Scene built:\n" + ScenePath + "\n\nPress Play, pick a lab, experiment, then Reflect.", "OK");
        }

        static GameObject BuildGerminationStation()
        {
            var root = new GameObject("GerminationStation");
            root.transform.position = new Vector3(0f, 0f, 0f);
            root.AddComponent<GerminationExperiment>();

            var pot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pot.name = "Soil";
            pot.transform.SetParent(root.transform, false);
            pot.transform.localScale = new Vector3(1.4f, 0.25f, 1.4f);
            Object.DestroyImmediate(pot.GetComponent<Collider>());
            SetColor(pot, new Color(0.35f, 0.25f, 0.14f));

            var seed = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            seed.name = "Seed";
            seed.transform.SetParent(root.transform, false);
            seed.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            seed.transform.localScale = Vector3.one * 0.18f;
            Object.DestroyImmediate(seed.GetComponent<Collider>());
            SetColor(seed, new Color(0.4f, 0.28f, 0.12f));

            var sprout = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            sprout.name = "Sprout";
            sprout.transform.SetParent(root.transform, false);
            Object.DestroyImmediate(sprout.GetComponent<Collider>());
            SetColor(sprout, new Color(0.25f, 0.7f, 0.3f));

            var leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leaves.name = "Leaves";
            leaves.transform.SetParent(root.transform, false);
            leaves.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            Object.DestroyImmediate(leaves.GetComponent<Collider>());
            SetColor(leaves, new Color(0.2f, 0.65f, 0.28f));

            var grow = new GameObject("GrowLight");
            grow.transform.SetParent(root.transform, false);
            grow.transform.localPosition = new Vector3(0.8f, 1.5f, -0.5f);
            var gl = grow.AddComponent<Light>();
            gl.type = LightType.Point;
            gl.range = 5f;
            gl.color = new Color(1f, 0.95f, 0.8f);
            return root;
        }

        static GameObject BuildPendulumStation()
        {
            var root = new GameObject("PendulumStation");
            root.transform.position = Vector3.zero;
            root.AddComponent<PendulumExperiment>();

            var stand = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stand.name = "Stand";
            stand.transform.SetParent(root.transform, false);
            stand.transform.localPosition = new Vector3(0f, 1.2f, 0.4f);
            stand.transform.localScale = new Vector3(1.6f, 0.08f, 0.08f);
            Object.DestroyImmediate(stand.GetComponent<Collider>());
            SetColor(stand, new Color(0.5f, 0.5f, 0.55f));

            var pivot = new GameObject("Pivot");
            pivot.transform.SetParent(root.transform, false);
            pivot.transform.position = new Vector3(0f, 1.2f, 0f);

            var bob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bob.name = "Bob";
            bob.transform.SetParent(root.transform, false);
            Object.DestroyImmediate(bob.GetComponent<Collider>());
            SetColor(bob, new Color(0.85f, 0.55f, 0.15f));

            var cordGo = new GameObject("Cord");
            cordGo.transform.SetParent(root.transform, false);
            var lr = cordGo.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Standard"));
            lr.startWidth = 0.02f;
            lr.endWidth = 0.02f;
            lr.startColor = Color.white;
            lr.endColor = Color.white;
            return root;
        }

        static GameObject BuildFlameStation()
        {
            var root = new GameObject("FlameStation");
            root.transform.position = Vector3.zero;
            root.AddComponent<BlueFlameExperiment>();

            var flask = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            flask.name = "Flask";
            flask.transform.SetParent(root.transform, false);
            flask.transform.localScale = new Vector3(0.7f, 0.55f, 0.7f);
            flask.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            Object.DestroyImmediate(flask.GetComponent<Collider>());
            SetColor(flask, new Color(0.7f, 0.85f, 0.95f, 0.25f));

            var liquid = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            liquid.name = "Liquid";
            liquid.transform.SetParent(root.transform, false);
            liquid.transform.localScale = new Vector3(0.62f, 0.28f, 0.62f);
            liquid.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            Object.DestroyImmediate(liquid.GetComponent<Collider>());
            SetColor(liquid, new Color(0.75f, 0.85f, 0.95f, 0.5f));

            var metal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            metal.name = "Aluminium";
            metal.transform.SetParent(root.transform, false);
            metal.transform.localScale = new Vector3(0.18f, 0.05f, 0.12f);
            metal.transform.localPosition = new Vector3(0f, 0.22f, 0f);
            Object.DestroyImmediate(metal.GetComponent<Collider>());
            SetColor(metal, new Color(0.75f, 0.78f, 0.8f));

            var bubbles = CreateParticles(root.transform, "Bubbles", new Vector3(0f, 0.55f, 0f), new Color(0.8f, 0.9f, 1f, 0.7f), 0.08f, 1.2f);
            var flame = CreateParticles(root.transform, "Flame", new Vector3(0f, 1.05f, 0f), new Color(0.4f, 0.65f, 1f, 0.85f), 0.15f, 0.6f);
            var fl = flame.main;
            fl.startSpeed = 0.4f;
            var shape = flame.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 12f;

            var flameLight = new GameObject("FlameLight");
            flameLight.transform.SetParent(root.transform, false);
            flameLight.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            var l = flameLight.AddComponent<Light>();
            l.type = LightType.Point;
            l.range = 3f;
            l.color = new Color(0.35f, 0.55f, 1f);
            l.enabled = false;
            return root;
        }

        static ParticleSystem CreateParticles(Transform parent, string name, Vector3 pos, Color color, float size, float speed)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startSize = size;
            main.startSpeed = speed;
            main.startLifetime = 1.2f;
            main.startColor = color;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.15f;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return ps;
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

        static Canvas CreateCanvas()
        {
            var go = new GameObject("Canvas", typeof(RectTransform));
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            es.AddComponent<StandaloneInputModule>();
#endif
        }

        static GameObject CreatePanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.AddComponent<Image>().color = color;
            return go;
        }

        static Text CreateText(Transform parent, string name, string content, int size, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                        ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.text = content;
            text.fontSize = size;
            text.color = Color.white;
            text.alignment = anchor;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        static Button CreateButton(Transform parent, string name, string label, Vector2 aMin, Vector2 aMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            go.AddComponent<Image>().color = new Color(0.12f, 0.5f, 0.72f);
            var btn = go.AddComponent<Button>();
            var text = CreateText(go.transform, "Label", label, 15, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);
            return btn;
        }

        static Slider CreateLabeledSlider(Transform parent, string label, float yAnchor)
        {
            var row = new GameObject(label + "Row", typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var rt = row.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.02f, yAnchor);
            rt.anchorMax = new Vector2(0.5f, yAnchor + 0.22f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var t = CreateText(row.transform, "Label", label, 13, TextAnchor.MiddleLeft);
            Pin(t.rectTransform, 0f, 0f, 0.32f, 1f);

            var sliderGo = new GameObject("Slider", typeof(RectTransform));
            sliderGo.transform.SetParent(row.transform, false);
            Pin(sliderGo.GetComponent<RectTransform>(), 0.34f, 0.2f, 1f, 0.8f);
            var bg = new GameObject("Background", typeof(RectTransform));
            bg.transform.SetParent(sliderGo.transform, false);
            Stretch(bg.GetComponent<RectTransform>());
            bg.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.2f);
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderGo.transform, false);
            Stretch(fillArea.GetComponent<RectTransform>(), 5f, 0f, 5f, 0f);
            var fill = new GameObject("Fill", typeof(RectTransform));
            fill.transform.SetParent(fillArea.transform, false);
            Stretch(fill.GetComponent<RectTransform>());
            fill.AddComponent<Image>().color = new Color(0.25f, 0.75f, 0.85f);
            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(sliderGo.transform, false);
            Stretch(handleArea.GetComponent<RectTransform>());
            var handle = new GameObject("Handle", typeof(RectTransform));
            handle.transform.SetParent(handleArea.transform, false);
            handle.GetComponent<RectTransform>().sizeDelta = new Vector2(16f, 16f);
            handle.AddComponent<Image>().color = Color.white;
            var slider = sliderGo.AddComponent<Slider>();
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.3f;
            return slider;
        }

        static InputField CreateInput(Transform parent)
        {
            var go = new GameObject("Input", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.AddComponent<Image>().color = Color.white;
            var input = go.AddComponent<InputField>();
            var text = CreateText(go.transform, "Text", "", 15, TextAnchor.UpperLeft);
            Stretch(text.rectTransform, 8f);
            text.color = Color.black;
            var ph = CreateText(go.transform, "Placeholder", "Type one sentence…", 15, TextAnchor.UpperLeft);
            Stretch(ph.rectTransform, 8f);
            ph.color = new Color(0.4f, 0.4f, 0.4f);
            input.textComponent = text;
            input.placeholder = ph;
            input.lineType = InputField.LineType.MultiLineNewline;
            return input;
        }

        static void Stretch(RectTransform rt, float pad = 0f) => Stretch(rt, pad, pad, pad, pad);

        static void Stretch(RectTransform rt, float l, float b, float r, float t)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(l, b);
            rt.offsetMax = new Vector2(-r, -t);
        }

        static void Pin(RectTransform rt, float x0, float y0, float x1, float y1)
        {
            rt.anchorMin = new Vector2(x0, y0);
            rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static void AddBuildSettings(string path)
        {
            foreach (var s in EditorBuildSettings.scenes)
                if (s.path == path) return;
            var list = new EditorBuildSettingsScene[EditorBuildSettings.scenes.Length + 1];
            EditorBuildSettings.scenes.CopyTo(list, 0);
            list[^1] = new EditorBuildSettingsScene(path, true);
            EditorBuildSettings.scenes = list;
        }
    }
}
#endif
