using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions.Motion;
using WildsEngine.Discoveries.Motion;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class DiscoveryVectorSeriesCertificationTests
    {
        [Fact] public void SeriesPreservesOrderCurrentPreviousAndExecutionIds()
        {
            var f=new H2TestFixture(); var t=DateTimeOffset.UtcNow; var a=f.Eval(t,("risk",.2)); var b=f.Eval(t.AddSeconds(1),("risk",.5));
            var s=f.Series(a,b); Assert.Same(b,s.Current); Assert.Same(a,s.Previous); Assert.Equal(a.VectorizationExecutionId,s.Evaluations[0].VectorizationExecutionId);
        }
        [Fact] public void SeriesSnapshotsCallerCollections()
        {
            var f=new H2TestFixture(); var e=f.Eval(DateTimeOffset.UtcNow,("risk",.2)); var inputs=new List<WildsEngine.Discoveries.Abstractions.IDiscoveryVectorInput>{f.Input}; var evals=new List<IDiscoveryVectorEvaluation>{e};
            var s=new DiscoveryVectorEvaluationSeriesFactory().Create("scope",inputs,evals); inputs.Clear(); evals.Clear(); Assert.Single(s.Inputs); Assert.Single(s.Evaluations);
        }
        [Fact] public void EqualOrDescendingTimesFail()
        {
            var f=new H2TestFixture(); var t=DateTimeOffset.UtcNow; var a=f.Eval(t,("risk",.2)); var b=f.Eval(t,("risk",.5)); var c=f.Eval(t.AddSeconds(-1),("risk",.8));
            Assert.Throws<ArgumentException>(()=>f.Series(a,b)); Assert.Throws<ArgumentException>(()=>f.Series(a,c));
        }
        [Fact] public void SingleEvaluationHasCurrentWithoutPrevious()
        { var f=new H2TestFixture(); var e=f.Eval(DateTimeOffset.UtcNow,("risk",.2)); var s=f.Series(e); Assert.Same(e,s.Current); Assert.Null(s.Previous); }
    }
}
