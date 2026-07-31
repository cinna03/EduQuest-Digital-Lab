using UnityEngine;

namespace EduQuest
{
    /// <summary>
    /// Clean flow:
    /// 1) Live camera / editor table
    /// 2) Scan for a table
    /// 3) Place 3D lab kit
    /// 4) Guide text only
    /// </summary>
    public class LabApp : MonoBehaviour
    {
        public enum Phase { Scan, Place, Ready }

        [SerializeField] GuideHud hud;
        [SerializeField] ArTablePlacer arPlacer;
        [SerializeField] GameObject desktopRoot;
        [SerializeField] GameObject arSession;
        [SerializeField] GameObject xrOrigin;
        [SerializeField] Camera desktopCamera;
        [SerializeField] Camera arCamera;
        [SerializeField] Transform labParent;
        [SerializeField] bool forceDesktopInEditor = true;

        Phase m_Phase = Phase.Scan;
        GameObject m_Lab;
        float m_DesktopTimer;
        bool m_IsPhone;

        public void Configure(
            GuideHud guide,
            ArTablePlacer placer,
            GameObject desktop,
            GameObject session,
            GameObject origin,
            Camera deskCam,
            Camera phoneCam,
            Transform spawnParent)
        {
            hud = guide;
            arPlacer = placer;
            desktopRoot = desktop;
            arSession = session;
            xrOrigin = origin;
            desktopCamera = deskCam;
            arCamera = phoneCam;
            labParent = spawnParent;
        }

        void Awake() => ApplyPlatform();

        void Start()
        {
            if (arPlacer != null)
            {
                arPlacer.PlanesFound += OnPlanesFound;
                arPlacer.Placed += OnPlaced;
            }
            BeginScan();
        }

        void OnDestroy()
        {
            if (arPlacer == null) return;
            arPlacer.PlanesFound -= OnPlanesFound;
            arPlacer.Placed -= OnPlaced;
        }

        void ApplyPlatform()
        {
#if UNITY_ANDROID || UNITY_IOS
            m_IsPhone = true;
#else
            m_IsPhone = false;
#endif
#if UNITY_EDITOR
            if (forceDesktopInEditor) m_IsPhone = false;
#endif
            if (desktopRoot) desktopRoot.SetActive(!m_IsPhone);
            if (arSession) arSession.SetActive(m_IsPhone);
            if (xrOrigin) xrOrigin.SetActive(m_IsPhone);

            if (desktopCamera)
            {
                desktopCamera.enabled = !m_IsPhone;
                desktopCamera.tag = m_IsPhone ? "Untagged" : "MainCamera";
            }
            if (arCamera)
            {
                arCamera.enabled = m_IsPhone;
                arCamera.tag = m_IsPhone ? "MainCamera" : "Untagged";
            }
        }

        void BeginScan()
        {
            m_Phase = Phase.Scan;
            m_DesktopTimer = 0f;
            if (m_Lab != null) Destroy(m_Lab);
            m_Lab = null;
            arPlacer?.ResetTracking();
            if (arPlacer != null) arPlacer.Armed = false;

            hud?.Show(
                "Step 1 · Scan",
                "Scan a table",
                "Point the camera at a flat table.\nMove slowly until a surface appears.",
                "Looking for a table…");
        }

        void OnPlanesFound()
        {
            if (m_Phase != Phase.Scan) return;
            EnterPlace();
        }

        void EnterPlace()
        {
            m_Phase = Phase.Place;
            if (arPlacer != null) arPlacer.Armed = true;

            hud?.Show(
                "Step 2 · Place",
                "Table found",
                "Tap the surface to place the lab.\nOr wait — it will place automatically.",
                "Table found…");

            // Auto-place on phone once a plane exists
            if (m_IsPhone && arPlacer != null)
                arPlacer.TryPlaceOnBestPlane();
        }

        void OnPlaced(Pose pose) => SpawnLab(pose.position, pose.rotation);

        void SpawnLab(Vector3 pos, Quaternion rot)
        {
            if (m_Lab != null) Destroy(m_Lab);
            var parent = labParent != null ? labParent : transform;
            m_Lab = LabFactory.CreateLabKit(parent, pos, rot);
            m_Phase = Phase.Ready;
            if (arPlacer != null) arPlacer.Armed = false;

            hud?.Show(
                "Step 3 · Ready",
                "Lab ready",
                "Your beaker and chemicals are on the table.",
                "Lab placed on the table.");
        }

        void Update()
        {
            // Editor desktop: fake scan, then drop lab on the preview table
            if (!m_IsPhone && m_Phase == Phase.Scan)
            {
                m_DesktopTimer += Time.deltaTime;
                hud?.Show(
                    "Step 1 · Scan",
                    "Scan a table",
                    "Editor preview — scanning the desktop table…",
                    $"Looking for a table… {Mathf.Clamp01(m_DesktopTimer / 1.5f):0%}");

                if (m_DesktopTimer >= 1.5f)
                {
                    SpawnLab(new Vector3(0f, 0f, 0.35f), Quaternion.identity);
                }
            }

            if (m_IsPhone && m_Phase == Phase.Scan && arPlacer != null && arPlacer.HasPlanes)
                EnterPlace();
        }
    }
}
