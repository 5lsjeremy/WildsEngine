using System;
using WildsEngine.Core.Emission;
using WildsEngine.Core.Entities;
using WildsEngine.Core.Entities.Abstractions;
using WildsEngine.Core.Tests.Fixtures;
using WildsEngine.Tests.Logging;
using WildsEngine.Tests.Registry;
using Xunit;

namespace WildsEngine.Core.Tests.Entities;

/// <summary>
/// Certifies experimental Discovery-gated exposure.
///
/// Entity
///     -> owns Fact
///     -> Fact owns Discovery
///     -> Discovery availability authorizes projection
///     -> EntityEmissionDataSource
///     -> existing EmitterBase
///
/// Discovery remains Entity-owned knowledge.
///
/// The emitter receives only the existing
/// IEmissionDataSource contract and remains unaware of
/// Entity, Fact, Discovery, and Discovery availability.
/// </summary>
public sealed class EntityDiscoveryExposureCertificationTests
{
    [Fact]
    public void
        Unavailable_discovery_does_not_expose_its_fact()
    {
        TestLog.Section(
            "ENTITY GATE E4-A - UNAVAILABLE DISCOVERY");

        TestLog.Test(
            "An unavailable Discovery does not authorize " +
            "its Fact for emission exposure.");

        var entity =
            new TestEntity(
                "machine.entity");

        var fact =
            new TestFact(
                "machine.radiation",
                entity,
                "Radiation",
                20.0);

        var discovery =
            new TestDiscovery(
                "machine.radiation.observable",
                fact,
                isAvailable: false);

        Assert.True(
            entity.RegisterFact(
                fact));

        Assert.True(
            fact.RegisterDiscovery(
                discovery));

        var dataSource =
            new EntityEmissionDataSource(
                entity);

        TestLog.State(
            $"Fact = {fact.Path} / {fact.Value}");

        TestLog.State(
            $"Discovery = {discovery.Slug}");

        TestLog.State(
            $"Discovery available = " +
            $"{discovery.IsAvailable}");

        TestLog.Task(
            "Attempting Discovery-authorized exposure.");

        var exposed =
            ExposePropertyIfAvailable(
                dataSource,
                discovery,
                "Radiation");

        Assert.False(
            exposed);

        Assert.False(
            dataSource.TryGetProperty(
                "Radiation",
                out var value));

        Assert.Null(
            value);

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
            $"Fact exposed = {exposed}");

        TestLog.State(
            $"Radiation available through data source = " +
            $"{value != null}");

        TestLog.Pass(
            "Unavailable Discovery kept its Fact private.");
    }

    [Fact]
    public void
        Available_discovery_permits_exact_fact_exposure()
    {
        TestLog.Section(
            "ENTITY GATE E4-B - AVAILABLE DISCOVERY");

        TestLog.Test(
            "An available Discovery permits exposure of " +
            "its exact originating Fact.");

        var entity =
            new TestEntity(
                "machine.entity");

        var fact =
            new TestFact(
                "machine.radiation",
                entity,
                "Radiation",
                20.0);

        var discovery =
            new TestDiscovery(
                "machine.radiation.observable",
                fact);

        Assert.True(
            entity.RegisterFact(
                fact));

        Assert.True(
            fact.RegisterDiscovery(
                discovery));

        Assert.False(
            discovery.IsAvailable);

        TestLog.Task(
            "Making the Discovery available.");

        discovery.SetAvailable();

        Assert.True(
            discovery.IsAvailable);

        var dataSource =
            new EntityEmissionDataSource(
                entity);

        var exposed =
            ExposePropertyIfAvailable(
                dataSource,
                discovery,
                "Radiation");

        Assert.True(
            exposed);

        Assert.True(
            dataSource.TryGetProperty(
                "Radiation",
                out var value));

        Assert.Equal(
            20.0,
            Assert.IsType<double>(
                value));

        Assert.Same(
            fact,
            discovery.Fact);

        Assert.Same(
            entity,
            discovery.Fact.Entity);

        Assert.Same(
            fact,
            entity.Facts[0]);

        Assert.Same(
            discovery,
            fact.Discoveries[0]);

        TestLog.State(
            $"Discovery available = " +
            $"{discovery.IsAvailable}");

        TestLog.State(
            $"Fact exposed = {exposed}");

        TestLog.State(
            $"Exposed Radiation = {value}");

        TestLog.State(
            $"Discovery -> exact Fact = " +
            $"{ReferenceEquals(
                fact,
                discovery.Fact)}");

        TestLog.Pass(
            "Available Discovery authorized exact Fact exposure.");
    }

    [Fact]
    public void
        Available_discovery_reaches_existing_emitter_unchanged()
    {
        TestLog.Section(
            "ENTITY GATE E4-C - EXISTING EMITTER");

        TestLog.Test(
            "A Discovery-authorized Fact reaches the existing " +
            "emitter without exposing Discovery to Wilds.");

        var entity =
            new TestEntity(
                "machine.entity");

        var fact =
            new TestFact(
                "machine.radiation",
                entity,
                "Radiation",
                20.0);

        var discovery =
            new TestDiscovery(
                "machine.radiation.observable",
                fact,
                isAvailable: true);

        Assert.True(
            entity.RegisterFact(
                fact));

        Assert.True(
            fact.RegisterDiscovery(
                discovery));

        var dataSource =
            new EntityEmissionDataSource(
                entity);

        Assert.True(
            ExposePropertyIfAvailable(
                dataSource,
                discovery,
                "Radiation"));

        var component =
            new TestComponent(
                "entity.e4.emitter");

        var emitter =
            new TestEmitter(
                component,
                "entity.e4.radiation",
                dataSource,
                new EmissionScalar());

        TestLog.State(
            $"Discovery available = " +
            $"{discovery.IsAvailable}");

        TestLog.State(
            $"Authorized Fact = " +
            $"{discovery.Fact.Path} / " +
            $"{discovery.Fact.Value}");

        TestLog.Task(
            "Calling existing emitter TryEmit.");

        var emitted =
            emitter.TryEmit(
                "Radiation",
                out var value);

        Assert.True(
            emitted);

        Assert.Equal(
            20.0,
            value,
            10);

        Assert.Same(
            component,
            emitter.Component);

        Assert.Same(
            fact,
            entity.Facts[0]);

        Assert.Same(
            discovery,
            fact.Discoveries[0]);

        Assert.Same(
            fact,
            discovery.Fact);

        TestLog.State(
            $"Emission succeeded = {emitted}");

        TestLog.State(
            $"Resolved Radiation = {value:R}");

        TestLog.State(
            $"Exact Fact survived = " +
            $"{ReferenceEquals(
                fact,
                entity.Facts[0])}");

        TestLog.Pass(
            "Existing emitter resolved the authorized Fact.");

        TestLog.Pass(
            "Emitter required no Discovery awareness.");
    }

    [Fact]
    public void
        Unavailable_sibling_fact_remains_invisible()
    {
        TestLog.Section(
            "ENTITY GATE E4-D - SELECTIVE EXPOSURE");

        TestLog.Test(
            "Only Facts with available Discoveries become " +
            "visible to the emitter.");

        var entity =
            new TestEntity(
                "machine.entity");

        var radiation =
            new TestFact(
                "machine.radiation",
                entity,
                "Radiation",
                20.0);

        var heat =
            new TestFact(
                "machine.heat",
                entity,
                "Heat",
                50.0);

        var radiationDiscovery =
            new TestDiscovery(
                "machine.radiation.observable",
                radiation,
                isAvailable: true);

        var heatDiscovery =
            new TestDiscovery(
                "machine.heat.observable",
                heat,
                isAvailable: false);

        Assert.True(
            entity.RegisterFact(
                radiation));

        Assert.True(
            entity.RegisterFact(
                heat));

        Assert.True(
            radiation.RegisterDiscovery(
                radiationDiscovery));

        Assert.True(
            heat.RegisterDiscovery(
                heatDiscovery));

        var dataSource =
            new EntityEmissionDataSource(
                entity);

        var radiationExposed =
            ExposePropertyIfAvailable(
                dataSource,
                radiationDiscovery,
                "Radiation");

        var heatExposed =
            ExposePropertyIfAvailable(
                dataSource,
                heatDiscovery,
                "Heat");

        Assert.True(
            radiationExposed);

        Assert.False(
            heatExposed);

        var component =
            new TestComponent(
                "entity.e4.selective");

        var emitter =
            new TestEmitter(
                component,
                "entity.e4.selective",
                dataSource,
                new EmissionScalar());

        var radiationEmitted =
            emitter.TryEmit(
                "Radiation",
                out var radiationValue);

        var heatEmitted =
            emitter.TryEmit(
                "Heat",
                out var heatValue);

        Assert.True(
            radiationEmitted);

        Assert.Equal(
            20.0,
            radiationValue,
            10);

        Assert.False(
            heatEmitted);

        Assert.Equal(
            0.0,
            heatValue);

        Assert.Equal(
            2,
            entity.Facts.Count);

        Assert.Single(
            radiation.Discoveries);

        Assert.Single(
            heat.Discoveries);

        TestLog.State(
            $"Entity knows Radiation = {radiation.Value}");

        TestLog.State(
            $"Entity knows Heat = {heat.Value}");

        TestLog.State(
            $"Radiation Discovery available = " +
            $"{radiationDiscovery.IsAvailable}");

        TestLog.State(
            $"Heat Discovery available = " +
            $"{heatDiscovery.IsAvailable}");

        TestLog.State(
            $"Radiation emitted = " +
            $"{radiationEmitted} / {radiationValue:R}");

        TestLog.State(
            $"Heat emitted = " +
            $"{heatEmitted} / {heatValue:R}");

        TestLog.Pass(
            "Available Radiation became externally visible.");

        TestLog.Pass(
            "Known but unavailable Heat remained private.");
    }

    [Fact]
    public void
        Withdrawing_discovery_availability_retires_fresh_projection()
    {
        TestLog.Section(
            "ENTITY GATE E4-E - WITHDRAW EXPOSURE");

        TestLog.Test(
            "Withdrawing Discovery availability removes the " +
            "Fact from a newly built Surface projection.");

        var entity =
            new TestEntity(
                "machine.entity");

        var fact =
            new TestFact(
                "machine.radiation",
                entity,
                "Radiation",
                20.0);

        var discovery =
            new TestDiscovery(
                "machine.radiation.observable",
                fact,
                isAvailable: true);

        Assert.True(
            entity.RegisterFact(
                fact));

        Assert.True(
            fact.RegisterDiscovery(
                discovery));

        var component =
            new TestComponent(
                "entity.e4.withdrawal");

        /*
         * First projection is built while the Discovery
         * is available.
         */

        var availableDataSource =
            CreateProjection(
                entity,
                discovery,
                "Radiation");

        var availableEmitter =
            new TestEmitter(
                component,
                "entity.e4.available",
                availableDataSource,
                new EmissionScalar());

        Assert.True(
            availableEmitter.TryEmit(
                "Radiation",
                out var before));

        Assert.Equal(
            20.0,
            before,
            10);

        TestLog.State(
            $"Radiation before withdrawal = {before:R}");

        TestLog.Task(
            "Withdrawing Discovery availability.");

        discovery.SetUnavailable();

        Assert.False(
            discovery.IsAvailable);

        /*
         * Surface projection is rebuilt from the current
         * Discovery availability state.
         *
         * This experiment does not yet add live revocation
         * behavior to EntityEmissionDataSource.
         */

        var withdrawnDataSource =
            CreateProjection(
                entity,
                discovery,
                "Radiation");

        var withdrawnEmitter =
            new TestEmitter(
                component,
                "entity.e4.withdrawn",
                withdrawnDataSource,
                new EmissionScalar());

        var emittedAfterWithdrawal =
            withdrawnEmitter.TryEmit(
                "Radiation",
                out var after);

        Assert.False(
            emittedAfterWithdrawal);

        Assert.Equal(
            0.0,
            after);

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

        TestLog.State(
            $"Discovery available after withdrawal = " +
            $"{discovery.IsAvailable}");

        TestLog.State(
            $"Radiation emitted after withdrawal = " +
            $"{emittedAfterWithdrawal}");

        TestLog.State(
            $"Fact still exists = " +
            $"{ReferenceEquals(
                fact,
                entity.Facts[0])}");

        TestLog.State(
            $"Discovery still exists = " +
            $"{ReferenceEquals(
                discovery,
                fact.Discoveries[0])}");

        TestLog.Pass(
            "Fresh Surface projection retired unavailable Fact.");

        TestLog.Pass(
            "Knowledge and provenance remained intact.");
    }

    [Fact]
    public void
        Exposure_changes_never_mutate_entity_fact_or_discovery_provenance()
    {
        TestLog.Section(
            "ENTITY GATE E4-F - PROVENANCE CONTINUITY");

        TestLog.Test(
            "Availability and exposure changes preserve Entity, " +
            "Fact, and Discovery provenance.");

        var entity =
            new TestEntity(
                "machine.entity");

        var fact =
            new TestFact(
                "machine.radiation",
                entity,
                "Radiation",
                20.0);

        var discovery =
            new TestDiscovery(
                "machine.radiation.observable",
                fact);

        Assert.True(
            entity.RegisterFact(
                fact));

        Assert.True(
            fact.RegisterDiscovery(
                discovery));

        var entityId =
            entity.Id;

        var factId =
            fact.Id;

        var discoveryId =
            discovery.Id;

        Assert.False(
            discovery.IsAvailable);

        TestLog.Task(
            "Making Discovery available.");

        discovery.SetAvailable();

        var availableProjection =
            CreateProjection(
                entity,
                discovery,
                "Radiation");

        Assert.True(
            availableProjection.TryGetProperty(
                "Radiation",
                out var exposedValue));

        Assert.Equal(
            20.0,
            Assert.IsType<double>(
                exposedValue));

        TestLog.Task(
            "Making Discovery unavailable.");

        discovery.SetUnavailable();

        var unavailableProjection =
            CreateProjection(
                entity,
                discovery,
                "Radiation");

        Assert.False(
            unavailableProjection.TryGetProperty(
                "Radiation",
                out var withdrawnValue));

        Assert.Null(
            withdrawnValue);

        Assert.Equal(
            entityId,
            entity.Id);

        Assert.Equal(
            factId,
            fact.Id);

        Assert.Equal(
            discoveryId,
            discovery.Id);

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

        Assert.Same(
            entity,
            discovery.Fact.Entity);

        Assert.Equal(
            20.0,
            Assert.IsType<double>(
                fact.Value));

        TestLog.State(
            $"Entity identity preserved = " +
            $"{entityId == entity.Id}");

        TestLog.State(
            $"Fact identity preserved = " +
            $"{factId == fact.Id}");

        TestLog.State(
            $"Discovery identity preserved = " +
            $"{discoveryId == discovery.Id}");

        TestLog.State(
            $"Fact value preserved = {fact.Value}");

        TestLog.State(
            $"Exact lineage preserved = " +
            $"{ReferenceEquals(
                entity,
                discovery.Fact.Entity)}");

        TestLog.Pass(
            "Exposure transitions preserved all provenance.");

        TestLog.Pass(
            "Knowledge remained independent of exposure.");
    }

    [Fact]
    public void
        Foreign_discovery_cannot_authorize_another_entity_fact()
    {
        TestLog.Section(
            "ENTITY GATE E4-G - FOREIGN AUTHORIZATION");

        TestLog.Test(
            "A Discovery cannot authorize exposure through " +
            "another Entity's emission boundary.");

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
                "foreign.radiation.observable",
                foreignFact,
                isAvailable: true);

        Assert.True(
            foreignEntity.RegisterFact(
                foreignFact));

        Assert.True(
            foreignFact.RegisterDiscovery(
                foreignDiscovery));

        var machineDataSource =
            new EntityEmissionDataSource(
                machineEntity);

        TestLog.State(
            $"Foreign Discovery available = " +
            $"{foreignDiscovery.IsAvailable}");

        TestLog.State(
            $"Foreign Fact Entity = " +
            $"{foreignDiscovery.Fact.Entity.Slug}");

        TestLog.State(
            $"Target emission Entity = " +
            $"{machineEntity.Slug}");

        TestLog.Task(
            "Attempting foreign Discovery authorization.");

        TestLog.Expected(
            "ArgumentException");

        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    ExposePropertyIfAvailable(
                        machineDataSource,
                        foreignDiscovery,
                        "Radiation"));

        Assert.False(
            machineDataSource.TryGetProperty(
                "Radiation",
                out var value));

        Assert.Null(
            value);

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

        Assert.Same(
            foreignFact,
            foreignDiscovery.Fact);

        TestLog.State(
            $"Rejected with = " +
            $"{exception.GetType().Name}");

        TestLog.State(
            $"Machine Fact count = " +
            $"{machineEntity.Facts.Count}");

        TestLog.State(
            $"Foreign Fact count = " +
            $"{foreignEntity.Facts.Count}");

        TestLog.Pass(
            "Foreign Discovery authorization was rejected.");

        TestLog.Pass(
            "ENTITY GATE E4 CERTIFIED.");
    }

    /// <summary>
    /// Experimental E4 authorization policy.
    ///
    /// This method is intentionally test-local. It proves the
    /// semantic requirement before availability is promoted into
    /// the production Discovery or Surface abstractions.
    /// </summary>
    private static bool ExposePropertyIfAvailable(
        EntityEmissionDataSource dataSource,
        TestDiscovery discovery,
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

    /// <summary>
    /// Builds a fresh emission projection from the current
    /// Discovery availability state.
    /// </summary>
    private static EntityEmissionDataSource CreateProjection(
        IEntity entity,
        TestDiscovery discovery,
        string propertyName)
    {
        var dataSource =
            new EntityEmissionDataSource(
                entity);

        ExposePropertyIfAvailable(
            dataSource,
            discovery,
            propertyName);

        return dataSource;
    }
}