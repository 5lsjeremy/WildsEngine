using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Emission
{
    /// <summary>
    /// Normalizes one resolved-emission vector by
    /// proportional contribution.
    ///
    /// Resolved magnitudes remain unchanged. Each normalized
    /// component receives its proportional share of the
    /// resolved-vector total.
    /// </summary>
    public sealed class ProportionalEmissionNormalization :
        IEmissionNormalizationPolicy
    {
        public INormalizedEmissionVector Normalize(
            IResolvedEmissionVector vector)
        {
            if (vector == null)
            {
                throw new ArgumentNullException(
                    nameof(vector));
            }

            if (vector.ResolvedTotal <= 0.0)
            {
                throw new InvalidOperationException(
                    "An emission vector must contain a " +
                    "positive total before normalization.");
            }

            var normalized =
                new List<INormalizedEmission>(
                    vector.Dimension);

            for (var index = 0;
                 index < vector.Dimension;
                 index++)
            {
                var emission =
                    vector.Emissions[index];

                normalized.Add(
                    new NormalizedEmission(
                        emission,
                        emission.Value /
                        vector.ResolvedTotal));
            }

            return new NormalizedEmissionVector(
                vector,
                normalized);
        }
    }
}