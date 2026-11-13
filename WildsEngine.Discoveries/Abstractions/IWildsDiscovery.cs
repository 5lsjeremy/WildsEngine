namespace WildsEngine.Discoveries.Abstractions
{
    /// <summary>
    /// Wilds-compatible projection of one authoritative consumer-facing Discovery.
    /// </summary>
    public interface IWildsDiscovery :
        IWildsProjection<IDiscovery>
    {
        IDiscovery Discovery { get; }
    }
}
