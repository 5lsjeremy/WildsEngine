using System.Collections.Generic;
namespace WildsEngine.Core.Abstractions
{
    public interface INeighborRoute
    {
        string Membership { get; }
        IReadOnlyList<IBaseObject> Nodes { get; }
        double Distance { get; }
        int Hops { get; }
    }
}
