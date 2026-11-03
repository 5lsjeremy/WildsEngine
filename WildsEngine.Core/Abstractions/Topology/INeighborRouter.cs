namespace WildsEngine.Core.Abstractions
{
    public interface INeighborRouter
    {
        bool TryFindRoute(IBaseObject origin, IBaseObject target, string membership, out INeighborRoute? route);
    }
}
