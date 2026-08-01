namespace EduQuest
{
    /// <summary>Persistent unlock flags for the 3-level living-room campaign.</summary>
    public class GameProgress
    {
        public bool EquationRevealed;
        public bool PlantFound;
        public bool FlowersFound;
        public bool LightGatePassed;
        public bool UnlockedAgNO3;
        public bool UnlockedNaCl;
        public bool UnlockedFixer;
        public bool UnlockedLab;

        public void ResetAll()
        {
            EquationRevealed = false;
            PlantFound = false;
            FlowersFound = false;
            LightGatePassed = false;
            UnlockedAgNO3 = false;
            UnlockedNaCl = false;
            UnlockedFixer = false;
            UnlockedLab = false;
        }
    }

    public enum CampaignPhase
    {
        Level1Combat,
        Level1HuntPlant,
        Level2Combat,
        Level2HuntFlowers,
        Level3LightGate,
        Level3LabMix,
        CampaignWon
    }
}
