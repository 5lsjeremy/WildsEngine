using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Neighbors.Operators
{
    /// <summary>
    /// Supplies argument validation and a stable execution
    /// boundary for Neighbor operators.
    /// </summary>
    public abstract class NeighborOperatorBase<TResult> :
        INeighborOperator<TResult>
    {
        public TResult Apply(
            IBaseObject subject,
            IReadOnlyList<INeighborRelationship> relationships)
        {
            if (subject == null)
            {
                throw new ArgumentNullException(
                    nameof(subject));
            }

            if (relationships == null)
            {
                throw new ArgumentNullException(
                    nameof(relationships));
            }

            return ApplyCore(
                subject,
                relationships);
        }

        protected abstract TResult ApplyCore(
            IBaseObject subject,
            IReadOnlyList<INeighborRelationship> relationships);
    }
}
