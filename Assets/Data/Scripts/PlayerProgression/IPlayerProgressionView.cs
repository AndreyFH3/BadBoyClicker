using System.Collections.Generic;

namespace PlayerProgression
{
    public interface IPlayerProgressionView
    {
        event System.Action NewLevelRequested;
        event System.Action LevelUpResultClosed;

        void UpdateState(int level, long experience, long experienceToNextLevel, float progress);
        void SetNewLevelAvailable(bool isAvailable);
        void ShowAddedExperience(long amount);
        void ShowLevelUpOffer(System.Action confirmAction, IReadOnlyList<LevelRewardEntry> rewards, string lossText, bool showLossWarning);
        void ShowLevelUpResult(int previousLevel, int newLevel, IReadOnlyList<LevelRewardEntry> rewards);
        void RefreshLocalization();
    }
}
