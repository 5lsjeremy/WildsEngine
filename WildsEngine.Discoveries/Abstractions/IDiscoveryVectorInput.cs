namespace WildsEngine.Discoveries.Abstractions
{
    /// <summary>
    /// Represents one authoritative Discovery selected for
    /// vectorization.
    ///
    /// Provenance may carry the external source that caused this
    /// Discovery to enter the sample.
    ///
    /// Discoveries preserves that object without interpreting
    /// or owning it.
    /// </summary>
    public interface IDiscoveryVectorInput
    {
        IDiscovery Discovery { get; }

        object? Provenance { get; }
    }
}
