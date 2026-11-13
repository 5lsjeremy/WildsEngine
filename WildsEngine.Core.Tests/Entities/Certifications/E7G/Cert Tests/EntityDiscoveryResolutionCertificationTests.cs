using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Evidences;
using WildsEngine.Discoveries.Policies;
using WildsEngine.Core.Tests.Entities.Fixtures;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class EntityDiscoveryResolutionCertificationTests
    {
        [Fact]
        public void DiscoveryBeginsUnavailable()
        {
            var discovery = CreateDiscovery(new TestEntity("jerry"), 3);

            Assert.Empty(discovery.Facts);
            Assert.Equal(0, discovery.CurrentFactCount);
            Assert.Equal(3, discovery.RequiredFactCount);
            Assert.Equal(0.0, discovery.FactPercentage);
            Assert.False(discovery.IsAvailable);
        }

        [Fact]
        public void UnprovenFactRemainsHistoricalWithoutContributing()
        {
            var entity = new TestEntity("jerry");
            var discovery = CreateDiscovery(entity, 1);
            var fact = CreateUnprovenFact(entity, "RedKelpWarning", "Stay away");

            Assert.True(discovery.AddFactItem(fact));
            Assert.Single(discovery.Facts);
            Assert.Equal(0, discovery.CurrentFactCount);
            Assert.False(discovery.IsAvailable);
        }

        [Fact]
        public void ProvenFactContributesToDiscovery()
        {
            var entity = new TestEntity("jerry");
            var discovery = CreateDiscovery(entity, 1);
            var fact = CreateProvenFact(entity, "MonsterBassLocation", "North Cove", "fact-one");

            discovery.AddFactItem(fact);

            Assert.Equal(1, discovery.CurrentFactCount);
            Assert.True(discovery.IsAvailable);
        }

        [Fact]
        public void DiscoveryDoesNotResolveBelowRequiredFactCount()
        {
            var entity = new TestEntity("jerry");
            var discovery = CreateDiscovery(entity, 3);

            discovery.AddFactItem(CreateProvenFact(entity, "BassLocation", "North Cove", "one"));
            discovery.AddFactItem(CreateProvenFact(entity, "BoatTraffic", "North Cove", "two"));

            Assert.Equal(2, discovery.CurrentFactCount);
            Assert.Equal(2.0 / 3.0, discovery.FactPercentage, 10);
            Assert.False(discovery.IsAvailable);
        }

        [Fact]
        public void DiscoveryResolvesExactlyAtRequiredFactCount()
        {
            var entity = new TestEntity("jerry");
            var discovery = CreateDiscovery(entity, 3);

            discovery.AddFactItem(CreateProvenFact(entity, "BassLocation", "North Cove", "one"));
            discovery.AddFactItem(CreateProvenFact(entity, "BoatTraffic", "North Cove", "two"));
            discovery.AddFactItem(CreateProvenFact(entity, "DockReport", "North Cove", "three"));

            Assert.Equal(3, discovery.CurrentFactCount);
            Assert.Equal(1.0, discovery.FactPercentage);
            Assert.True(discovery.IsAvailable);
        }

        [Fact]
        public void DiscoveryPercentageCapsAtOneBeyondThreshold()
        {
            var entity = new TestEntity("jerry");
            var discovery = CreateDiscovery(entity, 2);

            discovery.AddFactItem(CreateProvenFact(entity, "One", true, "one"));
            discovery.AddFactItem(CreateProvenFact(entity, "Two", true, "two"));
            discovery.AddFactItem(CreateProvenFact(entity, "Three", true, "three"));

            Assert.Equal(3, discovery.CurrentFactCount);
            Assert.Equal(1.0, discovery.FactPercentage);
            Assert.True(discovery.IsAvailable);
        }

        [Fact]
        public void DuplicateFactCannotArtificiallyResolveDiscovery()
        {
            var entity = new TestEntity("jerry");
            var discovery = CreateDiscovery(entity, 2);
            var fact = CreateProvenFact(entity, "BassLocation", "North Cove", "one");

            Assert.True(discovery.AddFactItem(fact));
            Assert.False(discovery.AddFactItem(fact));

            Assert.Single(discovery.Facts);
            Assert.Equal(1, discovery.CurrentFactCount);
            Assert.False(discovery.IsAvailable);
        }

        [Fact]
        public void ForeignEntityFactIsRejected()
        {
            var jerry = new TestEntity("jerry");
            var tom = new TestEntity("tom");
            var discovery = CreateDiscovery(jerry, 1);
            var tomFact = CreateProvenFact(tom, "MonsterBassLocation", "Miller's Point", "tom-fact");

            Assert.Throws<ArgumentException>(() => discovery.AddFactItem(tomFact));
            Assert.Empty(discovery.Facts);
            Assert.False(discovery.IsAvailable);
        }

        [Fact]
        public void MatchingFactNameDoesNotBypassEntityOwnership()
        {
            var jerry = new TestEntity("jerry");
            var tom = new TestEntity("tom");
            var discovery = CreateDiscovery(jerry, 1);
            var tomFact = CreateProvenFact(tom, "MonsterBassLocation", "North Cove", "tom-match");

            Assert.Throws<ArgumentException>(() => discovery.AddFactItem(tomFact));
            Assert.Equal("MonsterBassLocation", tomFact.Name);
            Assert.Empty(discovery.Facts);
        }

        [Fact]
        public void RemovingContributingFactCanMakeDiscoveryUnavailableAgain()
        {
            var entity = new TestEntity("jerry");
            var discovery = CreateDiscovery(entity, 2);
            var first = CreateProvenFact(entity, "One", true, "one");
            var second = CreateProvenFact(entity, "Two", true, "two");

            discovery.AddFactItem(first);
            discovery.AddFactItem(second);
            Assert.True(discovery.IsAvailable);

            Assert.True(discovery.RemoveFactItem(second));

            Assert.Equal(1, discovery.CurrentFactCount);
            Assert.Equal(0.5, discovery.FactPercentage);
            Assert.False(discovery.IsAvailable);
        }

        [Fact]
        public void RemovingUnprovenContradictoryFactDoesNotChangeResolution()
        {
            var entity = new TestEntity("jerry");
            var discovery = CreateDiscovery(entity, 1);
            var proven = CreateProvenFact(entity, "FishingOpportunity", "Go to North Cove", "go");
            var warning = CreateUnprovenFact(entity, "RedKelpWarning", "Stay away from North Cove");

            discovery.AddFactItem(proven);
            discovery.AddFactItem(warning);

            Assert.True(discovery.IsAvailable);
            Assert.Equal(1, discovery.CurrentFactCount);
            Assert.Equal(2, discovery.Facts.Count);

            Assert.True(discovery.RemoveFactItem(warning));

            Assert.True(discovery.IsAvailable);
            Assert.Equal(1, discovery.CurrentFactCount);
            Assert.Single(discovery.Facts);
        }

        [Fact]
        public void ContradictoryProvenFactsBothContributeWhenBothAreProven()
        {
            var entity = new TestEntity("jerry");
            var discovery = CreateDiscovery(entity, 2);
            var opportunity = CreateProvenFact(entity, "FishingOpportunity", "Go to North Cove", "opportunity");
            var warning = CreateProvenFact(entity, "RedKelpWarning", "Stay away from North Cove", "warning");

            discovery.AddFactItem(opportunity);
            discovery.AddFactItem(warning);

            Assert.Equal(2, discovery.CurrentFactCount);
            Assert.True(discovery.IsAvailable);
            Assert.NotEqual(opportunity.Value, warning.Value);
        }

        [Fact]
        public void ContradictionDoesNotSilentlyReconcileFactValues()
        {
            var entity = new TestEntity("jerry");
            var discovery = CreateDiscovery(entity, 2);
            var northCove = CreateProvenFact(entity, "MonsterBassLocation", "North Cove", "north");
            var millersPoint = CreateProvenFact(entity, "MonsterBassLocation", "Miller's Point", "millers");

            discovery.AddFactItem(northCove);
            discovery.AddFactItem(millersPoint);

            Assert.True(discovery.IsAvailable);
            Assert.Equal("North Cove", northCove.Value);
            Assert.Equal("Miller's Point", millersPoint.Value);
            Assert.NotEqual(northCove.Value, millersPoint.Value);
            Assert.Same(northCove, discovery.Facts[0]);
            Assert.Same(millersPoint, discovery.Facts[1]);
        }

        [Fact]
        public void FactOrderDoesNotChangeResolutionOutcome()
        {
            var entity = new TestEntity("jerry");
            var first = CreateProvenFact(entity, "One", true, "one");
            var second = CreateProvenFact(entity, "Two", true, "two");
            var forward = CreateDiscovery(entity, 2, "forward");
            var reverse = CreateDiscovery(entity, 2, "reverse");

            forward.AddFactItem(first);
            forward.AddFactItem(second);
            reverse.AddFactItem(second);
            reverse.AddFactItem(first);

            Assert.Equal(forward.CurrentFactCount, reverse.CurrentFactCount);
            Assert.Equal(forward.FactPercentage, reverse.FactPercentage);
            Assert.Equal(forward.IsAvailable, reverse.IsAvailable);
        }

        [Fact]
        public void DiscoveryRejectsNullFact()
        {
            var discovery = CreateDiscovery(new TestEntity("jerry"), 1);
            Assert.Throws<ArgumentNullException>(() => discovery.AddFactItem(null!));
        }

        [Fact]
        public void DiscoveryRejectsNonPositiveRequiredFactCount()
        {
            var entity = new TestEntity("jerry");
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateDiscovery(entity, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateDiscovery(entity, -1));
        }

        [Fact]
        public void ResolutionAliasesRemainConsistent()
        {
            var entity = new TestEntity("jerry");
            var discovery = CreateDiscovery(entity, 2);
            discovery.AddFactItem(CreateProvenFact(entity, "One", true, "one"));

            Assert.Equal(discovery.RequiredFactCount, discovery.RequiredCount);
            Assert.Equal(discovery.CurrentFactCount, discovery.CurrentCount);
            Assert.Equal(discovery.FactPercentage, discovery.Percentage);
            Assert.Equal(discovery.IsAvailable, discovery.IsResolved);
        }

        private static TestDiscovery CreateDiscovery(
            IEntity entity,
            int requiredFactCount,
            string externalId = "jerry-discovery")
        {
            return new TestDiscovery(
                entity,
                requiredFactCount,
                "North Cove Fishing Opportunity",
                externalId);
        }

        private static TestFact CreateUnprovenFact(
            IEntity entity,
            string name,
            object value)
        {
            return new TestFact(
                entity,
                name,
                value,
                1,
                new MinimumBelievabilityPolicy(0.80));
        }

        private static TestFact CreateProvenFact(
            IEntity entity,
            string name,
            object value,
            string externalId)
        {
            var fact = new TestFact(
                entity,
                name,
                value,
                1,
                new MinimumBelievabilityPolicy(0.80),
                externalId);

            var evidence = new DiscoveryEvidence(
                entity,
                new Dictionary<string, object?>
                {
                    { "FactName", name }
                },
                value,
                1.0,
                1.0,
                new List<IBaseObject>(),
                null,
                externalId + "-evidence",
                name + " evidence");

            var proof = new TestProof(0.90, true, "Controlled E7-G proof.");
            Assert.True(evidence.ApplyProof(proof.Validate(evidence)));
            Assert.True(fact.AddEvidenceItem(evidence));
            Assert.True(fact.IsProven);
            return fact;
        }
    }
}
