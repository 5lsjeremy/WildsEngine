using System;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Units
{
    /// <summary>
    /// Represents one complete consumer request.
    ///
    /// The pulse identity is the canonical correlation
    /// identity for every work unit, instruction, task,
    /// and eventual moment produced while resolving the
    /// request.
    /// </summary>
    public sealed class Pulse :
        BaseUnit,
        IPulse
    {
        private readonly object _sync =
            new object();

        private PulseStatus _status;

        public Pulse(
            object? request)
            : base()
        {
            Request = request;

            _status =
                PulseStatus.Created;
        }

        public Pulse(
            Guid id,
            object? request)
            : base(
                id)
        {
            Request = request;

            _status =
                PulseStatus.Created;
        }

        public object? Request { get; }

        public PulseStatus Status
        {
            get
            {
                lock (_sync)
                {
                    return _status;
                }
            }
        }

        public bool IsTerminal
        {
            get
            {
                lock (_sync)
                {
                    return IsTerminalStatus(
                        _status);
                }
            }
        }

        public bool TryBeginDecomposition()
        {
            return TryTransition(
                PulseStatus.Created,
                PulseStatus.Decomposing);
        }

        public bool TryMarkPending()
        {
            return TryTransition(
                PulseStatus.Decomposing,
                PulseStatus.Pending);
        }

        public bool TryMarkReady()
        {
            return TryTransition(
                PulseStatus.Pending,
                PulseStatus.Ready);
        }

        public bool TryBeginProcessing()
        {
            return TryTransition(
                PulseStatus.Ready,
                PulseStatus.Processing);
        }

        public bool TryComplete()
        {
            return TryTransition(
                PulseStatus.Processing,
                PulseStatus.Completed);
        }

        public bool TryFail()
        {
            return TryTerminalTransition(
                PulseStatus.Failed);
        }

        public bool TryCancel()
        {
            return TryTerminalTransition(
                PulseStatus.Cancelled);
        }

        private bool TryTransition(
            PulseStatus expected,
            PulseStatus next)
        {
            lock (_sync)
            {
                if (_status != expected)
                {
                    return false;
                }

                _status = next;

                return true;
            }
        }

        private bool TryTerminalTransition(
            PulseStatus terminalStatus)
        {
            lock (_sync)
            {
                if (IsTerminalStatus(
                        _status))
                {
                    return false;
                }

                _status = terminalStatus;

                return true;
            }
        }

        private static bool IsTerminalStatus(
            PulseStatus status)
        {
            return status ==
                   PulseStatus.Completed ||
                   status ==
                   PulseStatus.Failed ||
                   status ==
                   PulseStatus.Cancelled;
        }
    }
}
