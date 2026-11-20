using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Clusters;
using WildsEngine.Discoveries.Policies;
using WildsEngine.Discoveries.Surface;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class DiscoveryRecognizedGeometryPopulationCertificationTests
    {
        [Fact]
        public void PopulationPreservesDistinctRecognizedStructuresWithoutAveraging()
        {
            var fixture = CreateSurface();
            var first = Recognize(fixture.Discoveries, fixture.FirstKey, 0.1);
            var second = Recognize(fixture.Discoveries, fixture.SecondKey, 9.9);
            var population = fixture.Discoveries.Narratives.CreatePopulation(
                fixture.RegistrationId,
                new List<IDiscoveryClusterRecognitionResult> { first, second });

            Assert.Equal(2, population.Members.Count);
            Assert.Equal(2, population.SemanticKeys.Count);
            Assert.Same(first, population.Members[0]);
            Assert.Same(second, population.Members[1]);
            Assert.Equal(first.Mapping.Id, population.Members[0].Mapping.Id);
            Assert.Equal(second.Mapping.Id, population.Members[1].Mapping.Id);
        }

        [Fact]
        public void PopulationSnapshotsCallerCollection()
        {
            var fixture = CreateSurface();
            var members = new List<IDiscoveryClusterRecognitionResult>
            {
                Recognize(fixture.Discoveries, fixture.FirstKey, 0.1)
            };
            var population = fixture.Discoveries.Narratives.CreatePopulation(fixture.RegistrationId, members);
            members.Clear();
            Assert.Single(population.Members);
        }

        [Fact]
        public void UnrecognizedMemberIsRejected()
        {
            var fixture = CreateSurface(minimumConsecutive: 2);
            var result = fixture.Discoveries.Geometry.Observe(
                fixture.Vector.Eval(
                    DateTimeOffset.UtcNow,
                    ("x", 0.1),
                    ("y", 0.0)),
                fixture.FirstKey,
                "unrecognized-subject");
            Assert.False(result.Recognition.IsRecognized);
            Assert.Throws<ArgumentException>(() =>
                fixture.Discoveries.Narratives.CreatePopulation(
                    fixture.RegistrationId,
                    new List<IDiscoveryClusterRecognitionResult> { result.Recognition }));
        }

        [Fact]
        public void ForeignRegistrationMemberIsRejected()
        {
            var first = CreateSurface();
            var second = CreateSurface();
            var foreign = Recognize(second.Discoveries, second.FirstKey, 0.1);
            Assert.Throws<ArgumentException>(() =>
                first.Discoveries.Narratives.CreatePopulation(
                    first.RegistrationId,
                    new List<IDiscoveryClusterRecognitionResult> { foreign }));
        }

        [Fact]
        public void UnknownRegistrationFails()
        {
            var fixture = CreateSurface();
            var recognized = Recognize(fixture.Discoveries, fixture.FirstKey, 0.1);
            Assert.Throws<KeyNotFoundException>(() =>
                fixture.Discoveries.Narratives.CreatePopulation(
                    Guid.NewGuid(),
                    new List<IDiscoveryClusterRecognitionResult> { recognized }));
        }

        [Fact]
        public void PopulationRetainsRegistrationScopeAndIdentity()
        {
            var fixture = CreateSurface();
            var recognized = Recognize(fixture.Discoveries, fixture.FirstKey, 0.1);
            var population = fixture.Discoveries.Narratives.CreatePopulation(
                fixture.RegistrationId,
                new List<IDiscoveryClusterRecognitionResult> { recognized });
            Assert.Equal("pizza-party", population.Scope);
            Assert.Equal(fixture.RegistrationId, population.Registration.Id);
            Assert.NotEqual(Guid.Empty, population.Id);
        }

        private static IDiscoveryClusterRecognitionResult Recognize(
            WildsDiscoveries discoveries,
            string key,
            double x)
        {
            var vectors =
                new H2TestFixture();

            var historyKey =
                "recognition-" +
                Guid.NewGuid().ToString("N");

            return discoveries.Geometry.Observe(
                    vectors.Eval(
                        DateTimeOffset.UtcNow,
                        ("x", x),
                        ("y", 0.0)),
                    key,
                    historyKey)
                .Recognition;
        }

        private static Fixture CreateSurface(int minimumConsecutive = 1)
        {
            var discoveries = new WildsDiscoveries();
            var profileId = Guid.NewGuid();
            const string first = "PepperoniOliveMushroomOnion";
            const string second = "PepperoniSausageJalapenoOnion";
            var mappings = new List<IDiscoveryClusterMapping>
            {
                Map(profileId, "pizza-a", first, 0.0),
                Map(profileId, "pizza-b", second, 10.0)
            };
            var profile = new DiscoveryGeometryProfile(
                profileId,
                "pizza-four-topping-narratives",
                "1.0",
                "pizza-party",
                mappings,
                new MinimumClusterPersistencePolicy("pizza-persistence", minimumConsecutive, 0.0, 0.0),
                new BoundaryVarianceClusterStabilityMetric(),
                new PersistentClusterRecognitionPolicy("pizza-recognition"));
            var registration = discoveries.Geometry.Register(profile);
            return new Fixture(discoveries, registration.Id, first, second, new H2TestFixture());
        }

        private static IDiscoveryClusterMapping Map(Guid profileId, string slug, string semanticKey, double x)
        {
            return new DiscoveryClusterMapping(
                Guid.NewGuid(), slug, profileId, "pizza-four-topping-narratives", "1.0",
                semanticKey, "hypersphere",
                new Dictionary<string, double> { { "x", x }, { "y", 0.0 } }, 1.0);
        }

        private sealed class Fixture
        {
            public Fixture(WildsDiscoveries discoveries, Guid registrationId, string firstKey, string secondKey, H2TestFixture vector)
            { Discoveries = discoveries; RegistrationId = registrationId; FirstKey = firstKey; SecondKey = secondKey; Vector = vector; }
            public WildsDiscoveries Discoveries { get; }
            public Guid RegistrationId { get; }
            public string FirstKey { get; }
            public string SecondKey { get; }
            public H2TestFixture Vector { get; }
        }
    }
}
