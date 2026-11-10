using System;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Emission
{
    public sealed class NormalizedEmission :
        INormalizedEmission
    {
        public NormalizedEmission(
            IResolvedEmission emission,
            double normalizedValue)
        {
            Emission =
                emission ??
                throw new ArgumentNullException(
                    nameof(emission));

            if (double.IsNaN(normalizedValue) ||
                double.IsInfinity(normalizedValue) ||
                normalizedValue < 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(normalizedValue),
                    normalizedValue,
                    "A normalized emission value must be " +
                    "finite and non-negative.");
            }

            ResolvedValue =
                emission.Value;

            NormalizedValue =
                normalizedValue;
        }

        public IResolvedEmission Emission { get; }

        public double ResolvedValue { get; }

        public double NormalizedValue { get; }
    }
}