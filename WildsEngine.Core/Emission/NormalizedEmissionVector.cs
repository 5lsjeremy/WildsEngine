using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Emission
{
    /// <summary>
    /// Immutable normalized representation of one resolved
    /// emission vector.
    ///
    /// Dimensions, ordering, exact resolved-emission
    /// references, and resolved magnitudes are preserved.
    /// </summary>
    public sealed class NormalizedEmissionVector :
        INormalizedEmissionVector
    {
        private const double NormalizationTolerance =
            1e-12;

        private readonly IReadOnlyList<INormalizedEmission>
            _emissions;

        public NormalizedEmissionVector(
            IResolvedEmissionVector source,
            IReadOnlyList<INormalizedEmission> emissions)
        {
            Source =
                source ??
                throw new ArgumentNullException(
                    nameof(source));

            if (emissions == null)
            {
                throw new ArgumentNullException(
                    nameof(emissions));
            }

            if (emissions.Count !=
                source.Dimension)
            {
                throw new ArgumentException(
                    "Normalized-vector dimension must match " +
                    "the resolved-vector dimension.",
                    nameof(emissions));
            }

            var snapshot =
                new INormalizedEmission[
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
                        "A normalized vector cannot contain " +
                        "a null component.",
                        nameof(emissions));

                if (!ReferenceEquals(
                        source.Emissions[index],
                        component.Emission))
                {
                    throw new ArgumentException(
                        "Normalization must preserve exact " +
                        "emission references and ordering.",
                        nameof(emissions));
                }

                if (component.ResolvedValue !=
                    source.Emissions[index].Value)
                {
                    throw new ArgumentException(
                        "Normalization must preserve exact " +
                        "resolved magnitudes.",
                        nameof(emissions));
                }

                var normalizedValue =
                    component.NormalizedValue;

                if (double.IsNaN(normalizedValue) ||
                    double.IsInfinity(normalizedValue) ||
                    normalizedValue < 0.0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(emissions),
                        normalizedValue,
                        "Normalized values must be finite " +
                        "and non-negative.");
                }

                snapshot[index] =
                    component;

                total +=
                    normalizedValue;

                if (double.IsNaN(total) ||
                    double.IsInfinity(total))
                {
                    throw new ArithmeticException(
                        "Normalized-vector accumulation " +
                        "produced a non-finite total.");
                }
            }

            if (Math.Abs(
                    total -
                    1.0) >
                NormalizationTolerance)
            {
                throw new ArgumentException(
                    "The normalized emission-vector total " +
                    "must equal one.",
                    nameof(emissions));
            }

            _emissions =
                Array.AsReadOnly(
                    snapshot);

            NormalizedTotal =
                total;
        }

        public IResolvedEmissionVector Source { get; }

        public IReadOnlyList<INormalizedEmission> Emissions =>
            _emissions;

        public int Dimension =>
            _emissions.Count;

        public double NormalizedTotal { get; }
    }
}