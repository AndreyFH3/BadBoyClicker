using System;

namespace DailyLoginMVP
{
    public class DailyLoginStartupGate : IDailyLoginStartupGate
    {
        public bool IsCompleted { get; private set; }
        public event Action Completed;

        public void Complete()
        {
            if (IsCompleted)
            {
                return;
            }

            IsCompleted = true;
            Completed?.Invoke();
        }
    }
}
