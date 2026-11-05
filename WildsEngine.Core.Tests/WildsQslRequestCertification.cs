using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using QSL.Core;
using QSL.Core.Runtime;
using QSL.Core.Runtime.Context;
using QSL.Core.Runtime.Packages;
using WildsEngine.Core.Abstractions;
using WildsEngine.Qsl;
using WildsEngine.Tests.Logging;
using WildsEngine.Tests.Registry;

namespace WildsEngine.Core.Tests.Certification
{
    /// <summary>
    /// Certifies the complete WildsQslRequest integration
    /// boundary.
    ///
    /// WildsQslRequest
    ///     -> Pulse
    ///     -> WorkUnit
    ///     -> consumer binding
    ///     -> attached state
    ///     -> QSL evaluation
    ///     -> Moment
    ///     -> Timeline
    ///     -> Epoch
    ///
    /// These tests intentionally exercise consumer-owned
    /// runtime objects through the public Wilds/QSL boundary.
    /// </summary>
    public static class WildsQslRequestCertification
    {
        public static async Task RunAllAsync()
        {
            TestLog.Write(
                "=== Wilds / QSL Request Certification ===");

            await CertifyEnvelopeExecutionAsync();

            await CertifyAttachedStateAsync();

            await CertifySpaceDustMutationAsync();

            TestLog.Pass(
                "Wilds / QSL request certification");
        }

        private static async Task CertifyEnvelopeExecutionAsync()
        {
            TestLog.Test(
                "WildsQslRequest traverses complete Wilds lifecycle");

            var qslEngine =
                CreateQslEngine();

            var component =
                CreateComponent(
                    "certification.qsl.request.envelope");

            using var wildsEngine =
                CreateWildsEngine(
                    component,
                    "certification.qsl.request.envelope");

            Assert(
                wildsEngine.Initialize(),
                "WildsEngine initialization should succeed.");

            var executor =
                new WildsQslExecutor(
                    qslEngine,
                    wildsEngine);

            var request =
                new WildsQslRequest(
                    "1 + 2");

            var moment =
                await executor.ExecuteAsync(
                    request);

            Assert(
                moment.Outcome ==
                MomentOutcome.Completed,
                "Envelope execution must produce a completed Moment.");

            Assert(
                moment.Exception == null,
                "Completed envelope execution must not carry an exception.");

            Assert(
                NumericEquals(
                    moment.Result,
                    3.0),
                "Envelope arithmetic result must equal 3.");

            Assert(
                moment.WorkUnitIds.Count == 1,
                "Envelope execution must resolve through exactly one WorkUnit.");

            AssertCompleteHistory(
                wildsEngine,
                moment);

            TestLog.State(
                "Envelope result: " +
                FormatValue(
                    moment.Result));

            TestLog.Pass(
                "WildsQslRequest completed through Wilds history");
        }

        private static async Task CertifyAttachedStateAsync()
        {
            TestLog.Test(
                "WildsQslRequest carries attached runtime vocabulary");

            var qslEngine =
                CreateQslEngine();

            var component =
                CreateComponent(
                    "certification.qsl.request.state");

            using var wildsEngine =
                CreateWildsEngine(
                    component,
                    "certification.qsl.request.state");

            Assert(
                wildsEngine.Initialize(),
                "WildsEngine initialization should succeed.");

            var executor =
                new WildsQslExecutor(
                    qslEngine,
                    wildsEngine);

            var action =
                new TestAction(
                    10.0);

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
                    "action.Amount + action.SpaceDustAppetite",
                    new[]
                    {
                        actionBinding
                    },
                    new[]
                    {
                        statePackage
                    });

            var moment =
                await executor.ExecuteAsync(
                    request);

            Assert(
                moment.Outcome ==
                MomentOutcome.Completed,
                "Attached-state request must produce a completed Moment.");

            Assert(
                moment.Exception == null,
                "Attached-state request must not carry an exception.");

            Assert(
                NumericEquals(
                    moment.Result,
                    97.0),
                "Native and attached Action state must compose to 97.");

            Assert(
                ReferenceEquals(
                    action,
                    actionBinding.Value),
                "Consumer binding must retain the exact Action object.");

            Assert(
                ReferenceEquals(
                    action,
                    statePackage.Target),
                "State package must target the exact Action object.");

            AssertCompleteHistory(
                wildsEngine,
                moment);

            TestLog.State(
                "Native Amount: " +
                action.Amount);

            TestLog.State(
                "Attached SpaceDustAppetite: 87");

            TestLog.State(
                "QSL result: " +
                FormatValue(
                    moment.Result));

            TestLog.Pass(
                "Native and attached runtime vocabulary composed through Wilds");
        }

        private static async Task CertifySpaceDustMutationAsync()
        {
            TestLog.Test(
                "Space Dust mutates exact consumer-owned Player");

            var qslEngine =
                CreateQslEngine();

            var component =
                CreateComponent(
                    "certification.qsl.request.space-dust");

            using var wildsEngine =
                CreateWildsEngine(
                    component,
                    "certification.qsl.request.space-dust");

            Assert(
                wildsEngine.Initialize(),
                "WildsEngine initialization should succeed.");

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

            var actionState =
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
                        actionState
                    });

            var moment =
                await executor.ExecuteAsync(
                    request);

            Assert(
                moment.Outcome ==
                MomentOutcome.Completed,
                "Space Dust request must produce a completed Moment.");

            Assert(
                moment.Exception == null,
                "Space Dust request must not carry an exception.");

            Assert(
                NumericEquals(
                    moment.Result,
                    41.3),
                "Space Dust QSL result must equal 41.3.");

            Assert(
                NumericEquals(
                    player.Health,
                    41.3),
                "Space Dust must mutate Player.Health to 41.3.");

            Assert(
                ReferenceEquals(
                    player,
                    playerBinding.Value),
                "Player binding must retain the exact consumer object.");

            Assert(
                ReferenceEquals(
                    action,
                    actionBinding.Value),
                "Action binding must retain the exact consumer object.");

            Assert(
                ReferenceEquals(
                    action,
                    actionState.Target),
                "Attached Action state must target the exact Action object.");

            AssertCompleteHistory(
                wildsEngine,
                moment);

            Assert(
                ReferenceEquals(
                    moment,
                    wildsEngine.Timeline.Moments[0]),
                "Timeline must retain the exact Space Dust Moment.");

            Assert(
                ReferenceEquals(
                    moment,
                    wildsEngine.Epoch
                        .Timelines[0]
                        .Moments[0]),
                "Epoch history must retain the exact Space Dust Moment.");

            TestLog.State(
                "Player.Health: 50 -> " +
                player.Health.ToString(
                    CultureInfo.InvariantCulture));

            TestLog.State(
                "Action.Amount: " +
                action.Amount.ToString(
                    CultureInfo.InvariantCulture));

            TestLog.State(
                "Action.SpaceDustAppetite: 87");

            TestLog.State(
                "Moment result: " +
                FormatValue(
                    moment.Result));

            TestLog.Pass(
                "Space Dust mutated the exact Player through Wilds");
        }

        private static void AssertCompleteHistory(
            global::WildsEngine.Core.WildsEngine wildsEngine,
            IMoment moment)
        {
            Assert(
                wildsEngine.Timeline.Count == 1,
                "Certification engine must record exactly one Moment.");

            Assert(
                ReferenceEquals(
                    moment,
                    wildsEngine.Timeline.Moments[0]),
                "Timeline must retain the exact terminal Moment.");

            Assert(
                wildsEngine.Epoch.Count == 1,
                "Certification Epoch must contain exactly one Timeline.");

            Assert(
                wildsEngine.Epoch.Timelines.Count == 1,
                "Epoch Timeline collection must contain one Timeline.");

            Assert(
                ReferenceEquals(
                    wildsEngine.Timeline,
                    wildsEngine.Epoch.Timelines[0]),
                "Epoch must retain the engine Timeline.");

            Assert(
                wildsEngine.Epoch
                    .Timelines[0]
                    .Moments
                    .Count == 1,
                "Epoch Timeline must expose the terminal Moment.");

            Assert(
                ReferenceEquals(
                    moment,
                    wildsEngine.Epoch
                        .Timelines[0]
                        .Moments[0]),
                "Epoch must expose the exact terminal Moment.");
        }

        private static bool NumericEquals(
            object? value,
            double expected)
        {
            if (value == null)
            {
                return false;
            }

            try
            {
                var actual =
                    Convert.ToDouble(
                        value,
                        CultureInfo.InvariantCulture);

                return Math.Abs(
                           actual -
                           expected) <
                       0.0000001;
            }
            catch (
                InvalidCastException)
            {
                return false;
            }
            catch (
                FormatException)
            {
                return false;
            }
            catch (
                OverflowException)
            {
                return false;
            }
        }

        private static string FormatValue(
            object? value)
        {
            if (value == null)
            {
                return "<null>";
            }

            var formattable =
                value as IFormattable;

            if (formattable != null)
            {
                return formattable.ToString(
                           null,
                           CultureInfo.InvariantCulture)
                       ?? string.Empty;
            }

            return value.ToString()
                   ?? string.Empty;
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

        private static IComponent CreateComponent(
            string layer)
        {
            return new TestComponent(
                layer);
        }

        private static
            global::WildsEngine.Core.WildsEngine
            CreateWildsEngine(
                IComponent component,
                string layer)
        {
            return new
                global::WildsEngine.Core.WildsEngine(
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

        private static void Assert(
            bool condition,
            string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(
                    message);
            }
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