using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Entities.Abstractions;
using WildsEngine.Core.Entities.Evidences;
using WildsEngine.Core.Entities.Policies;
using WildsEngine.Core.Tests.Entities.Fixtures;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class EntityFactPolicyCertificationTests
    {
        [Fact]
        public void FactRetainsExactProductionContributionPolicy()
        {
            var entity = new TestEntity("jerry");
            var policy = new TrueEvidencePolicy();
            var fact = CreateFact(entity, policy);

            Assert.Same(policy, fact.ContributionPolicy);
        }

        [Fact]
        public void FactRejectsNullContributionPolicy()
        {
            Assert.Throws<ArgumentNullException>(() =>
                CreateFact(new TestEntity("jerry"), null!));
        }

        [Fact]
        public void UnvalidatedEvidenceCannotContributeThroughProductionPolicy()
        {
            var entity = new TestEntity("jerry");
            var evidence = CreateEvidence(entity);
            var fact = CreateFact(entity, new TrueEvidencePolicy());

            Assert.True(fact.AddEvidenceItem(evidence));
            Assert.Single(fact.Evidence);
            Assert.Equal(0, fact.CurrentEvidenceCount);
            Assert.False(fact.IsProven);
        }

        [Fact]
        public void ProductionPolicyRejectionPreservesEvidenceMembership()
        {
            var entity = new TestEntity("jerry");
            var evidence = CreateValidatedEvidence(entity, 0.92, false);
            var fact = CreateFact(entity, new TrueEvidencePolicy());

            Assert.True(fact.AddEvidenceItem(evidence));
            Assert.Single(fact.Evidence);
            Assert.Same(evidence, fact.Evidence[0]);
            Assert.Equal(0, fact.CurrentEvidenceCount);
            Assert.False(fact.IsProven);
        }

        [Fact]
        public void ForeignEntityEvidenceIsRejectedBeforeProductionPolicyInterpretation()
        {
            var factEntity = new TestEntity("jerry");
            var foreignEntity = new TestEntity("tom");
            var evidence = CreateValidatedEvidence(foreignEntity, 0.92, false);
            var fact = CreateFact(factEntity, new FalseEvidencePolicy());

            Assert.Throws<ArgumentException>(() => fact.AddEvidenceItem(evidence));
            Assert.Empty(fact.Evidence);
            Assert.False(fact.IsProven);
        }

        [Fact]
        public void TruePolicyRejectsBelievableFalseEvidence()
        {
            var entity = new TestEntity("jerry");
            var evidence = CreateValidatedEvidence(entity, 0.92, false);
            var fact = CreateFact(entity, new TrueEvidencePolicy());

            fact.AddEvidenceItem(evidence);

            Assert.False(fact.IsProven);
            Assert.Equal(0, fact.CurrentEvidenceCount);
        }

        [Fact]
        public void FalsePolicyAcceptsBelievableFalseEvidence()
        {
            var entity = new TestEntity("jerry");
            var evidence = CreateValidatedEvidence(entity, 0.92, false);
            var fact = CreateFact(entity, new FalseEvidencePolicy());

            fact.AddEvidenceItem(evidence);

            Assert.True(fact.IsProven);
            Assert.Equal(1, fact.CurrentEvidenceCount);
        }

        [Fact]
        public void MinimumBelievabilityPolicyAcceptsBelievableFalseEvidence()
        {
            var entity = new TestEntity("jerry");
            var evidence = CreateValidatedEvidence(entity, 0.92, false);
            var fact = CreateFact(entity, new MinimumBelievabilityPolicy(0.80));

            fact.AddEvidenceItem(evidence);

            Assert.True(fact.IsProven);
            Assert.False(evidence.ProofResult.IsTrue);
            Assert.Equal(0.92, evidence.ProofResult.Believability);
        }

        [Fact]
        public void BelievableFalsePolicyAcceptsDocumentedCanonicalCase()
        {
            var entity = new TestEntity("jerry");
            var evidence = CreateValidatedEvidence(entity, 0.92, false);
            var fact = CreateFact(entity, new BelievableFalseEvidencePolicy(0.80));

            fact.AddEvidenceItem(evidence);

            Assert.True(fact.IsProven);
        }

        [Fact]
        public void SameEvidenceCanResolveDifferentlyUnderDifferentProductionPolicies()
        {
            var entity = new TestEntity("jerry");
            var evidence = CreateValidatedEvidence(entity, 0.92, false);
            var truthFact = CreateFact(entity, new TrueEvidencePolicy(), "ObjectivelyTrueReport");
            var beliefFact = CreateFact(entity, new MinimumBelievabilityPolicy(0.80), "JerryBelievesReport");

            truthFact.AddEvidenceItem(evidence);
            beliefFact.AddEvidenceItem(evidence);

            Assert.False(truthFact.IsProven);
            Assert.True(beliefFact.IsProven);
            Assert.Same(evidence, truthFact.Evidence[0]);
            Assert.Same(evidence, beliefFact.Evidence[0]);
        }

        [Fact]
        public void ProductionPolicyInterpretationDoesNotMutateProofResult()
        {
            var entity = new TestEntity("jerry");
            var evidence = CreateValidatedEvidence(entity, 0.92, false);
            var result = evidence.ProofResult;
            var truthFact = CreateFact(entity, new TrueEvidencePolicy(), "TruthFact");
            var beliefFact = CreateFact(entity, new MinimumBelievabilityPolicy(0.80), "BeliefFact");

            truthFact.AddEvidenceItem(evidence);
            beliefFact.AddEvidenceItem(evidence);

            Assert.Same(result, evidence.ProofResult);
            Assert.Equal(0.92, evidence.ProofResult.Believability);
            Assert.False(evidence.ProofResult.IsTrue);
        }

        [Fact]
        public void DocumentedAllPolicyCompositionRequiresEveryProductionPolicy()
        {
            var entity = new TestEntity("jerry");
            var matching = CreateValidatedEvidence(entity, 0.92, false, "North Cove", "Success", "matching");
            var wrongOutcome = CreateValidatedEvidence(entity, 0.92, false, "North Cove", "Failure", "wrong-outcome");
            var policy = new AllEvidencePolicies(
                new IEvidenceContributionPolicy[]
                {
                    new MinimumBelievabilityPolicy(0.70),
                    new ConditionEqualsPolicy("ClaimedLocation", "North Cove"),
                    new OutcomeEqualsPolicy("Success")
                });
            var matchingFact = CreateFact(entity, policy, "MatchingComposite");
            var rejectedFact = CreateFact(entity, policy, "RejectedComposite");

            matchingFact.AddEvidenceItem(matching);
            rejectedFact.AddEvidenceItem(wrongOutcome);

            Assert.True(matchingFact.IsProven);
            Assert.False(rejectedFact.IsProven);
        }

        private static TestFact CreateFact(
            IEntity entity,
            IEvidenceContributionPolicy policy,
            string name = "NorthCoveHasLargeBass")
        {
            return new TestFact(entity, name, true, 1, policy);
        }

        private static DiscoveryEvidence CreateEvidence(
            IEntity entity,
            string claimedLocation = "North Cove",
            object? outcome = null,
            string externalId = "tom-report")
        {
            return new DiscoveryEvidence(
                entity,
                new Dictionary<string, object?>
                {
                    { "ClaimedLocation", claimedLocation },
                    { "PriorReport", "North Cove closed for kelp removal" }
                },
                outcome ?? "Tom reports a monster bass at North Cove",
                1.0,
                1.0,
                new List<IBaseObject>(),
                null,
                externalId,
                "Tom fishing report");
        }

        private static DiscoveryEvidence CreateValidatedEvidence(
            IEntity entity,
            double believability,
            bool isTrue,
            string claimedLocation = "North Cove",
            object? outcome = null,
            string externalId = "tom-report")
        {
            var evidence = CreateEvidence(entity, claimedLocation, outcome, externalId);
            var proof = new TestProof(believability, isTrue, "Controlled E7-D proof.");
            Assert.True(evidence.ApplyProof(proof.Validate(evidence)));
            return evidence;
        }
    }
}
