using System;
using System.Linq;
using System.Threading.Tasks;
using WildsEngine.Discoveries.Curves;
using WildsEngine.Discoveries.Motion;
using WildsEngine.Discoveries.Services;
using WildsEngine.Tests.Logging;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class DiscoveryVectorMotionReadableTests
    {
        [Fact] public async Task KnowledgePositionBecomesAttributedTrajectory()
        {
            TestLog.Section("E7-H2 - KNOWLEDGE TRAJECTORY");
            var f=new H2TestFixture(); var t=DateTimeOffset.UtcNow;
            var e0=f.Eval(t,("risk",.2),("trust",.7)); var e1=f.Eval(t.AddSeconds(1),("risk",.5),("trust",.55)); var e2=f.Eval(t.AddSeconds(2),("risk",.8),("trust",.4));
            var series=f.Series(e0,e1,e2); var clock=new SystemWildsServiceClock(); var journal=new WildsServiceExecutionJournal(); var service=new FiniteDifferenceDiscoveryVectorMotionService("knowledge-motion",new WildsServiceContextFactory(clock),journal,clock);
            var raw=await new DiscoveryVectorMotionEvaluatorFactory().Create(service).EvaluateAsync(series,pulseId:Guid.NewGuid());
            var momentum=new DiscoveryVectorMomentumProjector().Project(raw,new BoundedTanhDiscoveryMotionCurve(2,1));
            TestLog.State($"Scope = {series.Scope}"); TestLog.State($"Vectorization executions = {string.Join(", ",series.Evaluations.Select(x=>x.VectorizationExecutionId))}"); TestLog.State($"Motion execution = {raw.ServiceExecutionId}");
            for(var i=0;i<raw.Motion.Count;i++){ TestLog.State($"{raw.Motion[i].Dimension}: velocity={raw.Motion[i].Velocity:R}, acceleration={raw.Motion[i].Acceleration:R}, adjustedVelocity={momentum.AdjustedMotion[i].Velocity:R}"); }
            Assert.Equal(.3,raw.Motion.Single(x=>x.Dimension=="risk").Velocity,10); Assert.Equal(-.15,raw.Motion.Single(x=>x.Dimension=="trust").Velocity,10); Assert.False(f.Discovery.IsAvailable);
            TestLog.Pass("Immutable vector history produced attributable raw motion and explicit curve-adjusted momentum without changing Discovery resolution.");
        }
    }
}
