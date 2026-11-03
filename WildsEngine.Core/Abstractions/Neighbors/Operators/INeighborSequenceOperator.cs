using System.Collections.Generic;

namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Produces an ordered or filtered relationship sequence.
    /// </summary>
    public interface INeighborSequenceOperator :
        INeighborOperator<IReadOnlyList<INeighborRelationship>>
    {
    }
}
