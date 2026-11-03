namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Describes a directional relationship from one Wilds
    /// object to another within a shared membership.
    ///
    /// Membership establishes neighborhood.
    /// Distance establishes proximity.
    /// </summary>
    public interface INeighborRelationship
    {
        IBaseObject Neighbor { get; }

        string Membership { get; }

        double Distance { get; }
    }
}