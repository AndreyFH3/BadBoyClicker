using System;
using UnityEngine;

namespace DailyLogin
{
    [Serializable]
    public class DailyLoginDayConfig
    {
        [SerializeField] private RewardConfig _reward = new();

        public RewardConfig Reward => _reward;
    }
}
