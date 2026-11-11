using System;
using QSL.Core;
using QSL.Core.Runtime;
using QSL.Core.Runtime.Context;
using WildsEngine.Core.Entities.Abstractions;
using WildsEngine.Qsl;
using WildsEngine.Tests.Logging;
using Xunit;

namespace WildsEngine.Core.Tests.Entities;

/// <summary>
/// Certifies the live Entity -> QSL state lifecycle.
///
/// Entity Surface
///     -> exact QSL consumer binding
///
/// available Discovery
///     -> exact Entity-owned Fact
///     -> existing QSL ReceiveState
///
/// updated Fact
///     -> existing QSL UpdateState
///
/// removed exposure
///     -> existing QSL RemoveState
///
/// retired Surface
///     -> existing QSL UnbindConsumer
///
/// EntityQslStateAdapter owns no attached-state store.
/// QSL remains authoritative for runtime vocabulary.
/// </summary>
public sealed class EntityQslStateLifecycleCertificationTests
{
    [Fact]
    public void
        Binding_surface_preserves_exact_consumer_target()
    {
        TestLog.Section(
            "ENTITY GATE E6-A - SURFACE BINDING");

        TestLog.Test(
            "Binding an Entity Surface preserves its exact " +
            "consumer-owned runtime target.");

        var engine =
            CreateQslEngine();

        var action =
            new TestAction(
                10.0);

        var entity =
            new TestEntity(
                "action.entity");

        var surface =
            new TestEngineSurface(
                entity,
                action,
                "action",
                "Action");

        var adapter =
            new EntityQslStateAdapter(
                engine);

        TestLog.Task(
            "Binding the exact Action Surface.");

        var binding =
            adapter.Bind(
                surface);

        Assert.True(
            adapter.IsBound(
                surface));

        Assert.True(
            adapter.TryGetBinding(
                surface,
                out var retained));

        Assert.Same(
            binding,
            retained);

        Assert.Same(
            action,
            binding.Value);

        Assert.Same(
            action,
            surface.Target);

        Assert.Same(
            entity,
            surface.Entity);

        var amount =
            engine.Evaluate(
                "action.Amount");

        AssertSucceededWithValue(
            amount,
            10.0);

        TestLog.State(
            $"Surface alias = {surface.Alias}");

        TestLog.State(
            $"Surface consumer type = " +
            $"{surface.ConsumerType}");

        TestLog.State(
            $"Exact binding retained = " +
            $"{ReferenceEquals(
                binding,
                retained)}");

        TestLog.State(
            $"Exact Action target retained = " +
            $"{ReferenceEquals(
                action,
                binding.Value)}");

        TestLog.State(
            $"Native action.Amount = {amount.Value}");

        TestLog.Pass(
            "Entity Surface bound the exact consumer target.");
    }

    [Fact]
    public void
        Available_discovery_adds_live_qsl_state()
    {
        TestLog.Section(
            "ENTITY GATE E6-B - STATE ADD");

        TestLog.Test(
            "An available Discovery adds its exact Fact " +
            "to live QSL attached state.");

        var engine =
            CreateQslEngine();

        var action =
            new TestAction(
                10.0);

        var entity =
            new TestEntity(
                "action.entity");

        var fact =
            new TestFact(
                "action.space-dust-appetite",
                entity,
                "SpaceDustAppetite",
                87.0);

        var discovery =
            new TestDiscovery(
                "action.space-dust-appetite.available",
                fact,
                isAvailable: true);

        Assert.True(
            entity.RegisterFact(
                fact));

        Assert.True(
            fact.RegisterDiscovery(
                discovery));

        var surface =
            new TestEngineSurface(
                entity,
                action,
                "action",
                "Action");

        var adapter =
            new EntityQslStateAdapter(
                engine);

        adapter.Bind(
            surface);

        TestLog.State(
            $"Fact = {fact.Path} / {fact.Value}");

        TestLog.State(
            $"Discovery available = " +
            $"{discovery.IsAvailable}");

        TestLog.Task(
            "Receiving Entity Fact into live QSL state.");

        var received =
            adapter.Receive(
                surface,
                discovery);

        Assert.True(
            received);

        var result =
            engine.Evaluate(
                "action.SpaceDustAppetite");

        AssertSucceededWithValue(
            result,
            87.0);

        Assert.Same(
            fact,
            discovery.Fact);

        Assert.Same(
            entity,
            discovery.Fact.Entity);

        Assert.Same(
            action,
            surface.Target);

        TestLog.State(
            $"State received = {received}");

        TestLog.State(
            $"action.SpaceDustAppetite = " +
            $"{result.Value}");

        TestLog.State(
            $"Exact Discovery provenance retained = " +
            $"{ReferenceEquals(
                entity,
                discovery.Fact.Entity)}");

        TestLog.Pass(
            "Available Discovery added live QSL vocabulary.");
    }

    [Fact]
    public void
        Unavailable_discovery_cannot_add_live_qsl_state()
    {
        TestLog.Section(
            "ENTITY GATE E6-C - UNAVAILABLE STATE REJECTION");

        TestLog.Test(
            "An unavailable Discovery cannot add live " +
            "QSL state.");

        var engine =
            CreateQslEngine();

        var action =
            new TestAction(
                10.0);

        var entity =
            new TestEntity(
                "action.entity");

        var fact =
            new TestFact(
                "action.space-dust-appetite",
                entity,
                "SpaceDustAppetite",
                87.0);

        var discovery =
            new TestDiscovery(
                "action.space-dust-appetite.private",
                fact,
                isAvailable: false);

        Assert.True(
            entity.RegisterFact(
                fact));

        Assert.True(
            fact.RegisterDiscovery(
                discovery));

        var surface =
            new TestEngineSurface(
                entity,
                action,
                "action",
                "Action");

        var adapter =
            new EntityQslStateAdapter(
                engine);

        adapter.Bind(
            surface);

        TestLog.State(
            $"Discovery available = " +
            $"{discovery.IsAvailable}");

        TestLog.Task(
            "Attempting to receive unavailable knowledge.");

        TestLog.Expected(
            "InvalidOperationException");

        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    adapter.Receive(
                        surface,
                        discovery));

        var unresolved =
            engine.Evaluate(
                "action.SpaceDustAppetite");

        Assert.False(
            unresolved.Succeeded);

        Assert.Single(
            entity.Facts);

        Assert.Single(
            fact.Discoveries);

        Assert.Same(
            fact,
            entity.Facts[0]);

        Assert.Same(
            discovery,
            fact.Discoveries[0]);

        TestLog.State(
            $"Rejected with = " +
            $"{exception.GetType().Name}");

        TestLog.State(
            $"QSL property resolved = " +
            $"{unresolved.Succeeded}");

        TestLog.Pass(
            "Unavailable Discovery remained private.");
    }

    [Fact]
    public void
        Updating_fact_updates_existing_live_qsl_state()
    {
        TestLog.Section(
            "ENTITY GATE E6-D - STATE UPDATE");

        TestLog.Test(
            "Updating the authoritative Fact updates " +
            "existing live QSL state.");

        var engine =
            CreateQslEngine();

        var action =
            new TestAction(
                10.0);

        var entity =
            new TestEntity(
                "action.entity");

        var fact =
            new TestFact(
                "action.space-dust-appetite",
                entity,
                "SpaceDustAppetite",
                87.0);

        var discovery =
            new TestDiscovery(
                "action.space-dust-appetite.available",
                fact,
                isAvailable: true);

        Assert.True(
            entity.RegisterFact(
                fact));

        Assert.True(
            fact.RegisterDiscovery(
                discovery));

        var surface =
            new TestEngineSurface(
                entity,
                action,
                "action",
                "Action");

        var adapter =
            new EntityQslStateAdapter(
                engine);

        adapter.Bind(
            surface);

        Assert.True(
            adapter.Receive(
                surface,
                discovery));

        var before =
            engine.Evaluate(
                "action.SpaceDustAppetite");

        AssertSucceededWithValue(
            before,
            87.0);

        TestLog.State(
            $"Attached state before update = " +
            $"{before.Value}");

        TestLog.Task(
            "Updating authoritative Fact from 87 to 35.");

        fact.UpdateValue(
            35.0);

        var updated =
            adapter.Update(
                surface,
                discovery);

        Assert.True(
            updated);

        var after =
            engine.Evaluate(
                "action.SpaceDustAppetite");

        AssertSucceededWithValue(
            after,
            35.0);

        Assert.Same(
            fact,
            entity.Facts[0]);

        Assert.Same(
            discovery,
            fact.Discoveries[0]);

        TestLog.State(
            $"Update succeeded = {updated}");

        TestLog.State(
            $"Authoritative Fact value = {fact.Value}");

        TestLog.State(
            $"Attached state after update = " +
            $"{after.Value}");

        TestLog.State(
            $"Same Fact reference retained = " +
            $"{ReferenceEquals(
                fact,
                entity.Facts[0])}");

        TestLog.Pass(
            "Existing live QSL state followed the " +
            "authoritative Fact value.");

        TestLog.Pass(
            "No duplicate Entity state store was introduced.");
    }

    [Fact]
    public void
        Update_cannot_introduce_missing_runtime_vocabulary()
    {
        TestLog.Section(
            "ENTITY GATE E6-E - UPDATE CANNOT ADD");

        TestLog.Test(
            "Update delegates to the existing QSL rule " +
            "and cannot introduce missing vocabulary.");

        var engine =
            CreateQslEngine();

        var action =
            new TestAction(
                10.0);

        var entity =
            new TestEntity(
                "action.entity");

        var fact =
            new TestFact(
                "action.urgency",
                entity,
                "Urgency",
                8.0);

        var discovery =
            new TestDiscovery(
                "action.urgency.available",
                fact,
                isAvailable: true);

        Assert.True(
            entity.RegisterFact(
                fact));

        Assert.True(
            fact.RegisterDiscovery(
                discovery));

        var surface =
            new TestEngineSurface(
                entity,
                action,
                "action",
                "Action");

        var adapter =
            new EntityQslStateAdapter(
                engine);

        adapter.Bind(
            surface);

        TestLog.Task(
            "Attempting Update before Receive.");

        var updated =
            adapter.Update(
                surface,
                discovery);

        Assert.False(
            updated);

        var unresolved =
            engine.Evaluate(
                "action.Urgency");

        Assert.False(
            unresolved.Succeeded);

        Assert.Single(
            entity.Facts);

        Assert.Single(
            fact.Discoveries);

        TestLog.State(
            $"Update succeeded = {updated}");

        TestLog.State(
            $"action.Urgency resolved = " +
            $"{unresolved.Succeeded}");

        TestLog.Pass(
            "Update did not introduce missing vocabulary.");
    }

    [Fact]
    public void
        Removing_attached_state_preserves_entity_knowledge()
    {
        TestLog.Section(
            "ENTITY GATE E6-F - STATE REMOVE");

        TestLog.Test(
            "Removing live QSL state preserves the Entity, " +
            "Fact, Discovery, and native consumer.");

        var engine =
            CreateQslEngine();

        var action =
            new TestAction(
                10.0);

        var entity =
            new TestEntity(
                "action.entity");

        var fact =
            new TestFact(
                "action.space-dust-appetite",
                entity,
                "SpaceDustAppetite",
                87.0);

        var discovery =
            new TestDiscovery(
                "action.space-dust-appetite.available",
                fact,
                isAvailable: true);

        Assert.True(
            entity.RegisterFact(
                fact));

        Assert.True(
            fact.RegisterDiscovery(
                discovery));

        var surface =
            new TestEngineSurface(
                entity,
                action,
                "action",
                "Action");

        var adapter =
            new EntityQslStateAdapter(
                engine);

        adapter.Bind(
            surface);

        Assert.True(
            adapter.Receive(
                surface,
                discovery));

        AssertSucceededWithValue(
            engine.Evaluate(
                "action.SpaceDustAppetite"),
            87.0);

        TestLog.Task(
            "Removing attached state through the bridge.");

        var removed =
            adapter.Remove(
                surface,
                discovery);

        Assert.True(
            removed);

        var removedResult =
            engine.Evaluate(
                "action.SpaceDustAppetite");

        var nativeResult =
            engine.Evaluate(
                "action.Amount");

        Assert.False(
            removedResult.Succeeded);

        AssertSucceededWithValue(
            nativeResult,
            10.0);

        Assert.Single(
            entity.Facts);

        Assert.Single(
            fact.Discoveries);

        Assert.Same(
            fact,
            entity.Facts[0]);

        Assert.Same(
            discovery,
            fact.Discoveries[0]);

        Assert.Same(
            fact,
            discovery.Fact);

        Assert.Equal(
            87.0,
            Assert.IsType<double>(
                fact.Value));

        Assert.Equal(
            10.0,
            action.Amount,
            10);

        TestLog.State(
            $"Removal succeeded = {removed}");

        TestLog.State(
            $"Attached state resolved after removal = " +
            $"{removedResult.Succeeded}");

        TestLog.State(
            $"Native action.Amount = {nativeResult.Value}");

        TestLog.State(
            $"Entity Fact still equals = {fact.Value}");

        TestLog.State(
            $"Discovery provenance survived = " +
            $"{ReferenceEquals(
                entity,
                discovery.Fact.Entity)}");

        TestLog.Pass(
            "Runtime exposure was removed.");

        TestLog.Pass(
            "Entity knowledge and native state remained intact.");
    }

    [Fact]
    public void
        Removing_one_property_preserves_other_attached_and_native_state()
    {
        TestLog.Section(
            "ENTITY GATE E6-G - SELECTIVE STATE REMOVE");

        TestLog.Test(
            "Removing one attached property preserves " +
            "other attached and native properties.");

        var engine =
            CreateQslEngine();

        var action =
            new TestAction(
                10.0);

        var entity =
            new TestEntity(
                "action.entity");

        var appetite =
            new TestFact(
                "action.space-dust-appetite",
                entity,
                "SpaceDustAppetite",
                87.0);

        var urgency =
            new TestFact(
                "action.urgency",
                entity,
                "Urgency",
                8.0);

        var appetiteDiscovery =
            new TestDiscovery(
                "action.space-dust-appetite.available",
                appetite,
                isAvailable: true);

        var urgencyDiscovery =
            new TestDiscovery(
                "action.urgency.available",
                urgency,
                isAvailable: true);

        Assert.True(
            entity.RegisterFact(
                appetite));

        Assert.True(
            entity.RegisterFact(
                urgency));

        Assert.True(
            appetite.RegisterDiscovery(
                appetiteDiscovery));

        Assert.True(
            urgency.RegisterDiscovery(
                urgencyDiscovery));

        var surface =
            new TestEngineSurface(
                entity,
                action,
                "action",
                "Action");

        var adapter =
            new EntityQslStateAdapter(
                engine);

        adapter.Bind(
            surface);

        Assert.True(
            adapter.Receive(
                surface,
                appetiteDiscovery));

        Assert.True(
            adapter.Receive(
                surface,
                urgencyDiscovery));

        TestLog.Task(
            "Removing SpaceDustAppetite only.");

        Assert.True(
            adapter.Remove(
                surface,
                appetiteDiscovery));

        var removed =
            engine.Evaluate(
                "action.SpaceDustAppetite");

        var preservedAttached =
            engine.Evaluate(
                "action.Urgency");

        var preservedNative =
            engine.Evaluate(
                "action.Amount");

        Assert.False(
            removed.Succeeded);

        AssertSucceededWithValue(
            preservedAttached,
            8.0);

        AssertSucceededWithValue(
            preservedNative,
            10.0);

        Assert.Equal(
            2,
            entity.Facts.Count);

        Assert.Same(
            appetite,
            entity.Facts[0]);

        Assert.Same(
            urgency,
            entity.Facts[1]);

        TestLog.State(
            $"SpaceDustAppetite resolved = " +
            $"{removed.Succeeded}");

        TestLog.State(
            $"Preserved action.Urgency = " +
            $"{preservedAttached.Value}");

        TestLog.State(
            $"Preserved action.Amount = " +
            $"{preservedNative.Value}");

        TestLog.Pass(
            "One live property was removed.");

        TestLog.Pass(
            "Unrelated attached and native state survived.");
    }

    [Fact]
    public void
        Removing_all_attached_state_preserves_native_consumer_state()
    {
        TestLog.Section(
            "ENTITY GATE E6-H - REMOVE ALL STATE");

        TestLog.Test(
            "Removing all attached state preserves native " +
            "consumer-owned properties and Entity knowledge.");

        var engine =
            CreateQslEngine();

        var action =
            new TestAction(
                10.0);

        var entity =
            new TestEntity(
                "action.entity");

        var appetite =
            new TestFact(
                "action.space-dust-appetite",
                entity,
                "SpaceDustAppetite",
                87.0);

        var urgency =
            new TestFact(
                "action.urgency",
                entity,
                "Urgency",
                8.0);

        var appetiteDiscovery =
            new TestDiscovery(
                "action.space-dust-appetite.available",
                appetite,
                isAvailable: true);

        var urgencyDiscovery =
            new TestDiscovery(
                "action.urgency.available",
                urgency,
                isAvailable: true);

        Assert.True(
            entity.RegisterFact(
                appetite));

        Assert.True(
            entity.RegisterFact(
                urgency));

        Assert.True(
            appetite.RegisterDiscovery(
                appetiteDiscovery));

        Assert.True(
            urgency.RegisterDiscovery(
                urgencyDiscovery));

        var surface =
            new TestEngineSurface(
                entity,
                action,
                "action",
                "Action");

        var adapter =
            new EntityQslStateAdapter(
                engine);

        adapter.Bind(
            surface);

        Assert.True(
            adapter.Receive(
                surface,
                appetiteDiscovery));

        Assert.True(
            adapter.Receive(
                surface,
                urgencyDiscovery));

        TestLog.Task(
            "Removing all attached state for Action Surface.");

        var removed =
            adapter.Remove(
                surface);

        Assert.True(
            removed);

        var appetiteResult =
            engine.Evaluate(
                "action.SpaceDustAppetite");

        var urgencyResult =
            engine.Evaluate(
                "action.Urgency");

        var nativeResult =
            engine.Evaluate(
                "action.Amount");

        Assert.False(
            appetiteResult.Succeeded);

        Assert.False(
            urgencyResult.Succeeded);

        AssertSucceededWithValue(
            nativeResult,
            10.0);

        Assert.Equal(
            10.0,
            action.Amount,
            10);

        Assert.Equal(
            2,
            entity.Facts.Count);

        TestLog.State(
            $"Remove all succeeded = {removed}");

        TestLog.State(
            $"SpaceDustAppetite resolved = " +
            $"{appetiteResult.Succeeded}");

        TestLog.State(
            $"Urgency resolved = " +
            $"{urgencyResult.Succeeded}");

        TestLog.State(
            $"Native action.Amount = {nativeResult.Value}");

        TestLog.State(
            $"Entity Fact count = {entity.Facts.Count}");

        TestLog.Pass(
            "All attached runtime vocabulary was removed.");

        TestLog.Pass(
            "Native consumer and Entity knowledge survived.");
    }

    [Fact]
    public void
        Unbinding_one_surface_preserves_another_surface()
    {
        TestLog.Section(
            "ENTITY GATE E6-I - SURFACE UNBIND ISOLATION");

        TestLog.Test(
            "Unbinding one Entity Surface retires only its " +
            "exact QSL binding and attached state.");

        var engine =
            CreateQslEngine();

        var action =
            new TestAction(
                10.0);

        var mara =
            new TestMara(
                100.0);

        var actionEntity =
            new TestEntity(
                "action.entity");

        var maraEntity =
            new TestEntity(
                "mara.entity");

        var appetite =
            new TestFact(
                "action.space-dust-appetite",
                actionEntity,
                "SpaceDustAppetite",
                87.0);

        var curiosity =
            new TestFact(
                "mara.curiosity",
                maraEntity,
                "Curiosity",
                11.0);

        var appetiteDiscovery =
            new TestDiscovery(
                "action.space-dust-appetite.available",
                appetite,
                isAvailable: true);

        var curiosityDiscovery =
            new TestDiscovery(
                "mara.curiosity.available",
                curiosity,
                isAvailable: true);

        Assert.True(
            actionEntity.RegisterFact(
                appetite));

        Assert.True(
            maraEntity.RegisterFact(
                curiosity));

        Assert.True(
            appetite.RegisterDiscovery(
                appetiteDiscovery));

        Assert.True(
            curiosity.RegisterDiscovery(
                curiosityDiscovery));

        var actionSurface =
            new TestEngineSurface(
                actionEntity,
                action,
                "action",
                "Action");

        var maraSurface =
            new TestEngineSurface(
                maraEntity,
                mara,
                "mara",
                "Mara");

        var adapter =
            new EntityQslStateAdapter(
                engine);

        adapter.Bind(
            actionSurface);

        adapter.Bind(
            maraSurface);

        Assert.True(
            adapter.Receive(
                actionSurface,
                appetiteDiscovery));

        Assert.True(
            adapter.Receive(
                maraSurface,
                curiosityDiscovery));

        AssertSucceededWithValue(
            engine.Evaluate(
                "action.SpaceDustAppetite"),
            87.0);

        AssertSucceededWithValue(
            engine.Evaluate(
                "mara.Curiosity"),
            11.0);

        TestLog.Task(
            "Unbinding the exact Action Surface.");

        var unbound =
            adapter.Unbind(
                actionSurface);

        Assert.True(
            unbound);

        Assert.False(
            adapter.IsBound(
                actionSurface));

        Assert.True(
            adapter.IsBound(
                maraSurface));

        var actionNative =
            engine.Evaluate(
                "action.Amount");

        var actionAttached =
            engine.Evaluate(
                "action.SpaceDustAppetite");

        var maraNative =
            engine.Evaluate(
                "mara.Health");

        var maraAttached =
            engine.Evaluate(
                "mara.Curiosity");

        Assert.False(
            actionNative.Succeeded);

        Assert.False(
            actionAttached.Succeeded);

        AssertSucceededWithValue(
            maraNative,
            100.0);

        AssertSucceededWithValue(
            maraAttached,
            11.0);

        TestLog.State(
            $"Action Surface unbound = {unbound}");

        TestLog.State(
            $"Action native state resolved = " +
            $"{actionNative.Succeeded}");

        TestLog.State(
            $"Action attached state resolved = " +
            $"{actionAttached.Succeeded}");

        TestLog.State(
            $"Mara native Health = {maraNative.Value}");

        TestLog.State(
            $"Mara attached Curiosity = " +
            $"{maraAttached.Value}");

        TestLog.Pass(
            "Action Surface and its live state were retired.");

        TestLog.Pass(
            "Mara Surface remained completely operational.");
    }

    [Fact]
    public void
        Foreign_discovery_cannot_enter_surface_qsl_state()
    {
        TestLog.Section(
            "ENTITY GATE E6-J - FOREIGN DISCOVERY REJECTION");

        TestLog.Test(
            "A Discovery from another Entity cannot attach " +
            "state to the Surface target.");

        var engine =
            CreateQslEngine();

        var machine =
            new TestMachine();

        var machineEntity =
            new TestEntity(
                "machine.entity");

        var foreignEntity =
            new TestEntity(
                "foreign.entity");

        var foreignFact =
            new TestFact(
                "foreign.radiation",
                foreignEntity,
                "Radiation",
                99.0);

        var foreignDiscovery =
            new TestDiscovery(
                "foreign.radiation.available",
                foreignFact,
                isAvailable: true);

        Assert.True(
            foreignEntity.RegisterFact(
                foreignFact));

        Assert.True(
            foreignFact.RegisterDiscovery(
                foreignDiscovery));

        var machineSurface =
            new TestEngineSurface(
                machineEntity,
                machine,
                "machine",
                "Machine");

        var adapter =
            new EntityQslStateAdapter(
                engine);

        adapter.Bind(
            machineSurface);

        TestLog.Task(
            "Attempting to receive foreign Discovery state.");

        TestLog.Expected(
            "ArgumentException");

        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    adapter.Receive(
                        machineSurface,
                        foreignDiscovery));

        var unresolved =
            engine.Evaluate(
                "machine.Radiation");

        Assert.False(
            unresolved.Succeeded);

        Assert.Empty(
            machineEntity.Facts);

        Assert.Single(
            foreignEntity.Facts);

        Assert.Single(
            foreignFact.Discoveries);

        Assert.Same(
            foreignFact,
            foreignEntity.Facts[0]);

        Assert.Same(
            foreignDiscovery,
            foreignFact.Discoveries[0]);

        TestLog.State(
            $"Rejected with = " +
            $"{exception.GetType().Name}");

        TestLog.State(
            $"machine.Radiation resolved = " +
            $"{unresolved.Succeeded}");

        TestLog.State(
            $"Machine Entity Fact count = " +
            $"{machineEntity.Facts.Count}");

        TestLog.State(
            $"Foreign Entity Fact count = " +
            $"{foreignEntity.Facts.Count}");

        TestLog.Pass(
            "Foreign Discovery was rejected.");

        TestLog.Pass(
            "ENTITY GATE E6 CERTIFIED.");
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

    private sealed class TestMara
    {
        public TestMara(
            double health)
        {
            Health =
                health;
        }

        public double Health { get; set; }
    }

    private sealed class TestMachine
    {
    }
}