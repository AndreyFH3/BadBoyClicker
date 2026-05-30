using Core;
using System;

namespace DailyLogin
{
    public interface IDailyLoginRuntimeSave : ISavable<DailyLoginSaveData>
    {
        int CurrentDayIndex { get; }
        long LastClaimUtcTicks { get; }
        event Action Changed;
        void SetClaimState(int currentDayIndex, long lastClaimUtcTicks);
    }
}
