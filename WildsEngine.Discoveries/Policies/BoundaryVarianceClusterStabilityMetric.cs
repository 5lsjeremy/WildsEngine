using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Base;

namespace WildsEngine.Discoveries.Policies
{
    public sealed class BoundaryVarianceClusterStabilityMetric :
        IdentifierBase,
        IDiscoveryClusterStabilityMetric
    {
        public BoundaryVarianceClusterStabilityMetric(
            string slug = "boundary-variance")
            : base(
                Guid.NewGuid(),
                slug)
        {
        }

        public double GetStability(
            IReadOnlyList<IDiscoveryClusterObservation> observations)
        {
            if (observations == null ||
                observations.Count == 0)
            {
                throw new ArgumentException(
                    "At least one observation is required.",
                    nameof(observations));
            }

            if (observations.Count == 1)
            {
                return 1.0;
            }

            double mean = 0.0;

            for (var index = 0;
                 index < observations.Count;
                 index++)
            {
                mean +=
                    observations[index]
                        .Relationship
                        .SignedDistanceToBoundary;
            }

            mean /= observations.Count;

            double variance = 0.0;

            for (var index = 0;
                 index < observations.Count;
                 index++)
            {
                var delta =
                    observations[index]
                        .Relationship
                        .SignedDistanceToBoundary -
                    mean;

                variance +=
                    delta * delta;
            }

            variance /=
                observations.Count;

            return
                1.0 /
                (1.0 + variance);
        }
    }
}