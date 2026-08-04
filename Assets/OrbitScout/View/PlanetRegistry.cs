using System.Collections.Generic;
using OrbitScout.View;
using UnityEngine;

namespace OrbitScout.View
{
    public static class PlanetRegistry
    {
        static readonly Dictionary<PlanetId, PlanetBody> Planets = new Dictionary<PlanetId, PlanetBody>();

        public static void Clear()
        {
            Planets.Clear();
        }

        public static void Register(PlanetBody body)
        {
            if (body == null)
                return;

            Planets[body.planetId] = body;
        }

        public static PlanetBody Get(PlanetId id)
        {
            Planets.TryGetValue(id, out PlanetBody body);
            return body;
        }

        public static IEnumerable<PlanetBody> AllActive()
        {
            foreach (PlanetBody body in Planets.Values)
            {
                if (body != null && body.gameObject.activeInHierarchy)
                    yield return body;
            }
        }

        public static void ResetAllForLevel(LevelVisualMode mode)
        {
            foreach (PlanetBody body in Planets.Values)
            {
                if (body == null)
                    continue;

                body.gameObject.SetActive(true);
                body.ResetForLevelStart(mode);
            }
        }
    }

    public enum LevelVisualMode
    {
        FullColor,
        Level2Greyscale
    }
}
