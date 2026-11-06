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

public sealed class WildsQslRuntimeIsolationTests
{
    [Fact]
    public async Task
        IndependentRuntimesCanUseIdenticalAliasesWithoutCrossContamination()
    {
        TestLog.Section(
            "WILDS / QSL RUNTIME ISOLATION");

        TestLog.Test(
            "Independent runtimes use identical aliases without sharing targets or attached state.");

        var qslA =
            CreateQslEngine();

        var qslB =
            CreateQslEngine();

        var componentA =
            new TestComponent(
                "qsl.isolation.a");

        var componentB =
            new TestComponent(
                "qsl.isolation.b");

        using var wildsA =
            CreateWildsEngine(
                componentA,
                "certification.qsl.isolation.a");

        using var wildsB =
            CreateWildsEngine(
                componentB,
                "certification.qsl.isolation.b");

        Assert.True(
            wildsA.Initialize());

        Assert.True(
            wildsB.Initialize());

        var executorA =
            new WildsQslExecutor(
                qslA,
                wildsA);

        var executorB =
            new WildsQslExecutor(
                qslB,
                wildsB);

        var playerA =
            new TestPlayer(
                100.0);

        var playerB =
            new TestPlayer(
                200.0);

        var actionA =
            new TestAction(
                10.0);

        var actionB =
            new TestAction(
                20.0);

        /*
         * Both runtimes intentionally use the exact
         * same consumer aliases.
         */
        var playerBindingA =
            new QslConsumerBinding(
                "player",
                "Player",
                playerA);

        var actionBindingA =
            new QslConsumerBinding(
                "action",
                "Action",
                actionA);

        var playerBindingB =
            new QslConsumerBinding(
                "player",
                "Player",
                playerB);

        var actionBindingB =
            new QslConsumerBinding(
                "action",
                "Action",
                actionB);

        /*
         * Both packages intentionally introduce the
         * same attached-property name with different
         * values and different exact target objects.
         */
        var packageA =
            new QslStatePackage(
                actionA,
                new Dictionary<string, object?>
                {
                    {
                        "SpaceDustAppetite",
                        25.0
                    }
                });

        var packageB =
            new QslStatePackage(
                actionB,
                new Dictionary<string, object?>
                {
                    {
                        "SpaceDustAppetite",
                        75.0
                    }
                });

        var source =
            "player.Health = player.Health - " +
            "(action.Amount * " +
            "(action.SpaceDustAppetite / 100))";

        var requestA =
            new WildsQslRequest(
                source,
                new[]
                {
                    playerBindingA,
                    actionBindingA
                },
                new[]
                {
                    packageA
                });

        var requestB =
            new WildsQslRequest(
                source,
                new[]
                {
                    playerBindingB,
                    actionBindingB
                },
                new[]
                {
                    packageB
                });

        TestLog.State(
            "Runtime A: player.Health=100, action.Amount=10, " +
            "SpaceDustAppetite=25");

        TestLog.State(
            "Runtime B: player.Health=200, action.Amount=20, " +
            "SpaceDustAppetite=75");

        TestLog.Task(
            "Executing both requests through their independent Wilds/QSL runtimes.");

        var momentA =
            await executorA.ExecuteAsync(
                requestA);

        var momentB =
            await executorB.ExecuteAsync(
                requestB);

        TestLog.Task(
            "Verifying both runtimes completed independently.");

        Assert.Equal(
            MomentOutcome.Completed,
            momentA.Outcome);

        Assert.Equal(
            MomentOutcome.Completed,
            momentB.Outcome);

        Assert.Null(
            momentA.Exception);

        Assert.Null(
            momentB.Exception);

        /*
         * A:
         *
         * 100 - (10 * 0.25)
         * = 97.5
         */
        Assert.Equal(
            97.5,
            playerA.Health,
            10);

        Assert.Equal(
            97.5,
            Convert.ToDouble(
                momentA.Result),
            10);

        /*
         * B:
         *
         * 200 - (20 * 0.75)
         * = 185
         */
        Assert.Equal(
            185.0,
            playerB.Health,
            10);

        Assert.Equal(
            185.0,
            Convert.ToDouble(
                momentB.Result),
            10);

        TestLog.State(
            $"Runtime A Player.Health = {playerA.Health}");

        TestLog.State(
            $"Runtime B Player.Health = {playerB.Health}");

        TestLog.Pass(
            "Identical aliases resolved against independent consumer objects.");

        TestLog.Task(
            "Verifying exact target and package identity remained isolated.");

        Assert.Same(
            playerA,
            playerBindingA.Value);

        Assert.Same(
            actionA,
            actionBindingA.Value);

        Assert.Same(
            actionA,
            packageA.Target);

        Assert.Same(
            playerB,
            playerBindingB.Value);

        Assert.Same(
            actionB,
            actionBindingB.Value);

        Assert.Same(
            actionB,
            packageB.Target);

        Assert.NotSame(
            playerA,
            playerB);

        Assert.NotSame(
            actionA,
            actionB);

        Assert.NotSame(
            packageA,
            packageB);

        TestLog.Pass(
            "Consumer and state-package target identities remained isolated.");

        TestLog.Task(
            "Verifying Wilds temporal histories remained isolated.");

        Assert.Equal(
            1,
            wildsA.Timeline.Count);

        Assert.Equal(
            1,
            wildsB.Timeline.Count);

        Assert.Same(
            momentA,
            wildsA.Timeline.Moments[0]);

        Assert.Same(
            momentB,
            wildsB.Timeline.Moments[0]);

        Assert.NotSame(
            momentA,
            momentB);

        Assert.NotEqual(
            momentA.PulseId,
            momentB.PulseId);

        Assert.Equal(
            1,
            wildsA.Epoch.Count);

        Assert.Equal(
            1,
            wildsB.Epoch.Count);

        Assert.Same(
            wildsA.Timeline,
            wildsA.Epoch.Timelines[0]);

        Assert.Same(
            wildsB.Timeline,
            wildsB.Epoch.Timelines[0]);

        Assert.NotSame(
            wildsA.Timeline,
            wildsB.Timeline);

        Assert.NotSame(
            wildsA.Epoch,
            wildsB.Epoch);

        TestLog.Pass(
            "Timeline and Epoch histories remained runtime-local.");

        TestLog.Pass(
            "Wilds/QSL runtime isolation certification completed successfully.");
    }
    
    

    [Fact]
    public async Task
        FailureInOneRuntimeDoesNotContaminateAnotherRuntime()
    {
        TestLog.Section(
            "WILDS / QSL FAILURE ISOLATION");

        TestLog.Test(
            "Failure in one runtime does not alter another runtime's result or history.");

        var qslA =
            CreateQslEngine();

        var qslB =
            CreateQslEngine();

        var componentA =
            new TestComponent(
                "qsl.failure-isolation.a");

        var componentB =
            new TestComponent(
                "qsl.failure-isolation.b");

        using var wildsA =
            CreateWildsEngine(
                componentA,
                "certification.qsl.failure-isolation.a");

        using var wildsB =
            CreateWildsEngine(
                componentB,
                "certification.qsl.failure-isolation.b");

        Assert.True(
            wildsA.Initialize());

        Assert.True(
            wildsB.Initialize());

        var executorA =
            new WildsQslExecutor(
                qslA,
                wildsA);

        var executorB =
            new WildsQslExecutor(
                qslB,
                wildsB);

        var playerA =
            new TestPlayer(
                50.0);

        var actionA =
            new TestAction(
                10.0);

        var playerB =
            new TestPlayer(
                80.0);

        var actionB =
            new TestAction(
                20.0);

        var requestA =
            new WildsQslRequest(
                "player.Health = player.Health - " +
                "(action.Amount * " +
                "(action.SpaceDustAppetite / 100))",
                new[]
                {
                    new QslConsumerBinding(
                        "player",
                        "Player",
                        playerA),

                    new QslConsumerBinding(
                        "action",
                        "Action",
                        actionA)
                },
                new[]
                {
                    new QslStatePackage(
                        actionA,
                        new Dictionary<string, object?>
                        {
                            {
                                "SpaceDustAppetite",
                                50.0
                            }
                        })
                });

        /*
         * Runtime B deliberately receives source that
         * cannot resolve successfully.
         *
         * We do not need its exact diagnostic shape here.
         * This test only certifies cross-runtime isolation.
         */
        var requestB =
            new WildsQslRequest(
                "missingRuntimeObject.Value",
                new[]
                {
                    new QslConsumerBinding(
                        "player",
                        "Player",
                        playerB),

                    new QslConsumerBinding(
                        "action",
                        "Action",
                        actionB)
                });

        TestLog.Task(
            "Executing successful Runtime A request.");

        var momentA =
            await executorA.ExecuteAsync(
                requestA);

        Assert.Equal(
            MomentOutcome.Completed,
            momentA.Outcome);

        Assert.Equal(
            45.0,
            playerA.Health,
            10);

        Assert.Equal(
            45.0,
            Convert.ToDouble(
                momentA.Result),
            10);

        TestLog.Task(
            "Executing intentionally failing Runtime B request.");

        var momentB =
            await executorB.ExecuteAsync(
                requestB);

        Assert.Equal(
            MomentOutcome.Failed,
            momentB.Outcome);

        Assert.Null(
            momentB.Result);

        Assert.NotNull(
            momentB.Exception);

        /*
         * Runtime B failure must not alter the already
         * resolved consumer state belonging to Runtime A.
         */
        Assert.Equal(
            45.0,
            playerA.Health,
            10);

        /*
         * The failed source did not assign playerB.Health,
         * so its native value must remain unchanged.
         */
        Assert.Equal(
            80.0,
            playerB.Health,
            10);

        TestLog.State(
            $"Runtime A outcome = {momentA.Outcome}");

        TestLog.State(
            $"Runtime B outcome = {momentB.Outcome}");

        TestLog.State(
            $"Runtime A Player.Health = {playerA.Health}");

        TestLog.State(
            $"Runtime B Player.Health = {playerB.Health}");

        TestLog.Task(
            "Verifying each engine retained only its own Moment.");

        Assert.Single(
            wildsA.Timeline.Moments);

        Assert.Single(
            wildsB.Timeline.Moments);

        Assert.Same(
            momentA,
            wildsA.Timeline.Moments[0]);

        Assert.Same(
            momentB,
            wildsB.Timeline.Moments[0]);

        Assert.NotSame(
            momentA,
            momentB);

        Assert.Equal(
            MomentOutcome.Completed,
            wildsA.Timeline.Moments[0].Outcome);

        Assert.Equal(
            MomentOutcome.Failed,
            wildsB.Timeline.Moments[0].Outcome);

        Assert.NotEqual(
            momentA.PulseId,
            momentB.PulseId);

        Assert.Same(
            wildsA.Timeline,
            wildsA.Epoch.Timelines[0]);

        Assert.Same(
            wildsB.Timeline,
            wildsB.Epoch.Timelines[0]);

        TestLog.Pass(
            "Failure remained local to the failing runtime.");

        TestLog.Pass(
            "Successful consumer state and history remained unchanged.");

        TestLog.Pass(
            "Wilds/QSL failure-isolation certification completed successfully.");
    }
    
    [Fact]
public void
    ClearingOneQslRuntimeDoesNotAffectAnotherRuntime()
{
    TestLog.Section(
        "WILDS / QSL CLEAR ISOLATION");

    TestLog.Test(
        "Clearing one QSL runtime does not alter another runtime.");

    var qslA =
        CreateQslEngine();

    var qslB =
        CreateQslEngine();

    var playerA =
        new TestPlayer(
            50.0);

    var playerB =
        new TestPlayer(
            80.0);

    var bindingA =
        new QslConsumerBinding(
            "player",
            "Player",
            playerA);

    var bindingB =
        new QslConsumerBinding(
            "player",
            "Player",
            playerB);

    qslA.BindConsumer(
        bindingA);

    qslB.BindConsumer(
        bindingB);

    Assert.True(
        qslA.ReceiveState(
            playerA,
            "Curiosity",
            11.0));

    Assert.True(
        qslB.ReceiveState(
            playerB,
            "Curiosity",
            22.0));

    var beforeA =
        qslA.Evaluate(
            "player.Curiosity");

    var beforeB =
        qslB.Evaluate(
            "player.Curiosity");

    Assert.True(
        beforeA.Succeeded);

    Assert.True(
        beforeB.Succeeded);

    Assert.Equal(
        11.0,
        Convert.ToDouble(
            beforeA.Value),
        10);

    Assert.Equal(
        22.0,
        Convert.ToDouble(
            beforeB.Value),
        10);

    TestLog.State(
        "Runtime A player.Curiosity = 11");

    TestLog.State(
        "Runtime B player.Curiosity = 22");

    TestLog.Task(
        "Clearing Runtime A only.");

    qslA.Clear();

    var afterA =
        qslA.Evaluate(
            "player.Curiosity");

    var afterB =
        qslB.Evaluate(
            "player.Curiosity");

    TestLog.Task(
        "Verifying Runtime A no longer resolves its cleared consumer state.");

    Assert.False(
        afterA.Succeeded);

    TestLog.Pass(
        "Runtime A active bindings and attached state were cleared.");

    TestLog.Task(
        "Verifying Runtime B remains completely operational.");

    Assert.True(
        afterB.Succeeded);

    Assert.Equal(
        22.0,
        Convert.ToDouble(
            afterB.Value),
        10);

    Assert.Same(
        playerB,
        bindingB.Value);

    Assert.Equal(
        80.0,
        playerB.Health,
        10);

    TestLog.State(
        $"Runtime B player.Curiosity = {afterB.Value}");

    TestLog.Pass(
        "Runtime B binding and attached state survived Runtime A clear.");

    TestLog.Pass(
        "QSL runtime clear isolation certification completed successfully.");
}

[Fact]
public void
    UnbindingConsumerInOneRuntimeDoesNotRemoveSameAliasFromAnotherRuntime()
{
    TestLog.Section(
        "WILDS / QSL CONSUMER UNBIND ISOLATION");

    TestLog.Test(
        "Unbinding one consumer does not remove the same alias from another runtime.");

    var qslA =
        CreateQslEngine();

    var qslB =
        CreateQslEngine();

    var playerA =
        new TestPlayer(
            50.0);

    var playerB =
        new TestPlayer(
            80.0);

    var bindingA =
        new QslConsumerBinding(
            "player",
            "Player",
            playerA);

    var bindingB =
        new QslConsumerBinding(
            "player",
            "Player",
            playerB);

    qslA.BindConsumer(
        bindingA);

    qslB.BindConsumer(
        bindingB);

    Assert.True(
        qslA.ReceiveState(
            playerA,
            "Curiosity",
            11.0));

    Assert.True(
        qslB.ReceiveState(
            playerB,
            "Curiosity",
            22.0));

    var beforeA =
        qslA.Evaluate(
            "player.Curiosity");

    var beforeB =
        qslB.Evaluate(
            "player.Curiosity");

    Assert.True(
        beforeA.Succeeded);

    Assert.True(
        beforeB.Succeeded);

    Assert.Equal(
        11.0,
        Convert.ToDouble(
            beforeA.Value),
        10);

    Assert.Equal(
        22.0,
        Convert.ToDouble(
            beforeB.Value),
        10);

    TestLog.Task(
        "Unbinding Runtime A player.");

    Assert.True(
        qslA.UnbindConsumer(
            bindingA));

    var afterA =
        qslA.Evaluate(
            "player.Curiosity");

    var afterB =
        qslB.Evaluate(
            "player.Curiosity");

    TestLog.Task(
        "Verifying Runtime A no longer resolves the unbound consumer.");

    Assert.False(
        afterA.Succeeded);

    TestLog.Pass(
        "Runtime A consumer binding was removed.");

    TestLog.Task(
        "Verifying Runtime B still resolves its independent player alias.");

    Assert.True(
        afterB.Succeeded);

    Assert.Equal(
        22.0,
        Convert.ToDouble(
            afterB.Value),
        10);

    Assert.Same(
        playerB,
        bindingB.Value);

    Assert.Equal(
        80.0,
        playerB.Health,
        10);

    TestLog.Pass(
        "Runtime B same-name consumer remained bound.");

    TestLog.Task(
        "Verifying Runtime A unbind retired attached state for the exact object.");

    /*
     * Rebinding the exact object must not resurrect
     * the attached Curiosity state that UnbindConsumer
     * retired.
     */
    qslA.BindConsumer(
        bindingA);

    var reboundA =
        qslA.Evaluate(
            "player.Curiosity");

    Assert.False(
        reboundA.Succeeded);

    /*
     * Runtime B must still retain its own attached state.
     */
    var stillBoundB =
        qslB.Evaluate(
            "player.Curiosity");

    Assert.True(
        stillBoundB.Succeeded);

    Assert.Equal(
        22.0,
        Convert.ToDouble(
            stillBoundB.Value),
        10);

    TestLog.Pass(
        "Unbinding retired only Runtime A attached state.");

    TestLog.Pass(
        "Runtime B attached state remained intact.");

    TestLog.Pass(
        "QSL consumer-unbind isolation certification completed successfully.");
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

        public double Amount { get; set; }
    }
}