using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Clusters;
using WildsEngine.Discoveries.Policies;
using WildsEngine.Discoveries.Surface;
using WildsEngine.Tests.Logging;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class DiscoveryRecognizedGeometryPopulationReadableTests
    {
        [Fact]
        public void CompetingPizzaPreferencesRemainDistinctInOnePopulation()
        {
            TestLog.Section("E7-H4A - RECOGNIZED GEOMETRY POPULATION");
            var discoveries = new WildsDiscoveries();
            var profileId = Guid.NewGuid();
            var mappings = new List<IDiscoveryClusterMapping>
            {
                Map(profileId, "pizza-a", "PepperoniOliveMushroomOnion", 0.0),
                Map(profileId, "pizza-b", "PepperoniSausageJalapenoOnion", 10.0)
            };
            var registration = discoveries.Geometry.Register(new DiscoveryGeometryProfile(
                profileId, "pizza-four-topping-narratives", "1.0", "pizza-party", mappings,
                new MinimumClusterPersistencePolicy("pizza-persistence", 1, 0.0, 0.0),
                new BoundaryVarianceClusterStabilityMetric(),
                new PersistentClusterRecognitionPolicy("pizza-recognition")));

            var vectorsA = new H2TestFixture();
            var vectorsB = new H2TestFixture();
            var first = discoveries.Geometry.Observe(
                vectorsA.Eval(
                    DateTimeOffset.UtcNow,
                    ("x", 0.1),
                    ("y", 0.0)),
                "PepperoniOliveMushroomOnion",
                "pizza-subject-a").Recognition;

            var second = discoveries.Geometry.Observe(
                vectorsB.Eval(
                    DateTimeOffset.UtcNow,
                    ("x", 9.9),
                    ("y", 0.0)),
                "PepperoniSausageJalapenoOnion",
                "pizza-subject-b").Recognition;

            var population = discoveries.Narratives.CreatePopulation(
                registration.Id,
                new List<IDiscoveryClusterRecognitionResult> { first, second });

            TestLog.State($"Scope = {population.Scope}");
            TestLog.State($"Registration = {population.Registration.Id}");
            TestLog.State($"Recognized members = {population.Members.Count}");
            for (var index = 0; index < population.SemanticKeys.Count; index++)
                TestLog.State($"Narrative candidate {index + 1} = {population.SemanticKeys[index]}");

            Assert.Equal(2, population.Members.Count);
            Assert.Equal(2, population.SemanticKeys.Count);
            TestLog.Pass("Two recognized four-topping preference structures entered one immutable scoped population without being averaged into a combination neither source expressed.");
        }

        private static IDiscoveryClusterMapping Map(Guid profileId, string slug, string semanticKey, double x)
        {
            return new DiscoveryClusterMapping(
                Guid.NewGuid(), slug, profileId, "pizza-four-topping-narratives", "1.0",
                semanticKey, "hypersphere",
                new Dictionary<string, double> { { "x", x }, { "y", 0.0 } }, 1.0);
        }
    }
}
