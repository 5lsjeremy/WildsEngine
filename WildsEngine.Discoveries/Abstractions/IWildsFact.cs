namespace WildsEngine.Discoveries.Abstractions
{
    /// <summary>
    /// Wilds-compatible projection of one authoritative consumer-facing Fact.
    /// </summary>
    public interface IWildsFact :
        IWildsProjection<IFact>
    {
        IFact Fact { get; }
    }
}
