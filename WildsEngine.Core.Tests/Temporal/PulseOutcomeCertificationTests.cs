using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Processing.Coordinator;
using WildsEngine.Core.Units;
using WildsEngine.Tests.Logging;
using Xunit;

namespace WildsEngine.Core.Tests
{
    public sealed class PulseOutcomeCertificationTests
    {
        [Fact]
        public void CompletedWorkProducesCompletedMoment()
        {
            TestLog.Test(
                "Completed WorkUnits produce a completed Moment");

            var pulse =
                CreateProcessingPulse(
                    "success-request");

            var work =
                CreateProcessingWork(
                    pulse,
                    "success-work");

            var result =
                new TestRuntimeObject(
                    "success-result");

            Assert.True(
                work.TryComplete(
                    result));

            var coordinator =
                new PulseCoordinator(
                    pulse);

            Assert.True(
                coordinator.AddWorkUnit(
                    work));

            Assert.True(
                coordinator.CanResolve);

            Assert.True(
                coordinator.CanFinalize);

            Assert.True(
                coordinator.TryFinalize(
                    result,
                    out var moment));

            Assert.NotNull(
                moment);

            Assert.Equal(
                MomentOutcome.Completed,
                moment!.Outcome);

            Assert.Equal(
                PulseStatus.Completed,
                pulse.Status);

            Assert.Same(
                result,
                moment.Result);

            Assert.Null(
                moment.Exception);

            TestLog.Pass(
                "Completed Pulse recorded as completed Moment");
        }

        [Fact]
        public void FailedWorkProducesFailedMoment()
        {
            TestLog.Test(
                "Failed WorkUnit produces a failed Moment");

            var pulse =
                CreateProcessingPulse(
                    "failure-request");

            var work =
                CreateProcessingWork(
                    pulse,
                    "failure-work");

            var exception =
                new InvalidOperationException(
                    "Expected processing failure.");

            Assert.True(
                work.TryFail(
                    exception));

            var coordinator =
                new PulseCoordinator(
                    pulse);

            Assert.True(
                coordinator.AddWorkUnit(
                    work));

            Assert.False(
                coordinator.CanResolve);

            Assert.True(
                coordinator.CanFinalize);

            Assert.True(
                coordinator.TryFinalize(
                    "must-not-be-used",
                    out var moment));

            Assert.NotNull(
                moment);

            Assert.Equal(
                MomentOutcome.Failed,
                moment!.Outcome);

            Assert.Equal(
                PulseStatus.Failed,
                pulse.Status);

            Assert.Same(
                exception,
                moment.Exception);

            Assert.Null(
                moment.Result);

            TestLog.State(
                $"Failed Pulse recorded | " +
                $"Exception={moment.Exception!.Message}");

            TestLog.Pass(
                "Failure became honest temporal history");
        }

        [Fact]
        public void CancelledWorkProducesCancelledMoment()
        {
            TestLog.Test(
                "Cancelled WorkUnit produces a cancelled Moment");

            var pulse =
                CreateProcessingPulse(
                    "cancellation-request");

            var work =
                new WorkUnit(
                    pulse.Id,
                    "cancellation-work");

            Assert.True(
                work.TryMarkPending());

            Assert.True(
                work.TryCancel());

            var coordinator =
                new PulseCoordinator(
                    pulse);

            Assert.True(
                coordinator.AddWorkUnit(
                    work));

            Assert.False(
                coordinator.CanResolve);

            Assert.True(
                coordinator.CanFinalize);

            Assert.True(
                coordinator.TryFinalize(
                    "must-not-be-used",
                    out var moment));

            Assert.NotNull(
                moment);

            Assert.Equal(
                MomentOutcome.Cancelled,
                moment!.Outcome);

            Assert.Equal(
                PulseStatus.Cancelled,
                pulse.Status);

            Assert.Null(
                moment.Result);

            Assert.Null(
                moment.Exception);

            TestLog.Pass(
                "Cancellation became honest temporal history");
        }

        [Fact]
        public void FailureTakesPrecedenceOverCancellation()
        {
            var pulse =
                CreateProcessingPulse(
                    "mixed-terminal-request");

            var failed =
                CreateProcessingWork(
                    pulse,
                    "failed-work");

            var cancelled =
                new WorkUnit(
                    pulse.Id,
                    "cancelled-work");

            var exception =
                new InvalidOperationException(
                    "Failure wins.");

            Assert.True(
                failed.TryFail(
                    exception));

            Assert.True(
                cancelled.TryMarkPending());

            Assert.True(
                cancelled.TryCancel());

            var coordinator =
                new PulseCoordinator(
                    pulse);

            Assert.True(
                coordinator.AddWorkUnit(
                    failed));

            Assert.True(
                coordinator.AddWorkUnit(
                    cancelled));

            Assert.True(
                coordinator.CanFinalize);

            Assert.True(
                coordinator.TryFinalize(
                    null,
                    out var moment));

            Assert.NotNull(
                moment);

            Assert.Equal(
                MomentOutcome.Failed,
                moment!.Outcome);

            Assert.Equal(
                PulseStatus.Failed,
                pulse.Status);

            Assert.Same(
                exception,
                moment.Exception);
        }

        [Fact]
        public void UnresolvedWorkCannotProduceAnyMoment()
        {
            var pulse =
                CreateProcessingPulse(
                    "unresolved-request");

            var work =
                new WorkUnit(
                    pulse.Id,
                    "pending-work");

            Assert.True(
                work.TryMarkPending());

            var coordinator =
                new PulseCoordinator(
                    pulse);

            Assert.True(
                coordinator.AddWorkUnit(
                    work));

            Assert.False(
                coordinator.CanResolve);

            Assert.False(
                coordinator.CanFinalize);

            Assert.False(
                coordinator.TryFinalize(
                    null,
                    out var moment));

            Assert.Null(
                moment);

            Assert.Equal(
                PulseStatus.Processing,
                pulse.Status);
        }

        private static Pulse CreateProcessingPulse(
            object? request)
        {
            var pulse =
                new Pulse(
                    request);

            Assert.True(
                pulse.TryBeginDecomposition());

            Assert.True(
                pulse.TryMarkPending());

            Assert.True(
                pulse.TryMarkReady());

            Assert.True(
                pulse.TryBeginProcessing());

            return pulse;
        }

        private static WorkUnit CreateProcessingWork(
            Pulse pulse,
            object? payload)
        {
            var work =
                new WorkUnit(
                    pulse.Id,
                    payload);

            Assert.True(
                work.TryMarkPending());

            Assert.True(
                work.TryBeginProcessing());

            return work;
        }

        private sealed class TestRuntimeObject
        {
            public TestRuntimeObject(
                string name)
            {
                Name = name;
            }

            public string Name { get; }
        }
    }
}