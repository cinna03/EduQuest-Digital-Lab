using OrbitScout.UI;
using UnityEngine;

namespace OrbitScout.UI
{
    /// <summary>
    /// Optional scene marker under "UI (Edit Here)" — wires the HUD for editor test scenes.
    /// </summary>
    public class OrbitScoutUiEditAnchor : MonoBehaviour
    {
        [SerializeField] OrbitScoutHudView hudView;

        public OrbitScoutHudView HudView => hudView;

        public void SetHudView(OrbitScoutHudView view) => hudView = view;
    }
}
