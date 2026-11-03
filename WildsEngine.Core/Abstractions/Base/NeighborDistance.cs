using System;

namespace WildsEngine.Core.Topology
{
    /// <summary>
    /// Defines the canonical Wilds rules for relational distance.
    ///
    /// Distance is abstract and does not imply a physical
    /// unit of measurement.
    /// </summary>
    public static class NeighborDistance
    {
        public static bool IsValid(
            double distance)
        {
            return
                !double.IsNaN(distance) &&
                !double.IsInfinity(distance) &&
                distance >= 0d;
        }

        public static double Validate(
            double distance,
            string parameterName)
        {
            if (!IsValid(distance))
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    distance,
                    "Neighbor distance must be finite and non-negative.");
            }

            return distance;
        }

        public static bool IsAdjacent(
            double distance)
        {
            return
                Validate(
                    distance,
                    nameof(distance)) == 1d;
        }
    }
}