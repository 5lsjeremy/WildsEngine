using System.Collections.Generic;

namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Represents a collection of Wilds timelines.
    /// </summary>
    public interface IEpoch :
        IBaseUnit
    {
        int Count { get; }

        IReadOnlyList<ITimeline> Timelines { get; }

        bool AddTimeline(
            ITimeline timeline);
    }
}
