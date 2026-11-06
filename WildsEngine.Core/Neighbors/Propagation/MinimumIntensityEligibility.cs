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
            ValidateIntensity(
                minimumIntensity,
                nameof(minimumIntensity));

            MinimumIntensity =
                minimumIntensity;
        }

        public double MinimumIntensity { get; }

        public bool IsEligible(
            double intensity)
        {
            ValidateIntensity(
                intensity,
                nameof(intensity));

            return intensity >=
                   MinimumIntensity;
        }

        private static void ValidateIntensity(
            double value,
            string parameterName)
        {
            if (double.IsNaN(
                    value) ||
                double.IsInfinity(
                    value) ||
                value < 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName);
            }
        }
    }
}