namespace PlayerProgression
{
    public class PlayerProgressionNullView : IPlayerProgressionView
    {
        public event System.Action NewLevelRequested
        {
            add { }
            remove { }
        }

        public void UpdateState(int level, long experience, long experienceToNextLevel, float progress)
        {
        }

        public void SetNewLevelAvailable(bool isAvailable)
        {
        }

        public void ShowAddedExperience(long amount)
        {
        }

        public void ShowLevelUpOffer(System.Action confirmAction, string rewardDescription, UnityEngine.Sprite rewardIcon)
        {
            confirmAction?.Invoke();
        }

        public void ShowLevelUpResult(string rewardDescription, UnityEngine.Sprite rewardIcon)
        {
        }
    }
}
