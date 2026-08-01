namespace EduQuest
{
    /// <summary>Unlock + scoring state for the timed riddle campaign.</summary>
    public class GameProgress
    {
        public bool SkyFound;
        public bool FlowersFound;
        public bool LightFound;

        public float TimeLimitSeconds = 180f;
        public float ElapsedSeconds;
        public int FinalScore;
        public int Stars;

        public float RemainingSeconds => UnityEngine.Mathf.Max(0f, TimeLimitSeconds - ElapsedSeconds);
        public bool AllLevelsComplete => SkyFound && FlowersFound && LightFound;

        public void ResetAll()
        {
            SkyFound = false;
            FlowersFound = false;
            LightFound = false;
            ElapsedSeconds = 0f;
            FinalScore = 0;
            Stars = 0;
        }

        /// <summary>Faster finish = higher score. Requires completing under the time limit.</summary>
        public void ComputeScore()
        {
            var remaining = RemainingSeconds;
            // Base for clearing all 3 levels + big bonus for leftover time
            FinalScore = 1000 + UnityEngine.Mathf.RoundToInt(remaining * 12f);

            if (remaining >= TimeLimitSeconds * 0.55f) Stars = 3;
            else if (remaining >= TimeLimitSeconds * 0.30f) Stars = 2;
            else Stars = 1;
        }
    }

    public enum CampaignPhase
    {
        Level1Combat,
        Level1RiddleSky,
        Level2Combat,
        Level2RiddleFlowers,
        Level3RiddleLight,
        CampaignWon,
        CampaignFailed
    }
}
