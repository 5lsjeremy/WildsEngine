namespace WildsEngine.Core.Abstractions.Neighbors.Propagation
{
    public interface INeighborPropagationEligibility
    {
        bool IsEligible(
            double intensity);
    }
}