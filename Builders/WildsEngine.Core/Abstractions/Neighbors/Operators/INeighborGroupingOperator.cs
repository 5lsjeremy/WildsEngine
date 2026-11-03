using System.Collections.Generic;

namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Groups Neighbor relationships under stable string keys.
    /// </summary>
    public interface INeighborGroupingOperator :
        INeighborOperator<
            IReadOnlyDictionary<
                string,
                IReadOnlyList<INeighborRelationship>>>
    {
    }
}
