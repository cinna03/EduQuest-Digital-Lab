using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EduQuest
{
    /// <summary>Editor bootstrap for the timed riddle combat campaign (no chemistry lab).</summary>
    public class EditorLabApp : MonoBehaviour
    {
        [SerializeField] CampaignHud campaignHud;
        [SerializeField] Transform arenaRoot;
        [SerializeField] Light keyLight;
        [SerializeField] Light fillLight;
        [SerializeField] Camera viewCamera;
        [SerializeField] CampaignFlow campaign;

        public void Configure(Transform arena, Light key, Light fill, Camera cam)
        {
            arenaRoot = arena;
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
            campaignHud = campaignHud != null ? campaignHud : CampaignHud.Create(canvas.transform);
            arenaRoot = arenaRoot != null ? arenaRoot : EnsureArena();

            if (campaign == null)
                campaign = GetComponent<CampaignFlow>() ?? gameObject.AddComponent<CampaignFlow>();
            campaign.Configure(campaignHud, arenaRoot, keyLight, fillLight, viewCamera);
            campaign.Begin();
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

        Transform EnsureArena()
        {
            var room = GameObject.Find("EditorRoom");
            if (room == null)
                room = new GameObject("EditorRoom");

            var marker = room.transform.Find("ArenaCenter");
            if (marker == null)
            {
                var go = new GameObject("ArenaCenter");
                go.transform.SetParent(room.transform, false);
                go.transform.position = new Vector3(0f, 0.03f, 0.4f);
                marker = go.transform;
            }

            return marker;
        }
    }
}
