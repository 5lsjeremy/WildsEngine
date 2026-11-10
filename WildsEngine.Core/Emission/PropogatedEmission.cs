using System;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Emission
{
    /// <summary>
    /// Immutable result of propagating one resolved emission
    /// through one exact Neighbor relationship.
    ///
    /// The original resolved emission and its authoritative
    /// magnitude are preserved unchanged.
    ///
    /// NormalizedValue is present when propagation occurred
    /// through a normalized emission vector. It is null for
    /// direct single-emission propagation.
    ///
    /// PropagationDistance preserves the relationship distance
    /// used when this result was produced. It remains stable
    /// even if the live relationship later changes.
    ///
    /// Value represents the intensity received through this
    /// particular relationship.
    ///
    /// The carrier owns no eligibility or receiver-state
    /// behavior.
    /// </summary>
    public sealed class PropagatedEmission :
        IPropagatedEmission
    {
        /// <summary>
        /// Creates a direct single-emission propagation result.
        ///
        /// No normalized vector input exists for this route.
        /// </summary>
        public PropagatedEmission(
            IResolvedEmission emission,
            INeighborRelationship relationship,
            double value)
            : this(
                emission,
                relationship,
                normalizedValue: null,
                value)
        {
        }

        /// <summary>
        /// Creates a normalized vector-propagation result.
        ///
        /// normalizedValue is the bounded component supplied
        /// to the propagation policy.
        /// </summary>
        public PropagatedEmission(
            IResolvedEmission emission,
            INeighborRelationship relationship,
            double normalizedValue,
            double value)
            : this(
                emission,
                relationship,
                normalizedValue: (double?)normalizedValue,
                value)
        {
        }

        private PropagatedEmission(
            IResolvedEmission emission,
            INeighborRelationship relationship,
            double? normalizedValue,
            double value)
        {
            Emission =
                emission ??
                throw new ArgumentNullException(
                    nameof(emission));

            Relationship =
                relationship ??
                throw new ArgumentNullException(
                    nameof(relationship));

            ValidateNonNegativeFinite(
                relationship.Distance,
                nameof(relationship));

            if (normalizedValue.HasValue)
            {
                ValidateNonNegativeFinite(
                    normalizedValue.Value,
                    nameof(normalizedValue));
            }

            ValidateNonNegativeFinite(
                value,
                nameof(value));

            PropagationDistance =
                relationship.Distance;

            ResolvedValue =
                emission.Value;

            NormalizedValue =
                normalizedValue;

            Value =
                value;
        }

        public IResolvedEmission Emission { get; }

        public INeighborRelationship Relationship { get; }

        public double PropagationDistance { get; }

        public double ResolvedValue { get; }

        public double? NormalizedValue { get; }

        public double Value { get; }

        private static void ValidateNonNegativeFinite(
            double value,
            string parameterName)
        {
            if (double.IsNaN(value) ||
                double.IsInfinity(value) ||
                value < 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    value,
                    "A propagated emission value must be " +
                    "finite and non-negative.");
            }
        }
    }
}