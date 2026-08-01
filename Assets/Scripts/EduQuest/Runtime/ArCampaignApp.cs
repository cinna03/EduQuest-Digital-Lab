using UnityEngine;

namespace EduQuest
{
    /// <summary>Phone AR entry: place arena on a table, then run the timed riddle campaign.</summary>
    public class ArCampaignApp : MonoBehaviour
    {
        [SerializeField] CampaignHud hud;
        [SerializeField] ArTablePlacer placer;
        [SerializeField] GameObject desktopPreview;
        [SerializeField] GameObject arSession;
        [SerializeField] GameObject xrOrigin;
        [SerializeField] Camera deskCam;
        [SerializeField] Camera arCam;
        [SerializeField] CampaignFlow campaign;

        Transform m_Arena;
        bool m_Started;

        public void Configure(
            CampaignHud campaignHud,
            ArTablePlacer tablePlacer,
            GameObject desktop,
            GameObject session,
            GameObject origin,
            Camera desktopCamera,
            Camera phoneCamera)
        {
            hud = campaignHud;
            placer = tablePlacer;
            desktopPreview = desktop;
            arSession = session;
            xrOrigin = origin;
            deskCam = desktopCamera;
            arCam = phoneCamera;
        }

        void Start()
        {
            bool phone = Application.isMobilePlatform && !Application.isEditor;
            if (desktopPreview != null) desktopPreview.SetActive(!phone);
            if (arSession != null) arSession.SetActive(phone);
            if (xrOrigin != null) xrOrigin.SetActive(phone);
            if (deskCam != null) deskCam.enabled = !phone;
            if (arCam != null) arCam.enabled = phone;

            if (campaign == null)
                campaign = GetComponent<CampaignFlow>() ?? gameObject.AddComponent<CampaignFlow>();

            if (!phone)
            {
                // Editor / desktop: start immediately on preview table
                m_Arena = EnsureArena(new Vector3(0f, 0.03f, 0.35f));
                BeginCampaign(deskCam != null ? deskCam : Camera.main);
                return;
            }

            if (hud != null)
                hud.Show("AR setup", "Scan a table", "Point at a flat surface, then tap to place the arena.",
                    "Looking for a horizontal plane…", HudTone.Normal,
                    showWin: false, showFound: false, showLight: false, showLab: false);

            if (placer != null)
            {
                placer.Armed = true;
                placer.Placed += OnPlaced;
                placer.PlanesFound += () =>
                {
                    hud?.Toast("Surface found — tap to place.");
                };
            }
        }

        void OnDestroy()
        {
            if (placer != null)
                placer.Placed -= OnPlaced;
        }

        void OnPlaced(Pose pose)
        {
            if (m_Started) return;
            m_Arena = EnsureArena(pose.position);
            m_Arena.rotation = pose.rotation;
            BeginCampaign(arCam != null ? arCam : Camera.main);
        }

        void BeginCampaign(Camera cam)
        {
            m_Started = true;
            campaign.Configure(hud, m_Arena, null, null, cam);
            campaign.Begin();
        }

        Transform EnsureArena(Vector3 pos)
        {
            var go = GameObject.Find("ArenaCenter");
            if (go == null) go = new GameObject("ArenaCenter");
            go.transform.position = pos;
            return go.transform;
        }
    }
}
