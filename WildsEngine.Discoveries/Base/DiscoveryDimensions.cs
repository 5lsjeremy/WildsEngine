using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Base
{
    /// <summary>
    /// Shared dimension validation for Discovery geometry.
    /// </summary>
    internal static class DiscoveryDimensions
    {
        public static void ValidateComponents(
            IReadOnlyList<IDiscoveryVectorComponent> components,
            string parameterName)
        {
            if (components == null)
            {
                throw new ArgumentNullException(
                    parameterName);
            }

            var names =
                new HashSet<string>(
                    StringComparer.Ordinal);

            for (var index = 0;
                 index < components.Count;
                 index++)
            {
                var component =
                    components[index] ??
                    throw new ArgumentException(
                        "Discovery-vector components cannot contain null items.",
                        parameterName);

                if (string.IsNullOrWhiteSpace(
                        component.Name))
                {
                    throw new ArgumentException(
                        "Discovery-vector component names cannot be empty.",
                        parameterName);
                }

                if (!names.Add(
                        component.Name))
                {
                    throw new ArgumentException(
                        "Discovery-vector component names must be unique.",
                        parameterName);
                }

                DiscoveryNumbers.Finite(
                    component.Value,
                    parameterName);
            }
        }

        public static void ValidateContributions(
            IReadOnlyList<IDiscoveryVectorContribution> contributions,
            IReadOnlyList<IDiscoveryVectorComponent> components,
            string parameterName)
        {
            if (contributions == null)
            {
                throw new ArgumentNullException(
                    parameterName);
            }

            if (components == null)
            {
                throw new ArgumentNullException(
                    nameof(components));
            }

            var dimensions =
                new HashSet<string>(
                    StringComparer.Ordinal);

            for (var index = 0;
                 index < components.Count;
                 index++)
            {
                dimensions.Add(
                    components[index].Name);
            }

            for (var index = 0;
                 index < contributions.Count;
                 index++)
            {
                var contribution =
                    contributions[index] ??
                    throw new ArgumentException(
                        "Discovery-vector contributions cannot contain null items.",
                        parameterName);

                if (contribution.Input == null)
                {
                    throw new ArgumentException(
                        "Discovery-vector contributions require an input.",
                        parameterName);
                }

                if (contribution.Input.Discovery == null)
                {
                    throw new ArgumentException(
                        "Discovery-vector inputs require a Discovery.",
                        parameterName);
                }

                if (string.IsNullOrWhiteSpace(
                        contribution.Dimension))
                {
                    throw new ArgumentException(
                        "Discovery-vector contribution dimensions cannot be empty.",
                        parameterName);
                }

                if (!dimensions.Contains(
                        contribution.Dimension))
                {
                    throw new ArgumentException(
                        "Every contribution dimension must exist in the vector.",
                        parameterName);
                }

                if (contribution.Fact != null &&
                    !ReferenceEquals(
                        contribution.Fact.Entity,
                        contribution.Input.Discovery.Entity))
                {
                    throw new ArgumentException(
                        "A contributing Fact must belong to the same exact Entity as its Discovery.",
                        parameterName);
                }

                DiscoveryNumbers.Finite(
                    contribution.Value,
                    parameterName);

                DiscoveryNumbers.NonNegative(
                    contribution.Weight,
                    parameterName);
            }
        }

        public static void ValidateMotion(
            IReadOnlyList<IDiscoveryVectorMotion> motion,
            IReadOnlyList<IDiscoveryVectorComponent> components,
            string parameterName)
        {
            if (motion == null)
            {
                throw new ArgumentNullException(
                    parameterName);
            }

            if (components == null)
            {
                throw new ArgumentNullException(
                    nameof(components));
            }

            var dimensions =
                new HashSet<string>(
                    StringComparer.Ordinal);

            for (var index = 0;
                 index < components.Count;
                 index++)
            {
                dimensions.Add(
                    components[index].Name);
            }

            var motionDimensions =
                new HashSet<string>(
                    StringComparer.Ordinal);

            for (var index = 0;
                 index < motion.Count;
                 index++)
            {
                var item =
                    motion[index] ??
                    throw new ArgumentException(
                        "Discovery-vector motion cannot contain null items.",
                        parameterName);

                if (string.IsNullOrWhiteSpace(
                        item.Dimension))
                {
                    throw new ArgumentException(
                        "Discovery-vector motion dimensions cannot be empty.",
                        parameterName);
                }

                if (!dimensions.Contains(
                        item.Dimension))
                {
                    throw new ArgumentException(
                        "Every motion dimension must exist in the vector.",
                        parameterName);
                }

                if (!motionDimensions.Add(
                        item.Dimension))
                {
                    throw new ArgumentException(
                        "Only one motion value may exist per vector dimension.",
                        parameterName);
                }

                DiscoveryNumbers.Finite(
                    item.Velocity,
                    parameterName);

                DiscoveryNumbers.Finite(
                    item.Acceleration,
                    parameterName);
            }
        }
    }
}
