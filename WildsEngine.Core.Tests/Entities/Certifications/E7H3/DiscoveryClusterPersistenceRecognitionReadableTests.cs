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
    public sealed class DiscoveryClusterPersistenceRecognitionReadableTests
    {
        [Fact]
        public void RepeatedSnapshotsBecomePersistentRecognizedGeometry()
        {
            TestLog.Section(
                "E7-H3E/F - PERSISTENCE AND RECOGNITION");

            var profileId =
                Guid.NewGuid();

            var discoveries =
                new WildsDiscoveries();

            var mapping =
                new DiscoveryClusterMapping(
                    Guid.NewGuid(),
                    "maintenance-concern",
                    profileId,
                    "machine-health",
                    "1.0",
                    "MaintenanceConcern",
                    "hypersphere",
                    new Dictionary<string, double>
                    {
                        { "x", 0 },
                        { "y", 0 }
                    },
                    1);

            discoveries.Geometry.Register(
                new DiscoveryGeometryProfile(
                    profileId,
                    "machine-health",
                    "1.0",
                    "equipment",
                    new List<IDiscoveryClusterMapping>
                    {
                        mapping
                    },
                    new MinimumClusterPersistencePolicy(
                        "three-stable-observations",
                        3,
                        0.6,
                        0.8),
                    new BoundaryVarianceClusterStabilityMetric(),
                    new PersistentClusterRecognitionPolicy(
                        "recognize-persistent")));

            var fixture =
                new H2TestFixture();

            var timestamp =
                DateTimeOffset.UtcNow;

            const string historyKey =
                "machine-1";

            discoveries.Geometry.Observe(
                fixture.Eval(
                    timestamp,
                    ("x", 2),
                    ("y", 0)),
                "MaintenanceConcern",
                historyKey);

            discoveries.Geometry.Observe(
                fixture.Eval(
                    timestamp.AddSeconds(1),
                    ("x", 0.5),
                    ("y", 0)),
                "MaintenanceConcern",
                historyKey);

            discoveries.Geometry.Observe(
                fixture.Eval(
                    timestamp.AddSeconds(2),
                    ("x", 0.45),
                    ("y", 0)),
                "MaintenanceConcern",
                historyKey);

            var result =
                discoveries.Geometry.Observe(
                    fixture.Eval(
                        timestamp.AddSeconds(3),
                        ("x", 0.4),
                        ("y", 0)),
                    "MaintenanceConcern",
                    historyKey);

            TestLog.State(
                $"Observation history = {historyKey}");

            TestLog.State(
                $"Observations = {result.Persistence.ObservationCount}");

            TestLog.State(
                $"Consecutive inside = {result.Persistence.ConsecutiveInsideCount}");

            TestLog.State(
                $"Occupancy ratio = {result.Persistence.OccupancyRatio:R}");

            TestLog.State(
                $"Observed duration inside = {result.Persistence.ObservedDurationInside}");

            TestLog.State(
                $"Stability = {result.Persistence.Stability:R}");

            TestLog.State(
                $"Persistent = {result.Persistence.IsPersistent}");

            TestLog.State(
                $"Recognized = {result.Recognition.IsRecognized}");

            Assert.Equal(
                4,
                result.Persistence.ObservationCount);

            Assert.Equal(
                3,
                result.Persistence.ConsecutiveInsideCount);

            Assert.True(
                result.Persistence.IsPersistent);

            Assert.True(
                result.Recognition.IsRecognized);

            TestLog.Pass(
                "Successive authoritative snapshots in one explicit " +
                "observation history produced persistence and recognition " +
                "without rewriting prior geometry or borrowing observations " +
                "from another history.");
        }
    }
}