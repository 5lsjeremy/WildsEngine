using System.Collections.Generic;
namespace WildsEngine.Core.Abstractions
{
    public interface INeighborPropagation<TResult>
    {
        IReadOnlyList<TResult> Propagate(IBaseObject origin, string membership, double maximumDistance);
    }
}
