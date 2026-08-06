using OrbitScout.UI;
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

            // Hide Mobile AR template coaching / object-menu chrome so Orbit Scout HUD is visible
            HideNamedUiRoots(
                "Coaching UI",
                "Object Menu",
                "Greeting Prompt",
                "AR Template Menu",
                "Hints Button",
                "UI");

            foreach (Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (canvas == null)
                    continue;
                if (canvas.GetComponent<OrbitScoutHudView>() != null)
                    continue;
                if (canvas.GetComponentInParent<OrbitScoutHudView>() != null)
                    continue;

                string n = canvas.gameObject.name;
                if (n == "UI" || n == "Coaching UI" || n.Contains("Object Menu") || n.Contains("Greeting"))
                    canvas.gameObject.SetActive(false);
            }
        }

        static void HideNamedUiRoots(params string[] names)
        {
            foreach (string name in names)
            {
                GameObject go = GameObject.Find(name);
                if (go != null)
                    go.SetActive(false);
            }
        }

        static void ClearSpawnedChildren(Transform spawnerRoot)
        {
            for (int i = spawnerRoot.childCount - 1; i >= 0; i--)
                Object.Destroy(spawnerRoot.GetChild(i).gameObject);
        }
    }
}
