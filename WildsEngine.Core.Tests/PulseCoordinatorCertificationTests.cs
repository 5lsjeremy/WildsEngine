using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Processing.Coordinator;
using WildsEngine.Core.Units;
using WildsEngine.Tests.Logging;
using Xunit;

namespace WildsEngine.Core.Tests
{
    /// <summary>
    /// Certifies Pulse resolution semantics.
    ///
    /// A complete consumer request cannot resolve until
    /// every required internal WorkUnit has completed
    /// successfully.
    /// </summary>
    public sealed class PulseCoordinatorCertificationTests
    {
        [Fact]
        public void PulseCannotResolveUntilAllRequiredWorkCompletes()
        {
            TestLog.Section(
                "Pulse Coordination Certification");

            TestLog.Test(
                "Pulse waits for every required WorkUnit");

            var request =
                new TestRuntimeObject(
                    "player-request");

            var pulse =
                new Pulse(
                    request);

            Assert.True(
                pulse.TryBeginDecomposition());

            var first =
                CreateWork(
                    pulse,
                    "QSL");

            var second =
                CreateWork(
                    pulse,
                    "Instruction");

            var third =
                CreateWork(
                    pulse,
                    "Task");

            var coordinator =
                new PulseCoordinator(
                    pulse);

            Assert.True(
                coordinator.AddWorkUnit(
                    first));

            Assert.True(
                coordinator.AddWorkUnit(
                    second));

            Assert.True(
                coordinator.AddWorkUnit(
                    third));

            TestLog.State(
                $"Pulse {pulse.Id} owns " +
                $"{coordinator.WorkUnitCount} required WorkUnits");

            Assert.True(
                pulse.TryMarkPending());

            Assert.True(
                pulse.TryMarkReady());

            Assert.True(
                pulse.TryBeginProcessing());

            BeginAndComplete(
                first,
                "qsl-result");

            TestLog.Task(
                "WorkUnit A completed");

            BeginAndComplete(
                second,
                "instruction-result");

            TestLog.Task(
                "WorkUnit B completed");

            Assert.True(
                third.TryBeginProcessing());

            TestLog.Task(
                "WorkUnit C still processing");

            Assert.False(
                coordinator.CanResolve);

            Assert.False(
                coordinator.TryResolve(
                    "too-early",
                    out var earlyMoment));

            Assert.Null(
                earlyMoment);

            Assert.Equal(
                PulseStatus.Processing,
                pulse.Status);

            TestLog.Expected(
                "Pulse refused early completion");

            var finalResult =
                new TestRuntimeObject(
                    "final-result");

            Assert.True(
                third.TryComplete(
                    finalResult));

            TestLog.Task(
                "WorkUnit C completed");

            Assert.True(
                coordinator.CanResolve);

            Assert.True(
                coordinator.TryResolve(
                    finalResult,
                    out var moment));

            Assert.NotNull(
                moment);

            Assert.Equal(
                PulseStatus.Completed,
                pulse.Status);

            Assert.True(
                pulse.IsTerminal);

            Assert.Equal(
                pulse.Id,
                moment!.PulseId);

            Assert.Equal(
                3,
                moment.WorkUnitIds.Count);

            Assert.Contains(
                first.Id,
                moment.WorkUnitIds);

            Assert.Contains(
                second.Id,
                moment.WorkUnitIds);

            Assert.Contains(
                third.Id,
                moment.WorkUnitIds);

            Assert.Same(
                finalResult,
                moment.Result);

            TestLog.State(
                "All required WorkUnits resolved");

            TestLog.State(
                $"Moment {moment.Id} closed Pulse {pulse.Id}");

            TestLog.Pass(
                "Pulse completed only after all required work completed");
        }

        [Fact]
        public void CoordinatorRejectsWorkFromAnotherPulse()
        {
            var firstPulse =
                new Pulse(
                    "first");

            var secondPulse =
                new Pulse(
                    "second");

            var foreignWork =
                new WorkUnit(
                    secondPulse.Id,
                    "foreign-work");

            var coordinator =
                new PulseCoordinator(
                    firstPulse);

            Assert.False(
                coordinator.AddWorkUnit(
                    foreignWork));

            Assert.Equal(
                0,
                coordinator.WorkUnitCount);
        }

        [Fact]
        public void CoordinatorRejectsDuplicateWorkUnit()
        {
            var pulse =
                new Pulse(
                    "request");

            var work =
                new WorkUnit(
                    pulse.Id,
                    "work");

            var coordinator =
                new PulseCoordinator(
                    pulse);

            Assert.True(
                coordinator.AddWorkUnit(
                    work));

            Assert.False(
                coordinator.AddWorkUnit(
                    work));

            Assert.Equal(
                1,
                coordinator.WorkUnitCount);
        }

        [Fact]
        public void FailedWorkPreventsSuccessfulResolution()
        {
            TestLog.Test(
                "Failed WorkUnit prevents successful Pulse resolution");

            var pulse =
                CreateProcessingPulse(
                    "failure-request");

            var completed =
                CreateWork(
                    pulse,
                    "completed");

            var failed =
                CreateWork(
                    pulse,
                    "failed");

            var coordinator =
                new PulseCoordinator(
                    pulse);

            Assert.True(
                coordinator.AddWorkUnit(
                    completed));

            Assert.True(
                coordinator.AddWorkUnit(
                    failed));

            BeginAndComplete(
                completed,
                "completed-result");

            Assert.True(
                failed.TryBeginProcessing());

            var failure =
                new InvalidOperationException(
                    "Expected WorkUnit failure.");

            Assert.True(
                failed.TryFail(
                    failure));

            Assert.False(
                coordinator.CanResolve);

            Assert.False(
                coordinator.TryResolve(
                    "must-not-succeed",
                    out var moment));

            Assert.Null(
                moment);

            Assert.Equal(
                PulseStatus.Processing,
                pulse.Status);

            Assert.Same(
                failure,
                failed.Exception);

            TestLog.Expected(
                "Failed WorkUnit blocked successful Pulse resolution");

            TestLog.Pass(
                "Failure cannot masquerade as successful completion");
        }

        [Fact]
        public void CancelledWorkPreventsSuccessfulResolution()
        {
            TestLog.Test(
                "Cancelled WorkUnit prevents successful Pulse resolution");

            var pulse =
                CreateProcessingPulse(
                    "cancellation-request");

            var completed =
                CreateWork(
                    pulse,
                    "completed");

            var cancelled =
                CreateWork(
                    pulse,
                    "cancelled");

            var coordinator =
                new PulseCoordinator(
                    pulse);

            Assert.True(
                coordinator.AddWorkUnit(
                    completed));

            Assert.True(
                coordinator.AddWorkUnit(
                    cancelled));

            BeginAndComplete(
                completed,
                "completed-result");

            Assert.Equal(
                WorkUnitStatus.Pending,
                cancelled.Status);

            Assert.True(
                cancelled.TryCancel());

            Assert.False(
                coordinator.CanResolve);

            Assert.False(
                coordinator.TryResolve(
                    "must-not-succeed",
                    out var moment));

            Assert.Null(
                moment);

            Assert.Equal(
                PulseStatus.Processing,
                pulse.Status);

            TestLog.Expected(
                "Cancelled WorkUnit blocked successful Pulse resolution");

            TestLog.Pass(
                "Cancellation cannot masquerade as successful completion");
        }

        [Fact]
        public void EmptyCoordinatorCannotResolvePulse()
        {
            var pulse =
                CreateProcessingPulse(
                    "empty-request");

            var coordinator =
                new PulseCoordinator(
                    pulse);

            Assert.Equal(
                0,
                coordinator.WorkUnitCount);

            Assert.False(
                coordinator.CanResolve);

            Assert.False(
                coordinator.TryResolve(
                    "result",
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

        private static WorkUnit CreateWork(
            Pulse pulse,
            object? payload)
        {
            var work =
                new WorkUnit(
                    pulse.Id,
                    payload);

            Assert.True(
                work.TryMarkPending());

            return work;
        }

        private static void BeginAndComplete(
            WorkUnit work,
            object? result)
        {
            Assert.True(
                work.TryBeginProcessing());

            Assert.True(
                work.TryComplete(
                    result));
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
