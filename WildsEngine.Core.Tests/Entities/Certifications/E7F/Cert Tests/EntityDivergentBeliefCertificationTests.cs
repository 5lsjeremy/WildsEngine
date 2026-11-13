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
    public sealed class EntityDivergentBeliefCertificationTests
    {
        [Fact]
        public void TwoEntitiesCanResolveContradictoryFactsAboutSameSituation()
        {
            var tom = new TestEntity("tom");
            var jerry = new TestEntity("jerry");

            var tomFact = CreateFact(tom, "MonsterBassLocation", "Miller's Point");
            var jerryFact = CreateFact(jerry, "MonsterBassLocation", "North Cove");

            tomFact.AddEvidenceItem(CreateValidatedEvidence(
                tom,
                "Tom personally caught the fish at Miller's Point",
                "Miller's Point",
                1.0,
                true,
                "tom-direct"));

            jerryFact.AddEvidenceItem(CreateValidatedEvidence(
                jerry,
                "Tom told Jerry the fish came from North Cove",
                "North Cove",
                0.92,
                false,
                "jerry-hears-tom"));

            Assert.True(tomFact.IsProven);
            Assert.True(jerryFact.IsProven);
            Assert.Equal("Miller's Point", tomFact.Value);
            Assert.Equal("North Cove", jerryFact.Value);
            Assert.NotSame(tomFact.Entity, jerryFact.Entity);
        }

        [Fact]
        public void EachEntityRetainsOnlyItsOwnEvidenceReferences()
        {
            var tom = new TestEntity("tom");
            var jerry = new TestEntity("jerry");
            var tomEvidence = CreateValidatedEvidence(tom, "Tom direct catch", "Miller's Point", 1.0, true, "tom");
            var jerryEvidence = CreateValidatedEvidence(jerry, "Jerry hears North Cove", "North Cove", 0.92, false, "jerry");
            var tomFact = CreateFact(tom, "TomLocation", "Miller's Point");
            var jerryFact = CreateFact(jerry, "JerryLocation", "North Cove");

            tomFact.AddEvidenceItem(tomEvidence);
            jerryFact.AddEvidenceItem(jerryEvidence);

            Assert.Single(tomFact.Evidence);
            Assert.Single(jerryFact.Evidence);
            Assert.Same(tomEvidence, tomFact.Evidence[0]);
            Assert.Same(jerryEvidence, jerryFact.Evidence[0]);
            Assert.DoesNotContain(jerryEvidence, tomFact.Evidence);
            Assert.DoesNotContain(tomEvidence, jerryFact.Evidence);
        }

        [Fact]
        public void FactRejectsOtherEntitysEvidenceEvenWhenClaimValueMatches()
        {
            var tom = new TestEntity("tom");
            var jerry = new TestEntity("jerry");
            var tomFact = CreateFact(tom, "TomLocation", "Miller's Point");
            var jerryEvidence = CreateValidatedEvidence(jerry, "Jerry repeats Miller's Point", "Miller's Point", 0.95, true, "jerry-match");

            Assert.Throws<ArgumentException>(() => tomFact.AddEvidenceItem(jerryEvidence));
            Assert.Empty(tomFact.Evidence);
            Assert.False(tomFact.IsProven);
        }

        [Fact]
        public void SameExternalIdDoesNotMergeEvidenceAcrossEntities()
        {
            var tom = new TestEntity("tom");
            var jerry = new TestEntity("jerry");
            var tomEvidence = CreateValidatedEvidence(tom, "Tom account", "Miller's Point", 1.0, true, "shared-report");
            var jerryEvidence = CreateValidatedEvidence(jerry, "Jerry account", "North Cove", 0.92, false, "shared-report");

            Assert.Equal(tomEvidence.ExternalId, jerryEvidence.ExternalId);
            Assert.NotSame(tomEvidence, jerryEvidence);
            Assert.Same(tom, tomEvidence.Entity);
            Assert.Same(jerry, jerryEvidence.Entity);
            Assert.NotEqual(tomEvidence.Outcome, jerryEvidence.Outcome);
        }

        [Fact]
        public void ResolvingJerrysFactDoesNotResolveTomsUnresolvedFact()
        {
            var tom = new TestEntity("tom");
            var jerry = new TestEntity("jerry");
            var tomFact = CreateFact(tom, "TomLocation", "Miller's Point", requiredCount: 2);
            var jerryFact = CreateFact(jerry, "JerryLocation", "North Cove", requiredCount: 1);

            tomFact.AddEvidenceItem(CreateValidatedEvidence(tom, "Tom direct catch", "Miller's Point", 1.0, true, "tom-one"));
            jerryFact.AddEvidenceItem(CreateValidatedEvidence(jerry, "Tom says North Cove", "North Cove", 0.92, false, "jerry-one"));

            Assert.False(tomFact.IsProven);
            Assert.True(jerryFact.IsProven);
            Assert.Equal(1, tomFact.CurrentEvidenceCount);
            Assert.Equal(1, jerryFact.CurrentEvidenceCount);
        }

        [Fact]
        public void RemovingJerrysEvidenceDoesNotChangeTomsResolvedFact()
        {
            var tom = new TestEntity("tom");
            var jerry = new TestEntity("jerry");
            var tomEvidence = CreateValidatedEvidence(tom, "Tom direct catch", "Miller's Point", 1.0, true, "tom");
            var jerryEvidence = CreateValidatedEvidence(jerry, "Tom says North Cove", "North Cove", 0.92, false, "jerry");
            var tomFact = CreateFact(tom, "TomLocation", "Miller's Point");
            var jerryFact = CreateFact(jerry, "JerryLocation", "North Cove");

            tomFact.AddEvidenceItem(tomEvidence);
            jerryFact.AddEvidenceItem(jerryEvidence);
            Assert.True(tomFact.IsProven);
            Assert.True(jerryFact.IsProven);

            Assert.True(jerryFact.RemoveEvidenceItem(jerryEvidence));

            Assert.True(tomFact.IsProven);
            Assert.False(jerryFact.IsProven);
            Assert.Same(tomEvidence, tomFact.Evidence[0]);
        }

        [Fact]
        public void JerrysBeliefResolutionDoesNotRewriteTomsProofResult()
        {
            var tom = new TestEntity("tom");
            var jerry = new TestEntity("jerry");
            var tomEvidence = CreateValidatedEvidence(tom, "Tom direct catch", "Miller's Point", 1.0, true, "tom");
            var originalTomResult = tomEvidence.ProofResult;
            var jerryEvidence = CreateValidatedEvidence(jerry, "Tom says North Cove", "North Cove", 0.92, false, "jerry");
            var jerryFact = CreateFact(jerry, "JerryLocation", "North Cove");

            jerryFact.AddEvidenceItem(jerryEvidence);

            Assert.True(jerryFact.IsProven);
            Assert.Same(originalTomResult, tomEvidence.ProofResult);
            Assert.True(tomEvidence.ProofResult.IsTrue);
            Assert.Equal(1.0, tomEvidence.ProofResult.Believability);
        }

        [Fact]
        public void ContradictoryFactsCanShareNameWithoutSharingOwnershipOrValue()
        {
            var tom = new TestEntity("tom");
            var jerry = new TestEntity("jerry");
            var tomFact = CreateFact(tom, "MonsterBassLocation", "Miller's Point");
            var jerryFact = CreateFact(jerry, "MonsterBassLocation", "North Cove");

            Assert.Equal(tomFact.Name, jerryFact.Name);
            Assert.NotEqual(tomFact.Value, jerryFact.Value);
            Assert.Same(tom, tomFact.Entity);
            Assert.Same(jerry, jerryFact.Entity);
        }

        [Fact]
        public void DuplicateJerryEvidenceCannotManufactureConsensus()
        {
            var jerry = new TestEntity("jerry");
            var evidence = CreateValidatedEvidence(jerry, "Tom says North Cove", "North Cove", 0.92, false, "jerry-one");
            var fact = CreateFact(jerry, "JerryLocation", "North Cove", requiredCount: 2);

            Assert.True(fact.AddEvidenceItem(evidence));
            Assert.False(fact.AddEvidenceItem(evidence));

            Assert.Single(fact.Evidence);
            Assert.Equal(1, fact.CurrentEvidenceCount);
            Assert.False(fact.IsProven);
        }

        private static TestFact CreateFact(
            IEntity entity,
            string name,
            object value,
            int requiredCount = 1)
        {
            return new TestFact(
                entity,
                name,
                value,
                requiredCount,
                new MinimumBelievabilityPolicy(0.80));
        }

        private static DiscoveryEvidence CreateValidatedEvidence(
            IEntity entity,
            string narrative,
            string claimedLocation,
            double believability,
            bool isTrue,
            string externalId)
        {
            var evidence = new DiscoveryEvidence(
                entity,
                new Dictionary<string, object?>
                {
                    { "ClaimedLocation", claimedLocation },
                    { "Narrative", narrative }
                },
                claimedLocation,
                1.0,
                1.0,
                new List<IBaseObject>(),
                null,
                externalId,
                narrative);

            var proof = new TestProof(believability, isTrue, narrative);
            Assert.True(evidence.ApplyProof(proof.Validate(evidence)));
            return evidence;
        }
    }
}
