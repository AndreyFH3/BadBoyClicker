using System;
using UnityEngine;

namespace DailyLogin
{
    [Serializable]
    public class DailyLoginDayConfig
    {
        [SerializeField] private RewardConfig _reward = new();
        [SerializeField] private bool _isMilestone;

        public RewardConfig Reward => _reward;
        public bool IsMilestone => _isMilestone;
    }
}
