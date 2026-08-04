using System.Collections.Generic;

namespace OrbitScout.Core
{
    public static class SolarQuizBank
    {
        public readonly struct Clue
        {
            public readonly PlanetId Answer;
            public readonly string Prompt;
            public readonly string Fact;
            public readonly string Hint;

            public Clue(PlanetId answer, string prompt, string fact, string hint)
            {
                Answer = answer;
                Prompt = prompt;
                Fact = fact;
                Hint = hint;
            }
        }

        static readonly Clue[] All =
        {
            new Clue(PlanetId.Mercury, "I orbit fastest and hug the Sun the tightest.",
                "Mercury — shortest year in the solar system.", "Hint: smallest major planet; scorched and cratered."),
            new Clue(PlanetId.Venus, "Thick clouds trap heat — I'm the hottest planet.",
                "Venus — thick CO₂ atmosphere.", "Hint: bright white clouds; often called Earth's twin in size."),
            new Clue(PlanetId.Earth, "Liquid water and life — the blue marble.",
                "Earth — our home world.", "Hint: the only planet known to have life."),
            new Clue(PlanetId.Mars, "Iron rust paints me red; robots explore my dust.",
                "Mars — the red planet.", "Hint: rusty red color; fourth from the Sun."),
            new Clue(PlanetId.Jupiter, "I'm the giant with a centuries-old red storm.",
                "Jupiter — most massive planet.", "Hint: largest planet; Great Red Spot."),
            new Clue(PlanetId.Saturn, "Ice and rock form my famous ring system.",
                "Saturn — spectacular rings.", "Hint: look for the flat ring disc."),
            new Clue(PlanetId.Uranus, "I roll on my side with a pale blue-green haze.",
                "Uranus — extreme axial tilt.", "Hint: blue-green ice giant; tilted rotation."),
            new Clue(PlanetId.Neptune, "Deep blue and windy — the far ice giant.",
                "Neptune — strongest winds.", "Hint: deep blue; farthest major planet.")
        };

        public static List<Clue> CreateShuffledMission(int count)
        {
            var list = new List<Clue>(All);
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }

            if (count < list.Count)
                return list.GetRange(0, count);

            return list;
        }
    }
}
