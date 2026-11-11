using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QSL.Core;
using QSL.Core.Runtime;
using QSL.Core.Runtime.Context;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Emission;
using WildsEngine.Core.Entities;
using WildsEngine.Core.Entities.Abstractions;
using WildsEngine.Core.Tests.Fixtures;
using WildsEngine.Qsl;
using WildsEngine.Tests.Logging;
using WildsEngine.Tests.Registry;
using Xunit;

namespace WildsEngine.Core.Tests.Entities;

/// <summary>
/// Certifies the first composed Entity interaction.
///
/// Machine Entity
///     -> owns Radiation Fact
///     -> owns available Discovery
///     -> EntityEmissionDataSource
///     -> existing Emitter
///     -> resolved consumer value
///
/// Consumer/application composition
///     -> places resolved value on exact Machine runtime object
///
/// Machine + Mara Surfaces
///     -> EntityQslRequestAdapter
///     -> WildsQslRequest
///     -> existing Wilds/QSL lifecycle
///     -> exact Mara runtime mutation
///     -> Moment
///     -> Timeline
///     -> Epoch
///
/// No Entity-specific behavior is added to Wilds or QSL.
/// </summary>
public sealed class
    EntityInteractionCompositionCertificationTests
{
    [Fact]
    public async Task
        Machine_discovery_emits_and_mara_consumes_through_existing_boundaries()
    {
        TestLog.Section(
            "ENTITY GATE E5 - MACHINE TO MARA COMPOSITION");

        TestLog.Test(
            "One Entity exposes discovered knowledge through " +
            "the existing emitter and another consumes the " +
            "resolved value through the existing Wilds/QSL boundary.");

        /*
         * Exact consumer-owned runtime objects.
         *
         * Neither object implements Entity, emitter,
         * Wilds, or QSL contracts.
         */

        var machine =
            new TestMachine();

        var mara =
            new TestMara(
                100.0);

        TestLog.State(
            $"Machine runtime Radiation before interaction = " +
            $"{machine.Radiation:R}");

        TestLog.State(
            $"Mara native Health before interaction = " +
            $"{mara.Health:R}");

        /*
         * Independent Entity identities.
         */

        var machineEntity =
            new TestEntity(
                "machine.entity");

        var maraEntity =
            new TestEntity(
                "mara.entity");

        Assert.NotEqual(
            machineEntity.Id,
            maraEntity.Id);

        TestLog.State(
            $"Machine Entity = " +
            $"{machineEntity.Slug} / {machineEntity.Id}");

        TestLog.State(
            $"Mara Entity = " +
            $"{maraEntity.Slug} / {maraEntity.Id}");

        /*
         * Machine knows one authoritative Fact.
         */

        var radiation =
            new TestFact(
                "machine.radiation",
                machineEntity,
                "Radiation",
                20.0);

        Assert.True(
            machineEntity.RegisterFact(
                radiation));

        Assert.Single(
            machineEntity.Facts);

        Assert.Empty(
            maraEntity.Facts);

        Assert.Same(
            radiation,
            machineEntity.Facts[0]);

        TestLog.State(
            $"Machine Fact = " +
            $"{radiation.Path} / {radiation.Value}");

        TestLog.Pass(
            "Machine owns authoritative Radiation knowledge.");

        /*
         * Machine has discovered that this Fact is
         * available for controlled exposure.
         */

        var radiationDiscovery =
            new TestDiscovery(
                "machine.radiation.observable",
                radiation,
                isAvailable: true);

        Assert.True(
            radiation.RegisterDiscovery(
                radiationDiscovery));

        Assert.Single(
            radiation.Discoveries);

        Assert.True(
            radiationDiscovery.IsAvailable);

        Assert.Same(
            radiation,
            radiationDiscovery.Fact);

        Assert.Same(
            machineEntity,
            radiationDiscovery.Fact.Entity);

        TestLog.State(
            $"Radiation Discovery = " +
            $"{radiationDiscovery.Slug}");

        TestLog.State(
            $"Discovery available = " +
            $"{radiationDiscovery.IsAvailable}");

        TestLog.State(
            $"Discovery -> exact Fact = " +
            $"{ReferenceEquals(
                radiation,
                radiationDiscovery.Fact)}");

        /*
         * E4-style authorization creates the existing
         * emission projection.
         */

        var emissionDataSource =
            new EntityEmissionDataSource(
                machineEntity);

        TestLog.Task(
            "Authorizing Machine Radiation for emission.");

        Assert.True(
            ExposePropertyIfAvailable(
                emissionDataSource,
                radiationDiscovery,
                "Radiation"));

        /*
         * Nothing new is introduced here.
         *
         * Existing EmitterBase + EmissionScalar take over.
         */

        var emitterComponent =
            new TestComponent(
                "entity.e5.machine-emitter");

        var emitter =
            new TestEmitter(
                emitterComponent,
                "entity.e5.machine-radiation",
                emissionDataSource,
                new EmissionScalar());

        TestLog.Section(
            "ENTITY GATE E5-A - EXISTING EMITTER");

        TestLog.Task(
            "Resolving Machine Radiation through existing emitter.");

        var emitted =
            emitter.TryEmit(
                "Radiation",
                out var resolvedRadiation);

        Assert.True(
            emitted);

        Assert.Equal(
            20.0,
            resolvedRadiation,
            10);

        TestLog.State(
            $"Emission succeeded = {emitted}");

        TestLog.State(
            $"Resolved Radiation = " +
            $"{resolvedRadiation:R}");

        TestLog.Pass(
            "Existing emitter resolved Machine knowledge.");

        /*
         * This assignment is deliberately application-side
         * composition.
         *
         * Entity does not decide what the result means.
         * Wilds does not decide what the result means.
         *
         * The consumer chooses to make the resolved emission
         * available on the exact Machine runtime object used
         * for the subsequent QSL interaction.
         */

        TestLog.Section(
            "ENTITY GATE E5-B - CONSUMER COMPOSITION");

        TestLog.Task(
            "Consumer maps resolved emission onto the exact " +
            "Machine interaction object.");

        machine.Radiation =
            resolvedRadiation;

        Assert.Equal(
            20.0,
            machine.Radiation,
            10);

        TestLog.State(
            $"Machine runtime Radiation = " +
            $"{machine.Radiation:R}");

        TestLog.Pass(
            "Consumer supplied the meaning of the resolved emission.");

        /*
         * Create controlled Surfaces for the exact runtime
         * objects participating in the QSL interaction.
         */

        var machineSurface =
            new TestEngineSurface(
                machineEntity,
                machine,
                "machine",
                "Machine");

        var maraSurface =
            new TestEngineSurface(
                maraEntity,
                mara,
                "mara",
                "Mara");

        Assert.Same(
            machine,
            machineSurface.Target);

        Assert.Same(
            mara,
            maraSurface.Target);

        Assert.Same(
            machineEntity,
            machineSurface.Entity);

        Assert.Same(
            maraEntity,
            maraSurface.Entity);

        TestLog.State(
            $"Machine Surface exact target = " +
            $"{ReferenceEquals(
                machine,
                machineSurface.Target)}");

        TestLog.State(
            $"Mara Surface exact target = " +
            $"{ReferenceEquals(
                mara,
                maraSurface.Target)}");

        /*
         * The interaction uses native runtime properties.
         *
         * No attached QSL package is needed here.
         * The resolved emission now exists on the exact
         * consumer-owned Machine object.
         */

        const string source =
            "mara.Health = mara.Health - machine.Radiation";

        var adapter =
            new EntityQslRequestAdapter();

        TestLog.Task(
            "Creating WildsQslRequest from Machine and Mara Surfaces.");

        var request =
            adapter.CreateRequest(
                source,
                new IEntitySurface[]
                {
                    maraSurface,
                    machineSurface
                },
                new Dictionary<
                    IEntitySurface,
                    IReadOnlyList<IFact>>());

        Assert.Equal(
            2,
            request.Consumers.Count);

        Assert.Empty(
            request.StatePackages);

        Assert.Same(
            mara,
            request.Consumers[0].Value);

        Assert.Same(
            machine,
            request.Consumers[1].Value);

        TestLog.State(
            $"Consumer bindings = " +
            $"{request.Consumers.Count}");

        TestLog.State(
            $"Attached state packages = " +
            $"{request.StatePackages.Count}");

        TestLog.State(
            "QSL source:");

        TestLog.Write(
            "    " + source);

        /*
         * Existing Wilds/QSL execution takes over.
         */

        TestLog.Section(
            "ENTITY GATE E5-C - EXISTING WILDS/QSL PIPELINE");

        var qslEngine =
            CreateQslEngine();

        var wildsComponent =
            new TestComponent(
                "entity.e5.wilds-qsl");

        using var wildsEngine =
            CreateWildsEngine(
                wildsComponent);

        Assert.True(
            wildsEngine.Initialize());

        var executor =
            new WildsQslExecutor(
                qslEngine,
                wildsEngine);

        Assert.Equal(
            0,
            wildsEngine.Timeline.Count);

        TestLog.State(
            $"Mara.Health before QSL = " +
            $"{mara.Health:R}");

        TestLog.State(
            $"machine.Radiation visible to QSL = " +
            $"{machine.Radiation:R}");

        TestLog.Task(
            "Submitting composed Machine -> Mara interaction.");

        var moment =
            await executor.ExecuteAsync(
                    request)
                .ConfigureAwait(false);

        TestLog.Task(
            "Wilds/QSL interaction returned.");

        /*
         * 100 - 20 = 80
         */

        TestLog.Expected(
            "Mara.Health should resolve from 100 to 80.");

        Assert.Equal(
            MomentOutcome.Completed,
            moment.Outcome);

        Assert.Null(
            moment.Exception);

        Assert.Single(
            moment.WorkUnitIds);

        Assert.Equal(
            80.0,
            Convert.ToDouble(
                moment.Result),
            10);

        Assert.Equal(
            80.0,
            mara.Health,
            10);

        TestLog.State(
            $"Moment outcome = {moment.Outcome}");

        TestLog.State(
            $"Moment result = {moment.Result}");

        TestLog.State(
            $"Mara.Health after QSL = " +
            $"{mara.Health:R}");

        TestLog.Pass(
            "Exact consumer-owned Mara object was mutated.");

        /*
         * Machine Entity knowledge remains unchanged.
         */

        TestLog.Section(
            "ENTITY GATE E5-D - KNOWLEDGE ISOLATION");

        Assert.Single(
            machineEntity.Facts);

        Assert.Same(
            radiation,
            machineEntity.Facts[0]);

        Assert.Equal(
            20.0,
            Assert.IsType<double>(
                radiation.Value));

        Assert.Single(
            radiation.Discoveries);

        Assert.Same(
            radiationDiscovery,
            radiation.Discoveries[0]);

        Assert.True(
            radiationDiscovery.IsAvailable);

        Assert.Empty(
            maraEntity.Facts);

        TestLog.State(
            $"Machine Fact count after interaction = " +
            $"{machineEntity.Facts.Count}");

        TestLog.State(
            $"Machine Radiation Fact after interaction = " +
            $"{radiation.Value}");

        TestLog.State(
            $"Discovery still available = " +
            $"{radiationDiscovery.IsAvailable}");

        TestLog.State(
            $"Mara Entity Fact count = " +
            $"{maraEntity.Facts.Count}");

        TestLog.Pass(
            "Machine knowledge remained unchanged.");

        TestLog.Pass(
            "Mara knowledge remained isolated.");

        /*
         * Exact identity continuity.
         */

        Assert.Same(
            machine,
            machineSurface.Target);

        Assert.Same(
            mara,
            maraSurface.Target);

        Assert.Same(
            machine,
            request.Consumers[1].Value);

        Assert.Same(
            mara,
            request.Consumers[0].Value);

        Assert.Same(
            radiation,
            radiationDiscovery.Fact);

        Assert.Same(
            machineEntity,
            radiationDiscovery.Fact.Entity);

        TestLog.State(
            $"Machine target identity survived = " +
            $"{ReferenceEquals(
                machine,
                request.Consumers[1].Value)}");

        TestLog.State(
            $"Mara target identity survived = " +
            $"{ReferenceEquals(
                mara,
                request.Consumers[0].Value)}");

        TestLog.State(
            $"Discovery provenance survived = " +
            $"{ReferenceEquals(
                machineEntity,
                radiationDiscovery.Fact.Entity)}");

        TestLog.Pass(
            "Consumer and Entity provenance survived composition.");

        /*
         * Existing Wilds temporal history remains authoritative.
         */

        TestLog.Section(
            "ENTITY GATE E5-E - WILDS HISTORY");

        Assert.Single(
            wildsEngine.Timeline.Moments);

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
            $"WorkUnits = {moment.WorkUnitIds.Count}");

        TestLog.State(
            $"Timeline Moments = " +
            $"{wildsEngine.Timeline.Count}");

        TestLog.State(
            $"Epoch Timelines = " +
            $"{wildsEngine.Epoch.Count}");

        TestLog.State(
            $"Exact Moment retained = " +
            $"{ReferenceEquals(
                moment,
                wildsEngine.Timeline.Moments[0])}");

        TestLog.Pass(
            "Existing Wilds temporal history remained authoritative.");

        TestLog.Pass(
            "ENTITY GATE E5 CERTIFIED.");
    }

    /*
     * Current Discovery authorization semantics.
     *
     * This remains outside Wilds. The existing emitter receives
     * only IEmissionDataSource, exactly as certified by E2/E4.
     */
    private static bool ExposePropertyIfAvailable(
        EntityEmissionDataSource dataSource,
        IDiscovery discovery,
        string propertyName)
    {
        if (dataSource == null)
        {
            throw new ArgumentNullException(
                nameof(dataSource));
        }

        if (discovery == null)
        {
            throw new ArgumentNullException(
                nameof(discovery));
        }

        if (!discovery.IsAvailable)
        {
            return false;
        }

        return dataSource.ExposeProperty(
            propertyName,
            discovery.Fact);
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
            IComponent component)
    {
        return new Core.WildsEngine(
            component,
            new WildsEngineOptions
            {
                Layer =
                    "certification.entity.e5",

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

    private sealed class TestMachine
    {
        public double Radiation { get; set; }
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
}