using System;

namespace Core.Time
{
    public class SystemTimeService : ITimeService
    {
        public long CurrentUtcTicks => DateTime.UtcNow.Ticks;
    }
}
