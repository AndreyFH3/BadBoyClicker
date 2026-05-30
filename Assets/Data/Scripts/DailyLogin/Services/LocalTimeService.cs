using System;
using Core.Time;

namespace DailyLogin
{
    public class LocalTimeService : ITimeService
    {
        public long CurrentUtcTicks => DateTime.UtcNow.Ticks;
    }
}
