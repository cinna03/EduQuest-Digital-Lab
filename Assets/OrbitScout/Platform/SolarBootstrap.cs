using OrbitScout.Core;
using OrbitScout.UI;
using OrbitScout.View;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace OrbitScout.Platform
{
    public enum SolarPlayMode
    {
        EditorDesktop,
        AugmentedReality
    }

    public class SolarBootstrap : MonoBehaviour
    {
        public static SolarBootstrap Instance { get; private set; }

        [Header("Mode")]
        public SolarPlayMode playMode = SolarPlayMode.EditorDesktop;

        [Header("Arena")]
        public Transform arenaAnchor;

        Transform activeViewRoot;
        LevelId pendingLevel = LevelId.Level1;
        bool arWaitingForLevel;

        void Awake()
        {
            Instance = this;

            if (arenaAnchor == null)
            {
                GameObject anchorObject = new GameObject("ArenaAnchor");
                arenaAnchor = anchorObject.transform;
            }

            OrbitScoutUiInputSetup.EnsureEventSystem();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        void Start()
        {
            // SceneEntry may flip playMode in Awake; apply the final mode here.
            if (playMode == SolarPlayMode.EditorDesktop)
            {
                EditorPlayRig.Ensure();
                if (arenaAnchor != null)
                    arenaAnchor.position = Vector3.zero;
            }
        }

        public void SetPendingLevel(LevelId level)
        {
            pendingLevel = level;
            arWaitingForLevel = true;
        }

        public void StartLevelSession(LevelId level)
        {
            pendingLevel = level;
            arWaitingForLevel = false;
            ClearView();

            if (playMode == SolarPlayMode.EditorDesktop)
            {
                arenaAnchor.position = Vector3.zero;
                arenaAnchor.rotation = Quaternion.identity;
                EditorPlayRig.FocusCameraOn(arenaAnchor);
            }

            LevelVisualMode visual = level == LevelId.Level2
                ? LevelVisualMode.Level2Greyscale
                : LevelVisualMode.FullColor;

            activeViewRoot = SolarSystemView.Build(arenaAnchor, visual);

            SpaceEnvironment.Ensure();
            EditorPlayRig.EnsureCamera();
            PlanetNameHoverLabel.EnsureOnMainCamera();

            MissionController mission = MissionController.Instance;
            if (mission != null)
                mission.StartLevel(level);
        }

        public void StartArSessionAt(Pose worldPose)
        {
            playMode = SolarPlayMode.AugmentedReality;
            ClearView();

            arenaAnchor.SetPositionAndRotation(worldPose.position, worldPose.rotation);
            arenaAnchor.localScale = Vector3.one * 0.55f;

            LevelId level = pendingLevel;
            LevelVisualMode visual = level == LevelId.Level2
                ? LevelVisualMode.Level2Greyscale
                : LevelVisualMode.FullColor;

            activeViewRoot = SolarSystemView.Build(arenaAnchor, visual);

            SpaceEnvironment.Ensure();
            EditorPlayRig.EnsureCamera();
            PlanetNameHoverLabel.EnsureOnMainCamera();

            if (arWaitingForLevel || MissionController.Instance != null)
            {
                MissionController mission = MissionController.Instance;
                if (mission != null)
                    mission.StartLevel(level);
            }

            ARPlaneManager planeManager = FindAnyObjectByType<ARPlaneManager>();
            if (planeManager != null)
                OrbitScoutArPlanePresentation.FadeOutAfterPlacement(planeManager);
        }

        void ClearView()
        {
            if (activeViewRoot != null)
                Destroy(activeViewRoot.gameObject);

            activeViewRoot = null;
            PlanetRegistry.Clear();
        }

        public void RestartCurrentLevelInPlace()
        {
            if (playMode != SolarPlayMode.AugmentedReality)
            {
                StartLevelSession(pendingLevel);
                return;
            }

            ClearView();
            LevelVisualMode visual = pendingLevel == LevelId.Level2
                ? LevelVisualMode.Level2Greyscale
                : LevelVisualMode.FullColor;

            activeViewRoot = SolarSystemView.Build(arenaAnchor, visual);
            EditorPlayRig.EnsureCamera();
            PlanetNameHoverLabel.EnsureOnMainCamera();

            MissionController mission = MissionController.Instance;
            if (mission != null)
                mission.StartLevel(pendingLevel);
        }

        public void EndPlaySession()
        {
            MissionController mission = MissionController.Instance;
            if (mission != null)
                mission.StopLevel();

            ClearView();
        }
    }
}
