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
            new Clue(PlanetId.Mercury, "I zip around the Sun faster than anyone else.",
                "Mercury has the shortest year in the solar system.", "Smallest major planet. Scorched and cratered."),
            new Clue(PlanetId.Venus, "Thick clouds trap my heat. I'm the hottest planet.",
                "Venus has a thick CO₂ atmosphere.", "Bright white clouds. Often called Earth's twin in size."),
            new Clue(PlanetId.Earth, "I have liquid water and life. People call me the blue marble.",
                "Earth is our home world.", "The only planet known to have life."),
            new Clue(PlanetId.Mars, "Iron rust paints me red, and robots roll through my dust.",
                "Mars is the red planet.", "Rusty red color. Fourth from the Sun."),
            new Clue(PlanetId.Jupiter, "I'm the giant with a centuries-old red storm.",
                "Jupiter is the most massive planet.", "Largest planet. Home of the Great Red Spot."),
            new Clue(PlanetId.Saturn, "Ice and rock make up my famous ring system.",
                "Saturn is known for spectacular rings.", "Look for the flat ring disc."),
            new Clue(PlanetId.Uranus, "I roll on my side under a pale blue-green haze.",
                "Uranus has an extreme axial tilt.", "Blue-green ice giant with tilted rotation."),
            new Clue(PlanetId.Neptune, "I'm deep blue, windy, and far out among the ice giants.",
                "Neptune has the strongest winds.", "Deep blue. Farthest major planet.")
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
