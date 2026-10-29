using System;
using System.Threading;
using System.Threading.Tasks;
using WildsEngine.Core.Abstractions;
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


