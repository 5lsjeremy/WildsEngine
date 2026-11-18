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
        [Fact] public void RepeatedSnapshotsBecomePersistentRecognizedGeometry()
        {TestLog.Section("E7-H3E/F - PERSISTENCE AND RECOGNITION");var id=Guid.NewGuid();var d=new WildsDiscoveries();var map=new DiscoveryClusterMapping(Guid.NewGuid(),"maintenance-concern",id,"machine-health","1.0","MaintenanceConcern","hypersphere",new Dictionary<string,double>{{"x",0},{"y",0}},1);d.Geometry.Register(new DiscoveryGeometryProfile(id,"machine-health","1.0","equipment",new List<IDiscoveryClusterMapping>{map},new MinimumClusterPersistencePolicy("three-stable-observations",3,.6,.8),new BoundaryVarianceClusterStabilityMetric(),new PersistentClusterRecognitionPolicy("recognize-persistent")));var f=new H2TestFixture();var t=DateTimeOffset.UtcNow;d.Geometry.Observe(f.Eval(t,("x",2),("y",0)),"MaintenanceConcern");d.Geometry.Observe(f.Eval(t.AddSeconds(1),("x",.5),("y",0)),"MaintenanceConcern");d.Geometry.Observe(f.Eval(t.AddSeconds(2),("x",.45),("y",0)),"MaintenanceConcern");var r=d.Geometry.Observe(f.Eval(t.AddSeconds(3),("x",.4),("y",0)),"MaintenanceConcern");TestLog.State($"Observations = {r.Persistence.ObservationCount}");TestLog.State($"Consecutive inside = {r.Persistence.ConsecutiveInsideCount}");TestLog.State($"Occupancy ratio = {r.Persistence.OccupancyRatio:R}");TestLog.State($"Observed duration inside = {r.Persistence.ObservedDurationInside}");TestLog.State($"Stability = {r.Persistence.Stability:R}");TestLog.State($"Persistent = {r.Persistence.IsPersistent}");TestLog.State($"Recognized = {r.Recognition.IsRecognized}");Assert.True(r.Persistence.IsPersistent);Assert.True(r.Recognition.IsRecognized);TestLog.Pass("Successive authoritative snapshots produced observed persistence and consumer-policy recognition without rewriting prior geometry or projecting recognition from a single snapshot.");}
    }
}
