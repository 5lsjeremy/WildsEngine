using System;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Neighbors.Operators
{
    /// <summary>
    /// Supplies validation and deterministic tie handling for
    /// Neighbor relationship comparers.
    /// </summary>
    public abstract class NeighborComparerBase :
        INeighborComparer
    {
        public int Compare(
            IBaseObject subject,
            INeighborRelationship left,
            INeighborRelationship right)
        {
            if (subject == null)
            {
                throw new ArgumentNullException(
                    nameof(subject));
            }

            if (left == null)
            {
                throw new ArgumentNullException(
                    nameof(left));
            }

            if (right == null)
            {
                throw new ArgumentNullException(
                    nameof(right));
            }

            var compared =
                CompareCore(
                    subject,
                    left,
                    right);

            if (compared != 0)
            {
                return compared;
            }

            compared = string.Compare(
                left.Membership,
                right.Membership,
                StringComparison.Ordinal);

            if (compared != 0)
            {
                return compared;
            }

            return left.Neighbor.Id.CompareTo(
                right.Neighbor.Id);
        }

        protected abstract int CompareCore(
            IBaseObject subject,
            INeighborRelationship left,
            INeighborRelationship right);
    }
}
