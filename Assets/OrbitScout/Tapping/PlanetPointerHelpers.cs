using System.Collections.Generic;
using OrbitScout.View;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OrbitScout.Tapping
{
    public static class PlanetPointerHelpers
    {
        static readonly List<RaycastResult> UiHits = new List<RaycastResult>();

        public static bool IsOverUiButton(Vector2 screenPosition, int pointerId)
        {
            if (EventSystem.current == null)
                return false;

            var data = new PointerEventData(EventSystem.current)
            {
                position = screenPosition,
                pointerId = pointerId
            };

            UiHits.Clear();
            EventSystem.current.RaycastAll(data, UiHits);

            foreach (RaycastResult hit in UiHits)
            {
                if (hit.gameObject.GetComponentInParent<UnityEngine.UI.Button>() != null)
                    return true;
            }

            return false;
        }

        public static bool TryPickSun(
            Camera cam,
            Vector2 screenPosition,
            float maxRange,
            out SunTapReject sun)
        {
            sun = null;
            if (cam == null)
                return false;

            Ray ray = cam.ScreenPointToRay(screenPosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, maxRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                return false;

            sun = hit.collider.GetComponentInParent<SunTapReject>();
            return sun != null;
        }

        public static bool TryPickPlanet(
            Camera cam,
            Vector2 screenPosition,
            float maxRange,
            out PlanetBody planet)
        {
            planet = null;
            if (cam == null)
                return false;

            Ray ray = cam.ScreenPointToRay(screenPosition);
            RaycastHit[] hits = Physics.RaycastAll(
                ray,
                maxRange,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);

            if (hits == null || hits.Length == 0)
                return false;

            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (RaycastHit hit in hits)
            {
                planet = hit.collider.GetComponentInParent<PlanetBody>();
                if (planet != null)
                    return true;
            }

            return false;
        }

        public static Vector3 GetLabelWorldPosition(PlanetBody planet)
        {
            if (planet == null)
                return Vector3.zero;

            float lift = 0.12f;
            Renderer renderer = planet.GetComponent<Renderer>();
            if (renderer != null)
                lift = renderer.bounds.extents.y + 0.06f;

            return planet.transform.position + Vector3.up * lift;
        }
    }
}
