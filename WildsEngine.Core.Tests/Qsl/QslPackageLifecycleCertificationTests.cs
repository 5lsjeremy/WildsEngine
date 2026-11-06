using System;
using System.Collections.Generic;
using QSL.Core;
using QSL.Core.Runtime;
using QSL.Core.Runtime.Context;
using QSL.Core.Runtime.Packages;
using WildsEngine.Tests.Logging;
using Xunit;

namespace WildsEngine.Core.Tests;

public sealed class QslPackageLifecycleCertificationTests
{
    [Fact]
    public void MultipleConsumersAndPackagesRemainTargetIsolated()
    {
        TestLog.Section(
            "QSL PACKAGE LIFECYCLE - MULTIPLE TARGETS");

        TestLog.Test(
            "Multiple consumers receive packages without cross-target contamination.");

        var engine = CreateQslEngine();

        var player = new TestPlayer(
            100.0);

        var action = new TestAction(
            20.0);

        var playerBinding = new QslConsumerBinding(
            "player",
            "Player",
            player);

        var actionBinding = new QslConsumerBinding(
            "action",
            "Action",
            action);

        engine.BindConsumer(
            playerBinding);

        engine.BindConsumer(
            actionBinding);

        var playerPackage = new QslStatePackage(
            player,
            new Dictionary<string, object?>
            {
                {
                    "Curiosity",
                    11.0
                },
                {
                    "Fatigue",
                    4.0
                }
            });

        var actionPackage = new QslStatePackage(
            action,
            new Dictionary<string, object?>
            {
                {
                    "SpaceDustAppetite",
                    75.0
                },
                {
                    "Urgency",
                    8.0
                }
            });

        TestLog.Task(
            "Receiving packages for two exact target objects.");

        var playerReceived = engine.ReceiveState(
            playerPackage);

        var actionReceived = engine.ReceiveState(
            actionPackage);

        Assert.Equal(
            2,
            playerReceived);

        Assert.Equal(
            2,
            actionReceived);

        Assert.Same(
            player,
            playerPackage.Target);

        Assert.Same(
            action,
            actionPackage.Target);

        var playerCuriosity = engine.Evaluate(
            "player.Curiosity");

        var playerFatigue = engine.Evaluate(
            "player.Fatigue");

        var actionAppetite = engine.Evaluate(
            "action.SpaceDustAppetite");

        var actionUrgency = engine.Evaluate(
            "action.Urgency");

        AssertSucceededWithValue(
            playerCuriosity,
            11.0);

        AssertSucceededWithValue(
            playerFatigue,
            4.0);

        AssertSucceededWithValue(
            actionAppetite,
            75.0);

        AssertSucceededWithValue(
            actionUrgency,
            8.0);

        TestLog.State(
            $"player.Curiosity = {playerCuriosity.Value}");

        TestLog.State(
            $"player.Fatigue = {playerFatigue.Value}");

        TestLog.State(
            $"action.SpaceDustAppetite = {actionAppetite.Value}");

        TestLog.State(
            $"action.Urgency = {actionUrgency.Value}");

        TestLog.Pass(
            "Every package property resolved against its exact target.");

        TestLog.Pass(
            "Multiple package targets remained isolated.");
    }

    [Fact]
    public void PackageReceiptIsNonDestructiveAndDuplicatePropertiesAreRejected()
    {
        TestLog.Section(
            "QSL PACKAGE LIFECYCLE - NON-DESTRUCTIVE RECEIPT");

        TestLog.Test(
            "Receiving duplicate package properties preserves existing attached state.");

        var engine = CreateQslEngine();

        var player = new TestPlayer(
            50.0);

        var binding = new QslConsumerBinding(
            "player",
            "Player",
            player);

        engine.BindConsumer(
            binding);

        var original = new QslStatePackage(
            player,
            new Dictionary<string, object?>
            {
                {
                    "Curiosity",
                    11.0
                },
                {
                    "Fatigue",
                    4.0
                }
            });

        var conflicting = new QslStatePackage(
            player,
            new Dictionary<string, object?>
            {
                {
                    "Curiosity",
                    99.0
                },
                {
                    "Fatigue",
                    88.0
                }
            });

        var originalReceived = engine.ReceiveState(
            original);

        var conflictingReceived = engine.ReceiveState(
            conflicting);

        Assert.Equal(
            2,
            originalReceived);

        Assert.Equal(
            0,
            conflictingReceived);

        var curiosity = engine.Evaluate(
            "player.Curiosity");

        var fatigue = engine.Evaluate(
            "player.Fatigue");

        AssertSucceededWithValue(
            curiosity,
            11.0);

        AssertSucceededWithValue(
            fatigue,
            4.0);

        TestLog.State(
            $"Original package received = {originalReceived}");

        TestLog.State(
            $"Conflicting package received = {conflictingReceived}");

        TestLog.State(
            $"Preserved player.Curiosity = {curiosity.Value}");

        TestLog.State(
            $"Preserved player.Fatigue = {fatigue.Value}");

        TestLog.Pass(
            "Duplicate package receipt did not overwrite existing state.");

        TestLog.Pass(
            "First accepted package values remained authoritative.");
    }

    [Fact]
    public void UpdateStateChangesExistingStateButCannotIntroduceMissingState()
    {
        TestLog.Section(
            "QSL PACKAGE LIFECYCLE - APPROVED UPDATE");

        TestLog.Test(
            "UpdateState modifies existing attached state without introducing missing state.");

        var engine = CreateQslEngine();

        var player = new TestPlayer(
            50.0);

        engine.BindConsumer(
            new QslConsumerBinding(
                "player",
                "Player",
                player));

        Assert.True(
            engine.ReceiveState(
                player,
                "Curiosity",
                11.0));

        Assert.True(
            engine.UpdateState(
                player,
                "Curiosity",
                27.0));

        Assert.False(
            engine.UpdateState(
                player,
                "MissingState",
                91.0));

        var curiosity = engine.Evaluate(
            "player.Curiosity");

        var missing = engine.Evaluate(
            "player.MissingState");

        AssertSucceededWithValue(
            curiosity,
            27.0);

        Assert.False(
            missing.Succeeded);

        TestLog.State(
            $"Updated player.Curiosity = {curiosity.Value}");

        TestLog.Pass(
            "Existing attached state accepted an approved update.");

        TestLog.Pass(
            "UpdateState did not introduce missing vocabulary.");
    }

    [Fact]
    public void RemovingOnePropertyPreservesOtherAttachedAndNativeProperties()
    {
        TestLog.Section(
            "QSL PACKAGE LIFECYCLE - PROPERTY REMOVAL");

        TestLog.Test(
            "Removing one attached property preserves unrelated attached and native properties.");

        var engine = CreateQslEngine();

        var player = new TestPlayer(
            50.0);

        engine.BindConsumer(
            new QslConsumerBinding(
                "player",
                "Player",
                player));

        var received = engine.ReceiveState(
            new QslStatePackage(
                player,
                new Dictionary<string, object?>
                {
                    {
                        "Curiosity",
                        11.0
                    },
                    {
                        "Fatigue",
                        4.0
                    }
                }));

        Assert.Equal(
            2,
            received);

        var removedOnce = engine.RemoveState(
            player,
            "Curiosity");

        var removedTwice = engine.RemoveState(
            player,
            "Curiosity");

        Assert.True(
            removedOnce);

        Assert.False(
            removedTwice);

        var removed = engine.Evaluate(
            "player.Curiosity");

        var preservedAttached = engine.Evaluate(
            "player.Fatigue");

        var preservedNative = engine.Evaluate(
            "player.Health");

        Assert.False(
            removed.Succeeded);

        AssertSucceededWithValue(
            preservedAttached,
            4.0);

        AssertSucceededWithValue(
            preservedNative,
            50.0);

        TestLog.State(
            $"Initial package properties received = {received}");

        TestLog.State(
            $"First removal succeeded = {removedOnce}");

        TestLog.State(
            $"Repeated removal succeeded = {removedTwice}");

        TestLog.State(
            $"Removed player.Curiosity resolved = {removed.Succeeded}");

        TestLog.State(
            $"Preserved player.Fatigue = {preservedAttached.Value}");

        TestLog.State(
            $"Preserved player.Health = {preservedNative.Value}");

        TestLog.Pass(
            "The requested attached property was removed.");

        TestLog.Pass(
            "Repeated removal correctly reported that no state remained.");

        TestLog.Pass(
            "Unrelated attached and native properties remained available.");
    }
    
    
[Fact]
public void RemovingOneConsumerInvalidatesOnlyThatBinding()
{
    TestLog.Section(
        "QSL PACKAGE LIFECYCLE - CONSUMER UNBIND");

    TestLog.Test(
        "Unbinding one exact consumer retires only that binding and its attached state.");

    var engine = CreateQslEngine();

    var player = new TestPlayer(
        50.0);

    var action = new TestAction(
        20.0);

    var playerBinding = new QslConsumerBinding(
        "player",
        "Player",
        player);

    var actionBinding = new QslConsumerBinding(
        "action",
        "Action",
        action);

    engine.BindConsumer(
        playerBinding);

    engine.BindConsumer(
        actionBinding);

    Assert.True(
        engine.ReceiveState(
            player,
            "Curiosity",
            11.0));

    Assert.True(
        engine.ReceiveState(
            action,
            "Urgency",
            8.0));

    AssertSucceededWithValue(
        engine.Evaluate(
            "player.Health"),
        50.0);

    AssertSucceededWithValue(
        engine.Evaluate(
            "player.Curiosity"),
        11.0);

    AssertSucceededWithValue(
        engine.Evaluate(
            "action.Amount"),
        20.0);

    AssertSucceededWithValue(
        engine.Evaluate(
            "action.Urgency"),
        8.0);

    TestLog.Task(
        "Unbinding the exact player consumer binding.");

    var unbound = engine.UnbindConsumer(
        playerBinding);

    Assert.True(
        unbound);

    var removedPlayerNative = engine.Evaluate(
        "player.Health");

    var removedPlayerAttached = engine.Evaluate(
        "player.Curiosity");

    var preservedActionNative = engine.Evaluate(
        "action.Amount");

    var preservedActionAttached = engine.Evaluate(
        "action.Urgency");

    Assert.False(
        removedPlayerNative.Succeeded);

    Assert.False(
        removedPlayerAttached.Succeeded);

    AssertSucceededWithValue(
        preservedActionNative,
        20.0);

    AssertSucceededWithValue(
        preservedActionAttached,
        8.0);

    TestLog.State(
        $"Player binding removed = {unbound}");

    TestLog.State(
        $"player.Health resolved after unbind = {removedPlayerNative.Succeeded}");

    TestLog.State(
        $"player.Curiosity resolved after unbind = {removedPlayerAttached.Succeeded}");

    TestLog.State(
        $"Preserved action.Amount = {preservedActionNative.Value}");

    TestLog.State(
        $"Preserved action.Urgency = {preservedActionAttached.Value}");

    TestLog.Pass(
        "The exact player consumer binding was removed.");

    TestLog.Pass(
        "Attached state associated with the unbound player was retired.");

    TestLog.Pass(
        "The unrelated action consumer and its attached state remained available.");
}

[Fact]
public void ClearRemovesAllAttachedRuntimeVocabulary()
{
    TestLog.Section(
        "QSL PACKAGE LIFECYCLE - CLEAR ATTACHED STATE");

    TestLog.Test(
        "Clear removes attached runtime vocabulary from all bound consumers.");

    var engine = CreateQslEngine();

    var player = new TestPlayer(
        50.0);

    var action = new TestAction(
        20.0);

    engine.BindConsumer(
        new QslConsumerBinding(
            "player",
            "Player",
            player));

    engine.BindConsumer(
        new QslConsumerBinding(
            "action",
            "Action",
            action));

    Assert.True(
        engine.ReceiveState(
            player,
            "Curiosity",
            11.0));

    Assert.True(
        engine.ReceiveState(
            action,
            "Urgency",
            8.0));

    AssertSucceededWithValue(
        engine.Evaluate(
            "player.Curiosity"),
        11.0);

    AssertSucceededWithValue(
        engine.Evaluate(
            "action.Urgency"),
        8.0);

    TestLog.Task(
        "Clearing QSL runtime state.");

    engine.Clear();

    var playerAttached = engine.Evaluate(
        "player.Curiosity");

    var actionAttached = engine.Evaluate(
        "action.Urgency");

    Assert.False(
        playerAttached.Succeeded);

    Assert.False(
        actionAttached.Succeeded);

    TestLog.State(
        $"player.Curiosity resolved after clear = {playerAttached.Succeeded}");

    TestLog.State(
        $"action.Urgency resolved after clear = {actionAttached.Succeeded}");

    TestLog.Pass(
        "Clear removed attached runtime vocabulary.");
}

[Fact]
public void ClearDoesNotDamageNativeConsumerOwnedProperties()
{
    TestLog.Section(
        "QSL PACKAGE LIFECYCLE - CLEAR PRESERVES CONSUMER");

    TestLog.Test(
        "Clear removes QSL runtime state without mutating native consumer-owned properties.");

    var engine = CreateQslEngine();

    var player = new TestPlayer(
        50.0);

    engine.BindConsumer(
        new QslConsumerBinding(
            "player",
            "Player",
            player));

    Assert.True(
        engine.ReceiveState(
            player,
            "Curiosity",
            11.0));

    AssertSucceededWithValue(
        engine.Evaluate(
            "player.Health"),
        50.0);

    AssertSucceededWithValue(
        engine.Evaluate(
            "player.Curiosity"),
        11.0);

    TestLog.Task(
        "Clearing QSL runtime state.");

    engine.Clear();

    Assert.Equal(
        50.0,
        player.Health,
        10);

    TestLog.State(
        $"Consumer-owned player.Health after clear = {player.Health}");

    TestLog.Pass(
        "Clear did not mutate the native consumer-owned object.");
}

[Fact]
public void ResolvedZeroValueRemovalPackageRemovesWatchedAttachedState()
{
    TestLog.Section(
        "QSL PACKAGE LIFECYCLE - RESOLVED REMOVAL PACKAGE");

    TestLog.Test(
        "A resolved zero-value decay package removes watched attached state without affecting unrelated state.");

    var engine = CreateQslEngine();

    var player = new TestPlayer(
        50.0);

    engine.BindConsumer(
        new QslConsumerBinding(
            "player",
            "Player",
            player));

    Assert.Equal(
        2,
        engine.ReceiveState(
            new QslStatePackage(
                player,
                new Dictionary<string, object?>
                {
                    {
                        "Curiosity",
                        11.0
                    },
                    {
                        "Fatigue",
                        4.0
                    }
                })));

    AssertSucceededWithValue(
        engine.Evaluate(
            "player.Curiosity"),
        11.0);

    AssertSucceededWithValue(
        engine.Evaluate(
            "player.Fatigue"),
        4.0);

    TestLog.Task(
        "Applying a Wilds-resolved zero-value decay package with removal enabled.");

    var decayPackage = new QslDecayPackage(
        player,
        new Dictionary<string, object?>
        {
            {
                "Curiosity",
                0.0
            }
        },
        removeAtZero: true);

    var affected = engine.ReceiveDecay(
        decayPackage);

    Assert.Equal(
        1,
        affected);

    var removed = engine.Evaluate(
        "player.Curiosity");

    var preservedAttached = engine.Evaluate(
        "player.Fatigue");

    var preservedNative = engine.Evaluate(
        "player.Health");

    Assert.False(
        removed.Succeeded);

    AssertSucceededWithValue(
        preservedAttached,
        4.0);

    AssertSucceededWithValue(
        preservedNative,
        50.0);

    TestLog.State(
        $"Decay properties affected = {affected}");

    TestLog.State(
        $"player.Curiosity resolved after decay = {removed.Succeeded}");

    TestLog.State(
        $"Preserved player.Fatigue = {preservedAttached.Value}");

    TestLog.State(
        $"Preserved player.Health = {preservedNative.Value}");

    TestLog.Pass(
        "The resolved zero-value decay package retired the watched attached property.");

    TestLog.Pass(
        "The decay package preserved unrelated attached state.");

    TestLog.Pass(
        "The decay package preserved native consumer-owned state.");
}

    private static QslEngine CreateQslEngine()
    {
        var registry = new QslRuntimeRegistry();

        registry.RegisterDefaultEvaluators();

        var context = new QslRuntimeContext(
            registry);

        return new QslEngine(
            context);
    }

    private static void AssertSucceededWithValue(
        QslProcessingResult result,
        double expected)
    {
        Assert.NotNull(
            result);

        Assert.True(
            result.Succeeded);

        Assert.False(
            result.ParseFailed);

        Assert.False(
            result.EvaluationFailed);

        Assert.False(
            result.HasException);

        Assert.Empty(
            result.ParserDiagnostics);

        Assert.Empty(
            result.RuntimeDiagnostics);

        Assert.Null(
            result.Exception);

        Assert.NotNull(
            result.Value);

        Assert.Equal(
            expected,
            Convert.ToDouble(
                result.Value),
            10);
    }

    private sealed class TestPlayer
    {
        public TestPlayer(
            double health)
        {
            Health = health;
        }

        public double Health { get; set; }
    }

    private sealed class TestAction
    {
        public TestAction(
            double amount)
        {
            Amount = amount;
        }

        public double Amount { get; set; }
    }
}