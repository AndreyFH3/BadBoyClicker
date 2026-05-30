using System.Collections.Generic;

namespace DailyLogin
{
    public interface IDailyLoginService
    {
        bool CanClaim();
        int GetCurrentDayIndex();
        DailyLoginDayConfig GetCurrentRewardDay();
        IReadOnlyList<DailyLoginDayConfig> GetRewardDays();
        bool Claim();
    }
}
