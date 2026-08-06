using OrbitScout.Core;
using UnityEngine;

namespace OrbitScout.UI
{
    /// <summary>
    /// Assign on each level card in the HUD prefab. Edit labels in the Hierarchy; MissionHud wires the click.
    /// </summary>
    public class OrbitScoutLevelCardButton : MonoBehaviour
    {
        public LevelId level = LevelId.Level1;
    }
}
