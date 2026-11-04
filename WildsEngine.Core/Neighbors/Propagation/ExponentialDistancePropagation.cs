using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Abstractions.Neighbors.Propagation;

namespace WildsEngine.Core.Neighbors.Propagation
{
    public sealed class ExponentialDistancePropagation :
        INeighborPropagation
    {
        private readonly double _decay;

        public ExponentialDistancePropagation(
            double decay)
        {
            if (decay < 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(decay));
            }

            _decay =
                decay;
        }

        public double Propagate(
            double intensity,
            INeighborRelationship relationship)
        {
            if (relationship == null)
            {
                throw new ArgumentNullException(
                    nameof(relationship));
            }

            if (intensity < 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(intensity));
            }

            return intensity *
                   Math.Exp(
                       -_decay *
                       relationship.Distance);
        }
    }
}