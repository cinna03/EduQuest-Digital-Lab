using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EduQuest
{
    /// <summary>
    /// Editor workspace: 3-level campaign first, lab mix unlocked at the end.
    /// </summary>
    public class EditorLabApp : MonoBehaviour
    {
        [SerializeField] CampaignHud campaignHud;
        [SerializeField] ExperimentHud labHud;
        [SerializeField] Transform kitRoot;
        [SerializeField] Light keyLight;
        [SerializeField] Light fillLight;
        [SerializeField] Camera viewCamera;
        [SerializeField] CampaignFlow campaign;
        [SerializeField] EditorCrystalExperiment experiment;

        public void Configure(
            GuideHud guide,
            Transform kit,
            Light key,
            Light fill,
            Camera cam)
        {
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

            EnsureEventSystemAndCanvas(out var canvas);
            HideLegacyGuideHud();

            campaignHud = campaignHud != null ? campaignHud : CampaignHud.Create(canvas.transform);
            labHud = labHud != null ? labHud : EnsureLabHud(canvas.transform);
            // Lab HUD stays available for DARK/LIGHT during light gate + mix
            kitRoot = EnsureExperimentKit();

            if (experiment == null)
                experiment = GetComponent<EditorCrystalExperiment>()
                             ?? gameObject.AddComponent<EditorCrystalExperiment>();
            experiment.Configure(labHud, kitRoot, keyLight, fillLight, viewCamera);
            // Do not Begin lab yet — campaign unlocks it

            if (campaign == null)
                campaign = GetComponent<CampaignFlow>() ?? gameObject.AddComponent<CampaignFlow>();
            campaign.Configure(campaignHud, kitRoot, keyLight, fillLight, viewCamera, experiment);
            campaign.Begin();

            // Let light-gate use ExperimentHud DARK/LIGHT without starting mix logic early
            labHud.DarkRequested += OnLabDarkRequested;
        }

        void OnDestroy()
        {
            if (labHud != null)
                labHud.DarkRequested -= OnLabDarkRequested;
        }

        void OnLabDarkRequested(bool dark)
        {
            if (keyLight != null) keyLight.intensity = dark ? 0.12f : 1.15f;
            if (fillLight != null) fillLight.intensity = dark ? 0.05f : 0.35f;
        }

        void EnsureEventSystemAndCanvas(out Canvas canvas)
        {
            canvas = FindAnyObjectByType<Canvas>();
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

            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }
        }

        void HideLegacyGuideHud()
        {
            foreach (var g in FindObjectsByType<GuideHud>(FindObjectsSortMode.None))
                g.gameObject.SetActive(false);
        }

        ExperimentHud EnsureLabHud(Transform canvas)
        {
            var existing = FindAnyObjectByType<ExperimentHud>();
            if (existing != null) return existing;
            return ExperimentHud.Create(canvas);
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
                spawnRot = Quaternion.identity;
                if (parent == null) parent = old.transform.parent;
                old.name = "LabKit_OLD";
                old.SetActive(false);
                Destroy(old);
            }

            if (parent == null)
                parent = new GameObject("EditorRoom").transform;

            var kit = LabFactory.CreateLabKit(parent, spawnPos, spawnRot, forExperiment: true);
            kit.name = "LabKit";
            kit.SetActive(false); // hidden until Level 3 lab
            Debug.Log("[EduQuest] Campaign ready — kit hidden until light gate + lab unlock.");
            return kit.transform;
        }
    }
}
