using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Evidences;
using WildsEngine.Discoveries.Policies;
using WildsEngine.Core.Tests.Entities.Fixtures;
using WildsEngine.Tests.Logging;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class EntityFactPolicyReadableTests
    {
        [Fact]
        public void SameTomReportDifferentProductionStandardsDifferentConclusions()
        {
            TestLog.Section("E7-D - SAME REPORT, DIFFERENT PRODUCTION POLICIES");
            TestLog.Test("Tom tells Jerry the monster bass came from North Cove.");

            var entity = new TestEntity("jerry");
            var evidence = CreateValidatedEvidence(entity, 0.92, false);
            var truthFact = CreateFact(entity, new TrueEvidencePolicy(), "TruthStandard");
            var beliefFact = CreateFact(entity, new MinimumBelievabilityPolicy(0.80), "BeliefStandard");

            truthFact.AddEvidenceItem(evidence);
            beliefFact.AddEvidenceItem(evidence);

            TestLog.State($"Tom's report: Believability={evidence.ProofResult.Believability}, IsTrue={evidence.ProofResult.IsTrue}");
            TestLog.State($"TrueEvidencePolicy proves Fact = {truthFact.IsProven}");
            TestLog.State($"MinimumBelievabilityPolicy(0.80) proves Fact = {beliefFact.IsProven}");

            Assert.False(truthFact.IsProven);
            Assert.True(beliefFact.IsProven);
            TestLog.Pass("The same stable Evidence produced different Fact outcomes under real production policies.");
        }

        [Fact]
        public void TomPassesTheProductionBelievableFalsePolicy()
        {
            TestLog.Section("E7-D - TOM PASSES THE PRODUCTION BELIEVABLE-FALSE POLICY");

            var entity = new TestEntity("jerry");
            var evidence = CreateValidatedEvidence(entity, 0.92, false);
            var fact = CreateFact(entity, new BelievableFalseEvidencePolicy(0.80), "TomProtectsFishingSpots");

            fact.AddEvidenceItem(evidence);

            TestLog.State("Policy = BelievableFalseEvidencePolicy(0.80)");
            TestLog.State($"Believability = {evidence.ProofResult.Believability}");
            TestLog.State($"IsTrue = {evidence.ProofResult.IsTrue}");
            TestLog.State($"Fact proven = {fact.IsProven}");

            Assert.True(fact.IsProven);
            TestLog.Pass("Tom's convincing lie contributed under the real production believable-false policy.");
        }

        [Fact]
        public void UnvalidatedFishingStoryCannotContributeThroughProductionPolicy()
        {
            TestLog.Section("E7-D - UNVALIDATED STORY CANNOT CONTRIBUTE");

            var entity = new TestEntity("jerry");
            var evidence = CreateEvidence(entity);
            var fact = CreateFact(entity, new TrueEvidencePolicy(), "UnvalidatedReport");

            fact.AddEvidenceItem(evidence);

            TestLog.State("Policy = TrueEvidencePolicy");
            TestLog.State($"Evidence validated = {evidence.IsValidated}");
            TestLog.State($"Evidence retained by Fact = {fact.Evidence.Count}");
            TestLog.State($"Contributing Evidence count = {fact.CurrentEvidenceCount}");
            TestLog.State($"Fact proven = {fact.IsProven}");

            Assert.False(evidence.IsValidated);
            Assert.Single(fact.Evidence);
            Assert.Equal(0, fact.CurrentEvidenceCount);
            Assert.False(fact.IsProven);
            TestLog.Pass("FactBase retained the observation but withheld it from contribution before production-policy interpretation.");
        }

        [Fact]
        public void ProductionInterpretationNeverRewritesTomsProofResult()
        {
            TestLog.Section("E7-D - PRODUCTION POLICIES DO NOT REWRITE HISTORY");

            var entity = new TestEntity("jerry");
            var evidence = CreateValidatedEvidence(entity, 0.92, false);
            var original = evidence.ProofResult;
            var truthFact = CreateFact(entity, new TrueEvidencePolicy(), "TruthStandard");
            var beliefFact = CreateFact(entity, new MinimumBelievabilityPolicy(0.80), "BeliefStandard");

            TestLog.State($"Before policies: {original.Believability} / {original.IsTrue}");
            truthFact.AddEvidenceItem(evidence);
            TestLog.State($"After TrueEvidencePolicy: {evidence.ProofResult.Believability} / {evidence.ProofResult.IsTrue}");
            beliefFact.AddEvidenceItem(evidence);
            TestLog.State($"After MinimumBelievabilityPolicy: {evidence.ProofResult.Believability} / {evidence.ProofResult.IsTrue}");

            Assert.Same(original, evidence.ProofResult);
            Assert.Equal(0.92, evidence.ProofResult.Believability);
            Assert.False(evidence.ProofResult.IsTrue);
            TestLog.Pass("Different production-policy interpretations left Tom's historical ProofResult untouched.");
        }

        private static TestFact CreateFact(IEntity entity, IEvidenceContributionPolicy policy, string name)
        {
            return new TestFact(entity, name, true, 1, policy);
        }

        private static DiscoveryEvidence CreateEvidence(IEntity entity)
        {
            return new DiscoveryEvidence(
                entity,
                new Dictionary<string, object?>
                {
                    { "ClaimedLocation", "North Cove" },
                    { "PriorReport", "North Cove closed for kelp removal" }
                },
                "Tom reports a monster bass at North Cove",
                1.0,
                1.0,
                new List<IBaseObject>(),
                null,
                "tom-report",
                "Tom fishing report");
        }

        private static DiscoveryEvidence CreateValidatedEvidence(IEntity entity, double believability, bool isTrue)
        {
            var evidence = CreateEvidence(entity);
            var proof = new TestProof(believability, isTrue, "Controlled E7-D proof.");
            Assert.True(evidence.ApplyProof(proof.Validate(evidence)));
            return evidence;
        }
    }
}
