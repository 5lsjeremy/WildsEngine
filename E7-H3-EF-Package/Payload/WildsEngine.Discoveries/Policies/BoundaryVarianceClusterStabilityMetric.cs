using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Base;
namespace WildsEngine.Discoveries.Policies
{
    public sealed class BoundaryVarianceClusterStabilityMetric : IdentifierBase, IDiscoveryClusterStabilityMetric
    {
        public BoundaryVarianceClusterStabilityMetric(string slug="boundary-variance"):base(Guid.NewGuid(),slug){}
        public double GetStability(IReadOnlyList<IDiscoveryClusterObservation> observations){if(observations==null||observations.Count==0)throw new ArgumentException("At least one observation is required.",nameof(observations));if(observations.Count==1)return 1.0;double mean=0;for(var i=0;i<observations.Count;i++)mean+=observations[i].Relationship.SignedDistanceToBoundary;mean/=observations.Count;double variance=0;for(var i=0;i<observations.Count;i++){var d=observations[i].Relationship.SignedDistanceToBoundary-mean;variance+=d*d;}variance/=observations.Count;return 1.0/(1.0+variance);}
    }
}
