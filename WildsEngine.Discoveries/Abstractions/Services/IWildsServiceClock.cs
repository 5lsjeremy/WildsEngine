using System;

namespace WildsEngine.Discoveries.Abstractions.Services
{
    public interface IWildsServiceClock
    {
        DateTimeOffset GetUtcNow();
        long GetTimestamp();
        TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp);
    }
}
