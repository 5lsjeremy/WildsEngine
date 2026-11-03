using System.Collections.Generic;
namespace WildsEngine.Core.Abstractions
{
    public interface INeighborCluster
    {
        string Membership { get; }
        IReadOnlyList<IBaseObject> Members { get; }
    }
}
