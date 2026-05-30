namespace PlayerProgression
{
    public interface IPlayerProgressionView
    {
        event System.Action NewLevelRequested;

        void UpdateState(int level, long experience, long experienceToNextLevel, float progress);
        void SetNewLevelAvailable(bool isAvailable);
        void ShowAddedExperience(long amount);
        void ShowLevelUpOffer(System.Action confirmAction, string rewardDescription, UnityEngine.Sprite rewardIcon);
        void ShowLevelUpResult(string rewardDescription, UnityEngine.Sprite rewardIcon);
    }
}
