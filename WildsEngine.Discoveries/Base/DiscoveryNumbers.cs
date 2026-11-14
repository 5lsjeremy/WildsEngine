using System;

namespace WildsEngine.Discoveries.Base
{
    /// <summary>
    /// Shared finite-number validation for Discoveries
    /// composition results.
    /// </summary>
    internal static class DiscoveryNumbers
    {
        public static double Finite(
            double value,
            string parameterName)
        {
            if (double.IsNaN(value) ||
                double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    value,
                    "Value must be finite.");
            }

            return value;
        }

        public static double NonNegative(
            double value,
            string parameterName)
        {
            Finite(
                value,
                parameterName);

            if (value < 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    value,
                    "Value must be non-negative.");
            }

            return value;
        }

        public static double Unit(
            double value,
            string parameterName)
        {
            Finite(
                value,
                parameterName);

            if (value < 0.0 ||
                value > 1.0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    value,
                    "Value must be within [0,1].");
            }

            return value;
        }
    }
}
