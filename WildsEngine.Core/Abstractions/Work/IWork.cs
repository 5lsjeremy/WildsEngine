using System;

namespace WildsEngine.Core.Abstractions
{
    public interface IWork
    {
        Guid Id { get; }

        string Source { get; }

        WorkStatus Status { get; }

        object? Result { get; }

        Exception? Exception { get; }

        bool IsTerminal { get; }

        bool TryBeginDecomposition();

        bool TryMarkPending();

        bool TryMarkReady();

        bool TryBeginProcessing();

        bool TryComplete(
            object? result);

        bool TryFail(
            Exception exception);

        bool TryCancel();
    }
}