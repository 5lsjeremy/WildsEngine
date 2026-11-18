using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Clusters;
using WildsEngine.Discoveries.Surface;
using WildsEngine.Tests.Logging;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class WildsDiscoveriesSurfaceReadableTests
    {
        [Fact]
        public void ConsumerProfileBecomesRegisteredQueryableGeometry()
        {
            TestLog.Section("S1 - DISCOVERIES CONSUMER SURFACE");
            var discoveries = new WildsDiscoveries();
            var profileId = Guid.NewGuid();
            var mapping = new DiscoveryClusterMapping(
                Guid.NewGuid(), "maintenance-concern", profileId, "machine-health", "1.0",
                "MaintenanceConcern", "hypersphere",
                new Dictionary<string, double> { { "x", 0.0 }, { "y", 0.0 } }, 1.0);
            var profile = new DiscoveryGeometryProfile(
                profileId, "machine-health", "1.0", "equipment",
                new List<IDiscoveryClusterMapping> { mapping });
            var registration = discoveries.Geometry.Register(profile);
            var fixture = new H2TestFixture();
            var evaluation = fixture.Eval(DateTimeOffset.UtcNow, ("x", 3.0), ("y", 0.0));
            var relationship = discoveries.Geometry.Locate(evaluation, "MaintenanceConcern");
            TestLog.State($"Profile = {registration.ProfileSlug} v{registration.ProfileVersion}");
            TestLog.State($"Scope = {registration.Scope}");
            TestLog.State($"Registration = {registration.Id}");
            TestLog.State($"Semantic key = {relationship.Mapping.SemanticKey}");
            TestLog.State($"Distance to centroid = {relationship.DistanceToCentroid:R}");
            TestLog.State($"Signed boundary distance = {relationship.SignedDistanceToBoundary:R}");
            Assert.Equal(3.0, relationship.DistanceToCentroid, 10);
            Assert.Equal(2.0, relationship.SignedDistanceToBoundary, 10);
            TestLog.Pass("Consumer options, typed profile, immutable registration, and geometry evaluation are available through one Discoveries-facing surface without constructing internal registries or projectors.");
        }
    }
}
