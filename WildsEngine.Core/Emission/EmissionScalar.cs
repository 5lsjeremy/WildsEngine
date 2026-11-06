using System;
using System.Globalization;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Emission
{
    /// <summary>
    /// Resolves ordinary finite numeric consumer values into
    /// Wilds emission scalars.
    ///
    /// No semantic conversion is performed for non-numeric
    /// consumer values.
    /// </summary>
    public sealed class EmissionScalar :
        IEmissionScalar
    {
        public double Resolve(
            object? value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(
                    nameof(value));
            }

            double scalar;

            switch (value)
            {
                case byte number:
                    scalar = number;
                    break;

                case sbyte number:
                    scalar = number;
                    break;

                case short number:
                    scalar = number;
                    break;

                case ushort number:
                    scalar = number;
                    break;

                case int number:
                    scalar = number;
                    break;

                case uint number:
                    scalar = number;
                    break;

                case long number:
                    scalar = number;
                    break;

                case ulong number:
                    scalar = number;
                    break;

                case float number:
                    scalar = number;
                    break;

                case double number:
                    scalar = number;
                    break;

                case decimal number:
                    scalar =
                        Convert.ToDouble(
                            number,
                            CultureInfo.InvariantCulture);
                    break;

                default:
                    throw new ArgumentException(
                        "Emission scalar values must be numeric.",
                        nameof(value));
            }

            if (double.IsNaN(scalar) ||
                double.IsInfinity(scalar))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    scalar,
                    "Emission scalar values must be finite.");
            }

            return scalar;
        }
    }
}