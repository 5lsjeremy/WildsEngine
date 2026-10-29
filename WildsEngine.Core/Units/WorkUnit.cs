using System;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Units
{
    /// <summary>
    /// Represents one internal piece of processing required
    /// to resolve a pulse.
    ///
    /// Work units are implementation details of Wilds.
    /// Consumers submit pulses and are not responsible for
    /// estimating, creating, or scheduling work units.
    /// </summary>
    public sealed class WorkUnit :
        BaseUnit,
        IWorkUnit
    {
        private readonly object _sync =
            new object();

        private WorkUnitStatus _status;
        private object? _result;
        private Exception? _exception;

        public WorkUnit(
            Guid pulseId,
            object? payload)
            : base()
        {
            PulseId =
                ValidatePulseId(
                    pulseId);

            Payload = payload;

            _status =
                WorkUnitStatus.Created;
        }

        public WorkUnit(
            Guid id,
            Guid pulseId,
            object? payload)
            : base(
                id)
        {
            PulseId =
                ValidatePulseId(
                    pulseId);

            Payload = payload;

            _status =
                WorkUnitStatus.Created;
        }

        public Guid PulseId { get; }

        public object? Payload { get; }

        public WorkUnitStatus Status
        {
            get
            {
                lock (_sync)
                {
                    return _status;
                }
            }
        }

        public object? Result
        {
            get
            {
                lock (_sync)
                {
                    return _result;
                }
            }
        }

        public Exception? Exception
        {
            get
            {
                lock (_sync)
                {
                    return _exception;
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

        public bool TryMarkPending()
        {
            return TryTransition(
                WorkUnitStatus.Created,
                WorkUnitStatus.Pending);
        }

        public bool TryBeginProcessing()
        {
            return TryTransition(
                WorkUnitStatus.Pending,
                WorkUnitStatus.Processing);
        }

        public bool TryComplete(
            object? result)
        {
            lock (_sync)
            {
                if (_status !=
                    WorkUnitStatus.Processing)
                {
                    return false;
                }

                _result = result;
                _exception = null;

                _status =
                    WorkUnitStatus.Completed;

                return true;
            }
        }

        public bool TryFail(
            Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(
                    nameof(exception));
            }

            lock (_sync)
            {
                if (IsTerminalStatus(
                        _status))
                {
                    return false;
                }

                _exception = exception;

                _status =
                    WorkUnitStatus.Failed;

                return true;
            }
        }

        public bool TryCancel()
        {
            lock (_sync)
            {
                if (IsTerminalStatus(
                        _status))
                {
                    return false;
                }

                _status =
                    WorkUnitStatus.Cancelled;

                return true;
            }
        }

        private bool TryTransition(
            WorkUnitStatus expected,
            WorkUnitStatus next)
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

        private static Guid ValidatePulseId(
            Guid pulseId)
        {
            if (pulseId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Work units require a non-empty pulse ID.",
                    nameof(pulseId));
            }

            return pulseId;
        }

        private static bool IsTerminalStatus(
            WorkUnitStatus status)
        {
            return status ==
                   WorkUnitStatus.Completed ||
                   status ==
                   WorkUnitStatus.Failed ||
                   status ==
                   WorkUnitStatus.Cancelled;
        }
    }
}
