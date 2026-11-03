namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Determines whether a relationship belongs in an
    /// operator result.
    /// </summary>
    public interface INeighborPredicate
    {
        bool Includes(
            IBaseObject subject,
            INeighborRelationship relationship);
    }
}
