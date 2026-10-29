using System;
using System.Collections.Generic;
using System.Linq;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Units;

namespace WildsEngine.Core.Processing.Coordinator
{
    /// <summary>
    /// Coordinates all required WorkUnits for one Pulse.
    ///
    /// The coordinator prevents early completion and records
    /// successful, failed, and cancelled Pulse outcomes.
    /// </summary>
    public sealed class PulseCoordinator :
        IPulseCoordinator
    {
        private readonly object _sync =
            new object();

        private readonly List<IWorkUnit> _workUnits =
            new List<IWorkUnit>();

        private readonly HashSet<Guid> _workUnitIds =
            new HashSet<Guid>();

        public PulseCoordinator(
            IPulse pulse)
        {
            Pulse = pulse ??
                throw new ArgumentNullException(
                    nameof(pulse));
        }

        public IPulse Pulse { get; }

        public int WorkUnitCount
        {
            get
            {
                lock (_sync)
                {
                    return _workUnits.Count;
                }
            }
        }

        public IReadOnlyList<IWorkUnit> WorkUnits
        {
            get
            {
                lock (_sync)
                {
                    return _workUnits.ToArray();
                }
            }
        }

        public bool CanResolve
        {
            get
            {
                lock (_sync)
                {
                    return CanResolveCore();
                }
            }
        }

        public bool CanFinalize
        {
            get
            {
                lock (_sync)
                {
                    return CanFinalizeCore();
                }
            }
        }

        public bool AddWorkUnit(
            IWorkUnit workUnit)
        {
            if (workUnit == null)
            {
                throw new ArgumentNullException(
                    nameof(workUnit));
            }

            if (workUnit.PulseId != Pulse.Id)
            {
                return false;
            }

            lock (_sync)
            {
                if (Pulse.IsTerminal)
                {
                    return false;
                }

                if (!_workUnitIds.Add(
                        workUnit.Id))
                {
                    return false;
                }

                _workUnits.Add(
                    workUnit);

                return true;
            }
        }

        public bool TryResolve(
            object? result,
            out IMoment? moment)
        {
            lock (_sync)
            {
                moment = null;

                if (!CanResolveCore())
                {
                    return false;
                }

                return TryCreateCompletedMoment(
                    result,
                    out moment);
            }
        }

        public bool TryFinalize(
            object? result,
            out IMoment? moment)
        {
            lock (_sync)
            {
                moment = null;

                if (!CanFinalizeCore())
                {
                    return false;
                }

                var failedWork =
                    GetFirstFailedWorkUnit();

                if (failedWork != null)
                {
                    return TryCreateFailedMoment(
                        failedWork,
                        out moment);
                }

                if (ContainsCancelledWorkUnit())
                {
                    return TryCreateCancelledMoment(
                        out moment);
                }

                return TryCreateCompletedMoment(
                    result,
                    out moment);
            }
        }

        private bool TryCreateCompletedMoment(
            object? result,
            out IMoment? moment)
        {
            moment = null;

            if (!CanResolveCore())
            {
                return false;
            }

            if (!Pulse.TryComplete())
            {
                return false;
            }

            moment =
                new Moment(
                    Pulse.Id,
                    MomentOutcome.Completed,
                    result,
                    null,
                    GetWorkUnitIds());

            return true;
        }

        private bool TryCreateFailedMoment(
            IWorkUnit failedWork,
            out IMoment? moment)
        {
            moment = null;

            var exception =
                failedWork.Exception;

            if (exception == null)
            {
                return false;
            }

            if (!Pulse.TryFail())
            {
                return false;
            }

            moment =
                new Moment(
                    Pulse.Id,
                    MomentOutcome.Failed,
                    null,
                    exception,
                    GetWorkUnitIds());

            return true;
        }

        private bool TryCreateCancelledMoment(
            out IMoment? moment)
        {
            moment = null;

            if (!Pulse.TryCancel())
            {
                return false;
            }

            moment =
                new Moment(
                    Pulse.Id,
                    MomentOutcome.Cancelled,
                    null,
                    null,
                    GetWorkUnitIds());

            return true;
        }

        private bool CanResolveCore()
        {
            if (Pulse.Status !=
                PulseStatus.Processing)
            {
                return false;
            }

            if (_workUnits.Count == 0)
            {
                return false;
            }

            for (
                var index = 0;
                index < _workUnits.Count;
                index++)
            {
                if (_workUnits[index].Status !=
                    WorkUnitStatus.Completed)
                {
                    return false;
                }
            }

            return true;
        }

        private bool CanFinalizeCore()
        {
            if (Pulse.Status !=
                PulseStatus.Processing)
            {
                return false;
            }

            if (_workUnits.Count == 0)
            {
                return false;
            }

            for (
                var index = 0;
                index < _workUnits.Count;
                index++)
            {
                if (!_workUnits[index].IsTerminal)
                {
                    return false;
                }
            }

            return true;
        }

        private IWorkUnit? GetFirstFailedWorkUnit()
        {
            for (
                var index = 0;
                index < _workUnits.Count;
                index++)
            {
                var workUnit =
                    _workUnits[index];

                if (workUnit.Status ==
                    WorkUnitStatus.Failed)
                {
                    return workUnit;
                }
            }

            return null;
        }

        private bool ContainsCancelledWorkUnit()
        {
            for (
                var index = 0;
                index < _workUnits.Count;
                index++)
            {
                if (_workUnits[index].Status ==
                    WorkUnitStatus.Cancelled)
                {
                    return true;
                }
            }

            return false;
        }

        private Guid[] GetWorkUnitIds()
        {
            return _workUnits
                .Select(
                    workUnit =>
                        workUnit.Id)
                .ToArray();
        }
    }
}