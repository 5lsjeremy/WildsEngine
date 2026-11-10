using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Emission
{
    /// <summary>
    /// Immutable ordered carrier for an arbitrary
    /// N-dimensional set of resolved emissions.
    ///
    /// Every component belongs to the same exact source
    /// emitter.
    ///
    /// Every emission name must be unique within the vector.
    ///
    /// The vector owns no normalization, propagation,
    /// eligibility, or receiver behavior.
    /// </summary>
    public sealed class ResolvedEmissionVector :
        IResolvedEmissionVector
    {
        private readonly IReadOnlyList<IResolvedEmission>
            _emissions;

        public ResolvedEmissionVector(
            IReadOnlyList<IResolvedEmission> emissions)
        {
            if (emissions == null)
            {
                throw new ArgumentNullException(
                    nameof(emissions));
            }

            if (emissions.Count == 0)
            {
                throw new ArgumentException(
                    "An emission vector cannot be empty.",
                    nameof(emissions));
            }

            var first =
                emissions[0] ??
                throw new ArgumentException(
                    "An emission vector cannot contain " +
                    "a null emission.",
                    nameof(emissions));

            Source =
                first.Source;

            var names =
                new HashSet<string>(
                    StringComparer.Ordinal);

            var snapshot =
                new IResolvedEmission[
                    emissions.Count];

            var total =
                0.0;

            for (var index = 0;
                 index < emissions.Count;
                 index++)
            {
                var emission =
                    emissions[index] ??
                    throw new ArgumentException(
                        "An emission vector cannot contain " +
                        "a null emission.",
                        nameof(emissions));

                if (!ReferenceEquals(
                        Source,
                        emission.Source))
                {
                    throw new ArgumentException(
                        "Every emission in a vector must " +
                        "preserve the same exact source emitter.",
                        nameof(emissions));
                }

                if (!names.Add(
                        emission.Name))
                {
                    throw new ArgumentException(
                        "An emission vector cannot contain " +
                        "duplicate emission names.",
                        nameof(emissions));
                }

                if (double.IsNaN(
                        emission.Value) ||
                    double.IsInfinity(
                        emission.Value) ||
                    emission.Value < 0.0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(emissions),
                        emission.Value,
                        "Emission vectors require finite, " +
                        "non-negative resolved magnitudes.");
                }

                snapshot[index] =
                    emission;

                total +=
                    emission.Value;

                if (double.IsNaN(total) ||
                    double.IsInfinity(total))
                {
                    throw new ArithmeticException(
                        "Resolved-vector accumulation produced " +
                        "a non-finite total.");
                }
            }

            _emissions =
                Array.AsReadOnly(
                    snapshot);

            ResolvedTotal =
                total;
        }

        public IEmitter Source { get; }

        public IReadOnlyList<IResolvedEmission> Emissions =>
            _emissions;

        public int Dimension =>
            _emissions.Count;

        public double ResolvedTotal { get; }
    }
}