using OrbitScout.Core;
using OrbitScout.View;
using UnityEngine;

namespace OrbitScout.Tapping
{
    public class PlanetTapInput : MonoBehaviour
    {
        public Camera viewCamera;
        public float maxRange = 40f;
        public bool inputEnabled = true;

        void Awake()
        {
            if (viewCamera == null)
                viewCamera = GetComponent<Camera>();
        }

        void Update()
        {
            if (!inputEnabled)
                return;

            MissionController session = MissionController.Instance;
            if (session == null || !session.IsPlaying)
                return;

            if (!session.CanAcceptPlanetTap())
                return;

            if (!PointerPressReader.TryGetPressThisFrame(out Vector2 screenPosition, out int pointerId))
                return;

            TryTap(screenPosition, pointerId);
        }

        void TryTap(Vector2 screenPosition, int pointerId)
        {
            if (PlanetPointerHelpers.IsOverUiButton(screenPosition, pointerId))
                return;

            Camera cam = viewCamera != null ? viewCamera : Camera.main;

            if (PlanetPointerHelpers.TryPickSun(cam, screenPosition, maxRange, out SunTapReject sun))
            {
                sun.NotifyTapped();
                return;
            }

            if (!PlanetPointerHelpers.TryPickPlanet(cam, screenPosition, maxRange, out PlanetBody planet))
                return;

            if (planet.IsExploded || !planet.gameObject.activeInHierarchy)
                return;

            MissionController session = MissionController.Instance;
            if (session == null)
                return;

            bool correct = session.SubmitPlanet(planet.planetId);

            if (session.ActiveLevel == LevelId.Level1)
            {
                if (correct)
                    planet.FlashCorrect();
                else
                    planet.FlashWrong();
            }
            else if (session.ActiveLevel == LevelId.Level2 && correct)
            {
                planet.FlashCorrect();
            }
        }
    }
}
