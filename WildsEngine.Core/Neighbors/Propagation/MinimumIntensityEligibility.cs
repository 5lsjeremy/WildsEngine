using System;
using WildsEngine.Core.Abstractions.Neighbors.Propagation;

namespace WildsEngine.Core.Neighbors.Propagation
{
    public sealed class MinimumIntensityEligibility :
        INeighborPropagationEligibility
    {
        public MinimumIntensityEligibility(
            double minimumIntensity)
        {
            if (minimumIntensity < 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumIntensity));
            }

            MinimumIntensity =
                minimumIntensity;
        }

        public double MinimumIntensity { get; }

        public bool IsEligible(
            double intensity)
        {
            if (intensity < 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(intensity));
            }

            return intensity >=
                   MinimumIntensity;
        }
    }
}