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
        [Fact] public void ConsecutiveObservedOccupancyBecomesPersistentAndRecognized()
        {var d=Create(3,.6,.8);var f=new H2TestFixture();var t=DateTimeOffset.UtcNow;d.Geometry.Observe(f.Eval(t,("x",2.0),("y",0)),"Concern");d.Geometry.Observe(f.Eval(t.AddSeconds(1),("x",.5),("y",0)),"Concern");d.Geometry.Observe(f.Eval(t.AddSeconds(2),("x",.4),("y",0)),"Concern");var r=d.Geometry.Observe(f.Eval(t.AddSeconds(3),("x",.3),("y",0)),"Concern");Assert.Equal(3,r.Persistence.ConsecutiveInsideCount);Assert.Equal(.75,r.Persistence.OccupancyRatio,10);Assert.True(r.Persistence.IsPersistent);Assert.True(r.Recognition.IsRecognized);Assert.Equal(TimeSpan.FromSeconds(2),r.Persistence.ObservedDurationInside);}
        [Fact] public void TransientOccupancyDoesNotRecognize()
        {var d=Create(2,.5,.7);var f=new H2TestFixture();var t=DateTimeOffset.UtcNow;d.Geometry.Observe(f.Eval(t,("x",.5),("y",0)),"Concern");var r=d.Geometry.Observe(f.Eval(t.AddSeconds(1),("x",2.0),("y",0)),"Concern");Assert.False(r.Persistence.IsPersistent);Assert.False(r.Recognition.IsRecognized);Assert.Equal(0,r.Persistence.ConsecutiveInsideCount);}
        [Fact] public void ExitIsObservedAndConsecutiveOccupancyResets()
        {var d=Create(2,.3,0);var f=new H2TestFixture();var t=DateTimeOffset.UtcNow;d.Geometry.Observe(f.Eval(t,("x",.5),("y",0)),"Concern");d.Geometry.Observe(f.Eval(t.AddSeconds(1),("x",.4),("y",0)),"Concern");var r=d.Geometry.Observe(f.Eval(t.AddSeconds(2),("x",2),("y",0)),"Concern");Assert.NotNull(r.Persistence.ObservedExitAt);Assert.Equal(TimeSpan.FromSeconds(2),r.Persistence.ObservedDurationInside);Assert.Equal(0,r.Persistence.ConsecutiveInsideCount);}
        [Fact] public void StabilityMetricDistinguishesStableFromVariableBoundaryHistory()
        {var metric=new BoundaryVarianceClusterStabilityMetric();var stable=MetricSeries(.2,.2,.2);var variable=MetricSeries(-.9,.9,-.9);Assert.True(metric.GetStability(stable)>metric.GetStability(variable));}
        [Fact] public void PolicyAndMetricIdentityArePreservedInPersistenceResult()
        {var d=Create(1,0,0);var f=new H2TestFixture();var r=d.Geometry.Observe(f.Eval(DateTimeOffset.UtcNow,("x",.5),("y",0)),"Concern");Assert.NotEqual(Guid.Empty,r.Persistence.PersistencePolicyId);Assert.NotEqual(Guid.Empty,r.Persistence.StabilityMetricId);Assert.NotEqual(Guid.Empty,r.Recognition.RecognitionPolicyId);}
        [Fact] public void ObserveRequiresProfilePersistenceAndRecognitionConfiguration()
        {var id=Guid.NewGuid();var d=new WildsDiscoveries();d.Geometry.Register(new DiscoveryGeometryProfile(id,"plain","1","scope",new List<IDiscoveryClusterMapping>{Map(id)}));var f=new H2TestFixture();Assert.Throws<InvalidOperationException>(()=>d.Geometry.Observe(f.Eval(DateTimeOffset.UtcNow,("x",0),("y",0)),"Concern"));}
        private static WildsDiscoveries Create(int consecutive,double occupancy,double stability){var id=Guid.NewGuid();var d=new WildsDiscoveries();d.Geometry.Register(new DiscoveryGeometryProfile(id,"profile","1","scope",new List<IDiscoveryClusterMapping>{Map(id)},new MinimumClusterPersistencePolicy("persist",consecutive,occupancy,stability),new BoundaryVarianceClusterStabilityMetric(),new PersistentClusterRecognitionPolicy()));return d;}
        private static IDiscoveryClusterMapping Map(Guid id)=>new DiscoveryClusterMapping(Guid.NewGuid(),"concern",id,"profile","1","Concern","hypersphere",new Dictionary<string,double>{{"x",0},{"y",0}},1);
        private static IReadOnlyList<IDiscoveryClusterObservation> MetricSeries(params double[] signed){var id=Guid.NewGuid();var m=Map(id);var f=new H2TestFixture();var list=new List<IDiscoveryClusterObservation>();var p=new HypersphereDiscoveryClusterRelationshipProjector();var t=DateTimeOffset.UtcNow;for(var i=0;i<signed.Length;i++){var x=1+signed[i];list.Add(new DiscoveryClusterObservation(p.Locate(f.Eval(t.AddSeconds(i),("x",x),("y",0)),m)));}return list;}
    }
}
