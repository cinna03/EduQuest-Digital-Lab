using System.Collections.Generic;
using UnityEngine;

namespace OrbitScout.Core
{
    public static class LevelCatalog
    {
        public struct FactQuestion
        {
            public PlanetId Planet;
            public string Prompt;
            public int FactIndex;
        }

        public struct MultiQuestion
        {
            public string Prompt;
            public PlanetId[] Planets;
        }

        static readonly FactQuestion[][] Level1Pool =
        {
            new[]
            {
                new FactQuestion { Planet = PlanetId.Mercury, FactIndex = 0, Prompt = "I zip around the Sun faster than anyone else." },
                new FactQuestion { Planet = PlanetId.Mercury, FactIndex = 1, Prompt = "I'm the smallest major planet, covered in craters." },
            },
            new[]
            {
                new FactQuestion { Planet = PlanetId.Venus, FactIndex = 0, Prompt = "Thick clouds trap my heat. I'm the hottest planet." },
                new FactQuestion { Planet = PlanetId.Venus, FactIndex = 1, Prompt = "I spin backwards and shine bright at dawn and dusk." },
            },
            new[]
            {
                new FactQuestion { Planet = PlanetId.Earth, FactIndex = 0, Prompt = "I have liquid water and life. People call me the blue marble." },
                new FactQuestion { Planet = PlanetId.Earth, FactIndex = 1, Prompt = "I'm the only world known to have life." },
            },
            new[]
            {
                new FactQuestion { Planet = PlanetId.Mars, FactIndex = 0, Prompt = "Iron rust paints me red, and robots roll through my dust." },
                new FactQuestion { Planet = PlanetId.Mars, FactIndex = 1, Prompt = "I'm the red planet with the largest volcano in the solar system." },
            },
            new[]
            {
                new FactQuestion { Planet = PlanetId.Jupiter, FactIndex = 0, Prompt = "I'm the giant with a centuries-old red storm." },
                new FactQuestion { Planet = PlanetId.Jupiter, FactIndex = 1, Prompt = "I'm the most massive planet, a true gas giant." },
            },
            new[]
            {
                new FactQuestion { Planet = PlanetId.Saturn, FactIndex = 0, Prompt = "Ice and rock make up my famous ring system." },
                new FactQuestion { Planet = PlanetId.Saturn, FactIndex = 1, Prompt = "My rings are the most famous in the solar system." },
            },
            new[]
            {
                new FactQuestion { Planet = PlanetId.Uranus, FactIndex = 0, Prompt = "I roll on my side under a pale blue-green haze." },
                new FactQuestion { Planet = PlanetId.Uranus, FactIndex = 1, Prompt = "I'm an ice giant that rotates on my side." },
            },
            new[]
            {
                new FactQuestion { Planet = PlanetId.Neptune, FactIndex = 0, Prompt = "I'm deep blue, windy, and far out among the ice giants." },
                new FactQuestion { Planet = PlanetId.Neptune, FactIndex = 1, Prompt = "I'm the farthest major planet, with winds faster than sound." },
            },
        };

        static readonly FactQuestion[] Level2Facts =
        {
            new FactQuestion { Planet = PlanetId.Mercury, FactIndex = 0, Prompt = "I have the shortest year. I zoom around the Sun." },
            new FactQuestion { Planet = PlanetId.Mercury, FactIndex = 1, Prompt = "No real atmosphere here. Scorched by day, freezing by night." },
            new FactQuestion { Planet = PlanetId.Mercury, FactIndex = 2, Prompt = "Named for the messenger god. Speedy and close to the Sun." },
            new FactQuestion { Planet = PlanetId.Venus, FactIndex = 0, Prompt = "Runaway greenhouse effect. Hotter than Mercury at the surface." },
            new FactQuestion { Planet = PlanetId.Venus, FactIndex = 1, Prompt = "Thick sulfuric acid clouds hide my surface." },
            new FactQuestion { Planet = PlanetId.Venus, FactIndex = 2, Prompt = "Earth's twin in size, but hostile in every other way." },
            new FactQuestion { Planet = PlanetId.Earth, FactIndex = 0, Prompt = "One moon, liquid oceans, and a breathable atmosphere." },
            new FactQuestion { Planet = PlanetId.Earth, FactIndex = 1, Prompt = "Third from the Sun, right in the goldilocks zone." },
            new FactQuestion { Planet = PlanetId.Earth, FactIndex = 2, Prompt = "Magnetic field and ozone help shield life below." },
            new FactQuestion { Planet = PlanetId.Mars, FactIndex = 0, Prompt = "Olympus Mons is a volcano taller than Everest." },
            new FactQuestion { Planet = PlanetId.Mars, FactIndex = 1, Prompt = "Thin CO₂ air and dusty red deserts." },
            new FactQuestion { Planet = PlanetId.Mars, FactIndex = 2, Prompt = "Rovers search for ancient water signs here." },
            new FactQuestion { Planet = PlanetId.Jupiter, FactIndex = 0, Prompt = "The Great Red Spot is a storm wider than Earth." },
            new FactQuestion { Planet = PlanetId.Jupiter, FactIndex = 1, Prompt = "Dozens of moons, including volcanic Io." },
            new FactQuestion { Planet = PlanetId.Jupiter, FactIndex = 2, Prompt = "Mostly hydrogen and helium. A failed star's cousin." },
            new FactQuestion { Planet = PlanetId.Saturn, FactIndex = 0, Prompt = "My density is so low I'd float in a giant bathtub." },
            new FactQuestion { Planet = PlanetId.Saturn, FactIndex = 1, Prompt = "Rings made of ice and rock particles." },
            new FactQuestion { Planet = PlanetId.Saturn, FactIndex = 2, Prompt = "Moon Titan has lakes of methane." },
            new FactQuestion { Planet = PlanetId.Uranus, FactIndex = 0, Prompt = "Axial tilt near 98°. I roll around the Sun." },
            new FactQuestion { Planet = PlanetId.Uranus, FactIndex = 1, Prompt = "Pale cyan from methane in my atmosphere." },
            new FactQuestion { Planet = PlanetId.Uranus, FactIndex = 2, Prompt = "Ice giant with faint rings of my own." },
            new FactQuestion { Planet = PlanetId.Neptune, FactIndex = 0, Prompt = "Discovered by math before anyone saw me in a telescope." },
            new FactQuestion { Planet = PlanetId.Neptune, FactIndex = 1, Prompt = "Deep azure color and the strongest winds recorded." },
            new FactQuestion { Planet = PlanetId.Neptune, FactIndex = 2, Prompt = "Moon Triton orbits backward. A captured world." },
        };

        static readonly MultiQuestion[] Level3Set =
        {
            new MultiQuestion { Prompt = "Rocky terrestrial worlds. Select all that apply.", Planets = new[] { PlanetId.Mercury, PlanetId.Venus, PlanetId.Earth, PlanetId.Mars } },
            new MultiQuestion { Prompt = "Gas giants. Select all that apply.", Planets = new[] { PlanetId.Jupiter, PlanetId.Saturn } },
            new MultiQuestion { Prompt = "Ice giants. Select all that apply.", Planets = new[] { PlanetId.Uranus, PlanetId.Neptune } },
            new MultiQuestion { Prompt = "Planets with ring systems. Select all that apply.", Planets = new[] { PlanetId.Saturn, PlanetId.Uranus, PlanetId.Neptune } },
            new MultiQuestion { Prompt = "Worlds closer to the Sun than Earth. Select all that apply.", Planets = new[] { PlanetId.Mercury, PlanetId.Venus } },
            new MultiQuestion { Prompt = "Reddish appearance from orbit. Select all that apply.", Planets = new[] { PlanetId.Mars } },
            new MultiQuestion { Prompt = "Planets with more mass than Earth. Select all that apply.", Planets = new[] { PlanetId.Jupiter, PlanetId.Saturn, PlanetId.Uranus, PlanetId.Neptune } },
            new MultiQuestion { Prompt = "Could host human life without a spacesuit today. Select all that apply.", Planets = new[] { PlanetId.Earth } },
            new MultiQuestion { Prompt = "Outer planets beyond the asteroid belt. Select all that apply.", Planets = new[] { PlanetId.Jupiter, PlanetId.Saturn, PlanetId.Uranus, PlanetId.Neptune } },
            new MultiQuestion { Prompt = "Known for extreme winds. Select all that apply.", Planets = new[] { PlanetId.Jupiter, PlanetId.Neptune } },
        };

        static readonly MultiQuestion[] Level4Set =
        {
            new MultiQuestion { Prompt = "Which world(s) have no solid surface to stand on?", Planets = new[] { PlanetId.Jupiter, PlanetId.Saturn, PlanetId.Uranus, PlanetId.Neptune } },
            new MultiQuestion { Prompt = "Which world(s) are sometimes called the morning or evening star?", Planets = new[] { PlanetId.Venus } },
            new MultiQuestion { Prompt = "Which world(s) match? None. Pluto is not in our model.", Planets = new PlanetId[0] },
            new MultiQuestion { Prompt = "Which world(s) are terrestrial?", Planets = new[] { PlanetId.Mercury, PlanetId.Venus, PlanetId.Earth, PlanetId.Mars } },
            new MultiQuestion { Prompt = "Which world(s) have prominent ring systems visible from Earth telescopes?", Planets = new[] { PlanetId.Saturn } },
        };

        public static List<FactQuestion> BuildLevel1Run()
        {
            var run = new List<FactQuestion>(8);
            foreach (FactQuestion[] options in Level1Pool)
            {
                int pick = Random.Range(0, options.Length);
                run.Add(options[pick]);
            }

            Shuffle(run);
            return run;
        }

        public static List<FactQuestion> BuildLevel2Run()
        {
            var run = new List<FactQuestion>(Level2Facts);
            Shuffle(run);
            return run;
        }

        public static List<MultiQuestion> BuildLevel3Run()
        {
            var run = new List<MultiQuestion>(Level3Set);
            Shuffle(run);
            return run;
        }

        public static List<MultiQuestion> BuildLevel4Run()
        {
            return new List<MultiQuestion>(Level4Set);
        }

        static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
