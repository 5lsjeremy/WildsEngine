using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Processing.Coordinator;
using WildsEngine.Core.Units;
using WildsEngine.Tests.Logging;
using Xunit;

namespace WildsEngine.Core.Tests;

public sealed class PulseDecompositionCertificationTests
{
    [Fact]
    public void
        OneRequestCanOwnMultipleRequiredWorkUnits()
    {
        TestLog.Section(
            "PULSE MULTI-WORK DECOMPOSITION");

        TestLog.Test(
            "One request owns multiple required WorkUnits through one Pulse.");

        var request =
            new TestRuntimeObject(
                "consumer-request");

        var pulse =
            new Pulse(
                request);

        Assert.True(
            pulse.TryBeginDecomposition());

        var qslWork =
            new WorkUnit(
                pulse.Id,
                new TestRuntimeObject(
                    "qsl-work"));

        var instructionWork =
            new WorkUnit(
                pulse.Id,
                new TestRuntimeObject(
                    "instruction-work"));

        var taskWork =
            new WorkUnit(
                pulse.Id,
                new TestRuntimeObject(
                    "task-work"));

        var coordinator =
            new PulseCoordinator(
                pulse);

        Assert.True(
            coordinator.AddWorkUnit(
                qslWork));

        Assert.True(
            coordinator.AddWorkUnit(
                instructionWork));

        Assert.True(
            coordinator.AddWorkUnit(
                taskWork));

        Assert.Equal(
            3,
            coordinator.WorkUnitCount);

        Assert.Equal(
            pulse.Id,
            qslWork.PulseId);

        Assert.Equal(
            pulse.Id,
            instructionWork.PulseId);

        Assert.Equal(
            pulse.Id,
            taskWork.PulseId);

        Assert.NotEqual(
            qslWork.Id,
            instructionWork.Id);

        Assert.NotEqual(
            qslWork.Id,
            taskWork.Id);

        Assert.NotEqual(
            instructionWork.Id,
            taskWork.Id);

        TestLog.State(
            $"Pulse = {pulse.Id}");

        TestLog.State(
            $"Required WorkUnits = {coordinator.WorkUnitCount}");

        TestLog.State(
            $"QSL WorkUnit = {qslWork.Id}");

        TestLog.State(
            $"Instruction WorkUnit = {instructionWork.Id}");

        TestLog.State(
            $"Task WorkUnit = {taskWork.Id}");

        TestLog.Pass(
            "One request decomposed into three distinct required WorkUnits.");

        TestLog.Pass(
            "Every WorkUnit retained the owning Pulse identity.");
    }

    [Fact]
    public void
        SuccessfulMultiWorkCompletionProducesOneAuthoritativeMoment()
    {
        TestLog.Section(
            "PULSE MULTI-WORK SUCCESS");

        TestLog.Test(
            "All required WorkUnits produce one authoritative completed Moment.");

        var request =
            new TestRuntimeObject(
                "successful-multi-work-request");

        var pulse =
            CreateProcessingPulse(
                request);

        var qslWork =
            CreateProcessingWork(
                pulse,
                "qsl-work");

        var instructionWork =
            CreateProcessingWork(
                pulse,
                "instruction-work");

        var taskWork =
            CreateProcessingWork(
                pulse,
                "task-work");

        var coordinator =
            new PulseCoordinator(
                pulse);

        Assert.True(
            coordinator.AddWorkUnit(
                qslWork));

        Assert.True(
            coordinator.AddWorkUnit(
                instructionWork));

        Assert.True(
            coordinator.AddWorkUnit(
                taskWork));

        var qslResult =
            new TestRuntimeObject(
                "qsl-result");

        var instructionResult =
            new TestRuntimeObject(
                "instruction-result");

        var taskResult =
            new TestRuntimeObject(
                "task-result");

        var finalResult =
            new TestRuntimeObject(
                "authoritative-final-result");

        Assert.True(
            qslWork.TryComplete(
                qslResult));

        Assert.True(
            instructionWork.TryComplete(
                instructionResult));

        Assert.False(
            coordinator.CanResolve);

        Assert.False(
            coordinator.CanFinalize);

        Assert.False(
            coordinator.TryFinalize(
                finalResult,
                out var prematureMoment));

        Assert.Null(
            prematureMoment);

        Assert.Equal(
            PulseStatus.Processing,
            pulse.Status);

        TestLog.Expected(
            "Two completed WorkUnits could not resolve a three-work Pulse.");

        Assert.True(
            taskWork.TryComplete(
                taskResult));

        Assert.True(
            coordinator.CanResolve);

        Assert.True(
            coordinator.CanFinalize);

        Assert.True(
            coordinator.TryFinalize(
                finalResult,
                out var moment));

        Assert.NotNull(
            moment);

        Assert.Equal(
            MomentOutcome.Completed,
            moment!.Outcome);

        Assert.Equal(
            PulseStatus.Completed,
            pulse.Status);

        Assert.True(
            pulse.IsTerminal);

        Assert.Equal(
            pulse.Id,
            moment.PulseId);

        Assert.Equal(
            3,
            moment.WorkUnitIds.Count);

        Assert.Contains(
            qslWork.Id,
            moment.WorkUnitIds);

        Assert.Contains(
            instructionWork.Id,
            moment.WorkUnitIds);

        Assert.Contains(
            taskWork.Id,
            moment.WorkUnitIds);

        Assert.Same(
            finalResult,
            moment.Result);

        Assert.Null(
            moment.Exception);

        Assert.Same(
            qslResult,
            qslWork.Result);

        Assert.Same(
            instructionResult,
            instructionWork.Result);

        Assert.Same(
            taskResult,
            taskWork.Result);

        Assert.False(
            ReferenceEquals(
                qslResult,
                moment.Result));

        Assert.False(
            ReferenceEquals(
                instructionResult,
                moment.Result));

        Assert.False(
            ReferenceEquals(
                taskResult,
                moment.Result));

        Assert.False(
            coordinator.TryFinalize(
                new TestRuntimeObject(
                    "must-not-create-second-moment"),
                out var duplicateMoment));

        Assert.Null(
            duplicateMoment);

        TestLog.State(
            $"Pulse outcome = {pulse.Status}");

        TestLog.State(
            $"Moment outcome = {moment.Outcome}");

        TestLog.State(
            $"Moment WorkUnits = {moment.WorkUnitIds.Count}");

        TestLog.Pass(
            "All required work produced one authoritative completed Moment.");

        TestLog.Pass(
            "No subordinate result leaked into the final Pulse result.");

        TestLog.Pass(
            "The Pulse could not produce a second Moment.");
    }

    [Fact]
    public void
        PartialCompletionFollowedByFailureProducesFailedMoment()
    {
        TestLog.Section(
            "PULSE MULTI-WORK PARTIAL FAILURE");

        TestLog.Test(
            "Partial success followed by failure produces an honest failed Moment.");

        var pulse =
            CreateProcessingPulse(
                new TestRuntimeObject(
                    "partial-failure-request"));

        var completed =
            CreateProcessingWork(
                pulse,
                "completed-work");

        var failed =
            CreateProcessingWork(
                pulse,
                "failed-work");

        var remaining =
            CreateProcessingWork(
                pulse,
                "remaining-work");

        var coordinator =
            new PulseCoordinator(
                pulse);

        Assert.True(
            coordinator.AddWorkUnit(
                completed));

        Assert.True(
            coordinator.AddWorkUnit(
                failed));

        Assert.True(
            coordinator.AddWorkUnit(
                remaining));

        var subordinateResult =
            new TestRuntimeObject(
                "subordinate-success-result");

        var forbiddenFinalResult =
            new TestRuntimeObject(
                "must-not-be-final-result");

        var failure =
            new ExpectedDecompositionException(
                "Expected multi-work failure.");

        Assert.True(
            completed.TryComplete(
                subordinateResult));

        Assert.True(
            failed.TryFail(
                failure));

        Assert.False(
            coordinator.CanResolve);

        Assert.False(
            coordinator.CanFinalize);

        Assert.False(
            coordinator.TryFinalize(
                forbiddenFinalResult,
                out var prematureMoment));

        Assert.Null(
            prematureMoment);

        Assert.Equal(
            PulseStatus.Processing,
            pulse.Status);

        TestLog.Expected(
            "Known failure could not finalize while required work remained unresolved.");

        Assert.True(
            remaining.TryComplete(
                new TestRuntimeObject(
                    "remaining-result")));

        Assert.False(
            coordinator.CanResolve);

        Assert.True(
            coordinator.CanFinalize);

        Assert.True(
            coordinator.TryFinalize(
                forbiddenFinalResult,
                out var moment));

        Assert.NotNull(
            moment);

        Assert.Equal(
            MomentOutcome.Failed,
            moment!.Outcome);

        Assert.Equal(
            PulseStatus.Failed,
            pulse.Status);

        Assert.True(
            pulse.IsTerminal);

        Assert.Null(
            moment.Result);

        Assert.Same(
            failure,
            moment.Exception);

        Assert.Equal(
            3,
            moment.WorkUnitIds.Count);

        Assert.Contains(
            completed.Id,
            moment.WorkUnitIds);

        Assert.Contains(
            failed.Id,
            moment.WorkUnitIds);

        Assert.Contains(
            remaining.Id,
            moment.WorkUnitIds);

        Assert.Same(
            subordinateResult,
            completed.Result);

        Assert.False(
            ReferenceEquals(
                subordinateResult,
                moment.Result));

        Assert.False(
            ReferenceEquals(
                forbiddenFinalResult,
                moment.Result));

        TestLog.State(
            $"Pulse outcome = {pulse.Status}");

        TestLog.State(
            $"Moment outcome = {moment.Outcome}");

        TestLog.State(
            $"Exception = {moment.Exception!.Message}");

        TestLog.Pass(
            "Partial success could not masquerade as Pulse success.");

        TestLog.Pass(
            "The exact WorkUnit failure became authoritative history.");
    }

    [Fact]
    public void
        PartialCompletionFollowedByCancellationProducesCancelledMoment()
    {
        TestLog.Section(
            "PULSE MULTI-WORK PARTIAL CANCELLATION");

        TestLog.Test(
            "Partial success followed by cancellation produces an honest cancelled Moment.");

        var pulse =
            CreateProcessingPulse(
                new TestRuntimeObject(
                    "partial-cancellation-request"));

        var completed =
            CreateProcessingWork(
                pulse,
                "completed-work");

        var cancelled =
            CreateProcessingWork(
                pulse,
                "cancelled-work");

        var remaining =
            CreateProcessingWork(
                pulse,
                "remaining-work");

        var coordinator =
            new PulseCoordinator(
                pulse);

        Assert.True(
            coordinator.AddWorkUnit(
                completed));

        Assert.True(
            coordinator.AddWorkUnit(
                cancelled));

        Assert.True(
            coordinator.AddWorkUnit(
                remaining));

        var subordinateResult =
            new TestRuntimeObject(
                "subordinate-success-result");

        var forbiddenFinalResult =
            new TestRuntimeObject(
                "must-not-be-final-result");

        Assert.True(
            completed.TryComplete(
                subordinateResult));

        Assert.True(
            cancelled.TryCancel());

        Assert.False(
            coordinator.CanResolve);

        Assert.False(
            coordinator.CanFinalize);

        Assert.False(
            coordinator.TryFinalize(
                forbiddenFinalResult,
                out var prematureMoment));

        Assert.Null(
            prematureMoment);

        Assert.Equal(
            PulseStatus.Processing,
            pulse.Status);

        TestLog.Expected(
            "Known cancellation could not finalize while required work remained unresolved.");

        Assert.True(
            remaining.TryComplete(
                new TestRuntimeObject(
                    "remaining-result")));

        Assert.False(
            coordinator.CanResolve);

        Assert.True(
            coordinator.CanFinalize);

        Assert.True(
            coordinator.TryFinalize(
                forbiddenFinalResult,
                out var moment));

        Assert.NotNull(
            moment);

        Assert.Equal(
            MomentOutcome.Cancelled,
            moment!.Outcome);

        Assert.Equal(
            PulseStatus.Cancelled,
            pulse.Status);

        Assert.True(
            pulse.IsTerminal);

        Assert.Null(
            moment.Result);

        Assert.Null(
            moment.Exception);

        Assert.Equal(
            3,
            moment.WorkUnitIds.Count);

        Assert.Contains(
            completed.Id,
            moment.WorkUnitIds);

        Assert.Contains(
            cancelled.Id,
            moment.WorkUnitIds);

        Assert.Contains(
            remaining.Id,
            moment.WorkUnitIds);

        Assert.Same(
            subordinateResult,
            completed.Result);

        Assert.False(
            ReferenceEquals(
                subordinateResult,
                moment.Result));

        Assert.False(
            ReferenceEquals(
                forbiddenFinalResult,
                moment.Result));

        TestLog.State(
            $"Pulse outcome = {pulse.Status}");

        TestLog.State(
            $"Moment outcome = {moment.Outcome}");

        TestLog.Pass(
            "Partial success could not override Pulse cancellation.");

        TestLog.Pass(
            "Cancellation became the authoritative terminal history.");
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
            Name =
                name;
        }

        public string Name { get; }
    }

    private sealed class ExpectedDecompositionException :
        Exception
    {
        public ExpectedDecompositionException(
            string message)
            : base(
                message)
        {
        }
    }
}