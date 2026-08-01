using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EduQuest
{
    /// <summary>
    /// Editor workspace bootstrap: clear experiment kit + on-screen UI.
    /// </summary>
    public class EditorLabApp : MonoBehaviour
    {
        [SerializeField] ExperimentHud ui;
        [SerializeField] Transform kitRoot;
        [SerializeField] Light keyLight;
        [SerializeField] Light fillLight;
        [SerializeField] Camera viewCamera;
        [SerializeField] EditorCrystalExperiment experiment;

        public void Configure(
            GuideHud guide,
            Transform kit,
            Light key,
            Light fill,
            Camera cam)
        {
            // guide ignored — ExperimentHud replaces GuideHud for clarity
            kitRoot = kit;
            keyLight = key;
            fillLight = fill;
            viewCamera = cam;
        }

        void Start()
        {
            viewCamera = viewCamera != null ? viewCamera : Camera.main;
            if (keyLight == null)
            {
                var sun = GameObject.Find("Sun");
                if (sun != null) keyLight = sun.GetComponent<Light>();
            }
            if (fillLight == null)
            {
                var fill = GameObject.Find("Fill");
                if (fill != null) fillLight = fill.GetComponent<Light>();
            }

            ui = EnsureUi();
            kitRoot = EnsureExperimentKit();

            if (experiment == null)
                experiment = GetComponent<EditorCrystalExperiment>()
                             ?? gameObject.AddComponent<EditorCrystalExperiment>();

            experiment.Configure(ui, kitRoot, keyLight, fillLight, viewCamera);
            experiment.Begin();
        }

        ExperimentHud EnsureUi()
        {
            if (ui != null) return ui;
            var existing = FindAnyObjectByType<ExperimentHud>();
            if (existing != null) return existing;

            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasGo = new GameObject("Canvas", typeof(RectTransform));
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            // Hide old GuideHud clutter if present
            foreach (var g in FindObjectsByType<GuideHud>(FindObjectsSortMode.None))
                g.gameObject.SetActive(false);

            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            return ExperimentHud.Create(canvas.transform);
        }

        Transform EnsureExperimentKit()
        {
            var room = GameObject.Find("EditorRoom");
            var parent = room != null ? room.transform : null;
            var spawnPos = new Vector3(0f, 0.03f, 0.4f);
            var spawnRot = Quaternion.identity;

            var old = GameObject.Find("LabKit");
            if (old != null)
            {
                spawnPos = old.transform.position;
                spawnRot = Quaternion.identity; // force upright kit root
                if (parent == null) parent = old.transform.parent;
                old.name = "LabKit_OLD";
                old.SetActive(false);
                Destroy(old);
            }

            if (parent == null)
                parent = new GameObject("EditorRoom").transform;

            var kit = LabFactory.CreateLabKit(parent, spawnPos, spawnRot, forExperiment: true);
            kit.name = "LabKit";
            Debug.Log($"[EduQuest] Experiment kit ready ({kit.transform.childCount} pieces). Use on-screen buttons.");
            return kit.transform;
        }
    }
}
