using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Clusters;
using WildsEngine.Discoveries.Policies;
using WildsEngine.Discoveries.Surface;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class DiscoveryClusterPersistenceRecognitionCertificationTests
    {
        [Fact]
        public void ConsecutiveObservedOccupancyBecomesPersistentAndRecognized()
        {
            var discoveries = Create(
                consecutive: 3,
                occupancy: 0.6,
                stability: 0.8);

            var fixture = new H2TestFixture();
            var timestamp = DateTimeOffset.UtcNow;

            discoveries.Geometry.Observe(
                fixture.Eval(
                    timestamp,
                    ("x", 2.0),
                    ("y", 0.0)),
                "Concern");

            discoveries.Geometry.Observe(
                fixture.Eval(
                    timestamp.AddSeconds(1),
                    ("x", 0.5),
                    ("y", 0.0)),
                "Concern");

            discoveries.Geometry.Observe(
                fixture.Eval(
                    timestamp.AddSeconds(2),
                    ("x", 0.4),
                    ("y", 0.0)),
                "Concern");

            var result =
                discoveries.Geometry.Observe(
                    fixture.Eval(
                        timestamp.AddSeconds(3),
                        ("x", 0.3),
                        ("y", 0.0)),
                    "Concern");

            Assert.Equal(
                3,
                result.Persistence.ConsecutiveInsideCount);

            Assert.Equal(
                0.75,
                result.Persistence.OccupancyRatio,
                10);

            Assert.True(
                result.Persistence.IsPersistent,
                $"Persistence failed. " +
                $"ConsecutiveInside={result.Persistence.ConsecutiveInsideCount}, " +
                $"OccupancyRatio={result.Persistence.OccupancyRatio:R}, " +
                $"Stability={result.Persistence.Stability:R}, " +
                $"Policy={result.Persistence.PersistencePolicySlug}");

            Assert.True(
                result.Recognition.IsRecognized,
                $"Recognition failed. " +
                $"Persistent={result.Persistence.IsPersistent}, " +
                $"RecognitionPolicy={result.Recognition.RecognitionPolicySlug}");

            Assert.Equal(
                TimeSpan.FromSeconds(2),
                result.Persistence.ObservedDurationInside);
        }

        [Fact]
        public void TransientOccupancyDoesNotRecognize()
        {
            var discoveries = Create(
                consecutive: 2,
                occupancy: 0.5,
                stability: 0.7);

            var fixture = new H2TestFixture();
            var timestamp = DateTimeOffset.UtcNow;

            discoveries.Geometry.Observe(
                fixture.Eval(
                    timestamp,
                    ("x", 0.5),
                    ("y", 0.0)),
                "Concern");

            var result =
                discoveries.Geometry.Observe(
                    fixture.Eval(
                        timestamp.AddSeconds(1),
                        ("x", 2.0),
                        ("y", 0.0)),
                    "Concern");

            Assert.False(result.Persistence.IsPersistent);
            Assert.False(result.Recognition.IsRecognized);

            Assert.Equal(
                0,
                result.Persistence.ConsecutiveInsideCount);
        }

        [Fact]
        public void ExitIsObservedAndConsecutiveOccupancyResets()
        {
            var discoveries = Create(
                consecutive: 2,
                occupancy: 0.3,
                stability: 0.0);

            var fixture = new H2TestFixture();
            var timestamp = DateTimeOffset.UtcNow;

            discoveries.Geometry.Observe(
                fixture.Eval(
                    timestamp,
                    ("x", 0.5),
                    ("y", 0.0)),
                "Concern");

            discoveries.Geometry.Observe(
                fixture.Eval(
                    timestamp.AddSeconds(1),
                    ("x", 0.4),
                    ("y", 0.0)),
                "Concern");

            var result =
                discoveries.Geometry.Observe(
                    fixture.Eval(
                        timestamp.AddSeconds(2),
                        ("x", 2.0),
                        ("y", 0.0)),
                    "Concern");

            Assert.NotNull(
                result.Persistence.ObservedExitAt);

            Assert.Equal(
                TimeSpan.FromSeconds(2),
                result.Persistence.ObservedDurationInside);

            Assert.Equal(
                0,
                result.Persistence.ConsecutiveInsideCount);
        }

        [Fact]
        public void StabilityMetricDistinguishesStableFromVariableBoundaryHistory()
        {
            var metric =
                new BoundaryVarianceClusterStabilityMetric();

            var stable =
                MetricSeries(
                    0.2,
                    0.2,
                    0.2);

            var variable =
                MetricSeries(
                    -0.9,
                    0.9,
                    -0.9);

            var stableScore =
                metric.GetStability(stable);

            var variableScore =
                metric.GetStability(variable);

            Assert.True(
                stableScore > variableScore,
                $"Expected stable history to score above variable history. " +
                $"Stable={stableScore:R}, " +
                $"Variable={variableScore:R}");
        }

        [Fact]
        public void PolicyAndMetricIdentityArePreservedInPersistenceResult()
        {
            var discoveries = Create(
                consecutive: 1,
                occupancy: 0.0,
                stability: 0.0);

            var fixture = new H2TestFixture();

            var result =
                discoveries.Geometry.Observe(
                    fixture.Eval(
                        DateTimeOffset.UtcNow,
                        ("x", 0.5),
                        ("y", 0.0)),
                    "Concern");

            Assert.NotEqual(
                Guid.Empty,
                result.Persistence.PersistencePolicyId);

            Assert.NotEqual(
                Guid.Empty,
                result.Persistence.StabilityMetricId);

            Assert.NotEqual(
                Guid.Empty,
                result.Recognition.RecognitionPolicyId);
        }

        [Fact]
        public void ObserveRequiresProfilePersistenceAndRecognitionConfiguration()
        {
            var profileId = Guid.NewGuid();
            var discoveries = new WildsDiscoveries();

            discoveries.Geometry.Register(
                new DiscoveryGeometryProfile(
                    profileId,
                    "plain",
                    "1",
                    "scope",
                    new List<IDiscoveryClusterMapping>
                    {
                        Map(profileId)
                    }));

            var fixture = new H2TestFixture();

            Assert.Throws<InvalidOperationException>(
                () =>
                    discoveries.Geometry.Observe(
                        fixture.Eval(
                            DateTimeOffset.UtcNow,
                            ("x", 0.0),
                            ("y", 0.0)),
                        "Concern"));
        }

        private static WildsDiscoveries Create(
            int consecutive,
            double occupancy,
            double stability)
        {
            var profileId = Guid.NewGuid();
            var discoveries = new WildsDiscoveries();

            discoveries.Geometry.Register(
                new DiscoveryGeometryProfile(
                    profileId,
                    "profile",
                    "1",
                    "scope",
                    new List<IDiscoveryClusterMapping>
                    {
                        Map(profileId)
                    },
                    new MinimumClusterPersistencePolicy(
                        "persist",
                        consecutive,
                        occupancy,
                        stability),
                    new BoundaryVarianceClusterStabilityMetric(),
                    new PersistentClusterRecognitionPolicy()));

            return discoveries;
        }

        private static IDiscoveryClusterMapping Map(
            Guid profileId)
        {
            return new DiscoveryClusterMapping(
                Guid.NewGuid(),
                "concern",
                profileId,
                "profile",
                "1",
                "Concern",
                "hypersphere",
                new Dictionary<string, double>
                {
                    { "x", 0.0 },
                    { "y", 0.0 }
                },
                1.0);
        }

        private static IReadOnlyList<IDiscoveryClusterObservation> MetricSeries(
            params double[] signedDistances)
        {
            var profileId = Guid.NewGuid();
            var mapping = Map(profileId);
            var fixture = new H2TestFixture();

            var observations =
                new List<IDiscoveryClusterObservation>();

            var projector =
                new HypersphereDiscoveryClusterRelationshipProjector();

            var timestamp =
                DateTimeOffset.UtcNow;

            for (var index = 0;
                 index < signedDistances.Length;
                 index++)
            {
                var x =
                    1.0 + signedDistances[index];

                var evaluation =
                    fixture.Eval(
                        timestamp.AddSeconds(index),
                        ("x", x),
                        ("y", 0.0));

                var relationship =
                    projector.Locate(
                        evaluation,
                        mapping);

                observations.Add(
                    new DiscoveryClusterObservation(
                        relationship));
            }

            return observations;
        }
    }
}