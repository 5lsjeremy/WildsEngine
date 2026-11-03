namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Compares two relationships for deterministic ordering.
    /// </summary>
    public interface INeighborComparer
    {
        int Compare(
            IBaseObject subject,
            INeighborRelationship left,
            INeighborRelationship right);
    }
}
