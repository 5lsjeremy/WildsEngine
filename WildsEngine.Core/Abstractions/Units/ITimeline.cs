using System.Collections.Generic;

namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Represents an ordered history of completed moments.
    /// </summary>
    public interface ITimeline :
        IBaseUnit
    {
        int Count { get; }

        IReadOnlyList<IMoment> Moments { get; }

        bool AddMoment(
            IMoment moment);
    }
}
