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
    public sealed class EntityFactResolutionCertificationTests
    {
        [Fact]
        public void FactDoesNotResolveBeforeRequiredContributorCount()
        {
            var entity = new TestEntity("jerry");
            var fact = CreateFact(entity, 3);

            fact.AddEvidenceItem(CreateValidatedEvidence(entity, "tom", 0.92, false));
            fact.AddEvidenceItem(CreateValidatedEvidence(entity, "boat-ramp", 0.86, false));

            Assert.Equal(2, fact.CurrentEvidenceCount);
            Assert.Equal(3, fact.RequiredEvidenceCount);
            Assert.Equal(2.0 / 3.0, fact.EvidencePercentage, 10);
            Assert.False(fact.IsProven);
        }

        [Fact]
        public void FactResolvesExactlyAtRequiredContributorCount()
        {
            var entity = new TestEntity("jerry");
            var fact = CreateFact(entity, 3);

            fact.AddEvidenceItem(CreateValidatedEvidence(entity, "tom", 0.92, false));
            fact.AddEvidenceItem(CreateValidatedEvidence(entity, "boat-ramp", 0.86, false));
            fact.AddEvidenceItem(CreateValidatedEvidence(entity, "dock-talk", 0.81, false));

            Assert.Equal(3, fact.CurrentEvidenceCount);
            Assert.Equal(3, fact.RequiredEvidenceCount);
            Assert.Equal(1.0, fact.EvidencePercentage);
            Assert.True(fact.IsProven);
        }

        [Fact]
        public void ResolutionPercentageCapsAtOneAfterThreshold()
        {
            var entity = new TestEntity("jerry");
            var fact = CreateFact(entity, 2);

            fact.AddEvidenceItem(CreateValidatedEvidence(entity, "one", 0.92, false));
            fact.AddEvidenceItem(CreateValidatedEvidence(entity, "two", 0.91, false));
            fact.AddEvidenceItem(CreateValidatedEvidence(entity, "three", 0.90, false));

            Assert.Equal(3, fact.CurrentEvidenceCount);
            Assert.True(fact.IsProven);
            Assert.Equal(1.0, fact.EvidencePercentage);
        }

        [Fact]
        public void RejectedEvidenceRemainsHistoricalWithoutIncreasingResolutionCount()
        {
            var entity = new TestEntity("jerry");
            var fact = CreateFact(entity, 2);
            var accepted = CreateValidatedEvidence(entity, "accepted", 0.92, false);
            var rejected = CreateValidatedEvidence(entity, "rejected", 0.40, false);

            fact.AddEvidenceItem(accepted);
            fact.AddEvidenceItem(rejected);

            Assert.Equal(2, fact.Evidence.Count);
            Assert.Equal(1, fact.CurrentEvidenceCount);
            Assert.Equal(0.5, fact.EvidencePercentage);
            Assert.False(fact.IsProven);
            Assert.Contains(rejected, fact.Evidence);
        }

        [Fact]
        public void UnvalidatedEvidenceRemainsHistoricalWithoutIncreasingResolutionCount()
        {
            var entity = new TestEntity("jerry");
            var fact = CreateFact(entity, 1);
            var evidence = CreateEvidence(entity, "unvalidated");

            fact.AddEvidenceItem(evidence);

            Assert.Single(fact.Evidence);
            Assert.Equal(0, fact.CurrentEvidenceCount);
            Assert.Equal(0.0, fact.EvidencePercentage);
            Assert.False(fact.IsProven);
        }

        [Fact]
        public void DuplicateEvidenceCannotArtificiallyIncreaseResolutionCount()
        {
            var entity = new TestEntity("jerry");
            var fact = CreateFact(entity, 2);
            var evidence = CreateValidatedEvidence(entity, "tom", 0.92, false);

            Assert.True(fact.AddEvidenceItem(evidence));
            Assert.False(fact.AddEvidenceItem(evidence));

            Assert.Single(fact.Evidence);
            Assert.Equal(1, fact.CurrentEvidenceCount);
            Assert.False(fact.IsProven);
        }

        [Fact]
        public void RemovingContributingEvidenceCanReturnResolvedFactToUnresolvedState()
        {
            var entity = new TestEntity("jerry");
            var fact = CreateFact(entity, 2);
            var first = CreateValidatedEvidence(entity, "first", 0.92, false);
            var second = CreateValidatedEvidence(entity, "second", 0.90, false);

            fact.AddEvidenceItem(first);
            fact.AddEvidenceItem(second);

            Assert.True(fact.IsProven);
            Assert.Equal(2, fact.CurrentEvidenceCount);

            Assert.True(fact.RemoveEvidenceItem(second));

            Assert.False(fact.IsProven);
            Assert.Equal(1, fact.CurrentEvidenceCount);
            Assert.Equal(0.5, fact.EvidencePercentage);
        }

        [Fact]
        public void RemovingRejectedEvidenceDoesNotChangeResolutionCount()
        {
            var entity = new TestEntity("jerry");
            var fact = CreateFact(entity, 1);
            var accepted = CreateValidatedEvidence(entity, "accepted", 0.92, false);
            var rejected = CreateValidatedEvidence(entity, "rejected", 0.40, false);

            fact.AddEvidenceItem(accepted);
            fact.AddEvidenceItem(rejected);

            Assert.True(fact.IsProven);
            Assert.Equal(1, fact.CurrentEvidenceCount);

            Assert.True(fact.RemoveEvidenceItem(rejected));

            Assert.True(fact.IsProven);
            Assert.Equal(1, fact.CurrentEvidenceCount);
            Assert.Single(fact.Evidence);
        }

        [Fact]
        public void FactRejectsNonPositiveRequiredEvidenceCount()
        {
            var entity = new TestEntity("jerry");
            var policy = new MinimumBelievabilityPolicy(0.80);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new TestFact(entity, "InvalidZero", true, 0, policy));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new TestFact(entity, "InvalidNegative", true, -1, policy));
        }

        [Fact]
        public void ResolutionAliasesRemainConsistentWithFactProperties()
        {
            var entity = new TestEntity("jerry");
            var fact = CreateFact(entity, 2);
            fact.AddEvidenceItem(CreateValidatedEvidence(entity, "one", 0.92, false));

            Assert.Equal(fact.RequiredEvidenceCount, fact.RequiredCount);
            Assert.Equal(fact.CurrentEvidenceCount, fact.CurrentCount);
            Assert.Equal(fact.EvidencePercentage, fact.Percentage);
            Assert.Equal(fact.IsProven, fact.IsResolved);
        }

        private static TestFact CreateFact(IEntity entity, int requiredEvidenceCount)
        {
            return new TestFact(
                entity,
                "JerryBelievesNorthCoveHasLargeBass",
                true,
                requiredEvidenceCount,
                new MinimumBelievabilityPolicy(0.80));
        }

        private static DiscoveryEvidence CreateEvidence(IEntity entity, string externalId)
        {
            return new DiscoveryEvidence(
                entity,
                new Dictionary<string, object?>
                {
                    { "ClaimedLocation", "North Cove" },
                    { "PriorReport", "North Cove closed for kelp removal" }
                },
                "North Cove has monster bass",
                1.0,
                1.0,
                new List<IBaseObject>(),
                null,
                externalId,
                "Jerry fishing evidence");
        }

        private static DiscoveryEvidence CreateValidatedEvidence(
            IEntity entity,
            string externalId,
            double believability,
            bool isTrue)
        {
            var evidence = CreateEvidence(entity, externalId);
            var proof = new TestProof(believability, isTrue, "Controlled E7-E proof.");
            Assert.True(evidence.ApplyProof(proof.Validate(evidence)));
            return evidence;
        }
    }
}
