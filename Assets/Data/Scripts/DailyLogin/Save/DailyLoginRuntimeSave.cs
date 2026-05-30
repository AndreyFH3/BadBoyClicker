using System;

namespace DailyLogin
{
    public class DailyLoginRuntimeSave : IDailyLoginRuntimeSave
    {
        public int CurrentDayIndex { get; private set; }
        public long LastClaimUtcTicks { get; private set; }
        public event Action Changed;

        public void SetClaimState(int currentDayIndex, long lastClaimUtcTicks)
        {
            currentDayIndex = Math.Max(0, currentDayIndex);

            if (CurrentDayIndex == currentDayIndex && LastClaimUtcTicks == lastClaimUtcTicks)
            {
                return;
            }

            CurrentDayIndex = currentDayIndex;
            LastClaimUtcTicks = Math.Max(0, lastClaimUtcTicks);
            Changed?.Invoke();
        }

        public void Set(DailyLoginSaveData data)
        {
            if (data == null)
            {
                CurrentDayIndex = 0;
                LastClaimUtcTicks = 0;
            }
            else
            {
                CurrentDayIndex = Math.Max(0, data.CurrentDayIndex);
                LastClaimUtcTicks = Math.Max(0, data.LastClaimUtcTicks);
            }

            Changed?.Invoke();
        }

        public DailyLoginSaveData Get()
        {
            return new DailyLoginSaveData
            {
                CurrentDayIndex = CurrentDayIndex,
                LastClaimUtcTicks = LastClaimUtcTicks
            };
        }
    }
}
