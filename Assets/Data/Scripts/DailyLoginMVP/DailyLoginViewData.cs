using System.Collections.Generic;

namespace DailyLoginMVP
{
    public readonly struct DailyLoginViewData
    {
        public readonly IReadOnlyList<DailyLoginRewardViewData> Rewards;

        public DailyLoginViewData(IReadOnlyList<DailyLoginRewardViewData> rewards)
        {
            Rewards = rewards;
        }
    }
}
