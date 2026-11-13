using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Evidences;
using WildsEngine.Discoveries.Policies;
using WildsEngine.Core.Tests.Entities.Fixtures;
using WildsEngine.Tests.Registry;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class EntityProductionPolicyMatrixCertificationTests
    {
        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void TrueEvidencePolicyMatchesTruth(bool isTrue, bool expected)
        {
            Assert.Equal(expected, new TrueEvidencePolicy().Contributes(CreateValidatedEvidence(0.50, isTrue)));
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void FalseEvidencePolicyMatchesFalsehood(bool isTrue, bool expected)
        {
            Assert.Equal(expected, new FalseEvidencePolicy().Contributes(CreateValidatedEvidence(0.50, isTrue)));
        }

        [Fact]
        public void BelievableTrueEvidencePolicyRequiresBothConditions()
        {
            var policy = new BelievableTrueEvidencePolicy(0.80);
            Assert.True(policy.Contributes(CreateValidatedEvidence(0.80, true, "bt-pass")));
            Assert.False(policy.Contributes(CreateValidatedEvidence(0.79, true, "bt-low")));
            Assert.False(policy.Contributes(CreateValidatedEvidence(0.95, false, "bt-false")));
        }

        [Fact]
        public void BelievableFalseEvidencePolicyRequiresBothConditions()
        {
            var policy = new BelievableFalseEvidencePolicy(0.80);
            Assert.True(policy.Contributes(CreateValidatedEvidence(0.80, false, "bf-pass")));
            Assert.False(policy.Contributes(CreateValidatedEvidence(0.79, false, "bf-low")));
            Assert.False(policy.Contributes(CreateValidatedEvidence(0.95, true, "bf-true")));
        }

        [Fact]
        public void TrueButUnbelievableEvidencePolicyRequiresTruthAndMaximumBelievability()
        {
            var policy = new TrueButUnbelievableEvidencePolicy(0.20);
            Assert.True(policy.Contributes(CreateValidatedEvidence(0.20, true, "tu-pass")));
            Assert.False(policy.Contributes(CreateValidatedEvidence(0.21, true, "tu-high")));
            Assert.False(policy.Contributes(CreateValidatedEvidence(0.10, false, "tu-false")));
        }

        [Fact]
        public void MinimumBelievabilityPolicyIsInclusive()
        {
            var policy = new MinimumBelievabilityPolicy(0.80);
            Assert.True(policy.Contributes(CreateValidatedEvidence(0.80, false, "min-at")));
            Assert.False(policy.Contributes(CreateValidatedEvidence(0.79, true, "min-low")));
        }

        [Fact]
        public void MaximumBelievabilityPolicyIsInclusive()
        {
            var policy = new MaximumBelievabilityPolicy(0.20);
            Assert.True(policy.Contributes(CreateValidatedEvidence(0.20, true, "max-at")));
            Assert.False(policy.Contributes(CreateValidatedEvidence(0.21, false, "max-high")));
        }

        [Fact]
        public void BelievabilityRangePolicyIncludesBothBoundaries()
        {
            var policy = new BelievabilityRangePolicy(0.30, 0.70);
            Assert.True(policy.Contributes(CreateValidatedEvidence(0.30, true, "range-min")));
            Assert.True(policy.Contributes(CreateValidatedEvidence(0.70, false, "range-max")));
            Assert.False(policy.Contributes(CreateValidatedEvidence(0.29, true, "range-low")));
            Assert.False(policy.Contributes(CreateValidatedEvidence(0.71, false, "range-high")));
        }

        [Fact]
        public void MinimumStrengthPolicyUsesRawStrength()
        {
            var policy = new MinimumStrengthPolicy(10.0);
            Assert.True(policy.Contributes(CreateValidatedEvidence(0.50, true, "strength-at", strength: 10.0)));
            Assert.False(policy.Contributes(CreateValidatedEvidence(0.50, true, "strength-low", strength: 9.99)));
        }

        [Fact]
        public void MinimumNormalizedStrengthPolicyUsesNormalizedStrength()
        {
            var policy = new MinimumNormalizedStrengthPolicy(0.80);
            Assert.True(policy.Contributes(CreateValidatedEvidence(0.50, true, "normalized-at", normalizedStrength: 0.80)));
            Assert.False(policy.Contributes(CreateValidatedEvidence(0.50, true, "normalized-low", normalizedStrength: 0.79)));
        }

        [Fact]
        public void ConditionExistsPolicyRequiresNamedCondition()
        {
            var evidence = CreateValidatedEvidence(0.50, true, "condition-exists");
            Assert.True(new ConditionExistsPolicy("ClaimedLocation").Contributes(evidence));
            Assert.False(new ConditionExistsPolicy("MissingCondition").Contributes(evidence));
        }

        [Fact]
        public void ConditionEqualsPolicyRequiresMatchingValue()
        {
            var evidence = CreateValidatedEvidence(0.50, true, "condition-equals");
            Assert.True(new ConditionEqualsPolicy("ClaimedLocation", "North Cove").Contributes(evidence));
            Assert.False(new ConditionEqualsPolicy("ClaimedLocation", "Miller's Point").Contributes(evidence));
        }

        [Fact]
        public void OutcomeEqualsPolicyRequiresMatchingOutcome()
        {
            var evidence = CreateValidatedEvidence(0.50, true, "outcome", outcome: "Success");
            Assert.True(new OutcomeEqualsPolicy("Success").Contributes(evidence));
            Assert.False(new OutcomeEqualsPolicy("Failure").Contributes(evidence));
        }

        [Fact]
        public void HasSourcePolicyRejectsEvidenceWithoutSource()
        {
            Assert.False(new HasSourcePolicy().Contributes(CreateValidatedEvidence(0.50, true, "no-source")));
        }

        [Fact]
        public void MinimumNeighborCountPolicyUsesCapturedNeighborCount()
        {
            var none = CreateValidatedEvidence(0.50, true, "neighbors-none");
            var one = CreateValidatedEvidence(
                0.50,
                true,
                "neighbors-one",
                neighbors: new List<IBaseObject>
                {
                    new TestNeighbor("boat-a", new TestComponent("boat-a"))
                });

            Assert.False(new MinimumNeighborCountPolicy(1).Contributes(none));
            Assert.True(new MinimumNeighborCountPolicy(1).Contributes(one));
        }

        [Fact]
        public void AnyEvidencePolicyAcceptsValidatedEvidence()
        {
            Assert.True(new AnyEvidencePolicy().Contributes(CreateValidatedEvidence(0.10, false, "any")));
        }

        [Fact]
        public void NotEvidencePolicyNegatesInnerPolicy()
        {
            var policy = new NotEvidencePolicy(new TrueEvidencePolicy());
            Assert.False(policy.Contributes(CreateValidatedEvidence(0.50, true, "not-true")));
            Assert.True(policy.Contributes(CreateValidatedEvidence(0.50, false, "not-false")));
        }

        [Fact]
        public void AllEvidencePoliciesRequiresEveryInnerPolicy()
        {
            var policy = new AllEvidencePolicies(new IEvidenceContributionPolicy[]
            {
                new MinimumBelievabilityPolicy(0.80),
                new FalseEvidencePolicy()
            });

            Assert.True(policy.Contributes(CreateValidatedEvidence(0.90, false, "all-pass")));
            Assert.False(policy.Contributes(CreateValidatedEvidence(0.90, true, "all-truth-fail")));
            Assert.False(policy.Contributes(CreateValidatedEvidence(0.70, false, "all-belief-fail")));
        }

        [Fact]
        public void AnyEvidencePoliciesRequiresAtLeastOneInnerPolicy()
        {
            var policy = new AnyEvidencePolicies(new IEvidenceContributionPolicy[]
            {
                new TrueEvidencePolicy(),
                new MinimumBelievabilityPolicy(0.80)
            });

            Assert.True(policy.Contributes(CreateValidatedEvidence(0.20, true, "any-truth")));
            Assert.True(policy.Contributes(CreateValidatedEvidence(0.90, false, "any-belief")));
            Assert.False(policy.Contributes(CreateValidatedEvidence(0.20, false, "any-fail")));
        }

        [Fact]
        public void DelegateEvidencePolicyUsesConsumerPredicate()
        {
            var policy = new DelegateEvidencePolicy(evidence =>
                Equals(evidence.Conditions["ClaimedLocation"], "North Cove") &&
                evidence.ProofResult.Believability >= 0.80);

            Assert.True(policy.Contributes(CreateValidatedEvidence(0.90, false, "delegate-pass")));
            Assert.False(policy.Contributes(CreateValidatedEvidence(0.70, false, "delegate-fail")));
        }

        private static DiscoveryEvidence CreateValidatedEvidence(
            double believability,
            bool isTrue,
            string externalId = "policy-matrix",
            double strength = 10.0,
            double normalizedStrength = 0.80,
            object? outcome = null,
            IReadOnlyList<IBaseObject>? neighbors = null)
        {
            var evidence =
                new DiscoveryEvidence(
                    new TestEntity(
                        "jerry"),
                    new Dictionary<string, object?>
                    {
                        {
                            "ClaimedLocation",
                            "North Cove"
                        },
                        {
                            "PriorReport",
                            "North Cove closed for kelp removal"
                        }
                    },
                    outcome ??
                    "Tom reports a monster bass at North Cove",
                    strength,
                    normalizedStrength,
                    neighbors ??
                    new List<IBaseObject>(),
                    null,
                    externalId,
                    "Tom fishing report");

            var proof =
                new TestProof(
                    believability,
                    isTrue,
                    "Policy matrix proof.");

            Assert.True(
                evidence.ApplyProof(
                    proof.Validate(
                        evidence)));

            return evidence;
        }
    }
}
