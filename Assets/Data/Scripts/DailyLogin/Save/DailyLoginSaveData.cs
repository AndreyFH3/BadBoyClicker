using System;

namespace DailyLogin
{
    [Serializable]
    public class DailyLoginSaveData
    {
        public int CurrentDayIndex;
        public long LastClaimUtcTicks;
        public int CompletedCycles;
    }
}
