using System;
using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Vectors
{
    /// <summary>
    /// Concrete consumer-authorized contribution into one
    /// Discovery-vector dimension.
    ///
    /// The contribution preserves the exact vector input and,
    /// when applicable, the exact Fact from that Discovery that
    /// produced the contribution.
    /// </summary>
    public sealed class DiscoveryVectorContribution :
        IDiscoveryVectorContribution
    {
        public DiscoveryVectorContribution(
            IDiscoveryVectorInput input,
            string dimension,
            double value,
            double weight,
            IFact? fact = null)
        {
            Input =
                input ??
                throw new ArgumentNullException(
                    nameof(input));

            if (string.IsNullOrWhiteSpace(
                    dimension))
            {
                throw new ArgumentException(
                    "A vector contribution requires a dimension.",
                    nameof(dimension));
            }

            if (double.IsNaN(value) ||
                double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    "A vector contribution value must be finite.");
            }

            if (double.IsNaN(weight) ||
                double.IsInfinity(weight))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(weight),
                    "A vector contribution weight must be finite.");
            }

            if (fact != null &&
                !ReferenceEquals(
                    fact.Entity,
                    Input.Discovery.Entity))
            {
                throw new ArgumentException(
                    "A contributing Fact must belong to the exact " +
                    "Entity that owns the authoritative Discovery.",
                    nameof(fact));
            }

            Dimension =
                dimension.Trim();

            Value =
                value;

            Weight =
                weight;

            Fact =
                fact;
        }

        public IDiscoveryVectorInput Input { get; }

        public IFact? Fact { get; }

        public string Dimension { get; }

        public double Value { get; }

        public double Weight { get; }
    }
}