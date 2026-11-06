using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QSL.Core;
using QSL.Core.Runtime;
using QSL.Core.Runtime.Context;
using QSL.Core.Runtime.Packages;
using WildsEngine.Core.Abstractions;
using WildsEngine.Qsl;
using WildsEngine.Tests.Logging;
using WildsEngine.Tests.Registry;
using Xunit;

namespace WildsEngine.Core.Tests;

public class WildsQslProvenanceIntegrationTests
{
    public async Task
        QslParseDiagnosticsSurviveWildsFailureRoute()
    {
        TestLog.Section(
            "Wilds / QSL Provenance Integration");

        TestLog.Test(
            "QSL parse diagnostics survive Wilds failure route");

        var qslEngine =
            CreateQslEngine();

        var component =
            new TestComponent(
                "qsl.provenance.parse");

        using var wildsEngine =
            CreateWildsEngine(
                component,
                "qsl.provenance.parse");

        Assert.True(
            wildsEngine.Initialize());

        var executor =
            new WildsQslExecutor(
                qslEngine,
                wildsEngine);

        /*
         * Deliberately incomplete expression.
         *
         * The source reaches QSL through the Wilds-owned
         * Pulse / WorkUnit lifecycle and must fail during
         * parsing rather than escaping Wilds history.
         */
        var request =
            new WildsQslRequest(
                "1 +");

        var moment =
            await executor.ExecuteAsync(
                request);

        Assert.Equal(
            MomentOutcome.Failed,
            moment.Outcome);

        Assert.Null(
            moment.Result);

        Assert.Single(
            moment.WorkUnitIds);

        var qslException =
            Assert.IsType<WildsQslExecutionException>(
                moment.Exception);

        var processingResult =
            qslException.Result;

        Assert.False(
            processingResult.Succeeded);

        Assert.True(
            processingResult.ParseFailed);

        Assert.False(
            processingResult.EvaluationFailed);

        Assert.NotEmpty(
            processingResult.ParserDiagnostics);

        Assert.Empty(
            processingResult.RuntimeDiagnostics);

        Assert.Same(
            qslException,
            moment.Exception);

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

        Assert.Single(
            wildsEngine.Epoch
                .Timelines[0]
                .Moments);

        Assert.Same(
            moment,
            wildsEngine.Epoch
                .Timelines[0]
                .Moments[0]);

        TestLog.State(
            "Parser diagnostics: " +
            processingResult
                .ParserDiagnostics
                .Count);

        TestLog.State(
            "Moment outcome: " +
            moment.Outcome);

        TestLog.Pass(
            "Original QSL parse diagnostics survived " +
            "the Wilds failure route");
    }
    
    public async Task
        QslRuntimeDiagnosticsSurviveWildsFailureRoute()
    {
        TestLog.Section(
            "Wilds / QSL Provenance Integration");

        TestLog.Test(
            "QSL runtime provenance survives Wilds failure route");

        var qslEngine =
            CreateQslEngine();

        var component =
            new TestComponent(
                "qsl.provenance.runtime");

        using var wildsEngine =
            CreateWildsEngine(
                component,
                "qsl.provenance.runtime");

        Assert.True(
            wildsEngine.Initialize());

        var executor =
            new WildsQslExecutor(
                qslEngine,
                wildsEngine);

        /*
         * Valid QSL syntax with an identifier that has
         * intentionally not been bound.
         *
         * Existing certification recognizes the QSL
         * runtime diagnostic:
         *
         *     Unresolved identifier: missingConsumer
         */
        var request =
            new WildsQslRequest(
                "missingConsumer.Value");

        var moment =
            await executor.ExecuteAsync(
                request);

        Assert.Equal(
            MomentOutcome.Failed,
            moment.Outcome);

        Assert.Null(
            moment.Result);

        Assert.Single(
            moment.WorkUnitIds);

        var qslException =
            Assert.IsType<WildsQslExecutionException>(
                moment.Exception);

        var processingResult =
            qslException.Result;

        Assert.False(
            processingResult.Succeeded);

        Assert.False(
            processingResult.ParseFailed);

        Assert.True(
            processingResult.EvaluationFailed);

        Assert.Empty(
            processingResult.ParserDiagnostics);

        Assert.NotEmpty(
            processingResult.RuntimeDiagnostics);

        var foundExpectedDiagnostic =
            false;

        for (
            var index = 0;
            index <
            processingResult.RuntimeDiagnostics.Count;
            index++)
        {
            var diagnostic =
                processingResult
                    .RuntimeDiagnostics[index];

            if (diagnostic.Message.Contains(
                    "Unresolved identifier: missingConsumer",
                    StringComparison.Ordinal))
            {
                foundExpectedDiagnostic =
                    true;

                break;
            }
        }

        Assert.True(
            foundExpectedDiagnostic,
            "The original unresolved-identifier " +
            "QSL provenance entry must remain available.");

        Assert.Same(
            qslException,
            moment.Exception);

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

        Assert.Single(
            wildsEngine.Epoch
                .Timelines[0]
                .Moments);

        Assert.Same(
            moment,
            wildsEngine.Epoch
                .Timelines[0]
                .Moments[0]);

        TestLog.State(
            "Runtime diagnostics: " +
            processingResult
                .RuntimeDiagnostics
                .Count);

        TestLog.State(
            "Moment outcome: " +
            moment.Outcome);

        TestLog.Pass(
            "Original QSL runtime provenance remained " +
            "authoritative through Wilds");
    }

    
    public void
        QslStatePackageIdentitySurvivesRequestEnvelope()
    {
        TestLog.Section(
            "Wilds / QSL Provenance Integration");

        TestLog.Test(
            "QSL state package identity survives request envelope");

        var action =
            new TestAction(
                10.0);

        var binding =
            new QslConsumerBinding(
                "action",
                "Action",
                action);

        var package =
            new QslStatePackage(
                action,
                new Dictionary<string, object?>
                {
                    {
                        "SpaceDustAppetite",
                        87.0
                    }
                });

        var request =
            new WildsQslRequest(
                "action.Amount + action.SpaceDustAppetite",
                new[]
                {
                    binding
                },
                new[]
                {
                    package
                });

        Assert.Single(
            request.Consumers);

        Assert.Single(
            request.StatePackages);

        Assert.Same(
            binding,
            request.Consumers[0]);

        Assert.Same(
            action,
            request.Consumers[0].Value);

        Assert.Same(
            package,
            request.StatePackages[0]);

        Assert.Same(
            action,
            request.StatePackages[0].Target);

        Assert.Same(
            action,
            package.Target);

        TestLog.State(
            "Consumer binding reference preserved");

        TestLog.State(
            "State package reference preserved");

        TestLog.State(
            "State package target reference preserved");

        TestLog.Pass(
            "Exact QSL package and consumer target " +
            "survived the Wilds request envelope");
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

    private static Core.WildsEngine
        CreateWildsEngine(
            IComponent component,
            string layer)
    {
        return new Core.WildsEngine(
            component,
            new WildsEngineOptions
            {
                Layer =
                    layer,

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

    private sealed class TestAction
    {
        public TestAction(
            double amount)
        {
            Amount =
                amount;
        }

        public double Amount { get; }
    }
}