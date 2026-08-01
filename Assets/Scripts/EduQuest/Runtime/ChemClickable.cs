using UnityEngine;

namespace EduQuest
{
    public enum ChemRole
    {
        None,
        ReactionBeaker,
        SilverNitrate,   // A — AgNO3
        SodiumChloride,  // B — NaCl
        Fixer,           // C — thiosulfate
        Distractor       // wrong bottle
    }

    /// <summary>Click target for editor Play Mode (raycast hit).</summary>
    public class ChemClickable : MonoBehaviour
    {
        public ChemRole Role = ChemRole.None;
        public string DisplayName;

        public void Configure(ChemRole role, string displayName)
        {
            Role = role;
            DisplayName = displayName;
        }
    }
}
