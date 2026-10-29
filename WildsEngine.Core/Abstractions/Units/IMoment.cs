using System;
using System.Collections.Generic;

namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Represents the terminal outcome of one Pulse.
    ///
    /// Moments record successful, failed, and cancelled
    /// resolutions so Timeline history remains complete.
    /// </summary>
    public interface IMoment :
        IBaseUnit
    {
        Guid PulseId { get; }

        MomentOutcome Outcome { get; }

        object? Result { get; }

        Exception? Exception { get; }

        IReadOnlyList<Guid> WorkUnitIds { get; }
    }
}