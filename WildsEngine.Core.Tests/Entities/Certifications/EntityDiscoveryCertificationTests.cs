using System;
using WildsEngine.Tests.Logging;
using Xunit;

namespace WildsEngine.Core.Tests.Entities;

/// <summary>
/// Certifies the private Entity knowledge chain.
///
/// Entity
///     -> owns Fact
///     -> Fact owns Discovery
///
/// A Discovery preserves the exact Fact and Entity lineage
/// while remaining independent of Wilds, QSL, emitters,
/// processor registries, and Surface projections.
/// </summary>
public sealed class EntityDiscoveryCertificationTests
{
    [Fact]
    public void
        Discovery_preserves_exact_fact_and_entity_lineage()
    {
        TestLog.Section(
            "ENTITY GATE E3-A - DISCOVERY PROVENANCE");

        TestLog.Test(
            "Discovery preserves the exact Fact and " +
            "Entity lineage.");

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
                "machine.radiation.discovery",
                fact);

        TestLog.Task(
            "Registering the exact Fact with its Entity.");

        Assert.True(
            entity.RegisterFact(
                fact));

        TestLog.Task(
            "Registering the exact Discovery with its Fact.");

        Assert.True(
            fact.RegisterDiscovery(
                discovery));

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

        Assert.Same(
            entity,
            fact.Entity);

        TestLog.State(
            $"Entity = {entity.Slug} / {entity.Id}");

        TestLog.State(
            $"Fact = {fact.Slug} / {fact.Id}");

        TestLog.State(
            $"Fact path = {fact.Path}");

        TestLog.State(
            $"Fact value = {fact.Value}");

        TestLog.State(
            $"Discovery = {discovery.Slug} / " +
            $"{discovery.Id}");

        TestLog.State(
            $"Entity -> exact Fact = " +
            $"{ReferenceEquals(
                fact,
                entity.Facts[0])}");

        TestLog.State(
            $"Fact -> exact Discovery = " +
            $"{ReferenceEquals(
                discovery,
                fact.Discoveries[0])}");

        TestLog.State(
            $"Discovery -> exact Fact = " +
            $"{ReferenceEquals(
                fact,
                discovery.Fact)}");

        TestLog.State(
            $"Discovery -> Fact -> exact Entity = " +
            $"{ReferenceEquals(
                entity,
                discovery.Fact.Entity)}");

        TestLog.Pass(
            "Exact Entity -> Fact -> Discovery " +
            "lineage was preserved.");
    }

    [Fact]
    public void
        Entity_fact_and_discovery_have_independent_stable_identity()
    {
        TestLog.Section(
            "ENTITY GATE E3-B - INDEPENDENT IDENTITY");

        TestLog.Test(
            "Entity, Fact, and Discovery retain independent " +
            "stable identities.");

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
                "machine.radiation.discovery",
                fact);

        Assert.True(
            entity.RegisterFact(
                fact));

        Assert.True(
            fact.RegisterDiscovery(
                discovery));

        Assert.NotEqual(
            Guid.Empty,
            entity.Id);

        Assert.NotEqual(
            Guid.Empty,
            fact.Id);

        Assert.NotEqual(
            Guid.Empty,
            discovery.Id);

        Assert.NotEqual(
            entity.Id,
            fact.Id);

        Assert.NotEqual(
            entity.Id,
            discovery.Id);

        Assert.NotEqual(
            fact.Id,
            discovery.Id);

        Assert.Equal(
            "machine.entity",
            entity.Slug);

        Assert.Equal(
            "machine.radiation",
            fact.Slug);

        Assert.Equal(
            "machine.radiation.discovery",
            discovery.Slug);

        var entityId =
            entity.Id;

        var factId =
            fact.Id;

        var discoveryId =
            discovery.Id;

        TestLog.State(
            $"Entity Id = {entityId}");

        TestLog.State(
            $"Fact Id = {factId}");

        TestLog.State(
            $"Discovery Id = {discoveryId}");

        TestLog.State(
            $"Entity and Fact identities differ = " +
            $"{entityId != factId}");

        TestLog.State(
            $"Entity and Discovery identities differ = " +
            $"{entityId != discoveryId}");

        TestLog.State(
            $"Fact and Discovery identities differ = " +
            $"{factId != discoveryId}");

        /*
         * Re-reading the identity properties must return
         * the same stable identity values.
         */

        Assert.Equal(
            entityId,
            entity.Id);

        Assert.Equal(
            factId,
            fact.Id);

        Assert.Equal(
            discoveryId,
            discovery.Id);

        TestLog.Pass(
            "Entity, Fact, and Discovery retained " +
            "independent stable identities.");
    }

    [Fact]
    public void
        Fact_rejects_discovery_owned_by_another_fact()
    {
        TestLog.Section(
            "ENTITY GATE E3-C - FOREIGN DISCOVERY REJECTION");

        TestLog.Test(
            "Fact rejects a Discovery derived from another Fact.");

        var entity =
            new TestEntity(
                "machine.entity");

        var radiationFact =
            new TestFact(
                "machine.radiation",
                entity,
                "Radiation",
                20.0);

        var heatFact =
            new TestFact(
                "machine.heat",
                entity,
                "Heat",
                50.0);

        var heatDiscovery =
            new TestDiscovery(
                "machine.heat.discovery",
                heatFact);

        Assert.True(
            entity.RegisterFact(
                radiationFact));

        Assert.True(
            entity.RegisterFact(
                heatFact));

        Assert.True(
            heatFact.RegisterDiscovery(
                heatDiscovery));

        TestLog.State(
            $"Radiation Fact = {radiationFact.Id}");

        TestLog.State(
            $"Heat Fact = {heatFact.Id}");

        TestLog.State(
            $"Heat Discovery Fact = " +
            $"{heatDiscovery.Fact.Id}");

        TestLog.Task(
            "Attempting to register Heat Discovery " +
            "against Radiation Fact.");

        TestLog.Expected(
            "ArgumentException");

        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    radiationFact.RegisterDiscovery(
                        heatDiscovery));

        Assert.Empty(
            radiationFact.Discoveries);

        Assert.Single(
            heatFact.Discoveries);

        Assert.Same(
            heatDiscovery,
            heatFact.Discoveries[0]);

        Assert.Same(
            heatFact,
            heatDiscovery.Fact);

        Assert.Equal(
            2,
            entity.Facts.Count);

        TestLog.State(
            $"Rejected with = " +
            $"{exception.GetType().Name}");

        TestLog.State(
            $"Radiation Discovery count = " +
            $"{radiationFact.Discoveries.Count}");

        TestLog.State(
            $"Heat Discovery count = " +
            $"{heatFact.Discoveries.Count}");

        TestLog.Pass(
            "Foreign Discovery was rejected without " +
            "altering either Fact.");
    }

    [Fact]
    public void
        Fact_rejects_duplicate_discovery_registration()
    {
        TestLog.Section(
            "ENTITY GATE E3-D - DUPLICATE DISCOVERY");

        TestLog.Test(
            "Fact rejects duplicate registration of the " +
            "same Discovery reference.");

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
                "machine.radiation.discovery",
                fact);

        Assert.True(
            entity.RegisterFact(
                fact));

        TestLog.Task(
            "Registering Discovery for the first time.");

        var firstRegistration =
            fact.RegisterDiscovery(
                discovery);

        TestLog.Task(
            "Registering the exact Discovery again.");

        var secondRegistration =
            fact.RegisterDiscovery(
                discovery);

        Assert.True(
            firstRegistration);

        Assert.False(
            secondRegistration);

        Assert.Single(
            fact.Discoveries);

        Assert.Same(
            discovery,
            fact.Discoveries[0]);

        Assert.Same(
            fact,
            discovery.Fact);

        TestLog.State(
            $"First registration = {firstRegistration}");

        TestLog.State(
            $"Second registration = {secondRegistration}");

        TestLog.State(
            $"Discovery count = {fact.Discoveries.Count}");

        TestLog.State(
            $"Exact Discovery reference retained = " +
            $"{ReferenceEquals(
                discovery,
                fact.Discoveries[0])}");

        TestLog.Pass(
            "Duplicate Discovery registration was rejected.");
    }

    [Fact]
    public void
        Removing_discovery_preserves_original_provenance()
    {
        TestLog.Section(
            "ENTITY GATE E3-E - DISCOVERY REMOVAL");

        TestLog.Test(
            "Removing a Discovery from a Fact preserves " +
            "the original objects and provenance.");

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
                "machine.radiation.discovery",
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

        TestLog.State(
            $"Discovery count before removal = " +
            $"{fact.Discoveries.Count}");

        TestLog.Task(
            "Removing Discovery from the Fact collection.");

        var removed =
            fact.UnregisterDiscovery(
                discovery);

        Assert.True(
            removed);

        Assert.Empty(
            fact.Discoveries);

        /*
         * Collection membership is gone, but the objects and
         * their original provenance remain unchanged.
         */

        Assert.Equal(
            entityId,
            entity.Id);

        Assert.Equal(
            factId,
            fact.Id);

        Assert.Equal(
            discoveryId,
            discovery.Id);

        Assert.Same(
            fact,
            discovery.Fact);

        Assert.Same(
            entity,
            discovery.Fact.Entity);

        Assert.Single(
            entity.Facts);

        Assert.Same(
            fact,
            entity.Facts[0]);

        TestLog.State(
            $"Removal succeeded = {removed}");

        TestLog.State(
            $"Discovery count after removal = " +
            $"{fact.Discoveries.Count}");

        TestLog.State(
            $"Discovery still references exact Fact = " +
            $"{ReferenceEquals(
                fact,
                discovery.Fact)}");

        TestLog.State(
            $"Discovery lineage still reaches Entity = " +
            $"{ReferenceEquals(
                entity,
                discovery.Fact.Entity)}");

        TestLog.Pass(
            "Discovery collection membership was removed.");

        TestLog.Pass(
            "Original Discovery provenance remained intact.");
    }

    [Fact]
    public void
        One_fact_can_own_multiple_independent_discoveries()
    {
        TestLog.Section(
            "ENTITY GATE E3-F - MULTIPLE DISCOVERIES");

        TestLog.Test(
            "One Fact can own multiple independent " +
            "Discoveries.");

        var entity =
            new TestEntity(
                "machine.entity");

        var fact =
            new TestFact(
                "machine.radiation",
                entity,
                "Radiation",
                20.0);

        Assert.True(
            entity.RegisterFact(
                fact));

        var first =
            new TestDiscovery(
                "machine.radiation.discovery.first",
                fact);

        var second =
            new TestDiscovery(
                "machine.radiation.discovery.second",
                fact);

        var third =
            new TestDiscovery(
                "machine.radiation.discovery.third",
                fact);

        TestLog.Task(
            "Registering three independent Discoveries " +
            "against one exact Fact.");

        Assert.True(
            fact.RegisterDiscovery(
                first));

        Assert.True(
            fact.RegisterDiscovery(
                second));

        Assert.True(
            fact.RegisterDiscovery(
                third));

        Assert.Equal(
            3,
            fact.Discoveries.Count);

        Assert.Same(
            first,
            fact.Discoveries[0]);

        Assert.Same(
            second,
            fact.Discoveries[1]);

        Assert.Same(
            third,
            fact.Discoveries[2]);

        Assert.Same(
            fact,
            first.Fact);

        Assert.Same(
            fact,
            second.Fact);

        Assert.Same(
            fact,
            third.Fact);

        Assert.NotEqual(
            first.Id,
            second.Id);

        Assert.NotEqual(
            first.Id,
            third.Id);

        Assert.NotEqual(
            second.Id,
            third.Id);

        TestLog.State(
            $"Fact = {fact.Slug} / {fact.Id}");

        TestLog.State(
            $"Discovery count = {fact.Discoveries.Count}");

        TestLog.State(
            $"First Discovery = {first.Slug} / {first.Id}");

        TestLog.State(
            $"Second Discovery = {second.Slug} / {second.Id}");

        TestLog.State(
            $"Third Discovery = {third.Slug} / {third.Id}");

        TestLog.State(
            $"All Discoveries reference exact Fact = " +
            $"{ReferenceEquals(fact, first.Fact) &&
               ReferenceEquals(fact, second.Fact) &&
               ReferenceEquals(fact, third.Fact)}");

        TestLog.Pass(
            "One Fact retained multiple independent " +
            "Discoveries.");

        TestLog.Pass(
            "ENTITY GATE E3 CERTIFIED.");
    }
}