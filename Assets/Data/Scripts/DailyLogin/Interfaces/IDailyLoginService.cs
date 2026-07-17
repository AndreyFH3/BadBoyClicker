using System.Collections.Generic;

namespace DailyLogin
{
    public interface IDailyLoginService
    {
        event System.Action<int, RewardConfig> RewardClaimed;

        bool CanClaim();
        int GetCurrentDayIndex();
        int GetCurrentCycle();
        DailyLoginDayConfig GetCurrentRewardDay();
        IReadOnlyList<DailyLoginDayConfig> GetRewardDays();
        bool Claim();
    }
}
