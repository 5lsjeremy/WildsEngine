using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Neighbors.Operators
{
    /// <summary>
    /// Applies a deterministic sequence of Neighbor sequence
    /// operators. Each stage receives the prior stage snapshot.
    /// </summary>
    public abstract class NeighborCompositeOperatorBase :
        NeighborSequenceOperatorBase
    {
        private readonly IReadOnlyList<INeighborSequenceOperator>
            _operators;

        protected NeighborCompositeOperatorBase(
            IReadOnlyList<INeighborSequenceOperator> operators)
        {
            if (operators == null)
            {
                throw new ArgumentNullException(
                    nameof(operators));
            }

            var snapshot =
                new INeighborSequenceOperator[operators.Count];

            for (var index = 0;
                 index < operators.Count;
                 index++)
            {
                snapshot[index] =
                    operators[index] ??
                    throw new ArgumentException(
                        "Neighbor operator pipelines cannot contain null stages.",
                        nameof(operators));
            }

            _operators = snapshot;
        }

        protected sealed override
            IReadOnlyList<INeighborRelationship> ApplyCore(
                IBaseObject subject,
                IReadOnlyList<INeighborRelationship> relationships)
        {
            var current =
                relationships;

            for (var index = 0;
                 index < _operators.Count;
                 index++)
            {
                current =
                    _operators[index].Apply(
                        subject,
                        current);
            }

            return current;
        }
    }
}
