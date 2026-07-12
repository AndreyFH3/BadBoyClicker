using System.Collections.Generic;
using Core;

namespace PlayerProgression
{
    public interface IPlayerProgressionService : ISavable<PlayerProgressionRuntimeSave.SaveData>
    {
        int CurrentLevel { get; }
        long CurrentExperience { get; }
        long ExperienceToNextLevel { get; }
        float CurrentProgress { get; }
        bool CanCompleteLevel { get; }
        IReadOnlyList<LevelRewardEntry> NextLevelRewards { get; }
        string NextLevelLossText { get; }
        float ClickIncomeMultiplier { get; }
        float PassiveIncomeMultiplier { get; }
        event System.Action Changed;
        event System.Action<long> ExperienceAdded;
        event System.Action<int> LevelCompleted;

        void AddExperience(PlayerExperienceSource source, long contextAmount = 0);
        bool CompleteLevel();
    }
}
