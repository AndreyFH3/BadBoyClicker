namespace PlayerProgression
{
    public class PlayerProgressionNullView : IPlayerProgressionView
    {
        public event System.Action NewLevelRequested
        {
            add { }
            remove { }
        }

        public event System.Action LevelUpResultClosed
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

        public void ShowLevelUpOffer(System.Action confirmAction, System.Collections.Generic.IReadOnlyList<LevelRewardEntry> rewards, string lossText, bool showLossWarning)
        {
            confirmAction?.Invoke();
        }

        public void ShowLevelUpResult(int previousLevel, int newLevel, System.Collections.Generic.IReadOnlyList<LevelRewardEntry> rewards)
        {
        }

        public void RefreshLocalization()
        {
        }
    }
}
