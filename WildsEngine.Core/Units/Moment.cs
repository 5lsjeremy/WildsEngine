using System;
using System.Collections.Generic;
using System.Linq;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Units
{
    /// <summary>
    /// Immutable record of a terminal Pulse outcome.
    ///
    /// A Moment connects the Pulse identity, contributing
    /// WorkUnits, final result, and terminal outcome.
    /// </summary>
    public sealed class Moment :
        BaseUnit,
        IMoment
    {
        private readonly IReadOnlyList<Guid>
            _workUnitIds;

        public Moment(
            Guid pulseId,
            object? result,
            IEnumerable<Guid>? workUnitIds = null)
            : this(
                Guid.NewGuid(),
                pulseId,
                MomentOutcome.Completed,
                result,
                null,
                workUnitIds)
        {
        }

        public Moment(
            Guid id,
            Guid pulseId,
            object? result,
            IEnumerable<Guid>? workUnitIds = null)
            : this(
                id,
                pulseId,
                MomentOutcome.Completed,
                result,
                null,
                workUnitIds)
        {
        }

        public Moment(
            Guid pulseId,
            MomentOutcome outcome,
            object? result,
            Exception? exception,
            IEnumerable<Guid>? workUnitIds = null)
            : this(
                Guid.NewGuid(),
                pulseId,
                outcome,
                result,
                exception,
                workUnitIds)
        {
        }

        public Moment(
            Guid id,
            Guid pulseId,
            MomentOutcome outcome,
            object? result,
            Exception? exception,
            IEnumerable<Guid>? workUnitIds = null)
            : base(
                id)
        {
            PulseId =
                ValidatePulseId(
                    pulseId);

            ValidateOutcome(
                outcome,
                exception);

            Outcome = outcome;
            Result = result;
            Exception = exception;

            _workUnitIds =
                CopyWorkUnitIds(
                    workUnitIds);
        }

        public Guid PulseId { get; }

        public MomentOutcome Outcome { get; }

        public object? Result { get; }

        public Exception? Exception { get; }

        public IReadOnlyList<Guid> WorkUnitIds =>
            _workUnitIds;

        private static Guid ValidatePulseId(
            Guid pulseId)
        {
            if (pulseId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Moments require a non-empty Pulse ID.",
                    nameof(pulseId));
            }

            return pulseId;
        }

        private static void ValidateOutcome(
            MomentOutcome outcome,
            Exception? exception)
        {
            if (outcome ==
                MomentOutcome.Failed &&
                exception == null)
            {
                throw new ArgumentException(
                    "Failed Moments require an exception.",
                    nameof(exception));
            }

            if (outcome !=
                MomentOutcome.Failed &&
                exception != null)
            {
                throw new ArgumentException(
                    "Only failed Moments may carry an exception.",
                    nameof(exception));
            }
        }

        private static IReadOnlyList<Guid>
            CopyWorkUnitIds(
                IEnumerable<Guid>? workUnitIds)
        {
            if (workUnitIds == null)
            {
                return Array.Empty<Guid>();
            }

            var copy =
                workUnitIds
                    .Where(
                        id => id != Guid.Empty)
                    .Distinct()
                    .ToArray();

            return copy.Length == 0
                ? Array.Empty<Guid>()
                : copy;
        }
    }
}