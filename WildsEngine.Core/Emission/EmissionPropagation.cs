using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Abstractions.Neighbors.Propagation;

namespace WildsEngine.Core.Emission
{
    /// <summary>
    /// Bridges resolved Wilds emissions into the existing
    /// Neighbor intensity-propagation contract.
    ///
    /// Emission resolution remains complete before this
    /// operation begins. Propagation never recalculates the
    /// emitter's base scalar, topology scalar, or composition.
    /// </summary>
    public sealed class EmissionPropagation
    {
        private readonly INeighborPropagation _propagation;

        public EmissionPropagation(
            INeighborPropagation propagation)
        {
            _propagation =
                propagation ??
                throw new ArgumentNullException(
                    nameof(propagation));
        }

        public IPropagatedEmission Propagate(
            IResolvedEmission emission,
            INeighborRelationship relationship)
        {
            if (emission == null)
            {
                throw new ArgumentNullException(
                    nameof(emission));
            }

            if (relationship == null)
            {
                throw new ArgumentNullException(
                    nameof(relationship));
            }

            var propagated =
                _propagation.Propagate(
                    emission.Value,
                    relationship);

            return new PropagatedEmission(
                emission,
                relationship,
                propagated);
        }
    }
}