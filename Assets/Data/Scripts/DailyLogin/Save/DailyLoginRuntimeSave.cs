using System;

namespace DailyLogin
{
    public class DailyLoginRuntimeSave : IDailyLoginRuntimeSave
    {
        public int CurrentDayIndex { get; private set; }
        public long LastClaimUtcTicks { get; private set; }
        public int CompletedCycles { get; private set; }
        public event Action Changed;

        public void SetClaimState(int currentDayIndex, long lastClaimUtcTicks, int completedCycles)
        {
            currentDayIndex = Math.Max(0, currentDayIndex);
            completedCycles = Math.Max(0, completedCycles);

            if (CurrentDayIndex == currentDayIndex && LastClaimUtcTicks == lastClaimUtcTicks && CompletedCycles == completedCycles)
            {
                return;
            }

            CurrentDayIndex = currentDayIndex;
            LastClaimUtcTicks = Math.Max(0, lastClaimUtcTicks);
            CompletedCycles = completedCycles;
            Changed?.Invoke();
        }

        public void Set(DailyLoginSaveData data)
        {
            if (data == null)
            {
                CurrentDayIndex = 0;
                LastClaimUtcTicks = 0;
                CompletedCycles = 0;
            }
            else
            {
                CurrentDayIndex = Math.Max(0, data.CurrentDayIndex);
                LastClaimUtcTicks = Math.Max(0, data.LastClaimUtcTicks);
                CompletedCycles = Math.Max(0, data.CompletedCycles);
            }

            Changed?.Invoke();
        }

        public DailyLoginSaveData Get()
        {
            return new DailyLoginSaveData
            {
                CurrentDayIndex = CurrentDayIndex,
                LastClaimUtcTicks = LastClaimUtcTicks,
                CompletedCycles = CompletedCycles
            };
        }
    }
}
