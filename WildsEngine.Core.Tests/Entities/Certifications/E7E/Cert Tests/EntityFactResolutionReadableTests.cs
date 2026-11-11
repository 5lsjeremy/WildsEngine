using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Entities.Abstractions;
using WildsEngine.Core.Entities.Evidences;
using WildsEngine.Core.Entities.Policies;
using WildsEngine.Core.Tests.Entities.Fixtures;
using WildsEngine.Tests.Logging;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class EntityFactResolutionReadableTests
    {
        [Fact]
        public void JerryAccumulatesThreeContributingReportsBeforeFactResolves()
        {
            TestLog.Section("E7-E - JERRY BUILDS A FACT FROM THREE REPORTS");
            TestLog.Test("Jerry requires three believable observations before accepting North Cove as a monster-bass location.");

            var entity = new TestEntity("jerry");
            var fact = CreateFact(entity, 3);
            var tom = CreateValidatedEvidence(entity, "Tom says North Cove", 0.92, false, "tom");
            var ramp = CreateValidatedEvidence(entity, "Jerry sees a bass boat heading toward North Cove", 0.86, false, "ramp");
            var dock = CreateValidatedEvidence(entity, "Another fisherman says they were biting there", 0.81, false, "dock");

            LogFact("Initial", fact);
            fact.AddEvidenceItem(tom);
            LogFact("After Tom", fact);
            fact.AddEvidenceItem(ramp);
            LogFact("After boat-ramp observation", fact);
            fact.AddEvidenceItem(dock);
            LogFact("After dock report", fact);

            Assert.True(fact.IsProven);
            Assert.Equal(3, fact.CurrentEvidenceCount);
            TestLog.Pass("Jerry's Fact resolved only when the third policy-accepted Evidence item arrived.");
        }

        [Fact]
        public void WeakReportStaysInHistoryButDoesNotAdvanceJerrysFact()
        {
            TestLog.Section("E7-E - WEAK REPORT STAYS HISTORICAL");

            var entity = new TestEntity("jerry");
            var fact = CreateFact(entity, 2);
            var tom = CreateValidatedEvidence(entity, "Tom says North Cove", 0.92, false, "tom");
            var weak = CreateValidatedEvidence(entity, "Somebody vaguely mentions North Cove", 0.40, false, "weak");

            fact.AddEvidenceItem(tom);
            fact.AddEvidenceItem(weak);

            TestLog.State($"Historical Evidence count = {fact.Evidence.Count}");
            TestLog.State($"Contributing Evidence count = {fact.CurrentEvidenceCount}");
            TestLog.State($"Resolution percentage = {fact.EvidencePercentage:P0}");
            TestLog.State($"Fact proven = {fact.IsProven}");

            Assert.Equal(2, fact.Evidence.Count);
            Assert.Equal(1, fact.CurrentEvidenceCount);
            Assert.False(fact.IsProven);
            TestLog.Pass("The weak report remained part of Jerry's history without counting toward resolution.");
        }

        [Fact]
        public void LosingOneContributorCanMakeJerryUncertainAgain()
        {
            TestLog.Section("E7-E - FACT RESOLUTION IS DERIVED, NOT LATCHED");

            var entity = new TestEntity("jerry");
            var fact = CreateFact(entity, 2);
            var tom = CreateValidatedEvidence(entity, "Tom says North Cove", 0.92, false, "tom");
            var dock = CreateValidatedEvidence(entity, "Dock report supports North Cove", 0.85, false, "dock");

            fact.AddEvidenceItem(tom);
            fact.AddEvidenceItem(dock);
            LogFact("With both contributors", fact);

            fact.RemoveEvidenceItem(dock);
            LogFact("After dock report removed", fact);

            Assert.False(fact.IsProven);
            Assert.Equal(1, fact.CurrentEvidenceCount);
            TestLog.Pass("Fact resolution followed the current contributing Evidence instead of remaining permanently latched.");
        }

        private static void LogFact(string label, TestFact fact)
        {
            TestLog.State($"{label}: contributors={fact.CurrentEvidenceCount}/{fact.RequiredEvidenceCount}, percentage={fact.EvidencePercentage:P0}, proven={fact.IsProven}");
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

        private static DiscoveryEvidence CreateValidatedEvidence(
            IEntity entity,
            string outcome,
            double believability,
            bool isTrue,
            string externalId)
        {
            var evidence = new DiscoveryEvidence(
                entity,
                new Dictionary<string, object?>
                {
                    { "ClaimedLocation", "North Cove" }
                },
                outcome,
                1.0,
                1.0,
                new List<IBaseObject>(),
                null,
                externalId,
                "Jerry fishing evidence");

            var proof = new TestProof(believability, isTrue, "Controlled E7-E proof.");
            Assert.True(evidence.ApplyProof(proof.Validate(evidence)));
            return evidence;
        }
    }
}
