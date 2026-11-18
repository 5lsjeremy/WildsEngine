using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WildsEngine.Discoveries.Abstractions.Clusters;
namespace WildsEngine.Discoveries.Clusters
{
    public sealed class DiscoveryClusterPersistenceResult : IDiscoveryClusterPersistenceResult
    {
        public DiscoveryClusterPersistenceResult(IDiscoveryClusterMapping mapping,IReadOnlyList<IDiscoveryClusterObservation> observations,double stability,IDiscoveryClusterPersistencePolicy policy,IDiscoveryClusterStabilityMetric metric)
        {
            Mapping=mapping??throw new ArgumentNullException(nameof(mapping));
            if(observations==null||observations.Count==0)throw new ArgumentException("At least one observation is required.",nameof(observations));
            if(double.IsNaN(stability)||double.IsInfinity(stability)||stability<0||stability>1)throw new ArgumentOutOfRangeException(nameof(stability));
            if(policy==null)throw new ArgumentNullException(nameof(policy));if(metric==null)throw new ArgumentNullException(nameof(metric));
            var copy=new IDiscoveryClusterObservation[observations.Count];int inside=0,consecutive=0;DateTimeOffset? entry=null,exit=null;
            for(var i=0;i<observations.Count;i++){
                copy[i]=observations[i]??throw new ArgumentException("Observations cannot contain null.",nameof(observations));
                if(copy[i].Relationship.Mapping.Id!=mapping.Id)throw new ArgumentException("Every observation must reference the same mapping.",nameof(observations));
                if(i>0&&copy[i].ObservedAt<=copy[i-1].ObservedAt)throw new ArgumentException("Observation times must be strictly increasing.",nameof(observations));
                var isInside=copy[i].Relationship.IsInside||copy[i].Relationship.IsOnBoundary;
                if(isInside){inside++;consecutive++;if(!entry.HasValue||(i>0&&!(copy[i-1].Relationship.IsInside||copy[i-1].Relationship.IsOnBoundary)))entry=copy[i].ObservedAt;exit=null;}
                else {if(i>0&&(copy[i-1].Relationship.IsInside||copy[i-1].Relationship.IsOnBoundary))exit=copy[i].ObservedAt;consecutive=0;}
            }
            Observations=new ReadOnlyCollection<IDiscoveryClusterObservation>(copy);ObservationCount=copy.Length;InsideObservationCount=inside;ConsecutiveInsideCount=consecutive;OccupancyRatio=(double)inside/copy.Length;ObservedEntryAt=entry;ObservedExitAt=exit;
            if(entry.HasValue)ObservedDurationInside=(exit.HasValue?exit.Value:copy[copy.Length-1].ObservedAt)-entry.Value;
            Stability=stability;PersistencePolicyId=policy.Id;PersistencePolicySlug=policy.Slug;StabilityMetricId=metric.Id;StabilityMetricSlug=metric.Slug;IsPersistent=policy.IsSatisfied(this);
        }
        public IDiscoveryClusterMapping Mapping{get;} public IReadOnlyList<IDiscoveryClusterObservation> Observations{get;} public int ObservationCount{get;} public int InsideObservationCount{get;} public int ConsecutiveInsideCount{get;} public double OccupancyRatio{get;} public DateTimeOffset? ObservedEntryAt{get;} public DateTimeOffset? ObservedExitAt{get;} public TimeSpan? ObservedDurationInside{get;} public double Stability{get;} public bool IsPersistent{get;} public Guid PersistencePolicyId{get;} public string PersistencePolicySlug{get;} public Guid StabilityMetricId{get;} public string StabilityMetricSlug{get;}
    }
}
