using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Neighbors.Operators
{
    /// <summary>
    /// Base class for operators that return relationship
    /// sequences without mutating the source snapshot.
    /// </summary>
    public abstract class NeighborSequenceOperatorBase :
        NeighborOperatorBase<
            IReadOnlyList<INeighborRelationship>>,
        INeighborSequenceOperator
    {
        protected static IReadOnlyList<INeighborRelationship>
            Empty()
        {
            return new INeighborRelationship[0];
        }

        protected static IReadOnlyList<INeighborRelationship>
            Snapshot(
                List<INeighborRelationship> relationships)
        {
            return relationships.ToArray();
        }
    }
}
