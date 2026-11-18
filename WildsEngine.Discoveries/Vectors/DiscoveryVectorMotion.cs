using System;
using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Vectors
{
    /// <summary>
    /// Concrete temporal movement along one consumer-defined
    /// Discovery dimension.
    /// </summary>
    public sealed class DiscoveryVectorMotion :
        IDiscoveryVectorMotion
    {
        public DiscoveryVectorMotion(
            string dimension,
            double velocity,
            double acceleration)
        {
            if (string.IsNullOrWhiteSpace(
                    dimension))
            {
                throw new ArgumentException(
                    "Vector motion requires a dimension.",
                    nameof(dimension));
            }

            if (double.IsNaN(velocity) ||
                double.IsInfinity(velocity))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(velocity),
                    "Velocity must be finite.");
            }

            if (double.IsNaN(acceleration) ||
                double.IsInfinity(acceleration))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(acceleration),
                    "Acceleration must be finite.");
            }

            Dimension =
                dimension.Trim();

            Velocity =
                velocity;

            Acceleration =
                acceleration;
        }

        public string Dimension { get; }

        public double Velocity { get; }

        public double Acceleration { get; }
    }
}