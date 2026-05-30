using System;

namespace DailyLoginMVP
{
    public interface IDailyLoginStartupGate
    {
        bool IsCompleted { get; }
        event Action Completed;
        void Complete();
    }
}
