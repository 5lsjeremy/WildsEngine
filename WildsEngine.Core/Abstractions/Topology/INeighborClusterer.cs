using System.Collections.Generic;
namespace WildsEngine.Core.Abstractions
{
    public interface INeighborClusterer
    {
        IReadOnlyList<INeighborCluster> FindClusters(IReadOnlyList<IBaseObject> objects, string membership);
    }
}
