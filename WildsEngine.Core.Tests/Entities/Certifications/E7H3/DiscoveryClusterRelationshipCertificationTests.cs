using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Abstractions.Motion;
using WildsEngine.Discoveries.Clusters;
using WildsEngine.Discoveries.Motion;
using WildsEngine.Discoveries.Vectors;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class DiscoveryClusterRelationshipCertificationTests
    {
        [Fact] public void LocateReturnsCentroidAndSignedBoundaryDistance()
        {
            var f=new H2TestFixture();var e=f.Eval(DateTimeOffset.UtcNow,("x",3.0),("y",4.0));var m=Mapping(0,0,2);
            var r=new HypersphereDiscoveryClusterRelationshipProjector().Locate(e,m);
            Assert.Equal(5,r.DistanceToCentroid,10);Assert.Equal(3,r.SignedDistanceToBoundary,10);Assert.False(r.IsInside);Assert.Equal(1.2,r.NearestBoundaryPoint["x"],10);Assert.Equal(1.6,r.NearestBoundaryPoint["y"],10);
        }
        [Fact] public void InsideProducesNegativeBoundaryDistance()
        { var f=new H2TestFixture();var r=new HypersphereDiscoveryClusterRelationshipProjector().Locate(f.Eval(DateTimeOffset.UtcNow,("x",.5),("y",0)),Mapping(0,0,1));Assert.True(r.IsInside);Assert.Equal(-.5,r.SignedDistanceToBoundary,10); }
        [Fact] public void MissingMappedDimensionFails()
        { var f=new H2TestFixture();Assert.Throws<InvalidOperationException>(()=>new HypersphereDiscoveryClusterRelationshipProjector().Locate(f.Eval(DateTimeOffset.UtcNow,("x",.5)),Mapping(0,0,1))); }
        internal static IDiscoveryClusterMapping Mapping(double x,double y,double radius)=>new DiscoveryClusterMapping(Guid.NewGuid(),"target",Guid.NewGuid(),"doc","1","Target","hypersphere",new Dictionary<string,double>{{"x",x},{"y",y}},radius);
        internal static IDiscoveryVectorMotionResult Motion(IDiscoveryVectorEvaluation e,double x,double y)=>new DiscoveryVectorMotionResult(Guid.NewGuid(),new DiscoveryVectorMotionServiceResult(e,new List<IDiscoveryVectorMotion>{new DiscoveryVectorMotion("x",x,0),new DiscoveryVectorMotion("y",y,0)}));
    }
}
