using System;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Emission
{
    /// <summary>
    /// Base implementation for emission composition policies.
    ///
    /// Supplies finite-value validation around the composition
    /// operation while leaving the mathematical relationship
    /// between emission and topology to derived implementations.
    /// </summary>
    public abstract class EmissionCompositionBase :
        IEmissionComposition
    {
        public double Compose(
            double emission,
            double topology)
        {
            ValidateFinite(
                emission,
                nameof(emission));

            ValidateFinite(
                topology,
                nameof(topology));

            var resolved =
                ComposeCore(
                    emission,
                    topology);

            if (double.IsNaN(resolved) ||
                double.IsInfinity(resolved))
            {
                throw new ArithmeticException(
                    "Emission composition must produce a finite scalar.");
            }

            return resolved;
        }

        protected abstract double ComposeCore(
            double emission,
            double topology);

        private static void ValidateFinite(
            double value,
            string parameterName)
        {
            if (double.IsNaN(value) ||
                double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    value,
                    "Emission composition values must be finite.");
            }
        }
    }
}