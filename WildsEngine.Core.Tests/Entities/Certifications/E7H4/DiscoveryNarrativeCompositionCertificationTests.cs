using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Abstractions.Narratives;
using WildsEngine.Discoveries.Clusters;
using WildsEngine.Discoveries.Narratives;
using WildsEngine.Discoveries.Policies;
using WildsEngine.Discoveries.Surface;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class DiscoveryNarrativeCompositionCertificationTests
    {
        [Fact]
        public void SuppliedRelationshipComposesAndPreservesExactProvenance()
        {
            var fixture = CreateFixture();
            var relationship = new DiscoveryNarrativeRelationshipDefinition(
                Guid.NewGuid(), "similar-pizza", fixture.FirstKey, fixture.SecondKey, "SimilarPreference");
            var policy = new RelationshipKeyNarrativeCompositionPolicy("SimilarPreference");
            var metric = new RecognitionCentroidDistanceNarrativeMetric();
            var profile = new DiscoveryNarrativeProfile(
                Guid.NewGuid(), "pizza-narrative", "1.0", "pizza-party",
                policy, metric,
                new List<IDiscoveryNarrativeRelationshipDefinition> { relationship });
            var registration = fixture.Discoveries.Narratives.Register(profile);
            var result = fixture.Discoveries.Narratives.Compose(fixture.Population, registration.Id);

            Assert.Single(result.Groups);
            Assert.Equal(2, result.Groups[0].Members.Count);
            Assert.Same(fixture.Population, result.Population);
            Assert.Equal(registration.Id, result.Registration.Id);
            Assert.Equal(profile.Id, result.Registration.ProfileId);
            Assert.Equal(policy.Id, result.Registration.CompositionPolicyId);
            Assert.Equal(metric.Id, result.Registration.MetricId);
            Assert.Contains(relationship.Id, result.Registration.RelationshipIds);
            Assert.Single(result.Decisions);
            var decision = result.Decisions[0];
            Assert.Same(fixture.First, decision.Left);
            Assert.Same(fixture.Second, decision.Right);
            Assert.Same(relationship, decision.Relationship);
            Assert.True(decision.CanCompose);
            Assert.True(decision.Measurement >= 0.0);
            Assert.Same(fixture.First, result.Groups[0].Members[0]);
            Assert.Same(fixture.Second, result.Groups[0].Members[1]);
        }

        [Fact]
        public void MissingRelationshipPreservesSeparateNarrativesAndRejectedDecision()
        {
            var fixture = CreateFixture();
            var profile = new DiscoveryNarrativeProfile(
                Guid.NewGuid(), "pizza-narrative", "1.0", "pizza-party",
                new RelationshipKeyNarrativeCompositionPolicy("SimilarPreference"),
                new RecognitionCentroidDistanceNarrativeMetric(),
                new List<IDiscoveryNarrativeRelationshipDefinition>());
            var registration = fixture.Discoveries.Narratives.Register(profile);
            var result = fixture.Discoveries.Narratives.Compose(fixture.Population, registration.Id);
            Assert.Equal(2, result.Groups.Count);
            Assert.Single(result.Decisions);
            Assert.Null(result.Decisions[0].Relationship);
            Assert.False(result.Decisions[0].CanCompose);
        }

        [Fact]
        public void ScopeMismatchFails()
        {
            var fixture = CreateFixture();
            var profile = new DiscoveryNarrativeProfile(
                Guid.NewGuid(), "other", "1.0", "other-scope",
                new RelationshipKeyNarrativeCompositionPolicy("X"),
                new RecognitionCentroidDistanceNarrativeMetric(),
                new List<IDiscoveryNarrativeRelationshipDefinition>());
            var registration = fixture.Discoveries.Narratives.Register(profile);
            Assert.Throws<InvalidOperationException>(() =>
                fixture.Discoveries.Narratives.Compose(fixture.Population, registration.Id));
        }

        private static Fixture CreateFixture()
        {
            var discoveries = new WildsDiscoveries();
            var profileId = Guid.NewGuid();
            const string firstKey = "PepperoniOliveMushroomOnion";
            const string secondKey = "PepperoniSausageJalapenoOnion";
            var mappings = new List<IDiscoveryClusterMapping>
            {
                Mapping(profileId, "pizza-a", firstKey, 0.0),
                Mapping(profileId, "pizza-b", secondKey, 10.0)
            };
            var geometryRegistration = discoveries.Geometry.Register(
                new DiscoveryGeometryProfile(
                    profileId, "pizza", "1.0", "pizza-party", mappings,
                    new MinimumClusterPersistencePolicy("p", 1, 0.0, 0.0),
                    new BoundaryVarianceClusterStabilityMetric(),
                    new PersistentClusterRecognitionPolicy()));
            var vectorsA = new H2TestFixture();
            var vectorsB = new H2TestFixture();
            var first = discoveries.Geometry.Observe(
                vectorsA.Eval(DateTimeOffset.UtcNow, ("x", 0.1), ("y", 0.0)), firstKey).Recognition;
            var second = discoveries.Geometry.Observe(
                vectorsB.Eval(DateTimeOffset.UtcNow, ("x", 9.9), ("y", 0.0)), secondKey).Recognition;
            var population = discoveries.Narratives.CreatePopulation(
                geometryRegistration.Id,
                new List<IDiscoveryClusterRecognitionResult> { first, second });
            return new Fixture(discoveries, population, first, second, firstKey, secondKey);
        }

        private static IDiscoveryClusterMapping Mapping(
            Guid profileId, string slug, string semanticKey, double x) =>
            new DiscoveryClusterMapping(
                Guid.NewGuid(), slug, profileId, "pizza", "1.0", semanticKey,
                "hypersphere", new Dictionary<string, double> { { "x", x }, { "y", 0.0 } }, 1.0);

        private sealed class Fixture
        {
            public Fixture(
                WildsDiscoveries discoveries,
                IDiscoveryRecognizedGeometryPopulation population,
                IDiscoveryClusterRecognitionResult first,
                IDiscoveryClusterRecognitionResult second,
                string firstKey,
                string secondKey)
            { Discoveries = discoveries; Population = population; First = first; Second = second; FirstKey = firstKey; SecondKey = secondKey; }
            public WildsDiscoveries Discoveries { get; }
            public IDiscoveryRecognizedGeometryPopulation Population { get; }
            public IDiscoveryClusterRecognitionResult First { get; }
            public IDiscoveryClusterRecognitionResult Second { get; }
            public string FirstKey { get; }
            public string SecondKey { get; }
        }
    }
}
