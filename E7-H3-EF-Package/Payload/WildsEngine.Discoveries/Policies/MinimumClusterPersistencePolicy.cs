using System;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Base;
namespace WildsEngine.Discoveries.Policies
{
    public sealed class MinimumClusterPersistencePolicy : IdentifierBase, IDiscoveryClusterPersistencePolicy
    {
        public MinimumClusterPersistencePolicy(string slug,int minimumConsecutiveInside,double minimumOccupancyRatio,double minimumStability):base(Guid.NewGuid(),slug)
        {if(minimumConsecutiveInside<1)throw new ArgumentOutOfRangeException(nameof(minimumConsecutiveInside));if(minimumOccupancyRatio<0||minimumOccupancyRatio>1)throw new ArgumentOutOfRangeException(nameof(minimumOccupancyRatio));if(minimumStability<0||minimumStability>1)throw new ArgumentOutOfRangeException(nameof(minimumStability));MinimumConsecutiveInside=minimumConsecutiveInside;MinimumOccupancyRatio=minimumOccupancyRatio;MinimumStability=minimumStability;}
        public int MinimumConsecutiveInside{get;} public double MinimumOccupancyRatio{get;} public double MinimumStability{get;}
        public bool IsSatisfied(IDiscoveryClusterPersistenceResult r){if(r==null)throw new ArgumentNullException(nameof(r));return r.ConsecutiveInsideCount>=MinimumConsecutiveInside&&r.OccupancyRatio>=MinimumOccupancyRatio&&r.Stability>=MinimumStability;}
    }
}
