using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.ARStarterAssets;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

namespace OrbitScout.Platform
{
    /// <summary>
    /// SampleScene includes XR ITK tap-to-spawn cubes; turn that off so taps go to Orbit Scout only.
    /// </summary>
    public static class OrbitScoutArTemplateSuppress
    {
        public static void Apply()
        {
            foreach (ARInteractorSpawnTrigger trigger in Object.FindObjectsByType<ARInteractorSpawnTrigger>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                trigger.enabled = false;
            }

            foreach (ARContactSpawnTrigger trigger in Object.FindObjectsByType<ARContactSpawnTrigger>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                trigger.enabled = false;
            }

            foreach (ObjectSpawner spawner in Object.FindObjectsByType<ObjectSpawner>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                spawner.enabled = false;
                ClearSpawnedChildren(spawner.transform);
            }
        }

        static void ClearSpawnedChildren(Transform spawnerRoot)
        {
            for (int i = spawnerRoot.childCount - 1; i >= 0; i--)
                Object.Destroy(spawnerRoot.GetChild(i).gameObject);
        }
    }
}
