using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Evidences;
using WildsEngine.Core.Tests.Entities.Fixtures;
using WildsEngine.Tests.Registry;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class EntityEvidenceCertificationTests
    {
        [Fact]
        public void EvidencePreservesExactEntityAndConsumerMetadata()
        {
            var entity =
                new TestEntity(
                    "mara");

            var evidence =
                CreateEvidence(
                    entity,
                    externalId: "obs-001",
                    name: "Dock sighting");

            Assert.Same(
                entity,
                evidence.Entity);

            Assert.Equal(
                "obs-001",
                evidence.ExternalId);

            Assert.Equal(
                "Dock sighting",
                evidence.Name);
        }

        [Fact]
        public void EvidencePreservesOutcomeStrengthAndNormalizedStrength()
        {
            var evidence =
                CreateEvidence(
                    new TestEntity(
                        "mara"));

            Assert.Equal(
                "LargeBass",
                evidence.Outcome);

            Assert.Equal(
                18.5,
                evidence.Strength);

            Assert.Equal(
                0.84,
                evidence.NormalizedStrength);
        }

        [Fact]
        public void EvidenceConditionsAreSnapshotted()
        {
            var entity =
                new TestEntity(
                    "mara");

            var conditions =
                new Dictionary<string, object?>
                {
                    { "Location", "Miller's Point" },
                    { "Wind", "East" }
                };

            var evidence =
                CreateEvidence(
                    entity,
                    conditions: conditions);

            conditions["Location"] =
                "North Cove";

            conditions["Wind"] =
                "West";

            conditions["Depth"] =
                12.0;

            Assert.Equal(
                "Miller's Point",
                evidence.Conditions["Location"]);

            Assert.Equal(
                "East",
                evidence.Conditions["Wind"]);

            Assert.False(
                evidence.Conditions.ContainsKey(
                    "Depth"));
        }

        [Fact]
        public void EvidenceNeighborsAreSnapshotted()
        {
            var entity =
                new TestEntity(
                    "mara");

            var first =
                new TestNeighbor(
                    "neighbor-a",
                    new TestComponent(
                        "neighbor-a"));

            var second =
                new TestNeighbor(
                    "neighbor-b",
                    new TestComponent(
                        "neighbor-b"));

            var neighbors =
                new List<IBaseObject>
                {
                    first
                };

            var evidence =
                CreateEvidence(
                    entity,
                    neighbors: neighbors);

            neighbors.Clear();

            neighbors.Add(
                second);

            Assert.Single(
                evidence.Neighbors);

            Assert.Same(
                first,
                evidence.Neighbors[0]);

            Assert.DoesNotContain(
                second,
                evidence.Neighbors);
        }

        [Fact]
        public void EvidencePreservesExactOptionalSource()
        {
            var entity =
                new TestEntity(
                    "mara");

            var evidence =
                CreateEvidence(
                    entity,
                    source: null);

            Assert.Null(
                evidence.Source);
        }

        [Fact]
        public void NewEvidenceBeginsUnvalidated()
        {
            var evidence =
                CreateEvidence(
                    new TestEntity(
                        "mara"));

            Assert.False(
                evidence.IsValidated);
        }

        [Fact]
        public void EvidenceRejectsNullEntity()
        {
            Assert.Throws<ArgumentNullException>(
                () =>
                    new DiscoveryEvidence(
                        null!,
                        new Dictionary<string, object?>
                        {
                            { "Location", "Miller's Point" },
                            { "Wind", "East" }
                        },
                        "LargeBass",
                        18.5,
                        0.84,
                        new List<IBaseObject>(),
                        null,
                        null,
                        null));
        }

        [Fact]
        public void EvidenceRejectsNullConditions()
        {
            var entity =
                new TestEntity(
                    "mara");

            Assert.Throws<ArgumentNullException>(
                () =>
                    new DiscoveryEvidence(
                        entity,
                        null!,
                        "LargeBass",
                        18.5,
                        0.84,
                        new List<IBaseObject>(),
                        null,
                        null,
                        null));
        }

        [Fact]
        public void EvidenceRejectsNullNeighbors()
        {
            var entity =
                new TestEntity(
                    "mara");

            Assert.Throws<ArgumentNullException>(
                () =>
                    new DiscoveryEvidence(
                        entity,
                        new Dictionary<string, object?>
                        {
                            { "Location", "Miller's Point" },
                            { "Wind", "East" }
                        },
                        "LargeBass",
                        18.5,
                        0.84,
                        null!,
                        null,
                        null,
                        null));
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
                conditions ??
                new Dictionary<string, object?>
                {
                    { "Location", "Miller's Point" },
                    { "Wind", "East" }
                },
                "LargeBass",
                18.5,
                0.84,
                neighbors ??
                new List<IBaseObject>(),
                source,
                externalId,
                name);
        }
    }
}