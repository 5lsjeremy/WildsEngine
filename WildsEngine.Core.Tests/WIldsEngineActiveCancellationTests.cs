using System.Threading;
using System.Threading.Tasks;
using WildsEngine.Core.Abstractions;
using WildsEngine.Tests.Logging;
using WildsEngine.Tests.Registry;
using Xunit;

namespace WildsEngine.Core.Tests;

public sealed class WildsEngineActiveCancellationTests
{
    [Fact]
    public async Task
        ActiveExecuteAsyncCancellationProducesCancelledMoment()
    {
        TestLog.Section(
            "WILDSENGINE ACTIVE EXECUTION CANCELLATION");

        TestLog.Test(
            "Cancellation during active ExecuteAsync work produces a cancelled Moment.");

        var component =
            CreateComponent(
                "execute.active-cancellation");

        using var engine =
            CreateEngine(
                component);

        Assert.True(
            engine.Initialize());

        using var cancellation =
            new CancellationTokenSource();

        var enteredOperation =
            new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

        var observedCancellation =
            new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

        var request =
            new TestRuntimeObject(
                "active-cancellation-request");

        var forbiddenResult =
            new TestRuntimeObject(
                "must-not-be-recorded");

        TestLog.Task(
            "Starting ExecuteAsync with cancellable active work.");

        var execution =
            engine.ExecuteAsync(
                    request,
                    async (
                        observedRequest,
                        cancellationToken) =>
                    {
                        Assert.Same(
                            request,
                            observedRequest);

                        Assert.False(
                            cancellationToken.IsCancellationRequested);

                        enteredOperation.TrySetResult(
                            true);

                        TestLog.State(
                            "ExecuteAsync operation entered active work.");

                        try
                        {
                            await Task.Delay(
                                    Timeout.Infinite,
                                    cancellationToken)
                                .ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            observedCancellation.TrySetResult(
                                true);

                            TestLog.State(
                                "Active operation observed cancellation.");

                            throw;
                        }

                        return forbiddenResult;
                    },
                    cancellation.Token)
                .AsTask();

        await enteredOperation.Task
            .ConfigureAwait(false);

        Assert.False(
            execution.IsCompleted);

        TestLog.Task(
            "Cancelling the request after active execution has begun.");

        cancellation.Cancel();

        await observedCancellation.Task
            .ConfigureAwait(false);

        var moment =
            await execution
                .ConfigureAwait(false);

        TestLog.Task(
            "Verifying cancellation became honest temporal history.");

        Assert.Equal(
            MomentOutcome.Cancelled,
            moment.Outcome);

        Assert.Null(
            moment.Result);

        Assert.Null(
            moment.Exception);

        Assert.Single(
            moment.WorkUnitIds);

        Assert.NotEqual(
            System.Guid.Empty,
            moment.WorkUnitIds[0]);

        Assert.NotEqual(
            System.Guid.Empty,
            moment.PulseId);

        Assert.Equal(
            1,
            engine.Timeline.Count);

        Assert.Single(
            engine.Timeline.Moments);

        Assert.Same(
            moment,
            engine.Timeline.Moments[0]);

        Assert.Equal(
            1,
            engine.Epoch.Count);

        Assert.Single(
            engine.Epoch.Timelines);

        Assert.Same(
            engine.Timeline,
            engine.Epoch.Timelines[0]);

        Assert.Single(
            engine.Epoch
                .Timelines[0]
                .Moments);

        Assert.Same(
            moment,
            engine.Epoch
                .Timelines[0]
                .Moments[0]);

        Assert.DoesNotContain(
            engine.Timeline.Moments,
            recordedMoment =>
                ReferenceEquals(
                    forbiddenResult,
                    recordedMoment.Result));

        TestLog.State(
            $"Moment outcome = {moment.Outcome}");

        TestLog.State(
            $"Timeline Moments = {engine.Timeline.Count}");

        TestLog.State(
            $"Epoch Timelines = {engine.Epoch.Count}");

        TestLog.Pass(
            "Active cancellation produced one cancelled Moment.");

        TestLog.Pass(
            "Cancellation did not record a successful result.");
    }

    [Fact]
    public async Task
        EngineRecoversAfterActiveExecuteAsyncCancellation()
    {
        TestLog.Section(
            "WILDSENGINE ACTIVE CANCELLATION RECOVERY");

        TestLog.Test(
            "Engine accepts successful work after an actively cancelled request.");

        var component =
            CreateComponent(
                "execute.active-cancellation-recovery");

        using var engine =
            CreateEngine(
                component);

        Assert.True(
            engine.Initialize());

        using var cancellation =
            new CancellationTokenSource();

        var enteredOperation =
            new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

        var cancelledRequest =
            new TestRuntimeObject(
                "cancelled-request");

        var cancelledExecution =
            engine.ExecuteAsync(
                    cancelledRequest,
                    async (
                        observedRequest,
                        cancellationToken) =>
                    {
                        Assert.Same(
                            cancelledRequest,
                            observedRequest);

                        enteredOperation.TrySetResult(
                            true);

                        await Task.Delay(
                                Timeout.Infinite,
                                cancellationToken)
                            .ConfigureAwait(false);

                        return new TestRuntimeObject(
                            "must-not-complete");
                    },
                    cancellation.Token)
                .AsTask();

        await enteredOperation.Task
            .ConfigureAwait(false);

        TestLog.Task(
            "Cancelling active execution.");

        cancellation.Cancel();

        var cancelledMoment =
            await cancelledExecution
                .ConfigureAwait(false);

        Assert.Equal(
            MomentOutcome.Cancelled,
            cancelledMoment.Outcome);

        Assert.Null(
            cancelledMoment.Result);

        Assert.Null(
            cancelledMoment.Exception);

        Assert.Single(
            cancelledMoment.WorkUnitIds);

        Assert.Equal(
            1,
            engine.Timeline.Count);

        var recoveryRequest =
            new TestRuntimeObject(
                "recovery-request");

        var recoveryResult =
            new TestRuntimeObject(
                "recovery-result");

        TestLog.Task(
            "Submitting recovery work through the same engine.");

        var completedMoment =
            await engine.ExecuteAsync(
                    recoveryRequest,
                    (
                        observedRequest,
                        cancellationToken) =>
                    {
                        cancellationToken
                            .ThrowIfCancellationRequested();

                        Assert.Same(
                            recoveryRequest,
                            observedRequest);

                        return new ValueTask<object?>(
                            recoveryResult);
                    })
                .ConfigureAwait(false);

        Assert.Equal(
            MomentOutcome.Completed,
            completedMoment.Outcome);

        Assert.Same(
            recoveryResult,
            completedMoment.Result);

        Assert.Null(
            completedMoment.Exception);

        Assert.Single(
            completedMoment.WorkUnitIds);

        Assert.NotEqual(
            cancelledMoment.PulseId,
            completedMoment.PulseId);

        Assert.NotEqual(
            cancelledMoment.WorkUnitIds[0],
            completedMoment.WorkUnitIds[0]);

        Assert.Equal(
            2,
            engine.Timeline.Count);

        Assert.Same(
            cancelledMoment,
            engine.Timeline.Moments[0]);

        Assert.Same(
            completedMoment,
            engine.Timeline.Moments[1]);

        Assert.Equal(
            MomentOutcome.Cancelled,
            engine.Timeline.Moments[0].Outcome);

        Assert.Equal(
            MomentOutcome.Completed,
            engine.Timeline.Moments[1].Outcome);

        Assert.Equal(
            1,
            engine.Epoch.Count);

        Assert.Same(
            engine.Timeline,
            engine.Epoch.Timelines[0]);

        Assert.Equal(
            2,
            engine.Epoch
                .Timelines[0]
                .Moments
                .Count);

        TestLog.State(
            "Timeline outcomes = " +
            $"{engine.Timeline.Moments[0].Outcome} -> " +
            $"{engine.Timeline.Moments[1].Outcome}");

        TestLog.Pass(
            "The engine recovered after active cancellation.");

        TestLog.Pass(
            "Cancelled and completed requests remained distinct in history.");
    }

    [Fact]
    public async Task
        CancellationAfterOperationResultPreventsFalseSuccess()
    {
        TestLog.Section(
            "WILDSENGINE POST-OPERATION CANCELLATION");

        TestLog.Test(
            "Cancellation after operation work but before completion prevents false success.");

        var component =
            CreateComponent(
                "execute.post-operation-cancellation");

        using var engine =
            CreateEngine(
                component);

        Assert.True(
            engine.Initialize());

        using var cancellation =
            new CancellationTokenSource();

        var operationEntered =
            new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

        var allowOperationReturn =
            new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

        var request =
            new TestRuntimeObject(
                "post-operation-cancellation-request");

        var forbiddenResult =
            new TestRuntimeObject(
                "completed-operation-result");

        var execution =
            engine.ExecuteAsync(
                    request,
                    async (
                        observedRequest,
                        cancellationToken) =>
                    {
                        Assert.Same(
                            request,
                            observedRequest);

                        operationEntered.TrySetResult(
                            true);

                        await allowOperationReturn.Task
                            .ConfigureAwait(false);

                        /*
                         * Deliberately return a value without checking
                         * the token here.
                         *
                         * WildsEngine.ExecuteAsync must perform its own
                         * post-operation cancellation check before it
                         * completes the WorkUnit.
                         */
                        return forbiddenResult;
                    },
                    cancellation.Token)
                .AsTask();

        await operationEntered.Task
            .ConfigureAwait(false);

        TestLog.Task(
            "Cancelling after the operation entered but before it returned.");

        cancellation.Cancel();

        allowOperationReturn.TrySetResult(
            true);

        var moment =
            await execution
                .ConfigureAwait(false);

        Assert.Equal(
            MomentOutcome.Cancelled,
            moment.Outcome);

        Assert.Null(
            moment.Result);

        Assert.Null(
            moment.Exception);

        Assert.Single(
            moment.WorkUnitIds);

        Assert.Single(
            engine.Timeline.Moments);

        Assert.Same(
            moment,
            engine.Timeline.Moments[0]);

        Assert.False(
            ReferenceEquals(
                forbiddenResult,
                moment.Result));

        TestLog.State(
            $"Moment outcome = {moment.Outcome}");

        TestLog.Pass(
            "Post-operation cancellation prevented false successful completion.");
    }
    
    [Fact]
public async Task
    ShutdownDuringActiveExecutionAllowsHonestCompletionAndRejectsNewWork()
{
    TestLog.Section(
        "WILDSENGINE SHUTDOWN DURING ACTIVE EXECUTION");

    TestLog.Test(
        "Shutdown rejects new work while allowing active work to finish honestly.");

    var component =
        CreateComponent(
            "execute.active-shutdown");

    using var engine =
        CreateEngine(
            component);

    Assert.True(
        engine.Initialize());

    var enteredOperation =
        new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

    var allowCompletion =
        new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

    var request =
        new TestRuntimeObject(
            "active-shutdown-request");

    var result =
        new TestRuntimeObject(
            "active-shutdown-result");

    TestLog.Task(
        "Starting work that remains active until explicitly released.");

    var activeExecution =
        engine.ExecuteAsync(
                request,
                async (
                    observedRequest,
                    cancellationToken) =>
                {
                    cancellationToken
                        .ThrowIfCancellationRequested();

                    Assert.Same(
                        request,
                        observedRequest);

                    enteredOperation.TrySetResult(
                        true);

                    TestLog.State(
                        "ExecuteAsync operation entered active work.");

                    await allowCompletion.Task
                        .ConfigureAwait(false);

                    cancellationToken
                        .ThrowIfCancellationRequested();

                    TestLog.State(
                        "Active operation was released after shutdown.");

                    return result;
                })
            .AsTask();

    await enteredOperation.Task
        .ConfigureAwait(false);

    Assert.False(
        activeExecution.IsCompleted);

    Assert.True(
        engine.IsInitialized);

    Assert.Equal(
        0,
        engine.Timeline.Count);

    TestLog.Task(
        "Shutting down the engine while the request remains active.");

    var shutdown =
        engine.Shutdown();

    Assert.True(
        shutdown);

    Assert.False(
        engine.IsInitialized);

    Assert.False(
        activeExecution.IsCompleted);

    Assert.Equal(
        0,
        engine.Timeline.Count);

    TestLog.Pass(
        "Shutdown did not silently discard or prematurely complete active work.");

    TestLog.Task(
        "Verifying shutdown rejects new work.");

    await Assert.ThrowsAsync<InvalidOperationException>(
        async () =>
        {
            await engine.ExecuteAsync(
                    new TestRuntimeObject(
                        "must-not-start"),
                    (
                        _,
                        _) =>
                        new ValueTask<object?>(
                            new TestRuntimeObject(
                                "must-not-complete")))
                .ConfigureAwait(false);
        });

    Assert.Equal(
        0,
        engine.Timeline.Count);

    TestLog.Expected(
        "New ExecuteAsync work was rejected after shutdown.");

    TestLog.Task(
        "Allowing the already-active request to complete.");

    allowCompletion.TrySetResult(
        true);

    var moment =
        await activeExecution
            .ConfigureAwait(false);

    TestLog.Task(
        "Verifying active work produced honest temporal history.");

    Assert.Equal(
        MomentOutcome.Completed,
        moment.Outcome);

    Assert.Same(
        result,
        moment.Result);

    Assert.Null(
        moment.Exception);

    Assert.Single(
        moment.WorkUnitIds);

    Assert.NotEqual(
        System.Guid.Empty,
        moment.WorkUnitIds[0]);

    Assert.NotEqual(
        System.Guid.Empty,
        moment.PulseId);

    Assert.False(
        engine.IsInitialized);

    Assert.Equal(
        1,
        engine.Timeline.Count);

    Assert.Single(
        engine.Timeline.Moments);

    Assert.Same(
        moment,
        engine.Timeline.Moments[0]);

    Assert.Same(
        result,
        engine.Timeline
            .Moments[0]
            .Result);

    Assert.Equal(
        1,
        engine.Epoch.Count);

    Assert.Single(
        engine.Epoch.Timelines);

    Assert.Same(
        engine.Timeline,
        engine.Epoch.Timelines[0]);

    Assert.Single(
        engine.Epoch
            .Timelines[0]
            .Moments);

    Assert.Same(
        moment,
        engine.Epoch
            .Timelines[0]
            .Moments[0]);

    TestLog.State(
        $"Engine initialized = {engine.IsInitialized}");

    TestLog.State(
        $"Moment outcome = {moment.Outcome}");

    TestLog.State(
        $"Timeline Moments = {engine.Timeline.Count}");

    TestLog.State(
        $"Epoch Timelines = {engine.Epoch.Count}");

    TestLog.Pass(
        "Active work completed honestly after shutdown.");

    TestLog.Pass(
        "The completed Moment was retained in Timeline and Epoch.");

    TestLog.Pass(
        "Shutdown-during-active-work certification completed successfully.");
}

[Fact]
public async Task
    DisposeDuringActiveExecutionAllowsHonestCompletionAndRejectsFurtherUse()
{
    TestLog.Section(
        "WILDSENGINE DISPOSAL DURING ACTIVE EXECUTION");

    TestLog.Test(
        "Dispose rejects further use while allowing accepted active work to finish honestly.");

    var component =
        CreateComponent(
            "execute.active-disposal");

    var engine =
        CreateEngine(
            component);

    Assert.True(
        engine.Initialize());

    var enteredOperation =
        new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

    var allowCompletion =
        new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

    var request =
        new TestRuntimeObject(
            "active-disposal-request");

    var result =
        new TestRuntimeObject(
            "active-disposal-result");

    TestLog.Task(
        "Starting work that remains active until explicitly released.");

    var activeExecution =
        engine.ExecuteAsync(
                request,
                async (
                    observedRequest,
                    cancellationToken) =>
                {
                    cancellationToken
                        .ThrowIfCancellationRequested();

                    Assert.Same(
                        request,
                        observedRequest);

                    enteredOperation.TrySetResult(
                        true);

                    TestLog.State(
                        "ExecuteAsync operation entered active work.");

                    await allowCompletion.Task
                        .ConfigureAwait(false);

                    cancellationToken
                        .ThrowIfCancellationRequested();

                    TestLog.State(
                        "Active operation was released after disposal.");

                    return result;
                })
            .AsTask();

    await enteredOperation.Task
        .ConfigureAwait(false);

    Assert.False(
        activeExecution.IsCompleted);

    Assert.True(
        engine.IsInitialized);

    Assert.Equal(
        0,
        engine.Timeline.Count);

    TestLog.Task(
        "Disposing the engine while the accepted request remains active.");

    engine.Dispose();

    Assert.False(
        engine.IsInitialized);

    Assert.False(
        activeExecution.IsCompleted);

    Assert.Equal(
        0,
        engine.Timeline.Count);

    TestLog.Pass(
        "Dispose did not silently discard or prematurely complete accepted active work.");

    TestLog.Task(
        "Verifying disposal permanently rejects new ExecuteAsync work.");

    await Assert.ThrowsAsync<ObjectDisposedException>(
        async () =>
        {
            await engine.ExecuteAsync(
                    new TestRuntimeObject(
                        "must-not-start"),
                    (
                        _,
                        _) =>
                        new ValueTask<object?>(
                            new TestRuntimeObject(
                                "must-not-complete")))
                .ConfigureAwait(false);
        });

    Assert.Equal(
        0,
        engine.Timeline.Count);

    TestLog.Expected(
        "New ExecuteAsync work was rejected after disposal.");

    TestLog.Task(
        "Verifying disposal rejects reinitialization.");

    Assert.Throws<ObjectDisposedException>(
        () =>
            engine.Initialize());

    Assert.False(
        engine.IsInitialized);

    TestLog.Expected(
        "Disposed engine rejected reinitialization.");

    TestLog.Task(
        "Allowing the already-accepted request to complete.");

    allowCompletion.TrySetResult(
        true);

    var moment =
        await activeExecution
            .ConfigureAwait(false);

    TestLog.Task(
        "Verifying accepted active work produced honest temporal history.");

    Assert.Equal(
        MomentOutcome.Completed,
        moment.Outcome);

    Assert.Same(
        result,
        moment.Result);

    Assert.Null(
        moment.Exception);

    Assert.Single(
        moment.WorkUnitIds);

    Assert.NotEqual(
        System.Guid.Empty,
        moment.WorkUnitIds[0]);

    Assert.NotEqual(
        System.Guid.Empty,
        moment.PulseId);

    Assert.False(
        engine.IsInitialized);

    Assert.Equal(
        1,
        engine.Timeline.Count);

    Assert.Single(
        engine.Timeline.Moments);

    Assert.Same(
        moment,
        engine.Timeline.Moments[0]);

    Assert.Same(
        result,
        engine.Timeline
            .Moments[0]
            .Result);

    Assert.Equal(
        1,
        engine.Epoch.Count);

    Assert.Single(
        engine.Epoch.Timelines);

    Assert.Same(
        engine.Timeline,
        engine.Epoch.Timelines[0]);

    Assert.Single(
        engine.Epoch
            .Timelines[0]
            .Moments);

    Assert.Same(
        moment,
        engine.Epoch
            .Timelines[0]
            .Moments[0]);

    TestLog.State(
        $"Engine initialized = {engine.IsInitialized}");

    TestLog.State(
        $"Moment outcome = {moment.Outcome}");

    TestLog.State(
        $"Timeline Moments = {engine.Timeline.Count}");

    TestLog.State(
        $"Epoch Timelines = {engine.Epoch.Count}");

    TestLog.Pass(
        "Accepted active work completed honestly after disposal.");

    TestLog.Pass(
        "The completed Moment remained available in Timeline and Epoch.");

    TestLog.Pass(
        "The disposed engine permanently rejected further use.");

    TestLog.Pass(
        "Disposal-during-active-work certification completed successfully.");
}

    private static Core.WildsEngine CreateEngine(
        IComponent component)
    {
        return new Core.WildsEngine(
            component,
            new WildsEngineOptions
            {
                Layer =
                    "certification.execute.active-cancellation",

                RetainedInstructionsProcessors =
                    1,

                ConcurrentInstructionsProcessors =
                    1,

                RetainedTaskProcessors =
                    1,

                ConcurrentTaskProcessors =
                    1
            });
    }

    private static IComponent CreateComponent(
        string layer)
    {
        return new TestComponent(
            layer);
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
}