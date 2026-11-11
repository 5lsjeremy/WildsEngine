namespace WildsEngine.Core.Entities.Abstractions
{
    /// <summary>
    /// Represents the Wilds-compatible projection of one
    /// authoritative Entity.
    ///
    /// The original Entity remains authoritative for its
    /// Facts, Discoveries, and Entity identity.
    ///
    /// Promotion adds normal Wilds runtime semantics.
    /// </summary>
    public interface IWildsEntity :
        IWildsProjection<IEntity>
    {
        IEntity Entity { get; }
    }
}