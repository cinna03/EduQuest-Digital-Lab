using OrbitScout.Core;

namespace OrbitScout.UI
{
    public static class OrbitScoutLevelBriefings
    {
        public static string ShortTitle(LevelId level)
        {
            switch (level)
            {
                case LevelId.Level1: return "First Orbit";
                case LevelId.Level2: return "Save the Planets";
                case LevelId.Level3: return "Shared Traits";
                case LevelId.Level4: return "Gauntlet";
                default: return "Mission";
            }
        }

        public static string RomanNumeral(LevelId level)
        {
            switch (level)
            {
                case LevelId.Level1: return "I";
                case LevelId.Level2: return "II";
                case LevelId.Level3: return "III";
                case LevelId.Level4: return "IV";
                default: return "?";
            }
        }

        public static string Title(LevelId level)
        {
            switch (level)
            {
                case LevelId.Level1: return "Mission I · First Orbit";
                case LevelId.Level2: return "Mission II · Save the Planets";
                case LevelId.Level3: return "Mission III · Shared Traits";
                case LevelId.Level4: return "Mission IV · Gauntlet";
                default: return "Mission Briefing";
            }
        }

        public static string Body(LevelId level)
        {
            switch (level)
            {
                case LevelId.Level1:
                    return
                        "• Place the solar system, then read each clue\n" +
                        "• Tap the planet that matches\n" +
                        "• Hover a planet to see its name\n\n" +
                        "Take your time — no timer on this mission.";

                case LevelId.Level2:
                    return
                        "• Planets start greyscale — restore them with correct facts\n" +
                        "• Wrong taps crack a world; three cracks and it's lost\n" +
                        "• Lost planets leave the clue pool\n\n" +
                        "Restore three planets to continue.";

                case LevelId.Level3:
                    return
                        "• A clue may match more than one planet — tap every match\n" +
                        "• One wrong tap fails that clue\n" +
                        "• Ten minutes for the whole mission\n\n" +
                        "Find the shared traits before time runs out.";

                case LevelId.Level4:
                    return
                        "• READ phase — study the clue (no tapping)\n" +
                        "• ANSWER phase — tap matches quickly\n" +
                        "• If nothing matches, wait — don't tap\n\n" +
                        "All five clues play. Aim for a perfect run.";

                default:
                    return "Tap Start Mission when you are ready.";
            }
        }

        public static LevelId? NextLevel(LevelId level)
        {
            switch (level)
            {
                case LevelId.Level1: return LevelId.Level2;
                case LevelId.Level2: return LevelId.Level3;
                case LevelId.Level3: return LevelId.Level4;
                default: return null;
            }
        }
    }
}
