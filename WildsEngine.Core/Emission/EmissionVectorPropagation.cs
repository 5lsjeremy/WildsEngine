using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Abstractions.Neighbors.Propagation;

namespace WildsEngine.Core.Emission
{
    /// <summary>
    /// Normalizes and propagates one immutable resolved
    /// emission vector through one exact Neighbor
    /// relationship.
    ///
    /// The operation preserves vector identity, component
    /// ordering, exact emission references, resolved values,
    /// normalized values, and relationship identity.
    /// </summary>
    public sealed class EmissionVectorPropagation
    {
        private readonly IEmissionNormalizationPolicy
            _normalization;

        private readonly INeighborPropagation
            _propagation;

        public EmissionVectorPropagation(
            IEmissionNormalizationPolicy normalization,
            INeighborPropagation propagation)
        {
            _normalization =
                normalization ??
                throw new ArgumentNullException(
                    nameof(normalization));

            _propagation =
                propagation ??
                throw new ArgumentNullException(
                    nameof(propagation));
        }

        public IPropagatedEmissionVector Propagate(
            IResolvedEmissionVector vector,
            INeighborRelationship relationship)
        {
            if (vector == null)
            {
                throw new ArgumentNullException(
                    nameof(vector));
            }

            if (relationship == null)
            {
                throw new ArgumentNullException(
                    nameof(relationship));
            }

            var normalized =
                _normalization.Normalize(
                    vector);

            if (normalized == null)
            {
                throw new InvalidOperationException(
                    "The normalization policy returned null.");
            }

            if (!ReferenceEquals(
                    vector,
                    normalized.Source))
            {
                throw new InvalidOperationException(
                    "Normalization must preserve the exact " +
                    "resolved-emission vector.");
            }

            var propagated =
                new List<IPropagatedEmission>(
                    normalized.Dimension);

            for (var index = 0;
                 index < normalized.Dimension;
                 index++)
            {
                var component =
                    normalized.Emissions[index];

                var received =
                    _propagation.Propagate(
                        component.NormalizedValue,
                        relationship);

                propagated.Add(
                    new PropagatedEmission(
                        component.Emission,
                        relationship,
                        component.NormalizedValue,
                        received));
            }

            return new PropagatedEmissionVector(
                normalized,
                relationship,
                propagated);
        }
    }
}