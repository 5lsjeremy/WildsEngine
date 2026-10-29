using System.Collections.Generic;

namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Coordinates the internal work required to resolve
    /// one complete Pulse.
    /// </summary>
    public interface IPulseCoordinator
    {
        IPulse Pulse { get; }

        int WorkUnitCount { get; }

        IReadOnlyList<IWorkUnit> WorkUnits { get; }

        bool AddWorkUnit(
            IWorkUnit workUnit);

        /// <summary>
        /// True only when every required WorkUnit completed
        /// successfully.
        /// </summary>
        bool CanResolve { get; }

        /// <summary>
        /// True when every required WorkUnit has reached any
        /// terminal state.
        /// </summary>
        bool CanFinalize { get; }

        /// <summary>
        /// Resolves a successful Pulse.
        /// </summary>
        bool TryResolve(
            object? result,
            out IMoment? moment);

        /// <summary>
        /// Finalizes a Pulse according to the terminal state
        /// of all required WorkUnits.
        /// </summary>
        bool TryFinalize(
            object? result,
            out IMoment? moment);
    }
}