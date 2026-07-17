using QuestSystem;
using UnityEngine;

namespace OfflineIncome
{
    public class OfflineIncomeCustomRewardService : IQuestCustomRewardService
    {
        private const string DoubleNextOfflineIncomeRewardId = "double_next_offline_income";

        private readonly IOfflineIncomeRuntimeSave _save;

        public OfflineIncomeCustomRewardService(IOfflineIncomeRuntimeSave save)
        {
            _save = save;
        }

        public void GiveCustomReward(string rewardId)
        {
            if (rewardId == DoubleNextOfflineIncomeRewardId)
            {
                _save.SetPendingDoubleNextReward(true);
                return;
            }

            Debug.LogWarning($"Unsupported custom reward id: {rewardId}");
        }
    }
}
