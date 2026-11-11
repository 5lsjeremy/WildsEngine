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
    public sealed class EntityDiscoveryResolutionReadableTests
    {
        [Fact]
        public void JerryDerivesDiscoveryFromThreeResolvedFacts()
        {
            TestLog.Section("E7-G - JERRY CONNECTS THE DOTS");
            TestLog.Test("Jerry requires three proven Facts before deriving that North Cove is a productive fishing area.");

            var jerry = new TestEntity("jerry");
            var discovery = CreateDiscovery(jerry, 3);
            var location = CreateProvenFact(jerry, "MonsterBassLocation", "North Cove", "location");
            var traffic = CreateProvenFact(jerry, "BoatTraffic", "Heading toward North Cove", "traffic");
            var reports = CreateProvenFact(jerry, "DockReports", "Fish are biting", "reports");

            LogDiscovery("Initial", discovery);
            discovery.AddFactItem(location);
            LogDiscovery("After location Fact", discovery);
            discovery.AddFactItem(traffic);
            LogDiscovery("After boat-traffic Fact", discovery);
            discovery.AddFactItem(reports);
            LogDiscovery("After dock-report Fact", discovery);

            Assert.True(discovery.IsAvailable);
            Assert.Equal(3, discovery.CurrentFactCount);
            TestLog.Pass("Jerry derived a Discovery only after the third proven Fact contributed.");
        }

        [Fact]
        public void RedKelpWarningStaysHistoricalWithoutContributing()
        {
            TestLog.Section("E7-G - JERRY DISMISSES THE RED-KELP WARNING");

            var jerry = new TestEntity("jerry");
            var discovery = CreateDiscovery(jerry, 3);
            var location = CreateProvenFact(jerry, "MonsterBassLocation", "North Cove", "location");
            var traffic = CreateProvenFact(jerry, "BoatTraffic", "Heading toward North Cove", "traffic");
            var warning = CreateUnprovenFact(jerry, "RedKelpWarning", "Red kelp is forming; stay away from North Cove");

            discovery.AddFactItem(location);
            discovery.AddFactItem(traffic);
            discovery.AddFactItem(warning);

            TestLog.State($"Historical Fact count = {discovery.Facts.Count}");
            TestLog.State($"Contributing Fact count = {discovery.CurrentFactCount}");
            TestLog.State($"Red-kelp Fact proven = {warning.IsProven}");
            TestLog.State($"Discovery available = {discovery.IsAvailable}");

            Assert.Equal(3, discovery.Facts.Count);
            Assert.Equal(2, discovery.CurrentFactCount);
            Assert.False(warning.IsProven);
            Assert.False(discovery.IsAvailable);
            TestLog.Pass("The dismissed warning remained in Jerry's history without advancing Discovery resolution.");
        }

        [Fact]
        public void ProvenContradictionsRemainVisibleInsteadOfBeingReconciled()
        {
            TestLog.Section("E7-G - CONTRADICTIONS REMAIN EXPLICIT");

            var jerry = new TestEntity("jerry");
            var discovery = CreateDiscovery(jerry, 2);
            var go = CreateProvenFact(jerry, "FishingOpportunity", "Go to North Cove", "go");
            var stayAway = CreateProvenFact(jerry, "RedKelpWarning", "Stay away from North Cove", "stay-away");

            discovery.AddFactItem(go);
            discovery.AddFactItem(stayAway);

            TestLog.State($"Fact 1 = {go.Value} | proven={go.IsProven}");
            TestLog.State($"Fact 2 = {stayAway.Value} | proven={stayAway.IsProven}");
            TestLog.State($"Contributors = {discovery.CurrentFactCount}/{discovery.RequiredFactCount}");
            TestLog.State($"Discovery available = {discovery.IsAvailable}");

            Assert.True(discovery.IsAvailable);
            Assert.NotEqual(go.Value, stayAway.Value);
            TestLog.Pass("Discovery counted proven Facts without silently rewriting their contradictory values.");
        }

        [Fact]
        public void RemovingOneResolvedFactMakesDiscoveryUnavailableAgain()
        {
            TestLog.Section("E7-G - DISCOVERY AVAILABILITY IS DERIVED");

            var jerry = new TestEntity("jerry");
            var discovery = CreateDiscovery(jerry, 2);
            var location = CreateProvenFact(jerry, "MonsterBassLocation", "North Cove", "location");
            var report = CreateProvenFact(jerry, "DockReports", "Fish are biting", "report");

            discovery.AddFactItem(location);
            discovery.AddFactItem(report);
            LogDiscovery("Before removal", discovery);

            discovery.RemoveFactItem(report);
            LogDiscovery("After report removed", discovery);

            Assert.False(discovery.IsAvailable);
            Assert.Equal(1, discovery.CurrentFactCount);
            TestLog.Pass("Discovery availability followed the current proven Facts rather than remaining latched.");
        }

        private static void LogDiscovery(string label, TestDiscovery discovery)
        {
            TestLog.State($"{label}: contributors={discovery.CurrentFactCount}/{discovery.RequiredFactCount}, percentage={discovery.FactPercentage:P0}, available={discovery.IsAvailable}");
        }

        private static TestDiscovery CreateDiscovery(IEntity entity, int requiredFactCount)
        {
            return new TestDiscovery(
                entity,
                requiredFactCount,
                "North Cove Fishing Opportunity",
                "jerry-discovery");
        }

        private static TestFact CreateUnprovenFact(IEntity entity, string name, object value)
        {
            return new TestFact(
                entity,
                name,
                value,
                1,
                new MinimumBelievabilityPolicy(0.80));
        }

        private static TestFact CreateProvenFact(IEntity entity, string name, object value, string externalId)
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
            return fact;
        }
    }
}
