using System.Collections.Generic;

namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Selects a bounded sample from a relationship snapshot.
    /// </summary>
    public interface INeighborSampler :
        INeighborSequenceOperator
    {
        int Count { get; }
    }
}
