using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace OrbitScout.Platform
{
    public static class OrbitScoutUiInputSetup
    {
        public static void EnsureEventSystem()
        {
            EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
            GameObject host;

            if (eventSystem != null)
                host = eventSystem.gameObject;
            else
            {
                host = new GameObject("EventSystem");
                host.AddComponent<EventSystem>();
            }

            ConfigureInputModule(host);
        }

        static void ConfigureInputModule(GameObject eventSystemObject)
        {
#if ENABLE_INPUT_SYSTEM
            StandaloneInputModule legacy = eventSystemObject.GetComponent<StandaloneInputModule>();
            if (legacy != null)
                Object.Destroy(legacy);

            if (eventSystemObject.GetComponent<InputSystemUIInputModule>() == null)
                eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
            if (eventSystemObject.GetComponent<StandaloneInputModule>() == null)
                eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
        }
    }
}
