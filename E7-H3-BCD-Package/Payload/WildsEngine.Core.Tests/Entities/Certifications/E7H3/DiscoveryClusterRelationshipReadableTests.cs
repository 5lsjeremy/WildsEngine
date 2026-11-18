using System;
using WildsEngine.Discoveries.Clusters;
using WildsEngine.Tests.Logging;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class DiscoveryClusterRelationshipReadableTests
    {
        [Fact] public void SnapshotTrajectoryReturnsSpatialAndTemporalClusterRelationship()
        {
            TestLog.Section("E7-H3B/C/D - CLUSTER RELATIONSHIP AND OCCUPANCY");
            var fixture=new H2TestFixture();var evaluation=fixture.Eval(DateTimeOffset.UtcNow,("x",-3.0),("y",0.0));
            var projection=new HypersphereDiscoveryClusterRelationshipProjector().Project(DiscoveryClusterRelationshipCertificationTests.Motion(evaluation,1.0,0.0),DiscoveryClusterRelationshipCertificationTests.Mapping(0,0,1));
            TestLog.State($"Semantic key = {projection.Spatial.Mapping.SemanticKey}");
            TestLog.State($"Centroid distance = {projection.Spatial.DistanceToCentroid:R}");
            TestLog.State($"Signed boundary distance = {projection.Spatial.SignedDistanceToBoundary:R}");
            TestLog.State($"Transit = {projection.TransitKind}");
            TestLog.State($"Entry in = {projection.TimeToEntrySeconds:R} seconds");
            TestLog.State($"Exit in = {projection.TimeToExitSeconds:R} seconds");
            TestLog.State($"Projected duration inside = {projection.ProjectedDurationInsideSeconds:R} seconds");
            Assert.Equal(2,projection.TimeToEntrySeconds!.Value,10);Assert.Equal(2,projection.ProjectedDurationInsideSeconds!.Value,10);
            TestLog.Pass("The current snapshot was located against consumer geometry and its constant-velocity trajectory produced attributable entry, exit, and projected occupancy duration.");
        }
    }
}
