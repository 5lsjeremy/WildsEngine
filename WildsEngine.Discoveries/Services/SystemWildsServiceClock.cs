using System;
using System.Diagnostics;
using WildsEngine.Discoveries.Abstractions.Services;

namespace WildsEngine.Discoveries.Services
{
    public sealed class SystemWildsServiceClock : IWildsServiceClock
    {
        public DateTimeOffset GetUtcNow() => DateTimeOffset.UtcNow;
        public long GetTimestamp() => Stopwatch.GetTimestamp();
        public TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp)
        {
            var seconds = (double)(endingTimestamp - startingTimestamp) / Stopwatch.Frequency;
            return TimeSpan.FromSeconds(seconds);
        }
    }
}
