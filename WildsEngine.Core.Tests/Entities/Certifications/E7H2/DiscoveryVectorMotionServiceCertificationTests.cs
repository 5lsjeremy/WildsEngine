using System;
using System.Threading.Tasks;
using WildsEngine.Discoveries.Abstractions.Motion;
using WildsEngine.Discoveries.Services;
using WildsEngine.Discoveries.Motion;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class DiscoveryVectorMotionServiceCertificationTests
    {
        private static (IDiscoveryVectorMotionEvaluator Evaluator, WildsServiceExecutionJournal Journal) Create()
        { var clock=new SystemWildsServiceClock(); var journal=new WildsServiceExecutionJournal(); var service=new FiniteDifferenceDiscoveryVectorMotionService("h2-motion",new WildsServiceContextFactory(clock),journal,clock); return(new DiscoveryVectorMotionEvaluatorFactory().Create(service),journal); }
        [Fact] public async Task FirstObservationProducesNoMotion()
        { var f=new H2TestFixture(); var x=Create(); var r=await x.Evaluator.EvaluateAsync(f.Series(f.Eval(DateTimeOffset.UtcNow,("risk",.2)))); Assert.Empty(r.Motion); }
        [Fact] public async Task SecondObservationProducesVelocityAndZeroAcceleration()
        { var f=new H2TestFixture(); var t=DateTimeOffset.UtcNow; var x=Create(); var r=await x.Evaluator.EvaluateAsync(f.Series(f.Eval(t,("risk",.2)),f.Eval(t.AddSeconds(2),("risk",.8)))); Assert.Equal(.3,r.Motion[0].Velocity,10); Assert.Equal(0,r.Motion[0].Acceleration,10); }
        [Fact] public async Task ThirdObservationDerivesAccelerationAndReversal()
        { var f=new H2TestFixture(); var t=DateTimeOffset.UtcNow; var x=Create(); var r=await x.Evaluator.EvaluateAsync(f.Series(f.Eval(t,("risk",.2)),f.Eval(t.AddSeconds(1),("risk",.8)),f.Eval(t.AddSeconds(2),("risk",.4)))); Assert.Equal(-.4,r.Motion[0].Velocity,10); Assert.Equal(-1.0,r.Motion[0].Acceleration,10); }
        [Fact] public async Task MissingDimensionsAreOmittedAndExecutionIsJournaled()
        { var f=new H2TestFixture(); var t=DateTimeOffset.UtcNow; var x=Create(); var pulse=Guid.NewGuid(); var r=await x.Evaluator.EvaluateAsync(f.Series(f.Eval(t,("risk",.2),("trust",.7)),f.Eval(t.AddSeconds(1),("risk",.5))),pulseId:pulse); Assert.Single(r.Motion); Assert.True(x.Journal.TryGet(r.ServiceExecutionId,out var record)); Assert.Equal(pulse,record!.PulseId); }
        [Fact] public async Task SourceVectorsAndResolutionRemainUntouched()
        { var f=new H2TestFixture(); var t=DateTimeOffset.UtcNow; var a=f.Eval(t,("risk",.2)); var b=f.Eval(t.AddSeconds(1),("risk",.5)); var before=f.Discovery.CurrentFactCount; var x=Create(); await x.Evaluator.EvaluateAsync(f.Series(a,b)); Assert.Equal(.2,a.Vector.Components[0].Value); Assert.Equal(.5,b.Vector.Components[0].Value); Assert.Equal(before,f.Discovery.CurrentFactCount); Assert.False(f.Discovery.IsAvailable); }
    }
}
