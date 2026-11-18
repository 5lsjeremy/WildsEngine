using System;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Clusters;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class DiscoveryClusterTrajectoryCertificationTests
    {
        [Fact] public void OutsideTrajectoryCrossesSphereAndReturnsEntryExitDuration()
        { var f=new H2TestFixture();var e=f.Eval(DateTimeOffset.UtcNow,("x",-3.0),("y",0));var r=new HypersphereDiscoveryClusterRelationshipProjector().Project(DiscoveryClusterRelationshipCertificationTests.Motion(e,1,0),DiscoveryClusterRelationshipCertificationTests.Mapping(0,0,1));Assert.Equal(DiscoveryClusterTransitKind.PassingThrough,r.TransitKind);Assert.Equal(2,r.TimeToEntrySeconds!.Value,10);Assert.Equal(4,r.TimeToExitSeconds!.Value,10);Assert.Equal(2,r.ProjectedDurationInsideSeconds!.Value,10);Assert.Equal(-1,r.EntryPoint!["x"],10);Assert.Equal(1,r.ExitPoint!["x"],10); }
        [Fact] public void FlybyReturnsClosestApproachWithoutOccupancy()
        { var f=new H2TestFixture();var e=f.Eval(DateTimeOffset.UtcNow,("x",-3.0),("y",2.0));var r=new HypersphereDiscoveryClusterRelationshipProjector().Project(DiscoveryClusterRelationshipCertificationTests.Motion(e,1,0),DiscoveryClusterRelationshipCertificationTests.Mapping(0,0,1));Assert.Null(r.TimeToEntrySeconds);Assert.Null(r.ProjectedDurationInsideSeconds);Assert.Equal(3,r.TimeToClosestApproachSeconds,10);Assert.Equal(2,r.ClosestApproachDistanceToCentroid,10);Assert.Equal(1,r.ClosestApproachDistanceToBoundary,10); }
        [Fact] public void InsideTrajectoryReturnsExitAndRemainingDuration()
        { var f=new H2TestFixture();var e=f.Eval(DateTimeOffset.UtcNow,("x",0.0),("y",0));var r=new HypersphereDiscoveryClusterRelationshipProjector().Project(DiscoveryClusterRelationshipCertificationTests.Motion(e,1,0),DiscoveryClusterRelationshipCertificationTests.Mapping(0,0,1));Assert.Equal(DiscoveryClusterTransitKind.Exiting,r.TransitKind);Assert.Equal(0,r.TimeToEntrySeconds!.Value);Assert.Equal(1,r.TimeToExitSeconds!.Value,10);Assert.Equal(1,r.ProjectedDurationInsideSeconds!.Value,10); }
        [Fact] public void RecedingTrajectoryDoesNotInventFutureIntersection()
        { var f=new H2TestFixture();var e=f.Eval(DateTimeOffset.UtcNow,("x",2.0),("y",0));var r=new HypersphereDiscoveryClusterRelationshipProjector().Project(DiscoveryClusterRelationshipCertificationTests.Motion(e,1,0),DiscoveryClusterRelationshipCertificationTests.Mapping(0,0,1));Assert.True(r.IsReceding);Assert.Null(r.TimeToEntrySeconds);Assert.Equal(0,r.TimeToClosestApproachSeconds); }
        [Fact] public void ProjectionExplicitlyDeclaresConstantVelocityBasis()
        { var f=new H2TestFixture();var e=f.Eval(DateTimeOffset.UtcNow,("x",-3.0),("y",0));var r=new HypersphereDiscoveryClusterRelationshipProjector().Project(DiscoveryClusterRelationshipCertificationTests.Motion(e,1,0),DiscoveryClusterRelationshipCertificationTests.Mapping(0,0,1));Assert.Equal("constant-velocity",r.ProjectionBasis); }
    }
}
