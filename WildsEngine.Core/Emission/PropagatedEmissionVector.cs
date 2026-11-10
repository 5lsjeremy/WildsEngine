using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Emission
{
    /// <summary>
    /// Immutable result of propagating one normalized
    /// emission vector through one exact Neighbor
    /// relationship.
    ///
    /// Complete resolved and normalized provenance remains
    /// available through Source.
    ///
    /// PropagationDistance preserves the relationship distance
    /// used for the complete vector operation.
    /// </summary>
    public sealed class PropagatedEmissionVector :
        IPropagatedEmissionVector
    {
        private readonly IReadOnlyList<IPropagatedEmission>
            _emissions;

        public PropagatedEmissionVector(
            INormalizedEmissionVector source,
            INeighborRelationship relationship,
            IReadOnlyList<IPropagatedEmission> emissions)
        {
            Source =
                source ??
                throw new ArgumentNullException(
                    nameof(source));

            Relationship =
                relationship ??
                throw new ArgumentNullException(
                    nameof(relationship));

            ValidateNonNegativeFinite(
                relationship.Distance,
                nameof(relationship));

            PropagationDistance =
                relationship.Distance;

            if (emissions == null)
            {
                throw new ArgumentNullException(
                    nameof(emissions));
            }

            if (emissions.Count !=
                source.Dimension)
            {
                throw new ArgumentException(
                    "Propagated-vector dimension must match " +
                    "the normalized-vector dimension.",
                    nameof(emissions));
            }

            var snapshot =
                new IPropagatedEmission[
                    emissions.Count];

            var total =
                0.0;

            for (var index = 0;
                 index < emissions.Count;
                 index++)
            {
                var component =
                    emissions[index] ??
                    throw new ArgumentException(
                        "A propagated vector cannot contain " +
                        "a null component.",
                        nameof(emissions));

                var normalized =
                    source.Emissions[index];

                if (!ReferenceEquals(
                        normalized.Emission,
                        component.Emission))
                {
                    throw new ArgumentException(
                        "Propagation must preserve exact " +
                        "emission references and ordering.",
                        nameof(emissions));
                }

                if (!ReferenceEquals(
                        relationship,
                        component.Relationship))
                {
                    throw new ArgumentException(
                        "Every propagated component must " +
                        "preserve the vector relationship.",
                        nameof(emissions));
                }

                if (component.PropagationDistance !=
                    PropagationDistance)
                {
                    throw new ArgumentException(
                        "Every propagated component must " +
                        "preserve the vector propagation " +
                        "distance.",
                        nameof(emissions));
                }

                if (component.ResolvedValue !=
                    normalized.ResolvedValue)
                {
                    throw new ArgumentException(
                        "Propagation must preserve exact " +
                        "resolved magnitudes.",
                        nameof(emissions));
                }

                if (!component.NormalizedValue.HasValue ||
                    component.NormalizedValue.Value !=
                    normalized.NormalizedValue)
                {
                    throw new ArgumentException(
                        "Propagation must preserve exact " +
                        "normalized magnitudes.",
                        nameof(emissions));
                }

                ValidateNonNegativeFinite(
                    component.Value,
                    nameof(emissions));

                snapshot[index] =
                    component;

                total +=
                    component.Value;

                if (double.IsNaN(total) ||
                    double.IsInfinity(total))
                {
                    throw new ArithmeticException(
                        "Propagated-vector accumulation " +
                        "produced a non-finite total.");
                }
            }

            _emissions =
                Array.AsReadOnly(
                    snapshot);

            ReceivedTotal =
                total;
        }

        public INormalizedEmissionVector Source { get; }

        public INeighborRelationship Relationship { get; }

        public double PropagationDistance { get; }

        public IReadOnlyList<IPropagatedEmission> Emissions =>
            _emissions;

        public int Dimension =>
            _emissions.Count;

        public double ReceivedTotal { get; }

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
                    "A propagated vector value must be " +
                    "finite and non-negative.");
            }
        }
    }
}