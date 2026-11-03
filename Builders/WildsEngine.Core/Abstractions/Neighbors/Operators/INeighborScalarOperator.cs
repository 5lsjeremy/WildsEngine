namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Produces one scalar value from a Neighbor snapshot.
    /// </summary>
    public interface INeighborScalarOperator<TResult> :
        INeighborOperator<TResult>
    {
    }
}
