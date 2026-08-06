using UnityEngine;

namespace OrbitScout.Core
{
    public static class GameProgress
    {
        const string UnlockedKey = "orbitscout_unlocked_level_v2";
        const string HighPrefix = "orbitscout_high_v2_";
        const string OverallKey = "orbitscout_overall_total_v2";

        /// <summary>
        /// When true, every level is selectable. Keep false for normal progression / demos.
        /// </summary>
        public static bool BypassLevelLocks;

        public static void UnlockAllLevelsForTesting()
        {
            BypassLevelLocks = true;
            PlayerPrefs.SetInt(UnlockedKey, (int)LevelId.Level4);
            PlayerPrefs.Save();
        }

        public static void UseNormalProgression()
        {
            BypassLevelLocks = false;
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
            {
                PlayerPrefs.SetInt(UnlockedKey, (int)level);
                PlayerPrefs.Save();
            }
        }

        public static void ResetJourney()
        {
            BypassLevelLocks = false;
            PlayerPrefs.SetInt(UnlockedKey, (int)LevelId.Level1);
            PlayerPrefs.SetInt(HighPrefix + "1", 0);
            PlayerPrefs.SetInt(HighPrefix + "2", 0);
            PlayerPrefs.SetInt(HighPrefix + "3", 0);
            PlayerPrefs.SetInt(HighPrefix + "4", 0);
            PlayerPrefs.SetInt(OverallKey, 0);
            PlayerPrefs.Save();
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
