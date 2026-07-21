using System;
using System.Collections.Generic;
using Core;
using Core.Time;
using PlayerFeatures;
using UnityEngine;

namespace DailyLogin
{
    public class DailyLoginService : IDailyLoginService
    {
        private readonly DailyLoginConfig _config;
        private readonly IDailyLoginRuntimeSave _runtimeSave;
        private readonly IRewardService _rewardService;
        private readonly ISaveSystem _saveSystem;
        private readonly ITimeService _timeService;
        private readonly IPlayerFeatureUnlockService _featureUnlockService;

        public event Action<int, RewardConfig> RewardClaimed;

        public DailyLoginService(
            DailyLoginConfig config,
            IDailyLoginRuntimeSave runtimeSave,
            IRewardService rewardService,
            ISaveSystem saveSystem,
            ITimeService timeService,
            IPlayerFeatureUnlockService featureUnlockService)
        {
            _config = config;
            _runtimeSave = runtimeSave;
            _rewardService = rewardService;
            _saveSystem = saveSystem;
            _timeService = timeService;
            _featureUnlockService = featureUnlockService;
        }

        public bool CanClaim()
        {
            if (_config == null || !_config.HasRewards)
            {
                return false;
            }

            bool isDateAvailable;
            if (_runtimeSave.LastClaimUtcTicks <= 0)
            {
                isDateAvailable = true;
            }
            else
            {
                isDateAvailable = GetUtcDate(_runtimeSave.LastClaimUtcTicks) < GetUtcDate(_timeService.CurrentUtcTicks);
            }

            if (!isDateAvailable)
            {
                return false;
            }

            RewardConfig reward = GetCurrentRewardDay()?.GetReward(_runtimeSave.CompletedCycles);
            return RewardFeatureGate.IsAvailable(reward, _featureUnlockService);
        }

        public int GetCurrentDayIndex()
        {
            return NormalizeDayIndex(_runtimeSave.CurrentDayIndex);
        }

        public int GetCurrentCycle()
        {
            return _runtimeSave.CompletedCycles;
        }

        public DailyLoginDayConfig GetCurrentRewardDay()
        {
            if (_config == null || !_config.HasRewards)
            {
                return null;
            }

            int index = GetCurrentDayIndex();
            return _config.Days[index];
        }

        public IReadOnlyList<DailyLoginDayConfig> GetRewardDays()
        {
            return _config != null
                ? _config.Days
                : Array.Empty<DailyLoginDayConfig>();
        }

        public bool Claim()
        {
            if (!CanClaim())
            {
                return false;
            }

            DailyLoginDayConfig dayConfig = GetCurrentRewardDay();
            if (dayConfig == null)
            {
                return false;
            }

            int currentDayIndex = GetCurrentDayIndex();
            RewardConfig reward = dayConfig.GetReward(_runtimeSave.CompletedCycles);
            _rewardService.GiveReward(reward);

            bool completesCycle = currentDayIndex >= _config.DaysCount - 1;
            int nextDayIndex = NormalizeDayIndex(_runtimeSave.CurrentDayIndex + 1);
            int nextCompletedCycles = completesCycle ? _runtimeSave.CompletedCycles + 1 : _runtimeSave.CompletedCycles;
            _runtimeSave.SetClaimState(nextDayIndex, _timeService.CurrentUtcTicks, nextCompletedCycles);

            RewardClaimed?.Invoke(currentDayIndex, reward);
            _saveSystem.Save();
            return true;
        }

        private int NormalizeDayIndex(int index)
        {
            if (_config == null || _config.DaysCount <= 0)
            {
                return 0;
            }

            if (index < 0 || index >= _config.DaysCount)
            {
                Debug.LogWarning($"Daily login day index is out of range and will be reset: {index}");
                return 0;
            }

            return index;
        }

        private static DateTime GetUtcDate(long ticks)
        {
            if (ticks <= 0)
            {
                return DateTime.MinValue.Date;
            }

            try
            {
                return new DateTime(ticks, DateTimeKind.Utc).Date;
            }
            catch (ArgumentOutOfRangeException)
            {
                return DateTime.MinValue.Date;
            }
        }
    }
}
