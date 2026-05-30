using System;

namespace OfflineIncome
{
    public class OfflineIncomeRuntimeSave : IOfflineIncomeRuntimeSave
    {
        public long LastOnlineTicks { get; private set; }
        public event Action Changed;

        public void SetLastOnlineTicks(long ticks)
        {
            if (LastOnlineTicks == ticks)
            {
                return;
            }

            LastOnlineTicks = ticks;
            Changed?.Invoke();
        }

        public void Set(SaveData data)
        {
            LastOnlineTicks = data.LastOnlineTicks;
            Changed?.Invoke();
        }

        public SaveData Get()
        {
            return new SaveData
            {
                LastOnlineTicks = LastOnlineTicks
            };
        }

        [Serializable]
        public struct SaveData
        {
            public long LastOnlineTicks;
        }
    }
}
