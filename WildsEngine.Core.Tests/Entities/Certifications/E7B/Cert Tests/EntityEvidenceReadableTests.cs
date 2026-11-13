using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Evidences;
using WildsEngine.Core.Tests.Entities.Fixtures;
using WildsEngine.Tests.Logging;
using WildsEngine.Tests.Registry;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class EntityEvidenceReadableTests
    {
        [Fact]
        public void EvidenceRecordsOneHistoricalObservation()
        {
            TestLog.Section("E7-B - HISTORICAL WORLD OBSERVATION");
            TestLog.Test("Evidence records one consumer-facing observation without selecting a Fact or Discovery.");

            var entity = new TestEntity("angler-a");
            var evidence = CreateEvidence(entity, externalId: "catch-001", name: "Miller's Point morning catch");

            TestLog.State($"Entity = {entity.Slug}");
            TestLog.State($"ExternalId = {evidence.ExternalId}");
            TestLog.State($"Name = {evidence.Name}");
            TestLog.State($"Outcome = {evidence.Outcome}");
            TestLog.State($"Strength = {evidence.Strength}");
            TestLog.State($"NormalizedStrength = {evidence.NormalizedStrength}");
            TestLog.State($"IsValidated = {evidence.IsValidated}");

            Assert.Same(entity, evidence.Entity);
            Assert.False(evidence.IsValidated);
            TestLog.Pass("Evidence preserved the exact Entity and began as an unvalidated observation.");
        }

        [Fact]
        public void ConditionSnapshotSurvivesCallerMutation()
        {
            TestLog.Section("E7-B - CONDITION SNAPSHOT");

            var conditions = new Dictionary<string, object?>
            {
                { "Location", "Miller's Point" },
                { "Wind", "East" }
            };

            var evidence = CreateEvidence(new TestEntity("angler-a"), conditions: conditions);
            TestLog.State($"Captured Location = {evidence.Conditions["Location"]}");
            TestLog.State($"Captured Wind = {evidence.Conditions["Wind"]}");

            conditions["Location"] = "North Cove";
            conditions["Wind"] = "West";
            conditions["Depth"] = 12.0;

            TestLog.State($"Caller now says Location = {conditions["Location"]}");
            TestLog.State($"Evidence still says Location = {evidence.Conditions["Location"]}");
            TestLog.State($"Evidence still says Wind = {evidence.Conditions["Wind"]}");

            Assert.Equal("Miller's Point", evidence.Conditions["Location"]);
            Assert.Equal("East", evidence.Conditions["Wind"]);
            Assert.False(evidence.Conditions.ContainsKey("Depth"));
            TestLog.Pass("Later caller mutation did not rewrite the historical Evidence conditions.");
        }

        [Fact]
        public void NeighborSnapshotSurvivesCallerMutation()
        {
            TestLog.Section("E7-B - NEIGHBOR SNAPSHOT");

            var first = new TestNeighbor(
                "boat-a",
                new TestComponent("boat-a"));

            var second = new TestNeighbor(
                "boat-b",
                new TestComponent("boat-b"));

            var neighbors = new List<IBaseObject>
            {
                first
            };

            var evidence = CreateEvidence(
                new TestEntity("angler-a"),
                neighbors: neighbors);

            TestLog.State($"Captured neighbor count = {evidence.Neighbors.Count}");
            TestLog.State($"Captured neighbor = {first.Slug}");

            neighbors.Clear();
            neighbors.Add(second);

            TestLog.State($"Caller list now contains = {second.Slug}");
            TestLog.State($"Evidence neighbor remains = {evidence.Neighbors[0].Slug}");

            Assert.Single(evidence.Neighbors);
            Assert.Same(first, evidence.Neighbors[0]);
            Assert.DoesNotContain(second, evidence.Neighbors);

            TestLog.Pass("Later caller mutation did not rewrite the historical Evidence neighbors.");
        }

        [Fact]
        public void ObservationMagnitudeRemainsDistinctFromValidation()
        {
            TestLog.Section("E7-B - OBSERVATION MAGNITUDE VS VALIDATION");

            var evidence = CreateEvidence(new TestEntity("angler-a"));

            TestLog.State($"Strength = {evidence.Strength}");
            TestLog.State($"NormalizedStrength = {evidence.NormalizedStrength}");
            TestLog.State($"IsValidated = {evidence.IsValidated}");

            Assert.Equal(18.5, evidence.Strength);
            Assert.Equal(0.84, evidence.NormalizedStrength);
            Assert.False(evidence.IsValidated);

            TestLog.Pass("Evidence can carry observation magnitude before any Proof validates it.");
        }

        private static DiscoveryEvidence CreateEvidence(
            IEntity entity,
            IReadOnlyDictionary<string, object?>? conditions = null,
            IReadOnlyList<IBaseObject>? neighbors = null,
            IBaseUnit? source = null,
            string? externalId = null,
            string? name = null)
        {
            return new DiscoveryEvidence(
                entity,
                conditions ?? new Dictionary<string, object?>
                {
                    { "Location", "Miller's Point" },
                    { "Wind", "East" }
                },
                "LargeBass",
                18.5,
                0.84,
                neighbors ?? new List<IBaseObject>(),
                source,
                externalId,
                name);
        }
    }
}
