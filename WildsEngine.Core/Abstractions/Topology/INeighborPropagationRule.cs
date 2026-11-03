namespace WildsEngine.Core.Abstractions
{
    public interface INeighborPropagationRule<TResult>
    {
        TResult Apply(IBaseObject origin, IBaseObject current, INeighborRelationship relationship, double accumulatedDistance);
    }
}
