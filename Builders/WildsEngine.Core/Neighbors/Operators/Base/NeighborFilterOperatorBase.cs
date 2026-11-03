using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Neighbors.Operators
{
    /// <summary>
    /// Filters a relationship snapshot through one reusable
    /// predicate while preserving source order.
    /// </summary>
    public abstract class NeighborFilterOperatorBase :
        NeighborSequenceOperatorBase
    {
        private readonly INeighborPredicate _predicate;

        protected NeighborFilterOperatorBase(
            INeighborPredicate predicate)
        {
            _predicate =
                predicate ??
                throw new ArgumentNullException(
                    nameof(predicate));
        }

        protected sealed override
            IReadOnlyList<INeighborRelationship> ApplyCore(
                IBaseObject subject,
                IReadOnlyList<INeighborRelationship> relationships)
        {
            var result =
                new List<INeighborRelationship>();

            for (var index = 0;
                 index < relationships.Count;
                 index++)
            {
                var relationship =
                    relationships[index];

                if (_predicate.Includes(
                        subject,
                        relationship))
                {
                    result.Add(
                        relationship);
                }
            }

            return Snapshot(
                result);
        }
    }
}
