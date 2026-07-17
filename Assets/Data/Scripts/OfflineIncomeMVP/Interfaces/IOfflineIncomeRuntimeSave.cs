using Core;

namespace OfflineIncome
{
    public interface IOfflineIncomeRuntimeSave : ISavable<OfflineIncomeRuntimeSave.SaveData>
    {
        long LastOnlineTicks { get; }
        bool PendingDoubleNextReward { get; }
        event System.Action Changed;
        void SetLastOnlineTicks(long ticks);
        void SetPendingDoubleNextReward(bool value);
    }
}
