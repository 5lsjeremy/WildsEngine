using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Abstractions.Motion;
using WildsEngine.Discoveries.Curves;
using WildsEngine.Discoveries.Motion;
using WildsEngine.Discoveries.Vectors;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class DiscoveryVectorMomentumCurveCertificationTests
    {
        private static IDiscoveryVectorMotionResult Raw(double v,double a)
        { var f=new H2TestFixture(); var e=f.Eval(DateTimeOffset.UtcNow,("risk",.5)); return new DiscoveryVectorMotionResult(Guid.NewGuid(),new DiscoveryVectorMotionServiceResult(e,new List<IDiscoveryVectorMotion>{new DiscoveryVectorMotion("risk",v,a)})); }
        [Fact] public void IdentityCurvePreservesRawMotion()
        { var raw=Raw(.6,-.2); var r=new DiscoveryVectorMomentumProjector().Project(raw,new IdentityDiscoveryMotionCurve()); Assert.Equal(.6,r.AdjustedMotion[0].Velocity); Assert.Equal(-.2,r.AdjustedMotion[0].Acceleration); Assert.Same(raw.Motion[0],r.RawMotion[0]); }
        [Fact] public void BoundedCurveIsDeterministicAndPreservesSignsAndZero()
        { var curve=new BoundedTanhDiscoveryMotionCurve(2,1); var p=new DiscoveryVectorMomentumProjector(); var a=p.Project(Raw(.8,-.5),curve); var b=p.Project(Raw(.8,-.5),curve); Assert.Equal(a.AdjustedMotion[0].Velocity,b.AdjustedMotion[0].Velocity); Assert.True(a.AdjustedMotion[0].Velocity>0); Assert.True(a.AdjustedMotion[0].Acceleration<0); Assert.Equal(0,curve.TransformVelocity("risk",0)); }
        [Fact] public void InvalidCurveParametersFail()
        { Assert.Throws<ArgumentOutOfRangeException>(()=>new BoundedTanhDiscoveryMotionCurve(0,1)); Assert.Throws<ArgumentOutOfRangeException>(()=>new BoundedTanhDiscoveryMotionCurve(1,0)); }
        [Fact] public void DifferentCurvesProduceDifferentAdjustedMotionWithoutChangingRaw()
        { var raw=Raw(2,1); var p=new DiscoveryVectorMomentumProjector(); var identity=p.Project(raw,new IdentityDiscoveryMotionCurve()); var bounded=p.Project(raw,new BoundedTanhDiscoveryMotionCurve(1,1)); Assert.NotEqual(identity.AdjustedMotion[0].Velocity,bounded.AdjustedMotion[0].Velocity); Assert.Equal(2,raw.Motion[0].Velocity); Assert.Equal(raw.ServiceExecutionId,bounded.MotionServiceExecutionId); }
    }
}
