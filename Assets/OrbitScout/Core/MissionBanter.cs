using UnityEngine;

namespace OrbitScout.Core
{
    /// <summary>
    /// Playful copy for mission feedback — keeps rules in MissionController, voice here.
    /// </summary>
    public static class MissionBanter
    {
        static readonly string[] MissionStarts =
        {
            "Mission control is watching. No pressure. (Some pressure.)",
            "The Sun is not an answer. It's literally right there judging you.",
            "Six clues. One timer. Infinite cosmic drama.",
        };

        static readonly string[] CorrectCheers =
        {
            "Nailed it!",
            "Orbit approved!",
            "NASA would high-five you.",
            "That's the one!",
            "Gold star energy.",
        };

        static readonly string[] WrongGeneric =
        {
            "Wrong orbit — try again!",
            "Not that world. The clue is still right there, being patient.",
            "Negative. Houston suggests reading the clue again.",
            "That planet sends its regards, but no.",
        };

        static readonly string[] LowTime =
        {
            "Time's slipping — pick a planet!",
            "The clock is doing that rude ticking thing.",
            "Quick! Before Mercury laps you again.",
        };

        static readonly string[] HintLeadIns =
        {
            "Mission control whispering:",
            "Fine, we'll nudge you:",
            "Hint unlocked — don't tell the Sun:",
        };

        static readonly string[] CompleteHeadlines =
        {
            "Mission complete — you're basically an astronaut now.",
            "All questions down! The solar system applauds.",
            "You cleared the belt. Metaphorically.",
        };

        static readonly string[] FailHeadlines =
        {
            "Time's up — the universe waits for no one.",
            "Mission aborted. The planets keep orbiting anyway.",
            "Out of time! Even Pluto isn't this far behind.",
        };

        public static string PickMissionStart()
        {
            return MissionStarts[Random.Range(0, MissionStarts.Length)];
        }

        public static string FormatCorrect(string fact, int streak)
        {
            string cheer = CorrectCheers[Random.Range(0, CorrectCheers.Length)];
            if (streak >= 4)
                return cheer + " Four in a row — you're in sync with the cosmos! " + fact;
            if (streak >= 2)
                return cheer + " Streak building! " + fact;
            return cheer + " " + fact;
        }

        public static string GetStreakCallout(int streak, int points)
        {
            if (streak >= 5)
                return "SOLAR FLARE STREAK ×" + streak + "  +" + points;
            if (streak >= 3)
                return "Orbital genius ×" + streak + "  +" + points;
            if (streak >= 2)
                return streak + " in a row!  +" + points + " pts";
            return "+" + points + " pts";
        }

        public static string GetWrongTap(PlanetId picked, PlanetId correct)
        {
            if (picked == correct)
                return Pick(WrongGeneric);

            if (picked == PlanetId.Saturn && correct != PlanetId.Saturn)
                return "Saturn has the bling, but that's not today's answer.";
            if (picked == PlanetId.Mars)
                return "Mars says no — keep hunting.";
            if (picked == PlanetId.Jupiter)
                return "Jupiter's huge, but not the clue you're looking for.";
            if (picked == PlanetId.Venus)
                return "Venus is hot stuff, wrong answer though.";
            if (correct == PlanetId.Mercury && picked != PlanetId.Mercury)
                return "Closer to the Sun might help — think inner planets.";

            return Pick(WrongGeneric) + " (−8s on the clock)";
        }

        public static string FormatHint(string hintText)
        {
            return Pick(HintLeadIns) + " " + hintText;
        }

        public static string PickLowTimeWarning()
        {
            return Pick(LowTime);
        }

        public static string GetEndHeadline(bool completed, int stars)
        {
            if (!completed)
                return Pick(FailHeadlines);

            if (stars >= 3)
                return Pick(CompleteHeadlines) + " Perfect three stars!";
            if (stars >= 2)
                return Pick(CompleteHeadlines) + " Solid two stars.";
            return Pick(CompleteHeadlines) + " You made it — one star, still legendary.";
        }

        public static string GetSunTapReaction()
        {
            return "That's the Sun — a star, not a planet. Nice try, future astronomer.";
        }

        static string Pick(string[] lines)
        {
            return lines[Random.Range(0, lines.Length)];
        }
    }
}
