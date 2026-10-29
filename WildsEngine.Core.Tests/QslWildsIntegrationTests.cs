using System;
using System.Threading;
using System.Threading.Tasks;
using QSL.Core;
using QSL.Core.Runtime;
using QSL.Core.Runtime.Context;
using WildsEngine.Core.Abstractions;
using WildsEngine.Tests.Logging;
using WildsEngine.Tests.Registry;
using Xunit;

namespace WildsEngine.Core.Tests
{
    /// <summary>
    /// Certifies the QSL -> WildsEngine integration boundary.
    ///
    /// QSL source
    ///     -> QslEngine
    ///     -> QslProcessingResult
    ///     -> WildsEngine
    ///     -> Pulse
    ///     -> WorkUnit
    ///     -> Moment
    ///     -> Timeline
    ///     -> Epoch
    ///
    /// These tests verify that QSL-produced runtime values
    /// survive the complete Wilds processing route without
    /// losing their identity or outcome semantics.
    /// </summary>
    public sealed class QslWildsIntegrationTests
    {
        [Fact]
        public async Task
            CertifiedQslArithmeticTraversesCompleteWildsRoute()
        {
            TestLog.Section(
                "QSL -> WildsEngine Integration");

            TestLog.Test(
                "Certified QSL arithmetic traverses complete Wilds route");

            var qslEngine =
                CreateQslEngine();

            const string source =
                "1 + 2";

            TestLog.Write(
                $"[QSL] Evaluating source: {source}");

            var qslResult =
                qslEngine.Evaluate(
                    source);

            Assert.True(
                qslResult.Succeeded);

            Assert.Empty(
                qslResult.ParserDiagnostics);

            Assert.Empty(
                qslResult.RuntimeDiagnostics);

            Assert.Null(
                qslResult.Exception);

            TestLog.State(
                $"QSL evaluation succeeded | " +
                $"Value={qslResult.Value} | " +
                $"Type={qslResult.Value?.GetType().FullName ?? "null"}");

            var component =
                CreateComponent(
                    "qsl.integration.arithmetic");

            using var wildsEngine =
                CreateWildsEngine(
                    component);

            Assert.True(
                wildsEngine.Initialize());

            TestLog.State(
                "WildsEngine initialized");

            var moment =
                await wildsEngine.ExecuteAsync(
                    qslResult,
                    (
                        request,
                        cancellationToken) =>
                    {
                        cancellationToken
                            .ThrowIfCancellationRequested();

                        Assert.Same(
                            qslResult,
                            request);

                        TestLog.State(
                            "Exact QslProcessingResult reached " +
                            "Wilds execution");

                        return new ValueTask<object?>(
                            qslResult.Value);
                    });

            TestLog.State(
                $"Wilds Moment resolved | " +
                $"Outcome={moment.Outcome} | " +
                $"PulseId={moment.PulseId} | " +
                $"WorkUnits={moment.WorkUnitIds.Count}");

            TestLog.State(
                $"Wilds result | " +
                $"Value={moment.Result} | " +
                $"Type={moment.Result?.GetType().FullName ?? "null"}");

            Assert.Equal(
                MomentOutcome.Completed,
                moment.Outcome);

            Assert.Null(
                moment.Exception);

            Assert.Single(
                moment.WorkUnitIds);

            /*
             * This is intentionally stronger than:
             *
             * Assert.Equal(3, moment.Result);
             *
             * The integration boundary is responsible for
             * preserving the runtime value produced by QSL.
             * It is not responsible for coercing that value
             * into a test-selected numeric type.
             */
            Assert.Same(
                qslResult.Value,
                moment.Result);

            Assert.Equal(
                qslResult.Value?.GetType(),
                moment.Result?.GetType());

            Assert.Single(
                wildsEngine.Timeline.Moments);

            Assert.Same(
                moment,
                wildsEngine.Timeline.Moments[0]);

            Assert.Single(
                wildsEngine.Epoch.Timelines);

            Assert.Same(
                wildsEngine.Timeline,
                wildsEngine.Epoch.Timelines[0]);

            Assert.Same(
                qslResult.Value,
                wildsEngine.Timeline
                    .Moments[0]
                    .Result);

            TestLog.State(
                "Exact QSL-produced value survived " +
                "Pulse -> WorkUnit -> Moment -> " +
                "Timeline -> Epoch");

            TestLog.Pass(
                "Certified QSL arithmetic traversed " +
                "complete Wilds route");
        }

        [Fact]
        public async Task
            QslResultPreservesArbitraryRuntimeIdentity()
        {
            TestLog.Test(
                "QSL/Wilds route preserves arbitrary runtime identity");

            var component =
                CreateComponent(
                    "qsl.integration.identity");

            using var wildsEngine =
                CreateWildsEngine(
                    component);

            Assert.True(
                wildsEngine.Initialize());

            var runtimeObject =
                new TestRuntimeObject(
                    "qsl-consumer-object");

            TestLog.State(
                $"Runtime object created | " +
                $"Name={runtimeObject.Name}");

            var moment =
                await wildsEngine.ExecuteAsync(
                    runtimeObject,
                    (
                        request,
                        cancellationToken) =>
                    {
                        cancellationToken
                            .ThrowIfCancellationRequested();

                        Assert.Same(
                            runtimeObject,
                            request);

                        return new ValueTask<object?>(
                            request);
                    });

            Assert.Equal(
                MomentOutcome.Completed,
                moment.Outcome);

            Assert.Null(
                moment.Exception);

            Assert.Single(
                moment.WorkUnitIds);

            Assert.Same(
                runtimeObject,
                moment.Result);

            Assert.Single(
                wildsEngine.Timeline.Moments);

            Assert.Same(
                runtimeObject,
                wildsEngine.Timeline
                    .Moments[0]
                    .Result);

            TestLog.State(
                "Exact arbitrary runtime reference survived " +
                "Wilds temporal history");

            TestLog.Pass(
                "QSL/Wilds route preserved arbitrary runtime identity");
        }

        [Fact]
        public async Task
            QslRequestCancellationBecomesCancelledMoment()
        {
            TestLog.Test(
                "QSL/Wilds cancellation becomes cancelled Moment");

            var component =
                CreateComponent(
                    "qsl.integration.cancellation");

            using var wildsEngine =
                CreateWildsEngine(
                    component);

            Assert.True(
                wildsEngine.Initialize());

            using var cancellation =
                new CancellationTokenSource();

            cancellation.Cancel();

            var operationWasCalled =
                false;

            var moment =
                await wildsEngine.ExecuteAsync(
                    "cancelled-qsl-request",
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
                wildsEngine.Timeline.Moments);

            Assert.Same(
                moment,
                wildsEngine.Timeline.Moments[0]);

            TestLog.State(
                $"Cancellation recorded | " +
                $"Outcome={moment.Outcome} | " +
                $"PulseId={moment.PulseId}");

            TestLog.Pass(
                "QSL/Wilds cancellation became honest temporal history");
        }

[Fact]
public void InvalidQslInputCreatesNoWildsTemporalHistory()
{
    TestLog.Test(
        "Invalid QSL input creates no Wilds temporal history");

    var qslEngine =
        CreateQslEngine();

    const string source =
        "";

    TestLog.Write(
        "[QSL] Evaluating invalid empty source");

    var qslResult =
        qslEngine.Evaluate(
            source);

    Assert.False(
        qslResult.Succeeded);

    Assert.True(
        qslResult.ParseFailed);

    Assert.False(
        qslResult.EvaluationFailed);

    Assert.False(
        qslResult.HasException);

    Assert.NotEmpty(
        qslResult.ParserDiagnostics);

    Assert.Empty(
        qslResult.RuntimeDiagnostics);

    Assert.Null(
        qslResult.Exception);

    Assert.Null(
        qslResult.Value);

    TestLog.State(
        $"QSL rejected input | " +
        $"ParseFailed={qslResult.ParseFailed} | " +
        $"EvaluationFailed={qslResult.EvaluationFailed} | " +
        $"ParserDiagnostics=" +
        $"{qslResult.ParserDiagnostics.Count} | " +
        $"RuntimeDiagnostics=" +
        $"{qslResult.RuntimeDiagnostics.Count} | " +
        $"HasException={qslResult.HasException}");

    var component =
        CreateComponent(
            "qsl.integration.invalid");

    using var wildsEngine =
        CreateWildsEngine(
            component);

    Assert.True(
        wildsEngine.Initialize());

    /*
     * The failed QSL result is deliberately not submitted
     * through WildsEngine.ExecuteAsync.
     *
     * Therefore it creates no Pulse, WorkUnit, or Moment.
     */
    Assert.Equal(
        0,
        wildsEngine.Timeline.Count);

    Assert.Empty(
        wildsEngine.Timeline.Moments);

    /*
     * The Epoch already owns the engine's initialized
     * Timeline. An empty Timeline is structure, not
     * request history.
     */
    Assert.Equal(
        1,
        wildsEngine.Epoch.Count);

    Assert.Single(
        wildsEngine.Epoch.Timelines);

    Assert.Same(
        wildsEngine.Timeline,
        wildsEngine.Epoch.Timelines[0]);

    Assert.Empty(
        wildsEngine.Epoch
            .Timelines[0]
            .Moments);

    TestLog.State(
        "No QSL request was submitted to Wilds");

    TestLog.State(
        "No Pulse, WorkUnit, or Moment was created");

    TestLog.State(
        "Timeline contains no Moments");

    TestLog.State(
        "Epoch retains its initialized empty Timeline");

    TestLog.Pass(
        "Invalid QSL input created no Wilds request history");
}

        private static Core.WildsEngine CreateWildsEngine(
            IComponent component)
        {
            return new Core.WildsEngine(
                component,
                new WildsEngineOptions
                {
                    Layer =
                        "certification.qsl",

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

        private static QslEngine CreateQslEngine()
        {
            var registry =
                new QslRuntimeRegistry();

            registry.RegisterDefaultEvaluators();

            var context =
                new QslRuntimeContext(
                    registry);

            return new QslEngine(
                context);
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
}