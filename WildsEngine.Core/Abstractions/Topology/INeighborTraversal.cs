using System.Collections.Generic;
namespace WildsEngine.Core.Abstractions
{
    public interface INeighborTraversal
    {
        IReadOnlyList<IBaseObject> Traverse(IBaseObject origin, string membership, double maximumDistance);
        bool IsReachable(IBaseObject origin, IBaseObject target, string membership, double maximumDistance);
    }
}
