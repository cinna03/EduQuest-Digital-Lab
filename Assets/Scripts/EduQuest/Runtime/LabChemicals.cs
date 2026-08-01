using UnityEngine;

namespace EduQuest
{
    /// <summary>
    /// Real aqueous-solution appearances for the photography crystal lab.
    /// Clear reagents stay nearly colorless (accurate); labels tell them apart.
    /// </summary>
    public static class LabChemicals
    {
        // AgNO3(aq): colorless — like water (solid is white crystals).
        public static readonly Color AgNO3 = new Color(0.86f, 0.91f, 0.94f, 0.55f);
        // NaCl(aq): colorless brine — also water-clear.
        public static readonly Color NaCl = new Color(0.88f, 0.93f, 0.96f, 0.5f);
        // Photographic fixer (Na2S2O3 / hypo): colorless to pale yellow.
        public static readonly Color Fixer = new Color(0.93f, 0.86f, 0.52f, 0.75f);
        // CuSO4(aq): characteristic deep copper-sulfate blue.
        public static readonly Color CuSO4 = new Color(0.12f, 0.42f, 0.92f, 1f);
        // Empty / water rinse look
        public static readonly Color ClearMix = new Color(0.85f, 0.91f, 0.95f, 0.45f);
        // AgCl precipitate: white / milky
        public static readonly Color AgClPrecipitate = new Color(0.96f, 0.96f, 0.98f, 1f);

        public static string DisplayName(ChemRole role) => role switch
        {
            ChemRole.SilverNitrate => "A  AgNO3",
            ChemRole.SodiumChloride => "B  NaCl",
            ChemRole.Fixer => "C  Fixer",
            ChemRole.Distractor => "D  CuSO4",
            ChemRole.ReactionBeaker => "MIX",
            _ => "Vessel"
        };

        public static string ShortTag(ChemRole role) => role switch
        {
            ChemRole.SilverNitrate => "A",
            ChemRole.SodiumChloride => "B",
            ChemRole.Fixer => "C",
            ChemRole.Distractor => "D",
            ChemRole.ReactionBeaker => "MIX",
            _ => "?"
        };

        public static void Appearance(ChemRole role, out Color color, out float fill, out bool isClear)
        {
            fill = 0.88f;
            isClear = false;
            switch (role)
            {
                case ChemRole.SilverNitrate:
                    color = AgNO3;
                    isClear = true;
                    break;
                case ChemRole.SodiumChloride:
                    color = NaCl;
                    isClear = true;
                    break;
                case ChemRole.Fixer:
                    color = Fixer;
                    break;
                case ChemRole.Distractor:
                    color = CuSO4;
                    break;
                case ChemRole.ReactionBeaker:
                    color = ClearMix;
                    fill = 0f;
                    isClear = true;
                    break;
                default:
                    color = ClearMix;
                    fill = 0.5f;
                    isClear = true;
                    break;
            }
        }

        public static Color LabelColor(ChemRole role) => role switch
        {
            ChemRole.SilverNitrate => new Color(0.95f, 0.97f, 1f),
            ChemRole.SodiumChloride => new Color(0.85f, 0.95f, 1f),
            ChemRole.Fixer => new Color(1f, 0.92f, 0.55f),
            ChemRole.Distractor => new Color(0.45f, 0.75f, 1f),
            ChemRole.ReactionBeaker => new Color(0.65f, 1f, 0.75f),
            _ => Color.white
        };

        public static bool IsClearSolution(Color c) => c.a < 0.92f;
    }
}
