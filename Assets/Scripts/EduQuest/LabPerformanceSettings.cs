using UnityEngine;

namespace EduQuest
{
    /// <summary>
    /// Runtime performance defaults for the EduQuest vertical slice.
    /// Documented in the repository README (performance / rendering section).
    /// </summary>
    public class LabPerformanceSettings : MonoBehaviour
    {
        [SerializeField] int targetFps = 60;
        [Tooltip("Disable VSync so targetFrameRate is meaningful in the Editor/standalone.")]
        [SerializeField] bool preferTargetFrameRate = true;

        void Awake()
        {
            if (preferTargetFrameRate)
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = targetFps;
            }
        }
    }
}
