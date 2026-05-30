namespace OfflineIncome
{
    public readonly struct OfflineIncomeViewData
    {
        public readonly long Reward;
        public readonly long DoubledReward;
        public readonly int HardClaimCost;
        public readonly bool CanClaimForHard;

        public OfflineIncomeViewData(long reward, int hardClaimCost, bool canClaimForHard)
        {
            Reward = reward;
            DoubledReward = reward * 2;
            HardClaimCost = hardClaimCost;
            CanClaimForHard = canClaimForHard;
        }
    }
}
