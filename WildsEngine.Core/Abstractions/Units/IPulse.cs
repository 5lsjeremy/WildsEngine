namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Represents one complete request submitted to Wilds.
    ///
    /// A pulse remains the request boundary regardless of
    /// how many internal work units, instructions, tasks,
    /// host frames, or asynchronous operations are required
    /// to resolve it.
    /// </summary>
    public interface IPulse :
        IBaseUnit
    {
        object? Request { get; }

        PulseStatus Status { get; }

        bool IsTerminal { get; }

        bool TryBeginDecomposition();

        bool TryMarkPending();

        bool TryMarkReady();

        bool TryBeginProcessing();

        bool TryComplete();

        bool TryFail();

        bool TryCancel();
    }
}
