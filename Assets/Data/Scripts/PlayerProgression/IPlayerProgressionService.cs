using Core;
using UnityEngine;

namespace PlayerProgression
{
    public interface IPlayerProgressionService : ISavable<PlayerProgressionRuntimeSave.SaveData>
    {
        int CurrentLevel { get; }
        long CurrentExperience { get; }
        long ExperienceToNextLevel { get; }
        float CurrentProgress { get; }
        bool CanCompleteLevel { get; }
        string NextLevelRewardDescription { get; }
        Sprite NextLevelRewardIcon { get; }
        float ClickIncomeMultiplier { get; }
        float PassiveIncomeMultiplier { get; }
        event System.Action Changed;
        event System.Action<long> ExperienceAdded;
        event System.Action<int> LevelCompleted;

        void AddExperience(PlayerExperienceSource source);
        bool CompleteLevel();
    }
}
