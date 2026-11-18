using System;
using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Vectors
{
    /// <summary>
    /// Concrete immutable coordinate in consumer-defined
    /// Discovery geometry.
    /// </summary>
    public sealed class DiscoveryVectorComponent :
        IDiscoveryVectorComponent
    {
        public DiscoveryVectorComponent(
            string name,
            double value)
        {
            if (string.IsNullOrWhiteSpace(
                    name))
            {
                throw new ArgumentException(
                    "A vector component requires a name.",
                    nameof(name));
            }

            if (double.IsNaN(value) ||
                double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    "A vector component value must be finite.");
            }

            Name =
                name.Trim();

            Value =
                value;
        }

        public string Name { get; }

        public double Value { get; }
    }
}