using UnityEngine;

namespace EduQuest
{
    /// <summary>
    /// Editor workspace helper. Lab kit is already in the scene — no scan, no spawn wait.
    /// </summary>
    public class EditorLabApp : MonoBehaviour
    {
        [SerializeField] GuideHud hud;

        public void Configure(GuideHud guide, Transform spawn)
        {
            hud = guide;
            // spawn unused — kit is baked into the scene for easy editing
        }

        void Start()
        {
            hud?.Show(
                "Editor",
                "Edit the glassware",
                "Select pieces in the Hierarchy / Scene view.\nMove, rotate, scale freely.",
                "No scan — assets are already on the table.");
        }
    }
}
