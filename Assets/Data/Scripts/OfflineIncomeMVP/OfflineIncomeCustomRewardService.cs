using QuestSystem;

namespace OfflineIncome
{
    public class OfflineIncomeCustomRewardService : ICustomRewardHandler
    {
        private const string DoubleNextOfflineIncomeRewardId = "double_next_offline_income";

        private readonly IOfflineIncomeRuntimeSave _save;

        public OfflineIncomeCustomRewardService(IOfflineIncomeRuntimeSave save)
        {
            _save = save;
        }

        public bool TryGiveCustomReward(string rewardId)
        {
            if (rewardId != DoubleNextOfflineIncomeRewardId)
            {
                return false;
            }

            _save.SetPendingDoubleNextReward(true);
            return true;
        }
    }
}
