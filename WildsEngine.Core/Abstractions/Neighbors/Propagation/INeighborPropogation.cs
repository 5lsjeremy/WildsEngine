namespace WildsEngine.Core.Abstractions.Neighbors.Propagation
{
    public interface INeighborPropagation
    {
        double Propagate(
            double intensity,
            INeighborRelationship relationship);
    }
}