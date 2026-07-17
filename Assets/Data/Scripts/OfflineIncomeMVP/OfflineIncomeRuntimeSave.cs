using System;

namespace OfflineIncome
{
    public class OfflineIncomeRuntimeSave : IOfflineIncomeRuntimeSave
    {
        public long LastOnlineTicks { get; private set; }
        public bool PendingDoubleNextReward { get; private set; }
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

        public void SetPendingDoubleNextReward(bool value)
        {
            if (PendingDoubleNextReward == value)
            {
                return;
            }

            PendingDoubleNextReward = value;
            Changed?.Invoke();
        }

        public void Set(SaveData data)
        {
            LastOnlineTicks = data.LastOnlineTicks;
            PendingDoubleNextReward = data.PendingDoubleNextReward;
            Changed?.Invoke();
        }

        public SaveData Get()
        {
            return new SaveData
            {
                LastOnlineTicks = LastOnlineTicks,
                PendingDoubleNextReward = PendingDoubleNextReward
            };
        }

        [Serializable]
        public struct SaveData
        {
            public long LastOnlineTicks;
            public bool PendingDoubleNextReward;
        }
    }
}
