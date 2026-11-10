using System;
using WildsEngine.Core.Emission;
using WildsEngine.Core.Entities;
using WildsEngine.Core.Tests.Fixtures;
using WildsEngine.Tests.Logging;
using WildsEngine.Tests.Registry;
using Xunit;

namespace WildsEngine.Core.Tests.Entities;

/// <summary>
/// Certifies the Entity -> existing Wilds emitter boundary.
///
/// Entity
///     -> Entity-owned Fact
///     -> EntityEmissionDataSource
///     -> existing IEmissionDataSource contract
///     -> existing EmitterBase
///     -> existing EmissionScalar
///
/// This gate does not introduce new emission behavior.
/// It proves Entity-owned knowledge can satisfy the existing
/// certified emitter surface without Wilds understanding
/// Entity or Fact.
/// </summary>
public sealed class E2Testing
{
    [Fact]
    public void
        Entity_fact_is_exposed_through_existing_emission_data_source()
    {
        TestLog.Section(
            "ENTITY GATE E2-A - EMISSION DATA SOURCE");

        TestLog.Test(
            "Entity-owned Fact reaches the existing " +
            "IEmissionDataSource boundary.");

        var machineEntity =
            new TestEntity(
                "machine.entity");

        var radiation =
            new TestFact(
                "machine.radiation",
                machineEntity,
                "Radiation",
                20.0);

        Assert.True(
            machineEntity.RegisterFact(
                radiation));

        var dataSource =
            new EntityEmissionDataSource(
                machineEntity);

        Assert.True(
            dataSource.ExposeProperty(
                "Radiation",
                radiation));

        TestLog.State(
            $"Machine Entity = " +
            $"{machineEntity.Slug} / {machineEntity.Id}");

        TestLog.State(
            $"Fact = {radiation.Slug}");

        TestLog.State(
            $"Fact path = {radiation.Path}");

        TestLog.State(
            $"Fact value = {radiation.Value}");

        TestLog.State(
            $"Machine Fact count = " +
            $"{machineEntity.Facts.Count}");

        TestLog.Task(
            "Reading Radiation through existing " +
            "IEmissionDataSource contract.");

        var found =
            dataSource.TryGetProperty(
                "Radiation",
                out var value);

        Assert.True(
            found);

        Assert.Equal(
            20.0,
            Assert.IsType<double>(
                value));

        Assert.Single(
            machineEntity.Facts);

        Assert.Same(
            radiation,
            machineEntity.Facts[0]);

        Assert.Same(
            machineEntity,
            radiation.Entity);

        TestLog.State(
            $"Radiation found = {found}");

        TestLog.State(
            $"Radiation raw value = {value}");

        TestLog.State(
            $"Exact Fact reference retained = " +
            $"{ReferenceEquals(
                radiation,
                machineEntity.Facts[0])}");

        TestLog.Pass(
            "Entity Fact satisfied the existing " +
            "IEmissionDataSource contract.");
    }

    [Fact]
    public void
        Existing_emitter_resolves_entity_backed_fact()
    {
        TestLog.Section(
            "ENTITY GATE E2-B - EXISTING EMITTER");

        TestLog.Test(
            "Existing emitter resolves an Entity-backed " +
            "consumer Fact.");

        var machineEntity =
            new TestEntity(
                "machine.entity");

        var radiation =
            new TestFact(
                "machine.radiation",
                machineEntity,
                "Radiation",
                20.0);

        Assert.True(
            machineEntity.RegisterFact(
                radiation));

        var dataSource =
            new EntityEmissionDataSource(
                machineEntity);

        Assert.True(
            dataSource.ExposeProperty(
                "Radiation",
                radiation));

        var component =
            new TestComponent(
                "entity.e2.emitter");

        var emitter =
            new TestEmitter(
                component,
                "entity.e2.radiation",
                dataSource,
                new EmissionScalar());

        TestLog.State(
            $"Entity Fact Radiation = {radiation.Value}");

        TestLog.State(
            $"Emitter Id = {emitter.Id}");

        TestLog.State(
            $"Emitter Slug = {emitter.Slug}");

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
            radiation,
            machineEntity.Facts[0]);

        TestLog.State(
            $"Emission succeeded = {emitted}");

        TestLog.State(
            $"Resolved Radiation = {value:R}");

        TestLog.State(
            $"Exact Entity Fact retained = " +
            $"{ReferenceEquals(
                radiation,
                machineEntity.Facts[0])}");

        TestLog.Pass(
            "Existing emitter resolved Entity-backed " +
            "Radiation to 20.");

        TestLog.Pass(
            "Wilds required no Entity or Fact awareness.");
    }

    [Fact]
    public void
        Existing_emitter_observes_current_fact_value()
    {
        TestLog.Section(
            "ENTITY GATE E2-C - CURRENT FACT VALUE");

        TestLog.Test(
            "Existing emitter observes the current " +
            "authoritative Entity Fact value.");

        var machineEntity =
            new TestEntity(
                "machine.entity");

        var radiation =
            new TestFact(
                "machine.radiation",
                machineEntity,
                "Radiation",
                20.0);

        Assert.True(
            machineEntity.RegisterFact(
                radiation));

        var dataSource =
            new EntityEmissionDataSource(
                machineEntity);

        Assert.True(
            dataSource.ExposeProperty(
                "Radiation",
                radiation));

        var component =
            new TestComponent(
                "entity.e2.current-value");

        var emitter =
            new TestEmitter(
                component,
                "entity.e2.current-value",
                dataSource,
                new EmissionScalar());

        TestLog.Task(
            "Resolving initial Fact value.");

        Assert.True(
            emitter.TryEmit(
                "Radiation",
                out var before));

        Assert.Equal(
            20.0,
            before,
            10);

        TestLog.State(
            $"Radiation before = {before:R}");

        TestLog.Task(
            "Updating authoritative Fact from 20 to 35.");

        radiation.UpdateValue(
            35.0);

        Assert.Equal(
            35.0,
            Assert.IsType<double>(
                radiation.Value));

        TestLog.Task(
            "Resolving through the same emitter again.");

        Assert.True(
            emitter.TryEmit(
                "Radiation",
                out var after));

        Assert.Equal(
            35.0,
            after,
            10);

        Assert.Same(
            radiation,
            machineEntity.Facts[0]);

        TestLog.State(
            $"Radiation after = {after:R}");

        TestLog.State(
            $"Same Fact reference = " +
            $"{ReferenceEquals(
                radiation,
                machineEntity.Facts[0])}");

        TestLog.Pass(
            "Existing emitter observed updated Fact value.");

        TestLog.Pass(
            "No duplicated emission state store was required.");
    }

    [Fact]
    public void
        Entity_facts_resolve_through_existing_named_emission_group()
    {
        TestLog.Section(
            "ENTITY GATE E2-D - NAMED EMISSION GROUP");

        TestLog.Test(
            "Entity-owned Facts resolve through the existing " +
            "emitter group surface.");

        var machineEntity =
            new TestEntity(
                "machine.entity");

        var heat =
            new TestFact(
                "machine.heat",
                machineEntity,
                "Heat",
                50.0);

        var radiation =
            new TestFact(
                "machine.radiation",
                machineEntity,
                "Radiation",
                20.0);

        var pressure =
            new TestFact(
                "machine.pressure",
                machineEntity,
                "Pressure",
                8.0);

        Assert.True(
            machineEntity.RegisterFact(
                heat));

        Assert.True(
            machineEntity.RegisterFact(
                radiation));

        Assert.True(
            machineEntity.RegisterFact(
                pressure));

        var dataSource =
            new EntityEmissionDataSource(
                machineEntity);

        Assert.True(
            dataSource.ExposeGroupProperty(
                "machine.environment",
                "Heat",
                heat));

        Assert.True(
            dataSource.ExposeGroupProperty(
                "machine.environment",
                "Radiation",
                radiation));

        Assert.True(
            dataSource.ExposeGroupProperty(
                "machine.environment",
                "Pressure",
                pressure));

        var component =
            new TestComponent(
                "entity.e2.group");

        var emitter =
            new TestEmitter(
                component,
                "entity.e2.group",
                dataSource,
                new EmissionScalar());

        TestLog.Task(
            "Calling existing emitter TryEmitGroup.");

        var emitted =
            emitter.TryEmitGroup(
                "machine.environment",
                out var values);

        Assert.True(
            emitted);

        Assert.Equal(
            3,
            values.Count);

        Assert.Equal(
            50.0,
            values["Heat"],
            10);

        Assert.Equal(
            20.0,
            values["Radiation"],
            10);

        Assert.Equal(
            8.0,
            values["Pressure"],
            10);

        TestLog.State(
            $"Group emitted = {emitted}");

        TestLog.State(
            $"Heat = {values["Heat"]:R}");

        TestLog.State(
            $"Radiation = {values["Radiation"]:R}");

        TestLog.State(
            $"Pressure = {values["Pressure"]:R}");

        TestLog.Pass(
            "Entity Facts resolved through existing " +
            "named emitter group.");
    }

    [Fact]
    public void
        Emission_data_source_rejects_fact_owned_by_another_entity()
    {
        TestLog.Section(
            "ENTITY GATE E2-E - FOREIGN FACT REJECTION");

        TestLog.Test(
            "Entity emission boundary rejects another " +
            "Entity's Fact.");

        var machineEntity =
            new TestEntity(
                "machine.entity");

        var foreignEntity =
            new TestEntity(
                "foreign.entity");

        var foreignRadiation =
            new TestFact(
                "foreign.radiation",
                foreignEntity,
                "Radiation",
                99.0);

        Assert.True(
            foreignEntity.RegisterFact(
                foreignRadiation));

        var dataSource =
            new EntityEmissionDataSource(
                machineEntity);

        TestLog.Task(
            "Attempting to expose foreign Radiation Fact.");

        TestLog.Expected(
            "ArgumentException");

        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    dataSource.ExposeProperty(
                        "Radiation",
                        foreignRadiation));

        Assert.Empty(
            machineEntity.Facts);

        Assert.Single(
            foreignEntity.Facts);

        Assert.Same(
            foreignRadiation,
            foreignEntity.Facts[0]);

        TestLog.State(
            $"Rejected with = " +
            $"{exception.GetType().Name}");

        TestLog.State(
            $"Foreign Entity Fact count = " +
            $"{foreignEntity.Facts.Count}");

        TestLog.Pass(
            "Foreign Fact was rejected without " +
            "mutating Entity ownership.");
    }

    [Fact]
    public void
        Missing_entity_vocabulary_does_not_emit()
    {
        TestLog.Section(
            "ENTITY GATE E2-F - MISSING VOCABULARY");

        TestLog.Test(
            "Missing Entity vocabulary remains missing " +
            "through the existing emitter.");

        var machineEntity =
            new TestEntity(
                "machine.entity");

        var dataSource =
            new EntityEmissionDataSource(
                machineEntity);

        var component =
            new TestComponent(
                "entity.e2.missing");

        var emitter =
            new TestEmitter(
                component,
                "entity.e2.missing",
                dataSource,
                new EmissionScalar());

        TestLog.Task(
            "Requesting an unknown emission property.");

        var emitted =
            emitter.TryEmit(
                "AlienBananaDensity",
                out var value);

        Assert.False(
            emitted);

        Assert.Equal(
            0.0,
            value);

        TestLog.State(
            $"Emission succeeded = {emitted}");

        TestLog.State(
            $"Returned scalar = {value:R}");

        TestLog.Pass(
            "Missing Entity vocabulary did not emit.");
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void
        Existing_emission_scalar_remains_authoritative_for_invalid_values(
            double rawValue)
    {
        TestLog.Section(
            "ENTITY GATE E2-G - EMISSION VALIDATION OWNERSHIP");

        TestLog.Test(
            "Existing Wilds scalar validation remains " +
            "authoritative for Entity-backed values.");

        var machineEntity =
            new TestEntity(
                "machine.entity");

        var radiation =
            new TestFact(
                "machine.radiation",
                machineEntity,
                "Radiation",
                rawValue);

        Assert.True(
            machineEntity.RegisterFact(
                radiation));

        var dataSource =
            new EntityEmissionDataSource(
                machineEntity);

        Assert.True(
            dataSource.ExposeProperty(
                "Radiation",
                radiation));

        /*
         * The Entity layer has intentionally accepted and
         * exposed the raw object value.
         *
         * Numeric/finite emission validation belongs to
         * the existing Wilds EmissionScalar.
         */

        Assert.True(
            dataSource.TryGetProperty(
                "Radiation",
                out var exposed));

        Assert.Same(
            radiation.Value,
            exposed);

        var component =
            new TestComponent(
                "entity.e2.validation");

        var emitter =
            new TestEmitter(
                component,
                "entity.e2.validation",
                dataSource,
                new EmissionScalar());

        TestLog.State(
            $"Entity exposed raw value = {rawValue}");

        TestLog.Task(
            "Passing raw Entity value into existing emitter.");

        TestLog.Expected(
            "ArgumentOutOfRangeException from existing " +
            "EmissionScalar.");

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                emitter.TryEmit(
                    "Radiation",
                    out _));

        Assert.Single(
            machineEntity.Facts);

        Assert.Same(
            radiation,
            machineEntity.Facts[0]);

        TestLog.Pass(
            "Existing EmissionScalar rejected non-finite " +
            "Entity-backed value.");

        TestLog.Pass(
            "Entity layer did not duplicate scalar validation.");
    }

    [Fact]
    public void
        Existing_emission_scalar_rejects_non_numeric_entity_value()
    {
        TestLog.Section(
            "ENTITY GATE E2-H - NON-NUMERIC VALUE");

        TestLog.Test(
            "Existing Wilds scalar rejects non-numeric " +
            "Entity-backed value.");

        var machineEntity =
            new TestEntity(
                "machine.entity");

        var radiation =
            new TestFact(
                "machine.radiation",
                machineEntity,
                "Radiation",
                "not-a-number");

        Assert.True(
            machineEntity.RegisterFact(
                radiation));

        var dataSource =
            new EntityEmissionDataSource(
                machineEntity);

        Assert.True(
            dataSource.ExposeProperty(
                "Radiation",
                radiation));

        Assert.True(
            dataSource.TryGetProperty(
                "Radiation",
                out var raw));

        Assert.Equal(
            "not-a-number",
            Assert.IsType<string>(
                raw));

        var component =
            new TestComponent(
                "entity.e2.non-numeric");

        var emitter =
            new TestEmitter(
                component,
                "entity.e2.non-numeric",
                dataSource,
                new EmissionScalar());

        TestLog.State(
            $"Entity raw value = {raw}");

        TestLog.Expected(
            "ArgumentException from existing EmissionScalar.");

        Assert.Throws<ArgumentException>(
            () =>
                emitter.TryEmit(
                    "Radiation",
                    out _));

        Assert.Same(
            radiation,
            machineEntity.Facts[0]);

        TestLog.Pass(
            "Existing EmissionScalar rejected the " +
            "non-numeric Entity value.");

        TestLog.Pass(
            "Emission semantics remained owned by Wilds.");
    }

    [Fact]
    public void
        Existing_emitter_read_preserves_entity_fact_identity()
    {
        TestLog.Section(
            "ENTITY GATE E2-I - IDENTITY CONTINUITY");

        TestLog.Test(
            "Existing emitter reads Entity knowledge without " +
            "taking ownership of it.");

        var machineEntity =
            new TestEntity(
                "machine.entity");

        var radiation =
            new TestFact(
                "machine.radiation",
                machineEntity,
                "Radiation",
                20.0);

        Assert.True(
            machineEntity.RegisterFact(
                radiation));

        var dataSource =
            new EntityEmissionDataSource(
                machineEntity);

        Assert.True(
            dataSource.ExposeProperty(
                "Radiation",
                radiation));

        var entityId =
            machineEntity.Id;

        var factId =
            radiation.Id;

        var component =
            new TestComponent(
                "entity.e2.identity");

        var emitter =
            new TestEmitter(
                component,
                "entity.e2.identity",
                dataSource,
                new EmissionScalar());

        TestLog.State(
            $"Entity Id before = {entityId}");

        TestLog.State(
            $"Fact Id before = {factId}");

        TestLog.Task(
            "Resolving Entity-backed value through emitter.");

        Assert.True(
            emitter.TryEmit(
                "Radiation",
                out var value));

        Assert.Equal(
            20.0,
            value,
            10);

        Assert.Equal(
            entityId,
            machineEntity.Id);

        Assert.Equal(
            factId,
            radiation.Id);

        Assert.Single(
            machineEntity.Facts);

        Assert.Same(
            radiation,
            machineEntity.Facts[0]);

        Assert.Same(
            machineEntity,
            radiation.Entity);

        TestLog.State(
            $"Entity Id after = {machineEntity.Id}");

        TestLog.State(
            $"Fact Id after = {radiation.Id}");

        TestLog.State(
            $"Exact Fact reference survived = " +
            $"{ReferenceEquals(
                radiation,
                machineEntity.Facts[0])}");

        TestLog.State(
            $"Resolved value = {value:R}");

        TestLog.Pass(
            "Emitter read preserved Entity and Fact identity.");

        TestLog.Pass(
            "ENTITY GATE E2 CERTIFIED.");
    }
}