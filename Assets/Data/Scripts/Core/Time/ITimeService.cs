namespace Core.Time
{
    public interface ITimeService
    {
        long CurrentUtcTicks { get; }
    }
}
