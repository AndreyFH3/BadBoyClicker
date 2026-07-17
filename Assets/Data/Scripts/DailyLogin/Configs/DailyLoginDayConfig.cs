using System;
using System.Collections.Generic;
using UnityEngine;

namespace DailyLogin
{
    [Serializable]
    public class DailyLoginDayConfig
    {
        [SerializeField] private RewardConfig _reward = new();
        [SerializeField] private bool _isMilestone;
        [SerializeField] private List<RewardConfig> _cycleRewards = new();

        public RewardConfig Reward => _reward;
        public bool IsMilestone => _isMilestone;

        public RewardConfig GetReward(int completedCycles)
        {
            if (_cycleRewards == null || _cycleRewards.Count == 0)
            {
                return _reward;
            }

            int index = Mathf.Clamp(completedCycles, 0, _cycleRewards.Count - 1);
            return _cycleRewards[index];
        }
    }
}
