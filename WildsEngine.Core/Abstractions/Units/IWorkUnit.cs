using System;

namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Represents one internal unit of processing belonging
    /// to a complete pulse.
    /// </summary>
    public interface IWorkUnit :
        IBaseUnit
    {
        Guid PulseId { get; }

        object? Payload { get; }

        WorkUnitStatus Status { get; }

        object? Result { get; }

        Exception? Exception { get; }

        bool IsTerminal { get; }

        bool TryMarkPending();

        bool TryBeginProcessing();

        bool TryComplete(
            object? result);

        bool TryFail(
            Exception exception);

        bool TryCancel();
    }
}
