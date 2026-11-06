using System;
using System.Threading;
using System.Threading.Tasks;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Neighbors.Operators;
using WildsEngine.Core.Processing;
using WildsEngine.Core.Tests.Fixtures;
using WildsEngine.Tests.Logging;
using WildsEngine.Tests.Registry;
using Xunit;

namespace WildsEngine.Core.Tests
{
    /// <summary>
    /// Certifies that WildsEngine.ExecuteAsync owns the
    /// complete request lifecycle:
    ///
    /// request
    ///     -> Pulse
    ///     -> WorkUnit
    ///     -> coordinator
    ///     -> Moment
    ///     -> Timeline
    ///     -> Epoch
    /// </summary>
    public sealed class WildsEngineExecuteIntegrationTests
    {
        [Fact]
        public async Task ExecuteAsyncRecordsSuccessfulMoment()
        {
            TestLog.Section(
                "WildsEngine ExecuteAsync Integration");

            TestLog.Test(
                "Successful request becomes completed Moment");

            var component =
                CreateComponent(
                    "execute.success");

            using var engine =
                CreateEngine(
                    component);

            Assert.True(
                engine.Initialize());

            var request =
                new TestRuntimeObject(
                    "request");

            var result =
                new TestRuntimeObject(
                    "result");

            var moment =
                await engine.ExecuteAsync(
                        request,
                        (
                            observedRequest,
                            cancellationToken) =>
                        {
                            cancellationToken
                                .ThrowIfCancellationRequested();

                            Assert.Same(
                                request,
                                observedRequest);

                            return new ValueTask<object?>(
                                result);
                        });

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

            Assert.Equal(
                1,
                engine.Timeline.Count);

            Assert.Same(
                moment,
                engine.Timeline.Moments[0]);

            Assert.Equal(
                1,
                engine.Epoch.Count);

            Assert.Same(
                engine.Timeline,
                engine.Epoch.Timelines[0]);

            TestLog.State(
                $"Moment recorded | " +
                $"Outcome={moment.Outcome} | " +
                $"TimelineMoments={engine.Timeline.Count}");

            TestLog.Pass(
                "ExecuteAsync completed and recorded request history");
        }

        [Fact]
        public async Task ExecuteAsyncPreservesArbitraryRuntimeIdentity()
        {
            TestLog.Test(
                "ExecuteAsync preserves arbitrary runtime identity");

            var component =
                CreateComponent(
                    "execute.identity");

            using var engine =
                CreateEngine(
                    component);

            Assert.True(
                engine.Initialize());

            var runtimeObject =
                new TestRuntimeObject(
                    "consumer-owned-object");

            var moment =
                await engine.ExecuteAsync(
                        runtimeObject,
                        (
                            request,
                            cancellationToken) =>
                        {
                            cancellationToken
                                .ThrowIfCancellationRequested();

                            return new ValueTask<object?>(
                                request);
                        });

            Assert.Equal(
                MomentOutcome.Completed,
                moment.Outcome);

            Assert.Same(
                runtimeObject,
                moment.Result);

            Assert.Same(
                runtimeObject,
                engine.Timeline
                    .Moments[0]
                    .Result);

            TestLog.Pass(
                "Exact consumer runtime object survived engine execution");
        }

        [Fact]
        public async Task ExecuteAsyncRecordsFailedMoment()
        {
            TestLog.Test(
                "Execution failure becomes failed Moment");

            var component =
                CreateComponent(
                    "execute.failure");

            using var engine =
                CreateEngine(
                    component);

            Assert.True(
                engine.Initialize());

            var exception =
                new ExpectedExecutionException(
                    "Expected integration failure.");

            var moment =
                await engine.ExecuteAsync(
                        "failure-request",
                        (
                            _,
                            _) =>
                        {
                            throw exception;
                        });

            Assert.Equal(
                MomentOutcome.Failed,
                moment.Outcome);

            Assert.Null(
                moment.Result);

            Assert.Same(
                exception,
                moment.Exception);

            Assert.Single(
                moment.WorkUnitIds);

            Assert.Single(
                engine.Timeline.Moments);

            Assert.Same(
                moment,
                engine.Timeline.Moments[0]);

            TestLog.Expected(
                exception.Message);

            TestLog.Pass(
                "Execution failure was retained as history");
        }

        [Fact]
        public async Task ExecuteAsyncRecordsCancelledMoment()
        {
            TestLog.Test(
                "Execution cancellation becomes cancelled Moment");

            var component =
                CreateComponent(
                    "execute.cancellation");

            using var engine =
                CreateEngine(
                    component);

            Assert.True(
                engine.Initialize());

            using var cancellation =
                new CancellationTokenSource();

            cancellation.Cancel();

            var operationWasCalled =
                false;

            var moment =
                await engine.ExecuteAsync(
                        "cancellation-request",
                        (
                            _,
                            _) =>
                        {
                            operationWasCalled = true;

                            return new ValueTask<object?>(
                                "must-not-complete");
                        },
                        cancellation.Token);

            Assert.False(
                operationWasCalled);

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

            TestLog.Pass(
                "Cancellation was retained as history");
        }

        [Fact]
        public async Task ExecuteAsyncRecordsMomentsInRequestOrder()
        {
            TestLog.Test(
                "Successive requests preserve Timeline order");

            var component =
                CreateComponent(
                    "execute.timeline-order");

            using var engine =
                CreateEngine(
                    component);

            Assert.True(
                engine.Initialize());

            var first =
                await engine.ExecuteAsync(
                        "first",
                        (
                            _,
                            _) =>
                            new ValueTask<object?>(
                                "first-result"));

            var second =
                await engine.ExecuteAsync(
                        "second",
                        (
                            _,
                            _) =>
                            new ValueTask<object?>(
                                "second-result"));

            Assert.Equal(
                2,
                engine.Timeline.Count);

            Assert.Same(
                first,
                engine.Timeline.Moments[0]);

            Assert.Same(
                second,
                engine.Timeline.Moments[1]);

            Assert.Equal(
                "first-result",
                first.Result);

            Assert.Equal(
                "second-result",
                second.Result);

            Assert.NotEqual(
                first.PulseId,
                second.PulseId);

            TestLog.State(
                "Timeline retained two Moments in request order");

            TestLog.Pass(
                "Request ordering was preserved");
        }

        [Fact]
        public async Task ExecuteAsyncRejectsUseBeforeInitialization()
        {
            TestLog.Test(
                "ExecuteAsync rejects use before initialization");

            var component =
                CreateComponent(
                    "execute.uninitialized");

            using var engine =
                CreateEngine(
                    component);

            await Assert.ThrowsAsync<InvalidOperationException>(
                async () =>
                {
                    await engine.ExecuteAsync(
                            "request",
                            (
                                _,
                                _) =>
                                new ValueTask<object?>(
                                    "result"));
                });

            Assert.Equal(
                0,
                engine.Timeline.Count);

            TestLog.Expected(
                "Uninitialized engine rejected request");

            TestLog.Pass(
                "No temporal history was created before initialization");
        }
        
        [Fact]
public async Task
    TopologySelectedNeighborSurvivesCompleteWildsExecutionRoute()
{
    TestLog.Section(
        "WildsEngine Topology / ExecuteAsync Integration");

    TestLog.Test(
        "Topology-selected Neighbor survives the complete Wilds lifecycle");

    var topology =
        new NeighborTopologyFixture();

    TestLog.Task(
        "Building directional topology through A.State.Process().");

    Assert.True(
        topology.A.State.Process(
            new AddNeighborInstruction(
                topology.B,
                NeighborTopologyFixture.LocalMembership,
                6.0)));

    Assert.True(
        topology.A.State.Process(
            new AddNeighborInstruction(
                topology.C,
                NeighborTopologyFixture.LocalMembership,
                2.0)));

    Assert.True(
        topology.A.State.Process(
            new AddNeighborInstruction(
                topology.D,
                NeighborTopologyFixture.RemoteMembership,
                1.0)));

    Assert.Equal(
        3,
        topology.A.Neighbors.Count);

    var sourceBefore =
        topology.A.Neighbors
            .Relationships
            .Select(
                relationship =>
                    (
                        relationship.Neighbor.Id,
                        relationship.Membership,
                        relationship.Distance))
            .ToArray();

    TestLog.State(
        $"Source topology contains {sourceBefore.Length} relationships");

    TestLog.Task(
        "Selecting the nearest local Neighbor through the existing operator pipeline.");

    var pipeline =
        new NeighborPipeline(
            new MembershipNeighborFilter(
                NeighborTopologyFixture.LocalMembership),
            new NearestNeighborOrder());

    var selectedRelationships =
        topology.A.ApplyNeighborOperator(
            pipeline);

    Assert.Equal(
        2,
        selectedRelationships.Count);

    Assert.Equal(
        topology.C.Id,
        selectedRelationships[0].Neighbor.Id);

    Assert.Equal(
        topology.B.Id,
        selectedRelationships[1].Neighbor.Id);

    var selectedNeighbor =
        selectedRelationships[0].Neighbor;

    Assert.Same(
        topology.C,
        selectedNeighbor);

    TestLog.State(
        $"Topology selected {selectedNeighbor.Slug}");

    var component =
        CreateComponent(
            "execute.topology-selection");

    using var engine =
        CreateEngine(
            component);

    Assert.True(
        engine.Initialize());

    TestLog.Task(
        "Submitting the exact topology-selected Neighbor to Wilds.");

    var moment =
        await engine.ExecuteAsync(
            selectedNeighbor,
            (
                observedNeighbor,
                cancellationToken) =>
            {
                cancellationToken
                    .ThrowIfCancellationRequested();

                TestLog.State(
                    "Selected Neighbor entered Wilds execution");

                Assert.Same(
                    selectedNeighbor,
                    observedNeighbor);

                Assert.Same(
                    topology.C,
                    observedNeighbor);

                return new ValueTask<object?>(
                    observedNeighbor);
            });

    TestLog.Task(
        "Verifying the selected Neighbor survived Wilds temporal resolution.");

    Assert.Equal(
        MomentOutcome.Completed,
        moment.Outcome);

    Assert.Null(
        moment.Exception);

    Assert.Single(
        moment.WorkUnitIds);

    Assert.Same(
        selectedNeighbor,
        moment.Result);

    Assert.Same(
        topology.C,
        moment.Result);

    Assert.Equal(
        1,
        engine.Timeline.Count);

    Assert.Same(
        moment,
        engine.Timeline.Moments[0]);

    Assert.Same(
        selectedNeighbor,
        engine.Timeline
            .Moments[0]
            .Result);

    Assert.Equal(
        1,
        engine.Epoch.Count);

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

    Assert.Same(
        selectedNeighbor,
        engine.Epoch
            .Timelines[0]
            .Moments[0]
            .Result);

    TestLog.Pass(
        "Exact topology-selected Neighbor survived " +
        "Wilds execution and temporal history.");

    TestLog.Task(
        "Verifying execution did not mutate the source topology.");

    var sourceAfter =
        topology.A.Neighbors
            .Relationships
            .Select(
                relationship =>
                    (
                        relationship.Neighbor.Id,
                        relationship.Membership,
                        relationship.Distance))
            .ToArray();

    Assert.Equal(
        sourceBefore,
        sourceAfter);

    Assert.Equal(
        3,
        topology.A.Neighbors.Count);

    Assert.True(
        topology.A.Neighbors.Contains(
            topology.B.Id,
            NeighborTopologyFixture.LocalMembership));

    Assert.True(
        topology.A.Neighbors.Contains(
            topology.C.Id,
            NeighborTopologyFixture.LocalMembership));

    Assert.True(
        topology.A.Neighbors.Contains(
            topology.D.Id,
            NeighborTopologyFixture.RemoteMembership));

    TestLog.Pass(
        "Wilds execution preserved the source Neighbor topology.");

    TestLog.Pass(
        "Topology-to-execution identity certification completed successfully.");
}

[Fact]
public async Task
    ConcurrentExecuteAsyncRecordsEveryMomentExactlyOnce()
{
    const int requestCount =
        64;

    TestLog.Section(
        "WildsEngine ExecuteAsync Concurrency");

    TestLog.Test(
        "Concurrent requests record every Moment exactly once");

    var component =
        CreateComponent(
            "execute.concurrent-history");

    using var engine =
        CreateEngine(
            component);

    Assert.True(
        engine.Initialize());

    var requests =
        Enumerable
            .Range(
                0,
                requestCount)
            .Select(
                index =>
                    new TestRuntimeObject(
                        $"concurrent-request-{index:D3}"))
            .ToArray();

    var release =
        new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

    var enteredCount =
        0;

    TestLog.Task(
        $"Submitting {requestCount} concurrent requests.");

    var executions =
        requests
            .Select(
                request =>
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

                                Interlocked.Increment(
                                    ref enteredCount);

                                await release.Task
                                    .ConfigureAwait(false);

                                cancellationToken
                                    .ThrowIfCancellationRequested();

                                return observedRequest;
                            })
                        .AsTask())
            .ToArray();

    TestLog.State(
        $"Submitted requests = {executions.Length}");

    Assert.Equal(
        requestCount,
        executions.Length);

    release.TrySetResult(
        true);

    var moments =
        await Task.WhenAll(
                executions)
            .ConfigureAwait(false);

    TestLog.State(
        $"Entered operations = {enteredCount}");

    TestLog.State(
        $"Returned Moments = {moments.Length}");

    Assert.Equal(
        requestCount,
        enteredCount);

    Assert.Equal(
        requestCount,
        moments.Length);

    TestLog.Task(
        "Verifying every concurrent request completed successfully.");

    Assert.All(
        moments,
        moment =>
        {
            Assert.Equal(
                MomentOutcome.Completed,
                moment.Outcome);

            Assert.Null(
                moment.Exception);

            Assert.NotNull(
                moment.Result);

            Assert.Single(
                moment.WorkUnitIds);
        });

    TestLog.Pass(
        "Every concurrent request produced a completed Moment.");

    TestLog.Task(
        "Verifying request and result identity remained aligned.");

    for (var index = 0;
         index < requestCount;
         index++)
    {
        Assert.Same(
            requests[index],
            moments[index].Result);
    }

    TestLog.Pass(
        "Every Moment retained its exact request object as the result.");

    TestLog.Task(
        "Verifying Pulse identities are unique.");

    var pulseIds =
        moments
            .Select(
                moment =>
                    moment.PulseId)
            .ToArray();

    Assert.Equal(
        requestCount,
        pulseIds.Distinct().Count());

    Assert.DoesNotContain(
        Guid.Empty,
        pulseIds);

    TestLog.Pass(
        "Every concurrent request received a unique Pulse identity.");

    TestLog.Task(
        "Verifying WorkUnit identities are unique.");

    var workUnitIds =
        moments
            .SelectMany(
                moment =>
                    moment.WorkUnitIds)
            .ToArray();

    Assert.Equal(
        requestCount,
        workUnitIds.Length);

    Assert.Equal(
        requestCount,
        workUnitIds.Distinct().Count());

    Assert.DoesNotContain(
        Guid.Empty,
        workUnitIds);

    TestLog.Pass(
        "Every concurrent request received one unique WorkUnit identity.");

    TestLog.Task(
        "Verifying Timeline recorded every returned Moment exactly once.");

    Assert.Equal(
        requestCount,
        engine.Timeline.Count);

    var recordedMoments =
        engine.Timeline
            .Moments
            .ToArray();

    Assert.Equal(
        requestCount,
        recordedMoments.Length);

    Assert.Equal(
        requestCount,
        recordedMoments
            .Select(
                moment =>
                    moment.Id)
            .Distinct()
            .Count());

    foreach (var returnedMoment in moments)
    {
        Assert.Equal(
            1,
            recordedMoments.Count(
                recordedMoment =>
                    ReferenceEquals(
                        returnedMoment,
                        recordedMoment)));
    }

    TestLog.Pass(
        "Timeline recorded every returned Moment exactly once.");

    TestLog.Task(
        "Verifying every submitted request appears exactly once in history.");

    foreach (var request in requests)
    {
        Assert.Equal(
            1,
            recordedMoments.Count(
                moment =>
                    ReferenceEquals(
                        request,
                        moment.Result)));
    }

    TestLog.Pass(
        "Timeline retained every submitted request exactly once.");

    TestLog.Task(
        "Verifying Epoch retained the complete shared Timeline.");

    Assert.Equal(
        1,
        engine.Epoch.Count);

    Assert.Single(
        engine.Epoch.Timelines);

    Assert.Same(
        engine.Timeline,
        engine.Epoch.Timelines[0]);

    Assert.Equal(
        requestCount,
        engine.Epoch
            .Timelines[0]
            .Moments
            .Count);

    foreach (var returnedMoment in moments)
    {
        Assert.Equal(
            1,
            engine.Epoch
                .Timelines[0]
                .Moments
                .Count(
                    recordedMoment =>
                        ReferenceEquals(
                            returnedMoment,
                            recordedMoment)));
    }

    TestLog.Pass(
        "Epoch retained the complete concurrent Timeline.");

    TestLog.State(
        $"Requests = {requestCount}");

    TestLog.State(
        $"Unique Pulses = {pulseIds.Distinct().Count()}");

    TestLog.State(
        $"Unique WorkUnits = {workUnitIds.Distinct().Count()}");

    TestLog.State(
        $"Timeline Moments = {engine.Timeline.Count}");

    TestLog.State(
        $"Epoch Timelines = {engine.Epoch.Count}");

    TestLog.Pass(
        "Concurrent ExecuteAsync temporal-history certification completed successfully.");
}

        private static Core.WildsEngine CreateEngine(
            IComponent component)
        {
            return new Core.WildsEngine(
                component,
                new WildsEngineOptions
                {
                    Layer =
                        "certification.execute",

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
                Name = name;
            }

            public string Name { get; }
        }

        private sealed class ExpectedExecutionException :
            Exception
        {
            public ExpectedExecutionException(
                string message)
                : base(
                    message)
            {
            }
        }
    }
}


