using Core;

namespace OfflineIncome
{
    public interface IOfflineIncomeRuntimeSave : ISavable<OfflineIncomeRuntimeSave.SaveData>
    {
        long LastOnlineTicks { get; }
        event System.Action Changed;
        void SetLastOnlineTicks(long ticks);
    }
}
