using System;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Neighbors.Operators
{
    /// <summary>
    /// Supplies validation for reusable Neighbor predicates.
    /// </summary>
    public abstract class NeighborPredicateBase :
        INeighborPredicate
    {
        public bool Includes(
            IBaseObject subject,
            INeighborRelationship relationship)
        {
            if (subject == null)
            {
                throw new ArgumentNullException(
                    nameof(subject));
            }

            if (relationship == null)
            {
                throw new ArgumentNullException(
                    nameof(relationship));
            }

            return IncludesCore(
                subject,
                relationship);
        }

        protected abstract bool IncludesCore(
            IBaseObject subject,
            INeighborRelationship relationship);
    }
}
