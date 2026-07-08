namespace OfflineIncome
{
    public readonly struct OfflineIncomeViewData
    {
        public readonly long Reward;
        public readonly long DoubledReward;
        public readonly int HardClaimCost;
        public readonly bool CanClaimForHard;
        public readonly long ElapsedSeconds;

        public OfflineIncomeViewData(long reward, int hardClaimCost, bool canClaimForHard, long elapsedSeconds)
        {
            Reward = reward;
            DoubledReward = reward * 2;
            HardClaimCost = hardClaimCost;
            CanClaimForHard = canClaimForHard;
            ElapsedSeconds = elapsedSeconds;
        }
    }
}
