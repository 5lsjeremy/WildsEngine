using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Processing.Coordinator;
using WildsEngine.Core.Units;
using WildsEngine.Tests.Logging;
using Xunit;

namespace WildsEngine.Core.Tests;

public sealed class WorkUnitRaceCertificationTests
{
    private const int RaceIterations =
        256;

    [Fact]
    public async Task
        ConcurrentTerminalTransitionsProduceExactlyOneWinner()
    {
        TestLog.Section(
            "WORKUNIT TERMINAL-STATE RACE");

        TestLog.Test(
            "Concurrent complete, fail, and cancel attempts produce exactly one winner.");

        var completedWins =
            0;

        var failedWins =
            0;

        var cancelledWins =
            0;

        for (var iteration = 0;
             iteration < RaceIterations;
             iteration++)
        {
            var pulse =
                CreateProcessingPulse(
                    $"terminal-race-{iteration:D4}");

            var work =
                CreateProcessingWork(
                    pulse,
                    $"terminal-work-{iteration:D4}");

            var completionResult =
                new TestRuntimeObject(
                    $"completion-result-{iteration:D4}");

            var failure =
                new ExpectedRaceException(
                    $"Expected race failure {iteration:D4}.");

            using var start =
                new ManualResetEventSlim(
                    false);

            var completeAttempt =
                Task.Run(
                    () =>
                    {
                        start.Wait();

                        return work.TryComplete(
                            completionResult);
                    });

            var failAttempt =
                Task.Run(
                    () =>
                    {
                        start.Wait();

                        return work.TryFail(
                            failure);
                    });

            var cancelAttempt =
                Task.Run(
                    () =>
                    {
                        start.Wait();

                        return work.TryCancel();
                    });

            start.Set();

            var transitions =
                await Task.WhenAll(
                        completeAttempt,
                        failAttempt,
                        cancelAttempt)
                    .ConfigureAwait(false);

            Assert.Equal(
                1,
                transitions.Count(
                    transition =>
                        transition));

            Assert.True(
                work.IsTerminal);

            switch (work.Status)
            {
                case WorkUnitStatus.Completed:
                    completedWins++;

                    Assert.True(
                        transitions[0]);

                    Assert.Same(
                        completionResult,
                        work.Result);

                    Assert.Null(
                        work.Exception);

                    break;

                case WorkUnitStatus.Failed:
                    failedWins++;

                    Assert.True(
                        transitions[1]);

                    Assert.Null(
                        work.Result);

                    Assert.Same(
                        failure,
                        work.Exception);

                    break;

                case WorkUnitStatus.Cancelled:
                    cancelledWins++;

                    Assert.True(
                        transitions[2]);

                    Assert.Null(
                        work.Result);

                    Assert.Null(
                        work.Exception);

                    break;

                default:
                    throw new InvalidOperationException(
                        "The WorkUnit did not retain a valid " +
                        "terminal status after the race.");
            }

            Assert.False(
                work.TryComplete(
                    new TestRuntimeObject(
                        "must-not-replace-result")));

            Assert.False(
                work.TryFail(
                    new ExpectedRaceException(
                        "must-not-replace-failure")));

            Assert.False(
                work.TryCancel());
        }

        TestLog.State(
            $"Iterations = {RaceIterations}");

        TestLog.State(
            $"Completed winners = {completedWins}");

        TestLog.State(
            $"Failed winners = {failedWins}");

        TestLog.State(
            $"Cancelled winners = {cancelledWins}");

        TestLog.State(
            "Total winners = " +
            (completedWins +
             failedWins +
             cancelledWins));

        Assert.Equal(
            RaceIterations,
            completedWins +
            failedWins +
            cancelledWins);

        TestLog.Pass(
            "Every terminal-state race produced exactly one authoritative winner.");

        TestLog.Pass(
            "Winning WorkUnit state remained internally consistent.");

        TestLog.Pass(
            "All later terminal transitions were rejected.");
    }

    [Fact]
    public async Task
        ConcurrentTerminalWinnerProducesMatchingPulseMoment()
    {
        TestLog.Section(
            "WORKUNIT RACE - PULSE FINALIZATION");

        TestLog.Test(
            "Pulse finalization observes the authoritative WorkUnit race winner.");

        for (var iteration = 0;
             iteration < RaceIterations;
             iteration++)
        {
            var pulse =
                CreateProcessingPulse(
                    $"pulse-race-{iteration:D4}");

            var work =
                CreateProcessingWork(
                    pulse,
                    $"pulse-race-work-{iteration:D4}");

            var coordinator =
                new PulseCoordinator(
                    pulse);

            Assert.True(
                coordinator.AddWorkUnit(
                    work));

            var completionResult =
                new TestRuntimeObject(
                    $"pulse-result-{iteration:D4}");

            var failure =
                new ExpectedRaceException(
                    $"Expected Pulse race failure {iteration:D4}.");

            using var start =
                new ManualResetEventSlim(
                    false);

            var completeAttempt =
                Task.Run(
                    () =>
                    {
                        start.Wait();

                        return work.TryComplete(
                            completionResult);
                    });

            var failAttempt =
                Task.Run(
                    () =>
                    {
                        start.Wait();

                        return work.TryFail(
                            failure);
                    });

            var cancelAttempt =
                Task.Run(
                    () =>
                    {
                        start.Wait();

                        return work.TryCancel();
                    });

            start.Set();

            var transitions =
                await Task.WhenAll(
                        completeAttempt,
                        failAttempt,
                        cancelAttempt)
                    .ConfigureAwait(false);

            Assert.Equal(
                1,
                transitions.Count(
                    transition =>
                        transition));

            Assert.True(
                coordinator.CanFinalize);

            Assert.True(
                coordinator.TryFinalize(
                    completionResult,
                    out var moment));

            Assert.NotNull(
                moment);

            Assert.Equal(
                pulse.Id,
                moment!.PulseId);

            Assert.Single(
                moment.WorkUnitIds);

            Assert.Contains(
                work.Id,
                moment.WorkUnitIds);

            switch (work.Status)
            {
                case WorkUnitStatus.Completed:
                    Assert.Equal(
                        MomentOutcome.Completed,
                        moment.Outcome);

                    Assert.Equal(
                        PulseStatus.Completed,
                        pulse.Status);

                    Assert.Same(
                        completionResult,
                        moment.Result);

                    Assert.Null(
                        moment.Exception);

                    break;

                case WorkUnitStatus.Failed:
                    Assert.Equal(
                        MomentOutcome.Failed,
                        moment.Outcome);

                    Assert.Equal(
                        PulseStatus.Failed,
                        pulse.Status);

                    Assert.Null(
                        moment.Result);

                    Assert.Same(
                        failure,
                        moment.Exception);

                    break;

                case WorkUnitStatus.Cancelled:
                    Assert.Equal(
                        MomentOutcome.Cancelled,
                        moment.Outcome);

                    Assert.Equal(
                        PulseStatus.Cancelled,
                        pulse.Status);

                    Assert.Null(
                        moment.Result);

                    Assert.Null(
                        moment.Exception);

                    break;

                default:
                    throw new InvalidOperationException(
                        "Pulse finalization observed a WorkUnit " +
                        "without a valid terminal status.");
            }

            Assert.True(
                pulse.IsTerminal);

            Assert.False(
                coordinator.TryFinalize(
                    new object(),
                    out var duplicateMoment));

            Assert.Null(
                duplicateMoment);
        }

        TestLog.State(
            $"Finalized Pulse races = {RaceIterations}");

        TestLog.Pass(
            "Every Pulse finalized according to the authoritative WorkUnit winner.");

        TestLog.Pass(
            "No Pulse produced a second Moment after finalization.");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void
        MixedTerminalOutcomePrecedenceIsIndependentOfRegistrationOrder(
            bool reverseRegistrationOrder)
    {
        TestLog.Section(
            "PULSE MIXED-OUTCOME PRECEDENCE");

        TestLog.Test(
            "Failure precedence is independent of WorkUnit registration order.");

        var pulse =
            CreateProcessingPulse(
                reverseRegistrationOrder
                    ? "mixed-order-reversed"
                    : "mixed-order-forward");

        var completed =
            CreateProcessingWork(
                pulse,
                "completed-work");

        var failed =
            CreateProcessingWork(
                pulse,
                "failed-work");

        var cancelled =
            CreateProcessingWork(
                pulse,
                "cancelled-work");

        var completedResult =
            new TestRuntimeObject(
                "completed-result");

        var finalResult =
            new TestRuntimeObject(
                "must-not-be-used-as-final-result");

        var failure =
            new ExpectedRaceException(
                "Authoritative mixed-outcome failure.");

        Assert.True(
            completed.TryComplete(
                completedResult));

        Assert.True(
            failed.TryFail(
                failure));

        Assert.True(
            cancelled.TryCancel());

        var coordinator =
            new PulseCoordinator(
                pulse);

        if (reverseRegistrationOrder)
        {
            Assert.True(
                coordinator.AddWorkUnit(
                    cancelled));

            Assert.True(
                coordinator.AddWorkUnit(
                    failed));

            Assert.True(
                coordinator.AddWorkUnit(
                    completed));
        }
        else
        {
            Assert.True(
                coordinator.AddWorkUnit(
                    completed));

            Assert.True(
                coordinator.AddWorkUnit(
                    failed));

            Assert.True(
                coordinator.AddWorkUnit(
                    cancelled));
        }

        Assert.Equal(
            3,
            coordinator.WorkUnitCount);

        Assert.False(
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
            MomentOutcome.Failed,
            moment!.Outcome);

        Assert.Equal(
            PulseStatus.Failed,
            pulse.Status);

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
            cancelled.Id,
            moment.WorkUnitIds);

        TestLog.State(
            reverseRegistrationOrder
                ? "Registration order = Cancelled, Failed, Completed"
                : "Registration order = Completed, Failed, Cancelled");

        TestLog.State(
            $"Moment outcome = {moment.Outcome}");

        TestLog.State(
            $"Pulse status = {pulse.Status}");

        TestLog.Pass(
            "Failure remained authoritative regardless of registration order.");
    }

    [Fact]
public async Task
    RepeatedConcurrentTerminalTransitionsRemainRejected()
{
    const int contenderCount =
        64;

    TestLog.Section(
        "WORKUNIT TERMINAL REENTRY");

    TestLog.Test(
        "Repeated concurrent terminal attempts cannot replace an established outcome.");

    var pulse =
        CreateProcessingPulse(
            "terminal-reentry");

    var work =
        CreateProcessingWork(
            pulse,
            "terminal-reentry-work");

    var authoritativeResult =
        new TestRuntimeObject(
            "authoritative-result");

    Assert.True(
        work.TryComplete(
            authoritativeResult));

    Assert.Equal(
        WorkUnitStatus.Completed,
        work.Status);

    Assert.Same(
        authoritativeResult,
        work.Result);

    using var start =
        new ManualResetEventSlim(
            false);

    var attempts =
        new Task<bool>[contenderCount];

    for (var index = 0;
         index < contenderCount;
         index++)
    {
        var contenderIndex =
            index;

        attempts[index] =
            Task.Run(
                () =>
                {
                    start.Wait();

                    switch (contenderIndex % 3)
                    {
                        case 0:
                            return work.TryComplete(
                                new TestRuntimeObject(
                                    $"replacement-{contenderIndex:D3}"));

                        case 1:
                            return work.TryFail(
                                new ExpectedRaceException(
                                    $"replacement-{contenderIndex:D3}"));

                        default:
                            return work.TryCancel();
                    }
                });
    }

    start.Set();

    bool[] results =
        await Task.WhenAll(
                attempts)
            .ConfigureAwait(false);

    Assert.Equal(
        contenderCount,
        results.Length);

    for (var index = 0;
         index < results.Length;
         index++)
    {
        Assert.False(
            results[index]);
    }

    Assert.Equal(
        WorkUnitStatus.Completed,
        work.Status);

    Assert.True(
        work.IsTerminal);

    Assert.Same(
        authoritativeResult,
        work.Result);

    Assert.Null(
        work.Exception);

    TestLog.State(
        $"Rejected terminal reentry attempts = {results.Length}");

    TestLog.Pass(
        "Concurrent terminal reentry could not replace the established result.");
}

[Theory]
[InlineData(0, 1, 2)]
[InlineData(0, 2, 1)]
[InlineData(1, 0, 2)]
[InlineData(1, 2, 0)]
[InlineData(2, 0, 1)]
[InlineData(2, 1, 0)]
public void
    MixedWorkUnitOutcomePrecedenceIsIndependentOfCompletionOrder(
        int first,
        int second,
        int third)
{
    TestLog.Section(
        "PULSE MIXED-OUTCOME COMPLETION ORDER");

    TestLog.Test(
        "Failure precedence is independent of WorkUnit completion order.");

    var pulse =
        CreateProcessingPulse(
            $"completion-order-{first}-{second}-{third}");

    var completed =
        CreateProcessingWork(
            pulse,
            "completed-work");

    var failed =
        CreateProcessingWork(
            pulse,
            "failed-work");

    var cancelled =
        CreateProcessingWork(
            pulse,
            "cancelled-work");

    var completedResult =
        new TestRuntimeObject(
            "completed-result");

    var finalResult =
        new TestRuntimeObject(
            "must-not-be-used-as-final-result");

    var failure =
        new ExpectedRaceException(
            "Authoritative completion-order failure.");

    var coordinator =
        new PulseCoordinator(
            pulse);

    /*
     * Registration order remains fixed.
     *
     * Only terminal completion order changes.
     *
     * This isolates the contract being certified:
     *
     *     completion order must not affect
     *     mixed-outcome precedence.
     */
    Assert.True(
        coordinator.AddWorkUnit(
            completed));

    Assert.True(
        coordinator.AddWorkUnit(
            failed));

    Assert.True(
        coordinator.AddWorkUnit(
            cancelled));

    Assert.Equal(
        3,
        coordinator.WorkUnitCount);

    var order =
        new[]
        {
            first,
            second,
            third
        };

    for (var index = 0;
         index < order.Length;
         index++)
    {
        switch (order[index])
        {
            case 0:
                Assert.True(
                    completed.TryComplete(
                        completedResult));

                TestLog.State(
                    $"Terminal position {index + 1}: Completed");

                break;

            case 1:
                Assert.True(
                    failed.TryFail(
                        failure));

                TestLog.State(
                    $"Terminal position {index + 1}: Failed");

                break;

            case 2:
                Assert.True(
                    cancelled.TryCancel());

                TestLog.State(
                    $"Terminal position {index + 1}: Cancelled");

                break;

            default:
                throw new InvalidOperationException(
                    "Unknown completion-order index.");
        }

        /*
         * Until every required WorkUnit is terminal,
         * the Pulse must not finalize.
         */
        if (index < order.Length - 1)
        {
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
        }
    }

    Assert.Equal(
        WorkUnitStatus.Completed,
        completed.Status);

    Assert.Equal(
        WorkUnitStatus.Failed,
        failed.Status);

    Assert.Equal(
        WorkUnitStatus.Cancelled,
        cancelled.Status);

    Assert.True(
        coordinator.CanFinalize);

    Assert.False(
        coordinator.CanResolve);

    Assert.True(
        coordinator.TryFinalize(
            finalResult,
            out var moment));

    Assert.NotNull(
        moment);

    /*
     * Failure must remain authoritative regardless
     * of which WorkUnit reached its terminal state
     * first, second, or third.
     */
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
        pulse.Id,
        moment.PulseId);

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
        cancelled.Id,
        moment.WorkUnitIds);

    /*
     * Verify subordinate successful work has not
     * contaminated the authoritative failed Moment.
     */
    Assert.Same(
        completedResult,
        completed.Result);

    Assert.Null(
        completed.Exception);

    Assert.Null(
        failed.Result);

    Assert.Same(
        failure,
        failed.Exception);

    Assert.Null(
        cancelled.Result);

    Assert.Null(
        cancelled.Exception);

    /*
     * Finalization must remain single-shot.
     */
    Assert.False(
        coordinator.TryFinalize(
            new object(),
            out var duplicateMoment));

    Assert.Null(
        duplicateMoment);

    TestLog.State(
        $"Completion order = " +
        $"{DescribeOutcome(first)} -> " +
        $"{DescribeOutcome(second)} -> " +
        $"{DescribeOutcome(third)}");

    TestLog.State(
        $"Moment outcome = {moment.Outcome}");

    TestLog.State(
        $"Pulse status = {pulse.Status}");

    TestLog.Pass(
        "Failure remained authoritative regardless of completion order.");
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
    
    private static string DescribeOutcome(
        int outcome)
    {
        return outcome switch
        {
            0 => "Completed",
            1 => "Failed",
            2 => "Cancelled",

            _ =>
                throw new ArgumentOutOfRangeException(
                    nameof(outcome))
        };
    }

    private sealed class ExpectedRaceException :
        Exception
    {
        public ExpectedRaceException(
            string message)
            : base(
                message)
        {
        }
    }
}