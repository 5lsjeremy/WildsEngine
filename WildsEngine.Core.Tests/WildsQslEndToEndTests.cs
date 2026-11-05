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

namespace WildsEngine.Core.Tests
{
    /// <summary>
    /// Certifies the complete Wilds -> QSL -> Wilds route.
    ///
    /// Consumer request
    ///     -> Wilds
    ///     -> Pulse
    ///     -> WorkUnit
    ///     -> QSL
    ///     -> WorkUnit result
    ///     -> Moment
    ///     -> Timeline
    ///     -> Epoch
    ///
    /// Wilds owns request processing and temporal resolution.
    /// QSL performs delegated language work inside that route.
    /// </summary>
    public sealed class WildsQslEndToEndTests
    {
        [Fact]
        public async Task QslWorkResolvesThroughCompleteWildsLifecycle()
        {
            TestLog.Section(
                "Wilds / QSL End-to-End");

            TestLog.Test(
                "QSL work resolves inside the Wilds lifecycle");

            var qslEngine =
                CreateQslEngine();

            var component =
                new TestComponent(
                    "qsl.end-to-end");

            using var wildsEngine =
                CreateWildsEngine(
                    component);

            Assert.True(
                wildsEngine.Initialize());

            var executor =
                new WildsQslExecutor(
                    qslEngine,
                    wildsEngine);

            var player =
                new TestPlayer(
                    50.0);

            var action =
                new TestAction(
                    10.0);

            var playerBinding =
                new QslConsumerBinding(
                    "player",
                    "Player",
                    player);

            var actionBinding =
                new QslConsumerBinding(
                    "action",
                    "Action",
                    action);

            var statePackage =
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
                    "player.Health = player.Health - " +
                    "(action.Amount * " +
                    "(action.SpaceDustAppetite / 100))",
                    new[]
                    {
                        playerBinding,
                        actionBinding
                    },
                    new[]
                    {
                        statePackage
                    });

            Assert.Equal(
                0,
                wildsEngine.Timeline.Count);

            TestLog.State(
                "Submitting consumer request to Wilds");

            var moment =
                await executor.ExecuteAsync(
                    request);

            TestLog.State(
                "QSL work returned to Wilds");

            Assert.Equal(
                MomentOutcome.Completed,
                moment.Outcome);

            Assert.Null(
                moment.Exception);

            Assert.Single(
                moment.WorkUnitIds);

            Assert.Equal(
                41.3,
                Convert.ToDouble(
                    moment.Result),
                10);

            Assert.Equal(
                41.3,
                player.Health,
                10);

            Assert.Same(
                player,
                playerBinding.Value);

            Assert.Same(
                action,
                actionBinding.Value);

            Assert.Same(
                action,
                statePackage.Target);

            Assert.Equal(
                1,
                wildsEngine.Timeline.Count);

            Assert.Same(
                moment,
                wildsEngine.Timeline.Moments[0]);

            Assert.Equal(
                1,
                wildsEngine.Epoch.Count);

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
                "Player.Health: 50 -> " +
                player.Health);

            TestLog.State(
                "Moment outcome: " +
                moment.Outcome);

            TestLog.State(
                "WorkUnits: " +
                moment.WorkUnitIds.Count);

            TestLog.State(
                "Timeline Moments: " +
                wildsEngine.Timeline.Count);

            TestLog.State(
                "Epoch Timelines: " +
                wildsEngine.Epoch.Count);

            TestLog.Pass(
                "Wilds controlled the complete QSL-backed request lifecycle");
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

        private static Core.WildsEngine CreateWildsEngine(
            IComponent component)
        {
            return new Core.WildsEngine(
                component,
                new WildsEngineOptions
                {
                    Layer =
                        "certification.qsl.end-to-end",

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

        private sealed class TestPlayer
        {
            public TestPlayer(
                double health)
            {
                Health =
                    health;
            }

            public double Health { get; set; }
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
}