using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace OrbitScout.Platform
{
    /// <summary>
    /// SampleScene ships without an OrbitScout object until the setup menu runs; create systems at load.
    /// </summary>
    public static class OrbitScoutRuntimeEnsure
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AfterSceneLoad()
        {
            if (Object.FindAnyObjectByType<SolarBootstrap>() != null)
                return;

            if (Object.FindAnyObjectByType<ARRaycastManager>() == null)
                return;

            GameObject systems = new GameObject("OrbitScout");
            systems.AddComponent<OrbitScoutSceneEntry>();
        }
    }
}
