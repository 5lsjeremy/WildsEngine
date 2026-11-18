using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Clusters;
using WildsEngine.Discoveries.Surface;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class WildsDiscoveriesSurfaceCertificationTests
    {
        [Fact]
        public void ProgrammaticProfileRegistersWithoutExposingInternalRegistryConstruction()
        {
            var discoveries = new WildsDiscoveries();
            var profile = Profile("machine-health", "1.0", "equipment", "Concern");
            var registration = discoveries.Geometry.Register(profile);
            Assert.Equal(profile.Id, registration.ProfileId);
            Assert.Equal("1.0", registration.ProfileVersion);
            Assert.Equal("equipment", registration.Scope);
            Assert.Single(registration.MappingIds);
            Assert.True(discoveries.Geometry.TryGetMapping("Concern", out var mapping));
            Assert.Same(profile.Mappings[0], mapping);
        }

        [Fact]
        public void DuplicateProfileRegistrationFailsWithoutDuplicatingRegistration()
        {
            var discoveries = new WildsDiscoveries();
            var profile = Profile("machine-health", "1.0", "equipment", "Concern");
            discoveries.Geometry.Register(profile);
            Assert.Throws<InvalidOperationException>(() => discoveries.Geometry.Register(profile));
            Assert.Single(discoveries.Geometry.Registrations);
        }

        [Fact]
        public void ProfileSnapshotsCallerMappingCollection()
        {
            var id = Guid.NewGuid();
            var mappings = new List<IDiscoveryClusterMapping>
            {
                Mapping(id, "profile", "1.0", "Concern")
            };
            var profile = new DiscoveryGeometryProfile(id, "profile", "1.0", "equipment", mappings);
            mappings.Clear();
            Assert.Single(profile.Mappings);
        }

        [Fact]
        public void OptionsFlowToGeometryTolerance()
        {
            var discoveries = new WildsDiscoveries(new WildsDiscoveriesOptions { GeometryEpsilon = 0.1 });
            var profile = Profile("epsilon-profile", "1.0", "equipment", "NearBoundary");
            discoveries.Geometry.Register(profile);
            var fixture = new H2TestFixture();
            var evaluation = fixture.Eval(DateTimeOffset.UtcNow, ("x", 1.05), ("y", 0.0));
            var relationship = discoveries.Geometry.Locate(evaluation, "NearBoundary");
            Assert.True(relationship.IsOnBoundary);
        }

        [Fact]
        public async Task DocumentRegistrationUsesSameSurfaceAndKeepsExecutionProvenance()
        {
            var discoveries = new WildsDiscoveries();
            var document = "{\"id\":\"7d0fba79-9ea0-44ec-8d4e-45e11c45af10\",\"slug\":\"surface-doc\",\"version\":\"1.0\",\"mappings\":[{\"id\":\"4aeca222-8635-4130-a125-b5b9ce3e9202\",\"slug\":\"concern\",\"semanticKey\":\"Concern\",\"geometryType\":\"hypersphere\",\"centroid\":{\"x\":0.0,\"y\":0.0},\"radius\":1.0}]}";
            var pulse = Guid.NewGuid();
            var registration = await discoveries.Geometry.RegisterDocumentAsync("surface.json", document, provenance: "consumer-doc", pulseId: pulse);
            Assert.Equal("surface-doc", registration.ProfileSlug);
            Assert.Single(discoveries.Geometry.Registrations);
            Assert.True(discoveries.Geometry.TryGetMapping("Concern", out _));
            Assert.Single(discoveries.Executions.GetByPulse(pulse));
        }

        [Fact]
        public void RegisteredGeometryCanBeEvaluatedThroughPublicSurface()
        {
            var discoveries = new WildsDiscoveries();
            discoveries.Geometry.Register(Profile("geometry-eval", "1.0", "equipment", "Concern"));
            var fixture = new H2TestFixture();
            var evaluation = fixture.Eval(DateTimeOffset.UtcNow, ("x", 2.0), ("y", 0.0));
            var relationship = discoveries.Geometry.Locate(evaluation, "Concern");
            Assert.Equal(2.0, relationship.DistanceToCentroid, 10);
            Assert.Equal(1.0, relationship.SignedDistanceToBoundary, 10);
        }

        private static DiscoveryGeometryProfile Profile(string slug, string version, string scope, string semantic)
        {
            var id = Guid.NewGuid();
            return new DiscoveryGeometryProfile(id, slug, version, scope,
                new List<IDiscoveryClusterMapping> { Mapping(id, slug, version, semantic) });
        }
        private static IDiscoveryClusterMapping Mapping(Guid documentId, string documentSlug, string version, string semantic) =>
            new DiscoveryClusterMapping(
                Guid.NewGuid(), semantic.ToLowerInvariant(), documentId, documentSlug,
                version, semantic, "hypersphere",
                new Dictionary<string, double> { { "x", 0.0 }, { "y", 0.0 } },
                1.0);
    }
}
