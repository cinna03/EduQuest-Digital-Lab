using UnityEngine;

namespace OrbitScout.Core
{
    public static class GameProgress
    {
        const string UnlockedKey = "orbitscout_unlocked_level";
        const string HighPrefix = "orbitscout_high_";
        const string OverallKey = "orbitscout_overall_total";

        /// <summary>When true, every level is selectable (testing). Turn off for release builds.</summary>
        public static bool BypassLevelLocks = true;

        public static void UnlockAllLevelsForTesting()
        {
            BypassLevelLocks = true;
            PlayerPrefs.SetInt(UnlockedKey, (int)LevelId.Level4);
            PlayerPrefs.Save();
        }

        public static int GetUnlockedLevel()
        {
            return PlayerPrefs.GetInt(UnlockedKey, (int)LevelId.Level1);
        }

        public static bool IsLevelUnlocked(LevelId level)
        {
            if (BypassLevelLocks)
                return true;

            return (int)level <= GetUnlockedLevel();
        }

        public static void UnlockLevel(LevelId level)
        {
            int current = GetUnlockedLevel();
            if ((int)level > current)
                PlayerPrefs.SetInt(UnlockedKey, (int)level);
        }

        public static void ResetJourney()
        {
            PlayerPrefs.SetInt(UnlockedKey, (int)LevelId.Level1);
            PlayerPrefs.SetInt(HighPrefix + "1", 0);
            PlayerPrefs.SetInt(HighPrefix + "2", 0);
            PlayerPrefs.SetInt(HighPrefix + "3", 0);
            PlayerPrefs.SetInt(HighPrefix + "4", 0);
            PlayerPrefs.SetInt(OverallKey, 0);
            PlayerPrefs.Save();
            UnlockAllLevelsForTesting();
        }

        public static int GetLevelHighScore(LevelId level)
        {
            return PlayerPrefs.GetInt(HighPrefix + (int)level, 0);
        }

        public static int GetOverallScore()
        {
            int sum = 0;
            for (int i = 1; i <= 4; i++)
                sum += PlayerPrefs.GetInt(HighPrefix + i, 0);
            return sum;
        }

        public static void TrySetLevelHighScore(LevelId level, int score)
        {
            string key = HighPrefix + (int)level;
            int best = PlayerPrefs.GetInt(key, 0);
            if (score <= best)
                return;

            PlayerPrefs.SetInt(key, score);
            PlayerPrefs.Save();
        }

        public static void Save()
        {
            PlayerPrefs.Save();
        }
    }
}
