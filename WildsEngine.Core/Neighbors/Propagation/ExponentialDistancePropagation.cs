using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Abstractions.Neighbors.Propagation;

namespace WildsEngine.Core.Neighbors.Propagation
{
    public sealed class ExponentialDistancePropagation :
        INeighborPropagation
    {
        private readonly double _decay;

        public ExponentialDistancePropagation(
            double decay)
        {
            ValidateNonNegativeFinite(
                decay,
                nameof(decay));

            _decay =
                decay;
        }

        public double Propagate(
            double intensity,
            INeighborRelationship relationship)
        {
            if (relationship == null)
            {
                throw new ArgumentNullException(
                    nameof(relationship));
            }

            ValidateNonNegativeFinite(
                intensity,
                nameof(intensity));

            var distance =
                relationship.Distance;

            ValidateNonNegativeFinite(
                distance,
                nameof(relationship));

            var exponent =
                -_decay *
                distance;

            /*
             * Very large finite decay and distance values can
             * overflow their product to negative infinity.
             *
             * For non-negative decay and distance, that limit
             * legitimately represents complete attenuation:
             *
             *     Exp(-Infinity) = 0
             *
             * NaN remains invalid and must never pass through.
             */
            if (double.IsNaN(
                    exponent))
            {
                throw new ArithmeticException(
                    "Propagation produced an invalid exponent.");
            }

            var attenuation =
                Math.Exp(
                    exponent);

            if (double.IsNaN(
                    attenuation) ||
                double.IsInfinity(
                    attenuation))
            {
                throw new ArithmeticException(
                    "Propagation produced non-finite attenuation.");
            }

            var propagated =
                intensity *
                attenuation;

            if (double.IsNaN(
                    propagated) ||
                double.IsInfinity(
                    propagated))
            {
                throw new ArithmeticException(
                    "Propagation produced a non-finite result.");
            }

            return propagated;
        }

        private static void ValidateNonNegativeFinite(
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